using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Threading.Tasks;
using ApiInspector.WebUI.Components;
using Newtonsoft.Json;
using ReactWithDotNet;
using ReactWithDotNet.ThirdPartyLibraries.MonacoEditorReact;
using ReactWithDotNet.ThirdPartyLibraries.MUI.Material;
using ReactWithDotNet.ThirdPartyLibraries.ReactFreeScrollbar;
using static System.Environment;

namespace ApiInspector.WebUI;

class MainWindow : Component<MainWindowModel>
{
    Task OnSelectedAssemblyChanged(string assemblyfilename)
    {
        state = state with { SelectedAssemblyFileName = assemblyfilename };
        
        return Task.CompletedTask;
    }
    Task OnSelectedTypeChanged(string typeFullName)
    {
        state = state with { SelectedTypeFullName = typeFullName };
        
        return Task.CompletedTask;
    }
    Task OnSelectedMethodChanged(string methodFullName)
    {
        state = state with { SelectedMethodFullName = methodFullName };
        
        return Task.CompletedTask;
    }

    protected override Element render()
    {
        return new FlexRow(Padding(10), SizeFull, Theme.BackgroundForBrowser)
        {
            new FlexColumn
            {
                applicationTopPanel,
                createContent,
                
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
                new SplitRow
                {
                    sizes = [25,25,25,25],
                    style = { Padding(16) },
                    children =
                    {
                        new AssemblySelector
                        {
                            SelectedAssemblyFileName = state.SelectedAssemblyFileName,
                            SelectionChange          = OnSelectedAssemblyChanged
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
                                    Label = "Analyze",
                                    OnClicked = OnAnalyzeClicked,
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
                            
                            new p(WordBreakAll)
                            {
                                (b)"Assembly: " , state.SelectedAssemblyFileName,
                                br,
                                (b)"Class: ", state.SelectedTypeFullName,
                                br,
                                (b)"Method: ", state.SelectedMethodFullName
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

    Task OnAnalyzeClicked()
    {
        state = state with { IsAnalyzing = true };
        
        Client.GotoMethod(DoAnalyze);
        
        return Task.CompletedTask;
    }

    Task DoAnalyze()
    {
        state = state with { IsAnalyzing = false };

        state = state with { Records = AnalyzeMethod(state) };
        
        return Task.CompletedTask;
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
        
        var elements = new List<Element>();
        
        foreach (var relatedTableFullName in records.Select(x=>x.RelatedClassFullName).Distinct())
        {
            elements.Add(new ListView<TableModel>
            {
                ItemsSource = GetTypesInAssemblyFile(state.SelectedAssemblyFileName).FirstOrDefault(x=>x.FullName == relatedTableFullName)?.Properties.Select(p=>new TableModel
                {
                    RelatedClassFullName = relatedTableFullName,
                    RelatedPropertyFullName = p.FullName
                }).ToList(),
                
                SelectedItems = records.Where(x=>x.RelatedClassFullName == relatedTableFullName).ToList(),
                
                
            });
            
        }

        return new FlexRow(Gap(16))
        {
            elements
        };
    }
}

