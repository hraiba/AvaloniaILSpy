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

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media;

namespace ICSharpCode.ILSpy.Options;

/// <summary>
/// Description of DisplaySettings.
/// </summary>
public class DisplaySettings : INotifyPropertyChanged
{
    public DisplaySettings()
    {
    }

    #region INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    #endregion


    public FontFamily SelectedFont
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    }

    public double SelectedFontSize
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    }

    public bool ShowLineNumbers
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    }

    public bool ShowMetadataTokens
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    }

    public bool ShowMetadataTokensInBase10
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    }

    public bool EnableWordWrap
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    }

    public bool SortResults
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    } = true;

    public bool FoldBraces
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    } = false;

    public bool ExpandMemberDefinitions
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    } = false;

    public bool ExpandUsingDeclarations
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    } = false;

    public bool ShowDebugInfo
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IndentationUseTabs
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    } = true;

    public int IndentationTabSize
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    } = 4;

    public int IndentationSize
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    } = 4;

    public bool HighlightMatchingBraces
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    } = true;

    public void CopyValues(DisplaySettings s)
    {
        SelectedFont = s.SelectedFont;
        SelectedFontSize = s.SelectedFontSize;
        ShowLineNumbers = s.ShowLineNumbers;
        ShowMetadataTokens = s.ShowMetadataTokens;
        ShowMetadataTokensInBase10 = s.ShowMetadataTokensInBase10;
        ShowDebugInfo = s.ShowDebugInfo;
        EnableWordWrap = s.EnableWordWrap;
        SortResults = s.SortResults;
        FoldBraces = s.FoldBraces;
        ExpandMemberDefinitions = s.ExpandMemberDefinitions;
        ExpandUsingDeclarations = s.ExpandUsingDeclarations;
        IndentationUseTabs = s.IndentationUseTabs;
        IndentationTabSize = s.IndentationTabSize;
        IndentationSize = s.IndentationSize;
        HighlightMatchingBraces = s.HighlightMatchingBraces;
    }
}
