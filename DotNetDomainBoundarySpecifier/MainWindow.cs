using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Threading.Tasks;
using ApiInspector.WebUI.Components;
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
                    Padding(5, 30)
                }
            };
        }

        Element createContent()
        {
            return new FlexRow(Padding(16))
            {
                new AssemblySelector
                {
                    SelectedAssemblyFileName = state.SelectedAssemblyFileName,
                    SelectionChange = OnSelectedAssemblyChanged
                },
                new TypeSelector
                {
                    AssemblyFileName = state.SelectedAssemblyFileName,
                    SelectedTypeFullName = state.SelectedTypeFullName,
                    SelectionChange = OnSelectedTypeChanged
                },
                new div
                {
                    "SelectedFile:" + state.SelectedAssemblyFileName + Environment.NewLine + 
                    state.SelectedTypeFullName
                }
            };
        }
    }

  

    

    
}

