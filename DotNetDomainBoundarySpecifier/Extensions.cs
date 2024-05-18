﻿using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using ReactWithDotNet.ThirdPartyLibraries.ReactFreeScrollbar;

namespace ApiInspector.WebUI;

/// <summary>
/// Deneme
/// </summary>
static partial class Extensions
{
    public static string BluePrimary => "#1976d2";

    public static StyleModifier PrimaryBackground => Background("rgb(249, 249, 249)");
    
    public static StyleModifier ComponentBoxShadow => BoxShadow("6px 6px 20px 0px rgb(69 42 124 / 15%)");

    public static StyleModifier InputStyle => new Style
    {
        Hover(Border(Solid(1,"#3498ff"))),
        ComponentBoxShadow, FontSize12, Padding(8), Border(Solid(1, "#ced4da")), Focus(OutlineNone), BorderRadius(3), Color("#495057")
    };

    public static string GetSvgUrl(string svgFileName)
    {
        var resourceFilePathInAssembly = $"ApiInspector.WebUI.Resources.{svgFileName}.svg";

        return getDataUriFromSvgBytes(getEmbeddedFile(resourceFilePathInAssembly));

        static string getDataUriFromSvgBytes(byte[] bytesOfSvgFile)
        {
            var imageBase64 = Convert.ToBase64String(bytesOfSvgFile);

            return "data:image/svg+xml;base64," + imageBase64;
        }

        static byte[] getEmbeddedFile(string resourceFilePathInAssembly)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var resourceStream = assembly.GetManifestResourceStream(resourceFilePathInAssembly);
            if (resourceStream == null)
            {
                return null;
            }

            using var memoryStream = new MemoryStream();

            resourceStream.CopyTo(memoryStream);

            return memoryStream.ToArray();
        }
    }

    public static (bool isDotNetCore, bool isDotNetFramework) GetTargetFramework(FileInfo dll)
    {
        var CompiledNetCoreRegex      = new Regex(@".NETCoreApp,Version=v[0-9\.]+", RegexOptions.Compiled);
        var CompiledNetFrameworkRegex = new Regex(@".NETFramework,Version=v[0-9\.]+", RegexOptions.Compiled);
        var CompiledNetstandard= new Regex("netstandard+", RegexOptions.Compiled);

        var contents = File.ReadAllText(dll.FullName);

       

        var match = CompiledNetFrameworkRegex.Match(contents);
        if (match.Success)
        {
            return (false, true);
        }
        
        match = CompiledNetCoreRegex.Match(contents);
        if (match.Success)
        {
            return (true, false);
        }
        
        match = CompiledNetstandard.Match(contents);
        if (match.Success)
        {
            return (true, false);
        }

        return (false, false);
    }

    public static bool HasNoValue(this string value) => string.IsNullOrWhiteSpace(value);

    public static bool HasValue(this string value) => !string.IsNullOrWhiteSpace(value);
    
    public static readonly Modifier AutoHideScrollbar = FreeScrollBar.Modify(x => x.autohide = true);


    
    
    /// <summary>
    ///     Removes value from start of str
    /// </summary>
    public static string RemoveFromStart(this string data, string value)
    {
        return RemoveFromStart(data, value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Removes value from start of str
    /// </summary>
    public static string RemoveFromStart(this string data, string value, StringComparison comparison)
    {
        if (data == null)
        {
            return null;
        }

        if (data.StartsWith(value, comparison))
        {
            return data.Substring(value.Length, data.Length - value.Length);
        }

        return data;
    }
    
    
    /// <summary>
    ///     Removes value from end of str
    /// </summary>
    public static string RemoveFromEnd(this string data, string value)
    {
        return RemoveFromEnd(data, value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Removes from end.
    /// </summary>
    public static string RemoveFromEnd(this string data, string value, StringComparison comparison)
    {
        if (data.EndsWith(value, comparison))
        {
            return data.Substring(0, data.Length - value.Length);
        }

        return data;
    }
}
