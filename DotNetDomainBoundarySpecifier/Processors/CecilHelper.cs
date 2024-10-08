﻿namespace DotNetDomainBoundarySpecifier.Processors;

static class CecilHelper
{
    static readonly CachedObjectMap Cache = new()
    {
        Timeout = TimeSpan.FromDays(3)
    };

    public static AssemblyAnalyse AnalyzeAssembly(Scope scope, AssemblyDefinition assemblyDefinition)
    {
        var types = new List<TypeDefinition>();

        foreach (var moduleDefinition in assemblyDefinition.Modules)
        {
            foreach (var type in moduleDefinition.Types)
            {
                VisitType(types, type);
            }
        }

        var calledMethods = new List<MethodReference>();

        foreach (var typeDefinition in types)
        {
            if (typeDefinition.HasMethods)
            {
                foreach (var methodDefinition in typeDefinition.Methods)
                {
                    if (methodDefinition.HasBody)
                    {
                        foreach (var instruction in methodDefinition.Body.Instructions)
                        {
                            if (instruction.OpCode == OpCodes.Call ||
                                instruction.OpCode == OpCodes.Callvirt ||
                                instruction.OpCode == OpCodes.Calli)
                            {
                                if (instruction.Operand is MethodReference mr)
                                {
                                    if (IsMethodBelongToExternalDomain(scope, mr))
                                    {
                                        calledMethods.Add(mr);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return new()
        {
            AssemblyDefinition = assemblyDefinition,
            Types              = types,
            CalledMethods      = calledMethods
        };

        static void VisitType(List<TypeDefinition> types, TypeDefinition typeDefinition)
        {
            types.Add(typeDefinition);

            if (typeDefinition.HasNestedTypes)
            {
                foreach (var nestedType in typeDefinition.NestedTypes)
                {
                    VisitType(types, nestedType);
                }
            }
        }
    }

    public static MethodDefinition FindMethod(Scope scope, string assemblyFileName, string fullTypeName, string fullMethodName)
    {
        return scope.GetTypesInAssemblyFile(assemblyFileName)
                    .FirstOrDefault(t => t.FullName == fullTypeName)?
                    .Methods.FirstOrDefault(m => hasMatch(m, fullMethodName));


        static bool hasMatch(MethodDefinition methodDefinition, string fullMethodNameOrOnlyMethodName)
        {
            if (methodDefinition.FullName == fullMethodNameOrOnlyMethodName)
            {
                return true;
            }

            if (methodDefinition.Name == fullMethodNameOrOnlyMethodName)
            {
                return true;
            }

            return false;
        }
    }

    public static IReadOnlyList<PropertyDefinition> FindUsedProperties(this IReadOnlyList<AssemblyAnalyse> analyses, TypeDefinition searchType)
    {
        var usedProperties = new List<PropertyDefinition>();

        foreach (var propertyDefinition in searchType.Properties)
        {
            foreach (var assemblyAnalyse in analyses)
            {
                if (HasUsage(assemblyAnalyse, propertyDefinition))
                {
                    usedProperties.Add(propertyDefinition);
                }
            }
        }

        return usedProperties.Distinct(IsSame);

        static bool IsSame(PropertyDefinition a, PropertyDefinition b)
        {
            return a.FullName == b.FullName;
        }

        static bool HasUsage(AssemblyAnalyse assemblyAnalyse, PropertyDefinition propertyDefinition)
        {
            foreach (var mr in assemblyAnalyse.CalledMethods)
            {
                if (propertyDefinition.GetMethod?.FullName == mr.FullName ||
                    propertyDefinition.SetMethod?.FullName == mr.FullName)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public static string GetShortNameInCsharp(this TypeReference typeReference)
    {
        var name = typeReference.Name;

        if (name == "String")
        {
            return "string";
        }

        if (name == "Decimal")
        {
            return "decimal";
        }

        if (name == "Double")
        {
            return "double";
        }

        if (name is "Int64")
        {
            return "long";
        }

        if (name is "Int32")
        {
            return "int";
        }

        if (name is "Int16")
        {
            return "short";
        }

        if (name is "Byte")
        {
            return "byte";
        }

        if (name is "Boolean")
        {
            return "bool";
        }

        if (name is "DateTime")
        {
            return "DateTime";
        }

        if (typeReference is GenericInstanceType genericInstanceType)
        {
            if (name is "Nullable`1")
            {
                return $"{GetShortNameInCsharp(genericInstanceType.GenericArguments[0])}?";
            }

            if (name is "List`1")
            {
                return $"List<{GetShortNameInCsharp(genericInstanceType.GenericArguments[0])}>";
            }

            if (name is "ObservableCollection`1")
            {
                return $"ObservableCollection<{GetShortNameInCsharp(genericInstanceType.GenericArguments[0])}>";
            }
        }

        return name;
    }

    public static IReadOnlyList<TypeDefinition> GetTypesInAssemblyFile(Scope scope, string assemblyFileName)
    {
        if (assemblyFileName.HasNoValue())
        {
            return [];
        }

        var config = scope.Config;

        var filePath = Path.Combine(config.AssemblySearchDirectory, assemblyFileName);

        return Cache.AccessValue(nameof(GetTypesInAssemblyFile) + filePath, () =>
        {
            var result = ReadAssemblyDefinition(filePath);
            if (result.HasError)
            {
                return [];
            }

            var typeList = new List<TypeDefinition>();

            foreach (var moduleDefinition in result.Value.Modules)
            {
                foreach (var type in moduleDefinition.Types)
                {
                    if (config.SkipTypes.Contains(type.FullName))
                    {
                        continue;
                    }

                    typeList.Add(type);
                }
            }

            return typeList;
        });
    }

    public static bool IsMethodBelongToExternalDomain(Scope scope, MethodReference methodReference)
    {
        var config = scope.Config;

        foreach (var partOfFileName in config.ExternalDomainFileNameContains)
        {
            if (methodReference.DeclaringType.Scope.Name.Contains(partOfFileName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public static Result<AssemblyDefinition> ReadAssemblyDefinition(string filePath)
    {
        return Cache.AccessValue(nameof(ReadAssemblyDefinition) + filePath, () => Try(() =>
        {
            var resolver = new DefaultAssemblyResolver();

            resolver.AddSearchDirectory(Path.GetDirectoryName(filePath));

            return AssemblyDefinition.ReadAssembly(filePath, new() { AssemblyResolver = resolver });
        }));
    }
}