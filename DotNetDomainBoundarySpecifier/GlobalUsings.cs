﻿global using System;
global using ReactWithDotNet;
global using System.Collections.Generic;
global using System.Linq;
global using static ReactWithDotNet.Mixin;
global using System.Threading.Tasks;
global using static DotNetDomainBoundarySpecifier.Processors.Analyzer;
global using System.Collections.Immutable;
global using DotNetDomainBoundarySpecifier.Utilities;
global using System;
global using System.IO;
global using Mono.Cecil;
global using Mono.Cecil.Cil;
global using static DotNetDomainBoundarySpecifier.Utilities.FileHelper;
global using static DotNetDomainBoundarySpecifier.ConfigReader;
global using static DotNetDomainBoundarySpecifier.Processors.CecilHelper;
global using static DotNetDomainBoundarySpecifier.Extensions;
global using DotNetDomainBoundarySpecifier.Processors;
global using DotNetDomainBoundarySpecifier.WebUI.Components;
global using System.Text;
global using ReactWithDotNet.ThirdPartyLibraries.ReactWithDotNetSkeleton;



using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("Test.ExternalDomainY")]