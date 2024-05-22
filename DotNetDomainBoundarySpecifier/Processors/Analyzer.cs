using System.Text;

namespace DotNetDomainBoundarySpecifier.Processors;

static class Analyzer
{
    static readonly CachedObjectMap Cache = new()
    {
        Timeout = TimeSpan.FromDays(3)
    };

    public static TypeReference GetValueTypeIfTypeIsMonadType(TypeReference typeReference)
    {
        if (IsMonadType(typeReference))
        {
            return GetMonadValueType(typeReference);
        }

        return typeReference;
    }
    
    static bool IsMonadType(TypeReference typeReference)
    {
        if (typeReference.Name == "GenericResponse`1" &&
            typeReference is GenericInstanceType)
        {
            return true;
        }

        return false;
    }

     static TypeReference GetMonadValueType(TypeReference typeReference)
    {
        var genericInstanceType = (GenericInstanceType)typeReference;
        
        return genericInstanceType.GenericArguments[0];
    }
    
    public static ImmutableList<TableModel> AnalyzeMethod(ServiceContext serviceContext, AnalyzeMethodInput input)
    {
        var records = ImmutableList<TableModel>.Empty;

        var methodDefinition =
            serviceContext.GetTypesInAssemblyFile(input.AssemblyFileName)
               .FirstOrDefault(t => t.FullName == input.TypeFullName)
              ?.Methods.FirstOrDefault(m => m.FullName == input.MethodFullName);

        if (methodDefinition is null)
        {
            return records;
        }

        foreach (var parameterDefinition in methodDefinition.Parameters)
        {
            records = pushType(serviceContext, input, methodDefinition, records, parameterDefinition.ParameterType);
        }

        records = pushType(serviceContext, input, methodDefinition, records, methodDefinition.ReturnType);

        return records;

        static ImmutableList<TableModel> pushType(ServiceContext serviceContext, AnalyzeMethodInput input, MethodDefinition methodDefinition, ImmutableList<TableModel> records, TypeReference typeReference)
        {
            var config = serviceContext.Config;

            if (IsDotNetCoreType(typeReference.FullName))
            {
                return records;
            }

            typeReference = GetValueTypeIfTypeIsMonadType(typeReference);
            
            var typeDefinition = typeReference.Resolve();

            var usedProperties = GetDomainAssemblies(serviceContext).FindUsedProperties(typeDefinition);
            if (usedProperties.Count is 0)
            {
                return records;
            }

            foreach (var propertyDefinition in usedProperties)
            {
                records = records.Add(new()
                {
                    ExternalAssemblyFileName = input.AssemblyFileName,
                    ExternalClassFullName    = input.TypeFullName,
                    ExternalMethodFullName   = methodDefinition.FullName,
                    ModuleName               = config.ModuleName,
                    RelatedClassFullName     = typeDefinition.FullName,
                    RelatedPropertyFullName  = propertyDefinition.FullName
                });

                records = pushType(serviceContext, input, methodDefinition, records, propertyDefinition.PropertyType);
            }

            return records;
        }
    }

