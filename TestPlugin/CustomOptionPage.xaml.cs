// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System.ComponentModel;
using Avalonia.Controls;
using System.Xml.Linq;
using ICSharpCode.ILSpy;
using ICSharpCode.ILSpy.Options;
using Avalonia.Markup.Xaml;

namespace TestPlugin;

[ExportOptionPage(Title = "TestPlugin", Order = 0)]
partial class CustomOptionPage : UserControl, IOptionPage
{
    static readonly XNamespace ns = "http://www.ilspy.net/testplugin";

    public CustomOptionPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    public void Load(ILSpySettings settings)
    {
        // For loading options, use ILSpySetting's indexer.
        // If the specified section does exist, the indexer will return a new empty element.
        XElement element = settings[ns + "CustomOptions"];
        // Now load the options from the XML document:
        var options = new Options();
        options.UselessOption1 = (bool?)element.Attribute("useless1") ?? options.UselessOption1;
        options.UselessOption2 = (double?)element.Attribute("useless2") ?? options.UselessOption2;
        DataContext = options;
    }

    public void Save(XElement root)
    {
        Options options = (Options)DataContext;
        // Save the options back into XML:
        var section = new XElement(ns + "CustomOptions");
        section.SetAttributeValue("useless1", options.UselessOption1);
        section.SetAttributeValue("useless2", options.UselessOption2);

        // Replace the existing section in the settings file, or add a new section,
        // if required.
        XElement existingElement = root.Element(ns + "CustomOptions");
        if (existingElement != null)
        {
            existingElement.ReplaceWith(section);
        }
        else
        {
            root.Add(section);
        }
    }
}

class Options : INotifyPropertyChanged
{
    public bool UselessOption1
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged(nameof(UselessOption1));
            }
        }
    }

    public double UselessOption2
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged(nameof(UselessOption2));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
