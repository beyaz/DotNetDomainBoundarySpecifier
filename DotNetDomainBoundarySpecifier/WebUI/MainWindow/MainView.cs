namespace DotNetDomainBoundarySpecifier.WebUI.MainWindow;

sealed class MainView : Component<MainViewModel>
{
    protected override Task constructor()
    {
        state = StateFile.TryReadState() ?? new();

        state = state with
        {
            IsAnalyzing = false  ,
            IsSaving = false,
            IsExporting = false
        };

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
            },
            
            new NotificationHost()
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
                            SelectedTypeFullName     = state.SelectedClassFullName,
                            SelectionChange          = OnSelectedTypeChanged
                        },
                        new MethodSelector
                        {
                            SelectedAssemblyFileName = state.SelectedAssemblyFileName,
                            SelectedTypeFullName     = state.SelectedClassFullName,
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
                                    IsProcessing = state.IsAnalyzing,
                                    IsDisabled = state.IsAnalyzeButtonDisabled
                                },

                                new ActionButton
                                {
                                    Label        = "Save",
                                    OnClicked    = OnSaveClicked,
                                    IsProcessing = state.IsSaving
                                },

                                new ActionButton
                                {
                                    Label        = "Export cs files to project",
                                    OnClicked    = OnExportClicked,
                                    IsProcessing = state.IsExporting
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
                                    (b)"Class: ", state.SelectedClassFullName,
                                    br,
                                    (b)"Method: ", state.SelectedMethodFullName
                                }
                            }
                        }
                    }
                },

                new div(SizeFull)
                {
                    CreatePropertySelectors(state.Boundary)
                }
            };
        }
    }

    Element CreatePropertySelectors(ExternalDomainBoundary methodBoundary)
    {
        var scope = DefaultScope;
        
        var methodDefinition = scope.FindMethod(state.SelectedAssemblyFileName,state.SelectedClassFullName,state.SelectedMethodFullName);
        if (methodDefinition is null)
        {
            return null;
        }

        var elements = new List<Element>();
        
        var records = methodBoundary?.Properties ?? ImmutableList<ExternalDomainBoundaryProperty>.Empty;
        
        var relatedTypes = GetRelatedTypes(scope,methodDefinition);
        
        foreach (var typeReference in relatedTypes)
        {
            var typeDefinition = typeReference.Resolve();
            if (typeDefinition is null)
            {
                continue;
            }
            
            var itemsSource = typeDefinition.Properties.Select(p => p.Name).ToList();

            var selectedProperties = records.Where(x => x.RelatedClassFullName == typeReference.FullName).Select(x => x.RelatedPropertyName).ToList();

            elements.Add(new ListView<string>
            {
                Name                 = typeDefinition.Scope.Name +"|"+ typeDefinition.FullName,
                Title                = typeDefinition.Name,
                ItemsSource          = itemsSource,
                SelectedItems        = typeDefinition.Properties.Where(p => selectedProperties.Contains(p.Name)).Select(p => p.Name).ToList(),
                SelectedItemsChanged = PropertySelectionChanged
            });
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
    
    Task PropertySelectionChanged(string senderName, IReadOnlyList<string> selectedItems)
    {
        var assemblyFileName = senderName.Split('|').First();
        var classFullName = senderName.Split('|').Last();

        state = state with
        {
            Boundary = state.Boundary with
            {
                Properties = state.Boundary.Properties
                    .RemoveAll(x => x.AssemblyFileName == assemblyFileName && x.RelatedClassFullName == classFullName)
                    .AddRange(selectedItems.Select(p => new ExternalDomainBoundaryProperty
                    {
                        AssemblyFileName     = assemblyFileName,
                        RelatedClassFullName = classFullName,
                        RelatedPropertyName  = p
                    }))
            }
        };

        UpdateGeneratedCodePreview();
        
        return Task.CompletedTask;
    }

    void UpdateGeneratedCodePreview()
    {
        var analyzeMethodInput = new AnalyzeMethodInput
        {
            AssemblyFileName = state.SelectedAssemblyFileName,
            TypeFullName     = state.SelectedClassFullName,
            MethodFullName   = state.SelectedMethodFullName
        };

        var generatedCode = GenerateCode(DefaultScope, analyzeMethodInput, state.Boundary);

       
        
        var previewCode = "<< T Y P E S >>" + Environment.NewLine +
                          generatedCode?.ContractFile.Content
                          + Environment.NewLine
                          + "<< P R O C E S S >>"
                          + Environment.NewLine +
                          generatedCode?.ProcessFile.Content;
        
        state = state with
        {
            GeneratedCode = previewCode
        };
    }
    Task OnAnalyzeClicked1()
    {
        var analyzeMethodInput = new AnalyzeMethodInput
        {
            AssemblyFileName = state.SelectedAssemblyFileName,
            TypeFullName     = state.SelectedClassFullName,
            MethodFullName   = state.SelectedMethodFullName
        };

        var boundary = AnalyzeMethod(DefaultScope, analyzeMethodInput);
        
        state = state with
        {
            IsAnalyzing = false ,
            Boundary = boundary,
        };

        UpdateGeneratedCodePreview();

        this.NotifySuccess("Successfully analyzed");
        
        return Task.CompletedTask;
    }

    Task OnAnalyzeClicked()
    {
        state = state with { IsAnalyzing = true };

        Client.GotoMethod(OnAnalyzeClicked1);

        return Task.CompletedTask;
    }

    
    Task OnSaveClicked()
    {
        state = state with { IsSaving = true };

        Client.GotoMethod(OnSaveClicked1);
        
        return Task.CompletedTask;
    }
    
    Task OnSaveClicked1()
    {
        state = state with { IsSaving = false };
        
        Db.Save(DefaultScope, state.Boundary).ShowResult(this, "Saved");
        
        return Task.CompletedTask;
    }

    
    Task OnExportClicked()
    {
        state = state with { IsExporting = true };
        
        Client.GotoMethod(OnExportClicked1);
        
        return Task.CompletedTask;
    }
    
    Task OnExportClicked1()
    {
        state = state with { IsExporting = false };
        
        var analyzeMethodInput = new AnalyzeMethodInput
        {
            AssemblyFileName = state.SelectedAssemblyFileName,
            TypeFullName     = state.SelectedClassFullName,
            MethodFullName   = state.SelectedMethodFullName
        };

        var records = AnalyzeMethod(DefaultScope, analyzeMethodInput);

        var generatedCode = GenerateCode(DefaultScope, analyzeMethodInput, records);

        FileExporter.ExportToFile(DefaultScope, generatedCode).ShowResult(this, "Exported");

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
        state = state with
        {
            SelectedMethodFullName = methodFullName,
            Boundary = new (),
            IsAnalyzeButtonDisabled = false
        };

        var boundary = new ExternalDomainBoundaryMethod
        {
            ModuleName               = DefaultScope.Config.ModuleName,
            ExternalAssemblyFileName = state.SelectedAssemblyFileName,
            ExternalClassFullName    = state.SelectedClassFullName,
            ExternalMethodFullName   = state.SelectedMethodFullName
        };
        
        Db.TryGet(DefaultScope,boundary).Then(x =>
        {
            state = state with
            {
                Boundary = x,
                IsAnalyzeButtonDisabled = true
            };
        });
        
        return Task.CompletedTask;
    }

    Task OnSelectedTypeChanged(string typeFullName)
    {
        state = state with { SelectedClassFullName = typeFullName };

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

