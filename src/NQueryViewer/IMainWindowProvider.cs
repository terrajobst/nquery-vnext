using System;
using System.Windows;

namespace NQueryViewer
{
    internal interface IMainWindowProvider
    {
        Window Window { get; }
    }
}