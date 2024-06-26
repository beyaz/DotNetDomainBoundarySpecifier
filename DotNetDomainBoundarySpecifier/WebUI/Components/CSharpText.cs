﻿namespace DotNetDomainBoundarySpecifier.WebUI.Components;

sealed class CSharpText : PureComponent
{
    public string Value { get; init; }

    protected override Element render()
    {
        return new textarea(SizeFull, Border(1, solid, Theme.BorderColor), FontSize11, OverflowScroll, WhiteSpacePre)
        {
            value    = Value,
            readOnly = true
        };
    }
}