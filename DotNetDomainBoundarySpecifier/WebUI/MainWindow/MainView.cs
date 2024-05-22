namespace DotNetDomainBoundarySpecifier.WebUI.MainWindow;

class MainView : Component<MainViewModel>
{
    protected override Task constructor()
    {
        state = StateFile.TryReadState() ?? new();

        return base.constructor();
    }

    protected override Element render()
    {
        StateFile.SaveCurrentState(state);

        return new FlexRow(Padding(10), SizeFull, Theme.BackgroundForBrowser)
        {
            new FlexColumn
            {
                applicationTopPanel,
                createContent() + Padding(16) + OverflowAuto,

                new Style
                {
                    Theme.Border,
                    SizeFull,
                    Theme.BackgroundForWindow,
                    BorderRadius(10),
                    Theme.BoxShadowForWindow
                }
            }
        };

        Element applicationTopPanel()
        {
            return new FlexRow
            {
                new FlexRow(AlignItemsCenter, Gap(5))
                {
                    new h3 { "DotNet Domain Boundary Specifier" }
                },

                new LogoutButton(),

                new Style
                {
                    JustifyContentSpaceBetween,
                    AlignItemsCenter,
                    BorderBottom(Solid(1, Theme.BorderColor)),
                    Padding(8, 30)
                }
            };
        }

        Element createContent()
        {
            return new SplitColumn
            {
                new div
                {
                    new SplitRow
                    {
                        new AssemblySelector
                        {
                            SelectedAssemblyFileName = state.SelectedAssemblyFileName,
                            SelectionChange          = OnSelectedAssemblyChanged_Start
                        },
                        new TypeSelector
                        {
                            SelectedAssemblyFileName = state.SelectedAssemblyFileName,
                            SelectedTypeFullName     = state.SelectedTypeFullName,
                            SelectionChange          = OnSelectedTypeChanged
                        },
                        new MethodSelector
                        {
                            SelectedAssemblyFileName = state.SelectedAssemblyFileName,
                            SelectedTypeFullName     = state.SelectedTypeFullName,
                            SelectedMethodFullName   = state.SelectedMethodFullName,
                            SelectionChange          = OnSelectedMethodChanged
                        },
                        new FlexColumn(Gap(16))
                        {
                            new FlexRow(Gap(4))
                            {
                                new ActionButton
                                {
                                    Label        = "Analyze",
                                    OnClicked    = OnAnalyzeClicked,
                                    IsProcessing = state.IsAnalyzing
                                },

                                new ActionButton
                                {
                                    Label = "Save"
                                },

                                new ActionButton
                                {
                                    Label = "Export cs files to project"
                                }
                            },

                            new FlexColumn(WordBreakAll)
                            {
                                new FlexRow(Gap(4), PaddingTopBottom(4))
                                {
                                    new input
                                    {
                                        type      = "checkbox",
                                        valueBind = () => state.HasTransaction
                                    },
                                    "Has Transaction"
                                },

                                new p
                                {
                                    (b)"Assembly: ", state.SelectedAssemblyFileName,
                                    br,
                                    (b)"Class: ", state.SelectedTypeFullName,
                                    br,
                                    (b)"Method: ", state.SelectedMethodFullName
                                }
                            }
                        }
                    }
                },

                new div(SizeFull)
                {
                    CreatePropertySelectors(state.Records)
                }
            };
        }
    }

