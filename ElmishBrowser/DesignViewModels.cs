using ElmishBrowser.Core;
using System.Windows;

namespace ElmishBrowser
{
    public static class DesignViewModels
    {
        public static dynamic MainWindowDesignVm { get; } =
            Program.getDesignVm(Program.App.init.Item1, Program.App.bindings());
    }
}