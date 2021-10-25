using ElmishBrowser.Core;
using System;
using System.Windows;

namespace ElmishBrowser
{
    public partial class App : Application
    {
        public App()
        {
            this.Activated += StartElmish;
        }

        private void StartElmish(object sender, EventArgs e)
        {
            this.Activated -= StartElmish;
            var window = (MainWindow)MainWindow;
            Program.main(window);
        }
    }
}
