

using ReactWithDotNet.ThirdPartyLibraries.FramerMotion;

namespace DotNetDomainBoundarySpecifier.WebUI.MainWindow;

sealed class MainView : Component<MainViewModel>
{
    protected override Task constructor()
    {
        state = StateFile.TryReadState() ?? new();

        state = state with { IsAnalyzing = false };

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
                                    Label     = "Save",
                                    OnClicked = OnSaveClicked,
                                },

                                new ActionButton
                                {
                                    Label     = "Export cs files to project",
                                    OnClicked = OnExportClicked
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

    Element CreatePropertySelectors(ExternalDomainBoundary methodBoundary)
    {
        var records = methodBoundary?.Properties;
        
        if (records is null || records.Count == 0)
        {
            return null;
        }


        

        var methodDefinition = DefaultScope
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
            var typeDefinition = DefaultScope.GetTypesInAssemblyFile(state.SelectedAssemblyFileName).FirstOrDefault(x => x.FullName == relatedTableFullName);
            if (typeDefinition is null)
            {
                continue;
            }

            var itemsSource = typeDefinition.Properties.Select(p => p.Name).ToList();

            var selectedProperties = records.Where(x => x.RelatedClassFullName == relatedTableFullName).Select(x => x.RelatedPropertyFullName).ToList();

            elements.Add(new ListView<string>
            {
                Name          = typeDefinition.Name,
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
                    Name          = typeDefinition.Name,
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

        state = state with { Records = AnalyzeMethod(DefaultScope, analyzeMethodInput) };

        var generatedCode = GenerateCode(DefaultScope, analyzeMethodInput, state.Records);

        state = state with
        {
            GeneratedCode = "<< T Y P E S >>" + Environment.NewLine +
                            generatedCode.ContractFile.Content
                            + Environment.NewLine
                            + "<< P R O C E S S >>"
                            + Environment.NewLine +
                            generatedCode.ProcessFile.Content
        };

        this.NotifySuccess("Başarılı");
        
        return Task.CompletedTask;
    }

    Task OnAnalyzeClicked()
    {
        state = state with { IsAnalyzing = true };

        Client.GotoMethod(DoAnalyze);

        return Task.CompletedTask;
    }
    
    Task OnSaveClicked()
    {
        Db.Save(DefaultScope, state.Records);
        
        return Task.CompletedTask;
    }

    Task OnExportClicked()
    {
        var analyzeMethodInput = new AnalyzeMethodInput
        {
            AssemblyFileName = state.SelectedAssemblyFileName,
            TypeFullName     = state.SelectedTypeFullName,
            MethodFullName   = state.SelectedMethodFullName
        };

        var records = AnalyzeMethod(DefaultScope, analyzeMethodInput);

        var generatedCode = GenerateCode(DefaultScope, analyzeMethodInput, records);

        var result = FileExporter.ExportToFile(DefaultScope, generatedCode);
        if (result.HasError)
        {
            Client.RunJavascript($"alert(\"{result.Error}\")");
        }

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

static class Notification
{
    public static void NotifySuccess(this ReactComponentBase component, string message)
    {
        component.Client.DispatchEvent<NotificationHost.SuccessNotify>([message]);
    }
}
sealed class NotificationHost: Component<NotificationHost.State>
{
    internal record State
    {
        public string SuccessMessage { get; init; }
    }
    
   public delegate Task SuccessNotify(string message);
    
  
    
    protected override Task constructor()
    {
        Client.ListenEvent<SuccessNotify>(OnSuccessNotified);
        
        return Task.CompletedTask;
    }

    Task OnSuccessNotified(string message)
    {
        state = state with { SuccessMessage = message };
        
        Client.GotoMethod(ResetState, TimeSpan.FromSeconds(1));
        
        return Task.CompletedTask;
    }

    Task ResetState()
    {
        state = new();
        
        return Task.CompletedTask;
    }
    

    protected override Element render()
    {
        if (state.SuccessMessage.HasValue())
        {
            return AnimateVisibility(true,Draw(state.SuccessMessage));
        }

        return null;
    }

    static Element IconSuccess => new FlexRowCentered(Size(24), Color("#52c41a"))
    {
        new svg(Aria("hidden", "true"), Data("icon", "check-circle"), ViewBox(64, 64, 896, 896), svg.FocusableFalse, Fill("currentColor"), Size("1em"))
        {
            new path { d = "M512 64C264.6 64 64 264.6 64 512s200.6 448 448 448 448-200.6 448-448S759.4 64 512 64zm193.5 301.7l-210.6 292a31.8 31.8 0 01-51.7 0L318.5 484.9c-3.8-5.3 0-12.7 6.5-12.7h46.9c10.2 0 19.9 4.9 25.9 13.3l71.2 98.8 157.2-218c6-8.3 15.6-13.3 25.9-13.3H699c6.5 0 10.3 7.4 6.5 12.7z" }
        }
    };
    
    static Element IconFail => new FlexRowCentered(Size(24), Color("#52c41a"))
    {
        new svg(Aria("hidden", "true"), Data("icon", "check-circle"), ViewBox(64, 64, 896, 896), svg.FocusableFalse, Fill("currentColor"), Size("1em"))
        {
            new path { d = "M512 64c247.4 0 448 200.6 448 448S759.4 960 512 960 64 759.4 64 512 264.6 64 512 64zm127.98 274.82h-.04l-.08.06L512 466.75 384.14 338.88c-.04-.05-.06-.06-.08-.06a.12.12 0 00-.07 0c-.03 0-.05.01-.09.05l-45.02 45.02a.2.2 0 00-.05.09.12.12 0 000 .07v.02a.27.27 0 00.06.06L466.75 512 338.88 639.86c-.05.04-.06.06-.06.08a.12.12 0 000 .07c0 .03.01.05.05.09l45.02 45.02a.2.2 0 00.09.05.12.12 0 00.07 0c.02 0 .04-.01.08-.05L512 557.25l127.86 127.87c.04.04.06.05.08.05a.12.12 0 00.07 0c.03 0 .05-.01.09-.05l45.02-45.02a.2.2 0 00.05-.09.12.12 0 000-.07v-.02a.27.27 0 00-.05-.06L557.25 512l127.87-127.86c.04-.04.05-.06.05-.08a.12.12 0 000-.07c0-.03-.01-.05-.05-.09l-45.02-45.02a.2.2 0 00-.09-.05.12.12 0 00-.07 0z" }
        }
    };
    
    static Element Draw(string message)
    {
        return new FlexRow(AlignItemsCenter, Gap(8), BackgroundWhite, Theme.BoxShadowForWindow, BorderRadius(5),Padding(8), Border(1,solid,Theme.BorderColor))
        {
            IconSuccess,
            message,
            WidthFitContent
        };
    }
    
    
    public static Element AnimateVisibility(bool isVisible, Element content)
    {
        return new AnimatePresence
        {
            !isVisible
                ? null
                : new motion.div
                {
                    initial =
                    {
                        height  = 0,
                        opacity = 0,
                        
                        top=0,
                        right=24
                    },
                    animate =
                    {
                        height   = "auto",
                        opacity  = 1,
                        position ="fixed",
                        top   =30,
                        right    =24
                    },
                    exit =
                    {
                        height  = 0,
                        opacity = 0
                    },

                    style = { WidthAuto, PositionFixed },
                    children =
                    {
                        content
                    }
                }
        };
    }
}