    Element CreatePropertySelectors(ImmutableList<TableModel> records)
    {
        if (records is null || records.Count == 0)
        {
            return null;
        }

        records = records.Where(x => x.ExternalAssemblyFileName == state.SelectedAssemblyFileName &&
                                     x.ExternalClassFullName == state.SelectedTypeFullName &&
                                     x.ExternalMethodFullName == state.SelectedMethodFullName
                               ).ToImmutableList();

        if (records.Count == 0)
        {
            return null;
        }

        var serviceContext = new ServiceContext();

        var methodDefinition = serviceContext
                              .GetTypesInAssemblyFile(state.SelectedAssemblyFileName)
                              .FirstOrDefault(t => t.FullName == state.SelectedTypeFullName)?
                              .Methods.FirstOrDefault(m => m.FullName == state.SelectedMethodFullName);

        if (methodDefinition is null)
        {
            return null;
        }

        var elements = new List<Element>();

        foreach (var relatedTableFullName in records.Select(x => x.RelatedClassFullName).Distinct())
        {
            var typeDefinition = serviceContext.GetTypesInAssemblyFile(state.SelectedAssemblyFileName).FirstOrDefault(x => x.FullName == relatedTableFullName);
            if (typeDefinition is null)
            {
                continue;
            }

            var itemsSource = typeDefinition.Properties.Select(p => p.Name).ToList();

            var selectedProperties = records.Where(x => x.RelatedClassFullName == relatedTableFullName).Select(x => x.RelatedPropertyFullName).ToList();

            elements.Add(new ListView<string>
            {
                Title         = typeDefinition.Name,
                ItemsSource   = itemsSource,
                SelectedItems = typeDefinition.Properties.Where(p => selectedProperties.Contains(p.FullName)).Select(p => p.Name).ToList()
            });
        }

        var returnType = methodDefinition.ReturnType;

        returnType = GetValueTypeIfTypeIsMonadType(returnType);

        if (!IsDotNetCoreType(returnType.FullName))
        {
            var typeDefinition = Try(returnType.Resolve).Value;
            if (typeDefinition is not null)
            {
                var itemsSource = typeDefinition.Properties.Select(p => p.Name).ToList();

                var selectedProperties = records.Where(x => x.RelatedClassFullName == returnType.FullName).Select(x => x.RelatedPropertyFullName).ToList();

                elements.Add(new ListView<string>
                {
                    Title         = typeDefinition.Name,
                    ItemsSource   = itemsSource,
                    SelectedItems = typeDefinition.Properties.Where(p => selectedProperties.Contains(p.FullName)).Select(p => p.Name).ToList()
                });
            }
        }

        elements.Add(new FlexRowCentered(SizeFull)
        {
            new CSharpText
            {
                Value = state.GeneratedCode
            }
        });

        return new SplitRow
        {
            elements
        };
    }

    Task DoAnalyze()
    {
        state = state with { IsAnalyzing = false };

        var analyzeMethodInput = new AnalyzeMethodInput
        {
            AssemblyFileName = state.SelectedAssemblyFileName,
            TypeFullName     = state.SelectedTypeFullName,
            MethodFullName   = state.SelectedMethodFullName
        };

        state = state with { Records = AnalyzeMethod(new(), analyzeMethodInput) };

        var generatedCode = GenerateCode(new(), analyzeMethodInput, state.Records);

        state = state with
        {
            GeneratedCode = "<< T Y P E S >>" + Environment.NewLine +
                            generatedCode.ContractFile.Content
                            + Environment.NewLine
                            + "<< P R O C E S S >>"
                            + Environment.NewLine +
                            generatedCode.ProcessFile.Content
        };

        return Task.CompletedTask;
    }

    Task OnAnalyzeClicked()
    {
        state = state with { IsAnalyzing = true };

        Client.GotoMethod(DoAnalyze);

        return Task.CompletedTask;
    }

    Task OnSelectedAssemblyChanged_Finish(string assemblyFilename)
    {
        state = state with
        {
            SelectedAssemblyFileName = assemblyFilename,
        };

        return Task.CompletedTask;
    }

    Task OnSelectedAssemblyChanged_Start(string assemblyFilename)
    {
        IsInSkeletonMode[Context] = true;

        Client.GotoMethod(OnSelectedAssemblyChanged_Finish, assemblyFilename);

        return Task.CompletedTask;
    }

    Task OnSelectedMethodChanged(string methodFullName)
    {
        state = state with { SelectedMethodFullName = methodFullName };

        return Task.CompletedTask;
    }

    Task OnSelectedTypeChanged(string typeFullName)
    {
        state = state with { SelectedTypeFullName = typeFullName };

        return Task.CompletedTask;
    }

    static class StateFile
    {
        static string StateFilePath => Path.GetTempPath()
                                       + nameof(DotNetDomainBoundarySpecifier)
                                       + ".json";

        public static void SaveCurrentState(MainViewModel state)
            => File.WriteAllText(StateFilePath, Json.Serialize(state), Encoding.UTF8);

        public static MainViewModel TryReadState()
            => Try(() => Json.Deserialize<MainViewModel>(File.ReadAllText(StateFilePath))).Value;
    }
}