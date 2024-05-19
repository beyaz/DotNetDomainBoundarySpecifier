using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using ApiInspector.WebUI;

namespace DotNetDependencyExtractor;

static class Extractor
{
    public static Result<List<GenerateDependentCodeInput>> ExportUsedMethodsInCardSystem(string fileName)
    {
        var cardSystemSearchFiles = ReadAllCardSystemSearchFiles();

        var usageInfo = FindUsedMethodsInCardSystem(fileName, cardSystemSearchFiles);
        if (usageInfo.HasError)
        {
            return usageInfo.Error;
        }

        foreach (var generateDependentCodeInput in usageInfo.Value)
        {
            var output = GenerateDependentCode(generateDependentCodeInput with
            {
                CardSystemSearchFiles = cardSystemSearchFiles
            });
            if (output.HasError)
            {
                return output.Error;
            }

            const string solutionDirectory = @"D:\work\BOA.CardModules\Dev\BOA.Card.Banking\";

            {
                var fileModel = output.Value.ContractFile;

                const string projectDirectory = $@"{solutionDirectory}BOA.Card.Contracts.Banking\";

                WriteCSharpFile($"{projectDirectory}{fileModel.Name}.cs", fileModel.Content);
            }

            {
                var fileModel = output.Value.ProcessFile;

                const string projectDirectory = $@"{solutionDirectory}BOA.Process.Card.Banking\";

                WriteCSharpFile($"{projectDirectory}{fileModel.Name}.cs", fileModel.Content);
            }
        }

        return usageInfo;
    }

    public static Result<List<GenerateDependentCodeInput>>
        FindUsedMethodsInCardSystem(string fileName, IReadOnlyList<AssemblyAnalyse> cardSystemSearchFiles)
    {
        cardSystemSearchFiles ??= ReadAllCardSystemSearchFiles();

        var result = ResultFrom(new List<GenerateDependentCodeInput>());

        var records = result.Value;

        fileName = Path.GetFileName(fileName);

        var directory = Config.AssemblySearchDirectory;

        var targetAssemblyDefinition = ReadAssemblyDefinition(Path.Combine(directory, fileName));
        if (targetAssemblyDefinition.HasError)
        {
            return targetAssemblyDefinition.Error;
        }

        foreach (var analyse in cardSystemSearchFiles)
        {
            foreach (var methodReference in analyse.CalledMethods.Where(isMethodBelongToTargetAssembly))
            {
                var targetMethod = methodReference.Resolve();

                if (IsNotAllowedType(methodReference.DeclaringType))
                {
                    continue;
                }

                var targetType = methodReference.DeclaringType.Resolve();

                var newRecord = new GenerateDependentCodeInput
                {
                    FromAssembly = analyse.AssemblyDefinition.Name.Name+".dll",
                    FromAssemblyDefinition   = analyse.AssemblyDefinition,
                    
                    TargetAssembly           = targetAssemblyDefinition.Value.Name.Name+".dll",
                    TargetAssemblyDefinition = targetAssemblyDefinition.Value,
                    TargetMethod             = targetMethod,
                    TargetType               = targetType,

                    TargetTypeFullName = targetType.FullName,
                    TargetMethodName   = targetMethod.Name
                };

                if (records.Any(x => IsSame(x, newRecord)))
                {
                    continue;
                }

                records.Add(newRecord);
            }
        }

        return result;

        bool isMethodBelongToTargetAssembly(MethodReference methodReference)
        {
            return methodReference.DeclaringType.Scope.Name == targetAssemblyDefinition.Value.Name.Name;
        }

        static bool IsSame(GenerateDependentCodeInput x, GenerateDependentCodeInput y)
        {
            return x.TargetType.FullName == y.TargetType.FullName &&
                   x.TargetMethod.FullName == y.TargetMethod.FullName;
        }
    }

    static TypeExportContext AddIfNotExists(TypeExportContext typeExportContext, TypeExportInfo exportInfo)
    {
        if (typeExportContext.ExportList.Any(x => x.TypeDefinition == exportInfo.TypeDefinition))
        {
            return typeExportContext;
        }

        return typeExportContext with
        {
            ExportList = typeExportContext.ExportList.Add(exportInfo)
        };
    }

