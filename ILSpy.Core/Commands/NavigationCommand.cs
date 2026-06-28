using System;
using System.Windows.Input;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit;

namespace ICSharpCode.ILSpy;

/// <summary>
/// Navigation command. CanExecuteChanged will get called when focused is changed. 
/// </summary>
internal class NavigationCommand(string name, KeyGesture keyGesture) : RoutedCommand(name, keyGesture), ICommand
{
    static EventHandler interactiveEventHandler;

    static NavigationCommand()
    {
        InputElement.GotFocusEvent.AddClassHandler(typeof(InputElement), HandlePointerEvent);
    }

    private static void HandlePointerEvent(object sender, RoutedEventArgs args) => interactiveEventHandler?.Invoke(sender, args);

    event EventHandler ICommand.CanExecuteChanged
    {
        add { interactiveEventHandler += value; }
        remove { interactiveEventHandler -= value; }
    }
}