    public static CodeGenerationOutput GenerateCode(ServiceContext serviceContext, AnalyzeMethodInput input, ImmutableList<TableModel> records)
    {
        const string padding = "    ";

        var targetMethod =
            serviceContext.GetTypesInAssemblyFile(input.AssemblyFileName)
               .FirstOrDefault(t => t.FullName == input.TypeFullName)
              ?.Methods.FirstOrDefault(m => m.FullName == input.MethodFullName);

        if (targetMethod is null)
        {
            return default;
        }

        var targetType = targetMethod.DeclaringType;

        var names = CalculateNames(targetType.FullName, targetMethod.Name);

        var contractFile = new StringBuilder();
        var processFile = new StringBuilder();

        TypeReference outputTypeAsAlreadyExistingType = null;
        var outputTypeIsAlreadyExistingType = false;

        if (targetMethod.ReturnType is GenericInstanceType genericInstanceType)
        {
            //if (IsDotNetCoreType(genericInstanceType.GenericArguments[0].FullName))
            {
                outputTypeAsAlreadyExistingType = genericInstanceType.GenericArguments[0];

                outputTypeIsAlreadyExistingType = true;
                
                
            }
        }

        if (IsDotNetCoreType(targetMethod.ReturnType.FullName))
        {
            outputTypeAsAlreadyExistingType = targetMethod.ReturnType;

            outputTypeIsAlreadyExistingType = true;
        }

        var outputTypeName = "Output";
        if (outputTypeIsAlreadyExistingType)
        {
            outputTypeName = outputTypeAsAlreadyExistingType.GetShortNameInCsharp();
        }

        contractFile.AppendLine($"namespace {names.ContractsProject.NamespaceName};");
        contractFile.AppendLine();

        processFile.AppendLine($"using Input = {names.ContractsProject.NamespaceName}.{targetMethod.Name}Input;");
        if (outputTypeIsAlreadyExistingType)
        {
            processFile.AppendLine($"using Output = {(outputTypeName == "DateTime" ? "System.DateTime" : outputTypeName)};");
        }
        else
        {
            processFile.AppendLine($"using Output = {names.ContractsProject.NamespaceName}.{outputTypeName};");
        }

        processFile.AppendLine();
        processFile.AppendLine($"namespace {names.ProcessProject.NamespaceName};");
        processFile.AppendLine();
        processFile.AppendLine("static class Handler");
        processFile.AppendLine("{");
        processFile.AppendLine($"{padding}public static GenericResponse<Output> Execute(ObjectHelper objectHelper, Input input)");
        processFile.AppendLine($"{padding}{{");
        processFile.AppendLine($"{padding}{padding}var returnObject = objectHelper.InitializeResponse<Output>();");
        processFile.AppendLine();

        var constructorParameters = new List<string>();

        var constructorMethods = targetType.Methods.Where(m => m.IsConstructor && !m.IsStatic).ToList();
        if (constructorMethods.Count == 1 && constructorMethods[0].Parameters.Count == 1 && constructorMethods[0].Parameters[0].ParameterType.Name == "ExecutionDataContext")
        {
            constructorParameters.Add("objectHelper.Context");
        }

        processFile.AppendLine($"{padding}{padding}var bo = new {targetType.FullName.RemoveFromStart("BOA.Process.")}({string.Join(", ", constructorParameters)});");

        var targetMethodParameters = targetMethod.Parameters.Where(p => !CanIgnoreParameterType(serviceContext, p.ParameterType)).ToList();

        if (targetMethodParameters.Count == 1 &&
            !IsDotNetCoreType(targetMethodParameters[0].ParameterType.FullName))
        {
            processFile.AppendLine();
            processFile.AppendLine($"{padding}{padding}var parameter = ConvertTo<{targetMethodParameters[0].ParameterType.FullName}>(input);");

            var parameterPart = new List<string>();

            foreach (var parameterDefinition in targetMethod.Parameters)
            {
                if (parameterDefinition.ParameterType.Name == "ObjectHelper")
                {
                    parameterPart.Add("objectHelper");
                    continue;
                }

                parameterPart.Add("parameter");
            }

            processFile.AppendLine();
            processFile.AppendLine($"{padding}{padding}var response = bo.{targetMethod.Name}({string.Join(", ", parameterPart)});");
            processFile.AppendLine($"{padding}{padding}if (!response.Success)");
            processFile.AppendLine($"{padding}{padding}{{");
            processFile.AppendLine($"{padding}{padding}{padding}returnObject.Results.AddRange(response.Results);");
            processFile.AppendLine($"{padding}{padding}{padding}return returnObject;");
            processFile.AppendLine($"{padding}{padding}}}");
            processFile.AppendLine();
            processFile.AppendLine($"{padding}{padding}var value = response.Value;");
            processFile.AppendLine();
            if (outputTypeIsAlreadyExistingType)
            {
                processFile.AppendLine($"{padding}{padding}returnObject.Value = value;");
            }
            else
            {
                processFile.AppendLine($"{padding}{padding}returnObject.Value = ConvertTo<Output>(value);");
            }

            processFile.AppendLine();
            processFile.AppendLine($"{padding}{padding}return returnObject;");
            processFile.AppendLine($"{padding}}}"); // end of method
            processFile.AppendLine("}"); // end of class
        }
        else
        {
            {
                contractFile.AppendLine($"public sealed class {targetMethod.Name}Input : IBankingProxyInput<{outputTypeName}>");
                contractFile.AppendLine("{");
                foreach (var parameterDefinition in targetMethodParameters)
                {
                    var parameterTypeName = parameterDefinition.ParameterType.GetShortNameInCsharp();

                    var name = parameterDefinition.Name;

                    name = char.ToUpper(name[0], new("en-US")) + new string(name.Skip(1).ToArray());

                    contractFile.AppendLine($"    public {parameterTypeName} {name} {{ get; set; }}");
                }

                contractFile.AppendLine("}");
            }

            {
                var parameterPart = new List<string>();

                foreach (var parameterDefinition in targetMethod.Parameters)
                {
                    if (parameterDefinition.ParameterType.Name == "ObjectHelper")
                    {
                        parameterPart.Add("objectHelper");
                        continue;
                    }

                    var name = parameterDefinition.Name;

                    name = char.ToUpper(name[0], new("en-US")) + new string(name.Skip(1).ToArray());

                    parameterPart.Add($"input.{name}");
                }

                processFile.AppendLine();
                processFile.AppendLine($"{padding}{padding}var response = bo.{targetMethod.Name}({string.Join(", ", parameterPart)});");
                processFile.AppendLine($"{padding}{padding}if (!response.Success)");
                processFile.AppendLine($"{padding}{padding}{{");
                processFile.AppendLine($"{padding}{padding}{padding}returnObject.Results.AddRange(response.Results);");
                processFile.AppendLine($"{padding}{padding}{padding}return returnObject;");
                processFile.AppendLine($"{padding}{padding}}}");
                processFile.AppendLine();
                processFile.AppendLine($"{padding}{padding}var value = response.Value;");
                processFile.AppendLine();
                if (outputTypeIsAlreadyExistingType)
                {
                    processFile.AppendLine($"{padding}{padding}returnObject.Value = value;");
                }
                else
                {
                    processFile.AppendLine($"{padding}{padding}returnObject.Value = ConvertTo<Output>(value);");
                }

                processFile.AppendLine();
                processFile.AppendLine($"{padding}{padding}return returnObject;");
                processFile.AppendLine($"{padding}}}"); // end of method
                processFile.AppendLine("}"); // end of class
            }
        }

        return new()
        {
            ContractFile = new()
            {
                Name    = Path.Combine(names.ContractsProject.FolderName, names.ContractsProject.FileName),
                Content = contractFile.ToString().Trim()
            },
            ProcessFile = new()
            {
                Name    = Path.Combine(names.ProcessProject.FolderName, names.ProcessProject.FileName),
                Content = processFile.ToString().Trim()
            }
        };
    }