    sealed record ExportTypeToCsharpInput
    {
        public TypeDefinition TypeDefinition { get; init; }
        public IReadOnlyList<PropertyDefinition> UsedProperties { get; init; }
        public string ClassName { get; init; }
        public bool OutputTypeIsAlreadyExistingType { get; init; }
        public string OutputTypeName { get; init; }
        public MethodDefinition TargetMethod { get; init; }
    }
    static string ExportTypeToCsharp(ExportTypeToCsharpInput input)
    {
        var sb = new StringBuilder();

        if (input.TypeDefinition.IsEnum)
        {
            sb.AppendLine($"public enum {input.TypeDefinition.Name}");
            sb.AppendLine("{");
            foreach (var fieldDefinition in input.TypeDefinition.Fields)
            {
                if (fieldDefinition.Name == "value__")
                {
                    continue;
                }

                sb.AppendLine($"    {fieldDefinition.Name},");
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        if (input.ClassName == "Input")
        {
            sb.AppendLine($"public sealed class {input.TargetMethod.Name}Input : IBankingProxyInput<{input.OutputTypeName}>");
        }
        else
        {
            sb.AppendLine($"public sealed class {input.ClassName ?? input.TypeDefinition.Name}");
        }
        
        sb.AppendLine("{");
        foreach (var propertyDefinition in input.UsedProperties.OrderBy(p => p.Name))
        {
            sb.AppendLine($"    public {propertyDefinition.PropertyType.GetShortNameInCsharp()} {propertyDefinition.Name} {{ get; set; }}");
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    internal static ImmutableList<TableModel> AnalyzeMethod(MainWindowModel mainWindowModel)
    {
        var returnList = ImmutableList<TableModel>.Empty;

        returnList = returnList.Add(new()
        {
            ModuleName = "deenme"
        });
        
        
        return returnList;
    }
    static Result<GenerateDependentCodeOutput> GenerateDependentCode(GenerateDependentCodeInput input)
    {
        const string padding = "    ";

        var directory = Config.AssemblySearchDirectory;

        if (input.FromAssemblyDefinition is null)
        {
            var sourceAssemblyDefinitionResult = ReadAssemblyDefinition(Path.Combine(directory, input.FromAssembly));
            if (sourceAssemblyDefinitionResult.HasError)
            {
                return sourceAssemblyDefinitionResult.Error;
            }

            input = input with
            {
                FromAssemblyDefinition = sourceAssemblyDefinitionResult.Value
            };
        }

        if (input.TargetAssemblyDefinition is null)
        {
            var targetAssemblyDefinitionResult = ReadAssemblyDefinition(Path.Combine(directory, input.TargetAssembly));
            if (targetAssemblyDefinitionResult.HasError)
            {
                return targetAssemblyDefinitionResult.Error;
            }

            input = input with
            {
                TargetAssemblyDefinition = targetAssemblyDefinitionResult.Value
            };
        }

        var targetAssemblyAnalyse = AnalyzeAssembly(input.TargetAssemblyDefinition);

        if (input.TargetType is null)
        {
            input = input with
            {
                TargetType = targetAssemblyAnalyse.Types.FirstOrDefault(x => x.FullName == input.TargetTypeFullName)
            };
            if (input.TargetType is null)
            {
                return Fail($"{input.TargetTypeFullName} not found in {input.TargetAssembly}");
            }
        }

        var targetType = input.TargetType;

        if (input.TargetMethod is null)
        {
            input = input with
            {
                TargetMethod = targetType.Methods.FirstOrDefault(x => x.Name == input.TargetMethodName)
            };
            if (input.TargetMethod is null)
            {
                return Fail($"{input.TargetMethodName} not found in {input.TargetTypeFullName}");
            }
        }

        var targetMethod = input.TargetMethod;

        var names = CalculateNames(targetType.FullName, targetMethod.Name);

        var contractFile = new StringBuilder();
        var processFile = new StringBuilder();


        TypeReference outputTypeAsAlreadyExistingType = null;
        var outputTypeIsAlreadyExistingType = false;

        if (targetMethod.ReturnType is GenericInstanceType genericInstanceType)
        {
            if (IsDotNetCoreType(genericInstanceType.GenericArguments[0].FullName))
            {
                outputTypeAsAlreadyExistingType = genericInstanceType.GenericArguments[0];

                outputTypeIsAlreadyExistingType = true;
            }
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
            processFile.AppendLine($"using Output = {(outputTypeName == "DateTime" ? "System.DateTime" :outputTypeName) };");
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
        
        var constructorMethods = targetType.Methods.Where(m=>m.IsConstructor && !m.IsStatic).ToList();
        if (constructorMethods.Count == 1 && constructorMethods[0].Parameters.Count == 1 && constructorMethods[0].Parameters[0].ParameterType.Name == "ExecutionDataContext")
        {
            constructorParameters.Add("objectHelper.Context");
        }
        
        processFile.AppendLine($"{padding}{padding}var bo = new {targetType.FullName.RemoveFromStart("BOA.Process.")}({string.Join(", ", constructorParameters)});");

        var exportContext = new TypeExportContext
        {
            ExportList            = ImmutableList<TypeExportInfo>.Empty,
            CardSystemSearchFiles = input.CardSystemSearchFiles ?? ReadAllCardSystemSearchFiles()
        };

        var targetMethodParameters = targetMethod.Parameters.Where(p => p.ParameterType.Name != "ObjectHelper").ToList();

        if (targetMethodParameters.Count == 1 &&
            !IsDotNetCoreType(targetMethodParameters[0].ParameterType.FullName))
        {
            exportContext = PushType(exportContext, targetMethod.Parameters[0].ParameterType, "Input");

            
            
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
                
                parameterPart.Add($"parameter");
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
                    exportContext = PushType(exportContext, parameterDefinition.ParameterType);

                    var parameterTypeName = parameterDefinition.ParameterType.GetShortNameInCsharp();

                    var name = parameterDefinition.Name;

                    name = char.ToUpper(name[0], new CultureInfo("en-US")) + new string(name.Skip(1).ToArray());

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

                    name = char.ToUpper(name[0], new CultureInfo("en-US")) + new string(name.Skip(1).ToArray());

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

       

        if (outputTypeIsAlreadyExistingType is false)
        {
            exportContext = PushType(exportContext, targetMethod.ReturnType, "Output");
        }

        foreach (var exportInfo in exportContext.ExportList)
        {
            contractFile.AppendLine();

            contractFile.AppendLine(ExportTypeToCsharp(new ()
            {
                TypeDefinition                  = exportInfo.TypeDefinition, 
                UsedProperties                  = exportInfo.UsedProperties, 
                ClassName                       = exportInfo.ClassName,
                TargetMethod = targetMethod,
                OutputTypeIsAlreadyExistingType = outputTypeIsAlreadyExistingType,
                OutputTypeName                  = outputTypeName
            }));
        }

        return new GenerateDependentCodeOutput
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

        static bool IsDotNetCoreType(string fullTypeName)
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

        static TypeExportContext PushType(TypeExportContext exportContext, TypeReference typeReference, string className = null)
        {
            if (IsDotNetCoreType(typeReference.FullName))
            {
                return exportContext;
            }

            if (typeReference is GenericInstanceType genericInstanceType)
            {
                if (genericInstanceType.GenericArguments.Count == 1)
                {
                    var genericArgument = genericInstanceType.GenericArguments[0];

                    exportContext = PushType(exportContext, genericArgument, className == "Output" ? "Output" : null);

                    return exportContext;
                }
            }

            var typeDefinition = typeReference.Resolve();

            if (typeDefinition.IsEnum)
            {
                exportContext = AddIfNotExists(exportContext, new()
                {
                    TypeDefinition = typeDefinition,
                    ExportAsEnum   = true
                });

                return exportContext;
            }

            var usedProperties = exportContext.CardSystemSearchFiles.FindUsedProperties(typeDefinition);
            if (usedProperties.Count is 0)
            {
            }

            var newRecord = new TypeExportInfo
            {
                TypeDefinition = typeDefinition,
                ClassName      = className,
                UsedProperties = usedProperties
            };

            exportContext = AddIfNotExists(exportContext, newRecord);

            foreach (var propertyDefinition in usedProperties)
            {
                var propertyType = propertyDefinition.PropertyType;

                exportContext = PushType(exportContext, propertyType);
            }

            return exportContext;
        }
    }

    static bool IsNotAllowedType(TypeReference typeReference)
    {
        if (typeReference.FullName == "BOA.Business.Kernel.General.FTP")
        {
            return true;
        }

        return false;
    }

    static IReadOnlyList<AssemblyAnalyse> ReadAllCardSystemSearchFiles()
    {
        return cardOrchestrationFiles()
              .Select(ReadAssemblyDefinition)
              .Where(r => r.Success)
              .Select(x => x.Value)
              .Select(AnalyzeAssembly)
              .ToList();

        static IEnumerable<string> cardOrchestrationFiles()
        {
            var directory = Config.AssemblySearchDirectory;

            return Directory.GetFiles(directory, "*.dll").Where(isInDomain);

            
        }
    }
    
    public static bool isInDomain(string file)
    {
        foreach (var name in Config.DomainFiles)
        {
            if (file.Contains(name))
            {
                return true;
            }
        }

        return false;
    }
}