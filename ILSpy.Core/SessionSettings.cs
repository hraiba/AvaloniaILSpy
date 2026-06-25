// Copyright (c) 2011 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia;
using System.Xml.Linq;
using Avalonia.Controls;

namespace ICSharpCode.ILSpy;

/// <summary>
/// Per-session setting:
/// Loaded at startup; saved at exit.
/// </summary>
public sealed class SessionSettings : INotifyPropertyChanged
{
    public SessionSettings(ILSpySettings spySettings)
    {
        XElement doc = spySettings["SessionSettings"];

        XElement filterSettings = doc.Element("FilterSettings");
        filterSettings ??= new XElement("FilterSettings");

        FilterSettings = new FilterSettings(filterSettings);

        ActiveAssemblyList = (string)doc.Element("ActiveAssemblyList");

        XElement activeTreeViewPath = doc.Element("ActiveTreeViewPath");
        if (activeTreeViewPath != null)
        {
            ActiveTreeViewPath = [.. activeTreeViewPath.Elements().Select(e => Unescape((string)e))];
        }
        ActiveAutoLoadedAssembly = (string)doc.Element("ActiveAutoLoadedAssembly");

        WindowState = FromString((string)doc.Element("WindowState"), WindowState.Normal);
        WindowBounds = FromString((string)doc.Element("WindowBounds"), DefaultWindowBounds);
        SplitterPosition = FromString((string)doc.Element("SplitterPosition"), 0.4);
        TopPaneSplitterPosition = FromString((string)doc.Element("TopPaneSplitterPosition"), 0.3);
        BottomPaneSplitterPosition = FromString((string)doc.Element("BottomPaneSplitterPosition"), 0.3);
        SelectedSearchMode = FromString((string)doc.Element("SelectedSearchMode"), Search.SearchMode.TypeAndMember);
        Theme = (string)doc.Element("Theme");
    }

    public event PropertyChangedEventHandler PropertyChanged;

    void OnPropertyChanged(string propertyName)
    {
        if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }

    public FilterSettings FilterSettings { get; private set; }
    public Search.SearchMode SelectedSearchMode { get; set; }

    public string[] ActiveTreeViewPath;
    public string ActiveAutoLoadedAssembly;

    public string ActiveAssemblyList;

    public WindowState WindowState = WindowState.Normal;
    public Rect WindowBounds;
    internal static Rect DefaultWindowBounds = new Rect(10, 10, 750, 550);
    /// <summary>
    /// position of the left/right splitter
    /// </summary>
    public double SplitterPosition;
    public double TopPaneSplitterPosition, BottomPaneSplitterPosition;
    public string Theme;

    public void Save()
    {
        XElement doc = new XElement("SessionSettings");
        doc.Add(FilterSettings.SaveAsXml());
        if (ActiveAssemblyList != null)
        {
            doc.Add(new XElement("ActiveAssemblyList", ActiveAssemblyList));
        }
        if (ActiveTreeViewPath != null)
        {
            doc.Add(new XElement("ActiveTreeViewPath", ActiveTreeViewPath.Select(p => new XElement("Node", Escape(p)))));
        }
        if (ActiveAutoLoadedAssembly != null)
        {
            doc.Add(new XElement("ActiveAutoLoadedAssembly", ActiveAutoLoadedAssembly));
        }
        doc.Add(new XElement("WindowState", ToString(WindowState)));
        doc.Add(new XElement("WindowBounds", ToString(WindowBounds)));
        doc.Add(new XElement("SplitterPosition", ToString(SplitterPosition)));
        doc.Add(new XElement("TopPaneSplitterPosition", ToString(TopPaneSplitterPosition)));
        doc.Add(new XElement("BottomPaneSplitterPosition", ToString(BottomPaneSplitterPosition)));
        doc.Add(new XElement("SelectedSearchMode", ToString(SelectedSearchMode)));

        doc.Add(new XElement("Theme", Theme));

        ILSpySettings.SaveSettings(doc);
    }

    static Regex regex = new Regex("\\\\x(?<num>[0-9A-f]{4})");

    static string Escape(string p)
    {
        StringBuilder sb = new StringBuilder();
        foreach (char ch in p)
        {
            if (char.IsLetterOrDigit(ch))
                sb.Append(ch);
            else
                sb.AppendFormat("\\x{0:X4}", (int)ch);
        }
        return sb.ToString();
    }

    static string Unescape(string p)
    {
        return regex.Replace(p, m => ((char)int.Parse(m.Groups["num"].Value, NumberStyles.HexNumber)).ToString());
    }

    static T FromString<T>(string s, T defaultValue)
    {
        if (s == null)
            return defaultValue;
        try
        {
            TypeConverter c = TypeDescriptor.GetConverter(typeof(T));
            return (T)c.ConvertFromInvariantString(s);
        }
        catch (FormatException)
        {
            return defaultValue;
        }
    }

    static Rect FromString(string s, Rect defaultValue)
    {
        if (s == null)
            return defaultValue;
        return Rect.Parse(s);
    }

    static string ToString<T>(T obj)
    {
        TypeConverter c = TypeDescriptor.GetConverter(typeof(T));
        return c.ConvertToInvariantString(obj);
    }
}
