using ElmishBrowser.Core;

namespace ElmishBrowser
{
    public static class DesignViewModels
    {
        public static dynamic MainWindowDesignVm { get; } =
            Program.getDesignVm(Program.App.init, Program.App.bindings());
    }
}
