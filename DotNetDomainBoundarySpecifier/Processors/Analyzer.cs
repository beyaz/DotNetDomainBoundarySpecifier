namespace DotNetDomainBoundarySpecifier.Processors;

static class Analyzer
{
    static readonly CachedObjectMap Cache = new()
    {
        Timeout = TimeSpan.FromDays(3)
    };

    public static ImmutableList<TableModel> AnalyzeMethod(Scope scope, AnalyzeMethodInput input)
    {
        var records = ImmutableList<TableModel>.Empty;

        var methodDefinition =
            scope.GetTypesInAssemblyFile(input.AssemblyFileName)
                          .FirstOrDefault(t => t.FullName == input.TypeFullName)
                         ?.Methods.FirstOrDefault(m => m.FullName == input.MethodFullName);

        if (methodDefinition is null)
        {
            return records;
        }

        foreach (var parameterDefinition in methodDefinition.Parameters)
        {
            records = pushType(scope, input, methodDefinition, records, parameterDefinition.ParameterType);
        }

        records = pushType(scope, input, methodDefinition, records, methodDefinition.ReturnType);

        return records;

        static ImmutableList<TableModel> pushType(Scope scope, AnalyzeMethodInput input, MethodDefinition methodDefinition, ImmutableList<TableModel> records, TypeReference typeReference)
        {
            var config = scope.Config;

            if (IsDotNetCoreType(typeReference.FullName))
            {
                return records;
            }

            typeReference = GetValueTypeIfTypeIsMonadType(typeReference);

            var typeDefinition = typeReference.Resolve();

            var usedProperties = GetDomainAssemblies(scope).FindUsedProperties(typeDefinition);
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

                records = pushType(scope, input, methodDefinition, records, propertyDefinition.PropertyType);
            }

            return records;
        }
    }

    public static CodeGenerationOutput GenerateCode(Scope scope, AnalyzeMethodInput input, ImmutableList<TableModel> records)
    {
        const string padding = "    ";

        var targetMethod =
            scope.GetTypesInAssemblyFile(input.AssemblyFileName)
                          .FirstOrDefault(t => t.FullName == input.TypeFullName)
                         ?.Methods.FirstOrDefault(m => m.FullName == input.MethodFullName);

        if (targetMethod is null)
        {
            return default;
        }

        var targetType = targetMethod.DeclaringType;

        var names = CalculateNames(scope, targetType.FullName, targetMethod.Name);

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

        var targetMethodParameters = targetMethod.Parameters.Where(p => !CanIgnoreParameterType(scope, p.ParameterType)).ToList();

        
        var contracts = new List<(IReadOnlyList<string> lines, bool isInputType)>();
        
        // Input Output types
        static Option<IReadOnlyList<string>> tryCreateInputTypeLines(Scope scope, MethodDefinition targetMethod, string outputTypeName)
        {
            var parameters = targetMethod.Parameters.Where(p => !CanIgnoreParameterType(scope, p.ParameterType)).ToList();
            
            var isInputType = parameters.Count == 1 &&  IsDotNetCoreType(parameters[0].ParameterType.FullName)||
                              parameters.Count > 1;
            
            if (isInputType)
            {
                var lines = new List<string>
                {
                    $"public sealed class {targetMethod.Name}Input : IBankingProxyInput<{outputTypeName}>",
                    "{"
                };

                foreach (var parameterDefinition in parameters)
                {
                    var parameterTypeName = parameterDefinition.ParameterType.GetShortNameInCsharp();

                    var name = parameterDefinition.Name;

                    name = char.ToUpper(name[0], new("en-US")) + new string(name.Skip(1).ToArray());

                    lines.Add($"    public {parameterTypeName} {name} {{ get; set; }}");
                }
                lines.Add("}");
                
                return lines;
            }
            
            return None;
        }

        tryCreateInputTypeLines(scope, targetMethod, outputTypeName).Then(lines => contracts.Add((lines, true)));
        
        foreach (var tableModel in records.DistinctBy(x=>x.RelatedClassFullName))
        {
            var lines = new List<string>();
            
            var typeDefinition = scope.GetTypesInAssemblyFile(input.AssemblyFileName).First(t => t.FullName == tableModel.RelatedClassFullName);

            var parameters = targetMethod.Parameters.Where(p => !CanIgnoreParameterType(scope, p.ParameterType)).ToList();
            
            var isInputType = parameters.Count == 1 && parameters[0].ParameterType.FullName == typeDefinition.FullName;
            
            if (isInputType)
            {
                lines.Add($"public sealed class {targetMethod.Name}Input : IBankingProxyInput<{outputTypeName}>");    
            }
            else
            {
                lines.Add($"public sealed class {typeDefinition.Name}");
            }
                    
            lines.Add("{");
                
            foreach (var record in records.Where(x=>x.RelatedClassFullName == tableModel.RelatedClassFullName))
            {
                var propertyDefinition = typeDefinition.Properties.First(p=>p.FullName == record.RelatedPropertyFullName);

                lines.Add($"    public {propertyDefinition.PropertyType.GetShortNameInCsharp()} {propertyDefinition.Name} {{ get; set; }}");
            }
                
            lines.Add("}");
            
            contracts.Add((lines,isInputType));
        }
        
        foreach (var (lines, _) in contracts.OrderByDescending(x=>x.isInputType?1:0))
        {
            contractFile.AppendLine();
            
            foreach (var line in lines)
            {
                contractFile.AppendLine(line);
            }
        }
        
        
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

    public static IEnumerable<MethodDefinition> GetCalledMethodsFromExternalDomain(Scope scope, string assemblyFileNameInExternalDomain)
    {
        var config = scope.Config;

        var domainAssemblies = GetDomainAssemblies(scope);

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

                if (!IsMethodBelongToExternalDomain(scope, methodReference))
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

    public static TypeReference GetValueTypeIfTypeIsMonadType(TypeReference typeReference)
    {
        if (IsMonadType(typeReference))
        {
            return GetMonadValueType(typeReference);
        }

        return typeReference;
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

    public static bool IsInDomain(Scope scope, string file)
    {
        var config = scope.Config;

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
        CalculateNames(Scope scope, string targetTypeFullName, string methodName)
    {
        var config = scope.Config;
        
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
                NamespaceName: config.InputOutputsNamespacePrefix+ "." + string.Join(".", names.Add(methodName))
            ),
            ProcessProject:
            (
                FolderName: string.Join(".", names.Take(names.Length - 1)),
                FileName: string.Join(".", names[^1], methodName),
                NamespaceName: config.ProcessNamespacePrefix+ "." + string.Join(".", names.Add(methodName))
            )
        );
    }

    static bool CanIgnoreParameterType(Scope scope, TypeReference parameterTypeReference)
    {
        return scope.Config.IgnoreParameterTypeNamesLike.Contains(parameterTypeReference.Name);
    }

    static IReadOnlyList<AssemblyAnalyse> GetDomainAssemblies(Scope scope)
    {
        var config = scope.Config;

        return Cache.AccessValue(nameof(GetDomainAssemblies), () =>
        {
            var directory = config.AssemblySearchDirectory;

            var files = Directory.GetFiles(directory, "*.dll").Where(x => IsInDomain(scope, x));

            return files
                  .Select(ReadAssemblyDefinition)
                  .Where(r => r.Success)
                  .Select(x => x.Value)
                  .Select(x => AnalyzeAssembly(scope, x))
                  .ToList();
        });
    }

    static TypeReference GetMonadValueType(TypeReference typeReference)
    {
        var genericInstanceType = (GenericInstanceType)typeReference;

        return genericInstanceType.GenericArguments[0];
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

    internal sealed record AnalyzeMethodInput
    {
        public required string AssemblyFileName { get; init; }

        public required string TypeFullName { get; init; }

        public required string MethodFullName { get; init; }
    }
}