    public static IEnumerable<MethodDefinition> GetCalledMethodsFromExternalDomain(ServiceContext serviceContext, string assemblyFileNameInExternalDomain)
    {
        var config = serviceContext.Config;

        var domainAssemblies = GetDomainAssemblies(serviceContext);

        foreach (var analyse in domainAssemblies)
        {
            foreach (var methodReference in analyse.CalledMethods)
            {
                if (config.SkipTypes.Contains(methodReference.DeclaringType.FullName))
                {
                    continue;
                }

                var isPropertyGetOrSetMethod = methodReference.Name.StartsWith("set_") || methodReference.Name.StartsWith("get_");
                if (isPropertyGetOrSetMethod)
                {
                    continue;
                }

                if (!IsMethodBelongToExternalDomain(serviceContext, methodReference))
                {
                    continue;
                }

                var targetMethodResult = Try(methodReference.Resolve);
                if (targetMethodResult.Success)
                {
                    var targetMethod = targetMethodResult.Value;
                    if (targetMethod is null || targetMethod.IsConstructor)
                    {
                        continue;
                    }

                    if (targetMethod.DeclaringType.Scope.Name == assemblyFileNameInExternalDomain)
                    {
                        yield return targetMethod;
                    }
                }
            }
        }
    }

    public static bool IsInDomain(ServiceContext serviceContext, string file)
    {
        var config = serviceContext.Config;

        foreach (var name in config.DomainFiles)
        {
            if (file.Contains(name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    static ((string FolderName, string FileName, string NamespaceName) ContractsProject,
        (string FolderName, string FileName, string NamespaceName) ProcessProject)
        CalculateNames(string targetTypeFullName, string methodName)
    {
        var names = targetTypeFullName.Split('.').ToImmutableArray();

        var removeList = new[]
        {
            "BOA",
            "Process",
            "Business",
            "Kernel"
        };

        names = names.Where(x => !removeList.Contains(x)).ToImmutableArray();

        return (
            ContractsProject:
            (
                FolderName: string.Join(".", names.Take(names.Length - 1)),
                FileName: string.Join(".", names[^1], methodName),
                NamespaceName: "BOA.Card.Contracts.Banking." + string.Join(".", names.Add(methodName))
            ),
            ProcessProject:
            (
                FolderName: string.Join(".", names.Take(names.Length - 1)),
                FileName: string.Join(".", names[^1], methodName),
                NamespaceName: "BOA.Process.Card.Banking." + string.Join(".", names.Add(methodName))
            )
        );
    }

    static bool CanIgnoreParameterType(ServiceContext serviceContext, TypeReference parameterTypeReference)
    {
        return serviceContext.Config.IgnoreParameterTypeNamesLike.Contains(parameterTypeReference.Name);
    }

    static IReadOnlyList<AssemblyAnalyse> GetDomainAssemblies(ServiceContext serviceContext)
    {
        var config = serviceContext.Config;

        return Cache.AccessValue(nameof(GetDomainAssemblies), () =>
        {
            var directory = config.AssemblySearchDirectory;

            var files = Directory.GetFiles(directory, "*.dll").Where(x => IsInDomain(serviceContext, x));

            return files
                  .Select(ReadAssemblyDefinition)
                  .Where(r => r.Success)
                  .Select(x => x.Value)
                  .Select(x => AnalyzeAssembly(serviceContext, x))
                  .ToList();
        });
    }

    public static bool IsDotNetCoreType(string fullTypeName)
    {
        var coreTypes = new[]
        {
            "System.String",
            "System.Byte",
            "System.Int16",
            "System.Double",
            "System.Int32",
            "System.Int64",
            "System.Decimal",
            "System.DateTime",
            "System.Boolean",

            "System.Nullable`1<System.Byte>",
            "System.Nullable`1<System.Int16>",
            "System.Nullable`1<System.Double>",
            "System.Nullable`1<System.Int32>",
            "System.Nullable`1<System.Int64>",
            "System.Nullable`1<System.Decimal>",
            "System.Nullable`1<System.DateTime>",
            "System.Nullable`1<System.Boolean>"
        };

        return coreTypes.Contains(fullTypeName);
    }

    internal sealed record AnalyzeMethodInput
    {
        public required string AssemblyFileName { get; init; }

        public required string TypeFullName { get; init; }

        public required string MethodFullName { get; init; }
    }
}