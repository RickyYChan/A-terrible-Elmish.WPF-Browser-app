using Microsoft.Web.WebView2.Wpf;
using System.Windows;

namespace ElmishBrowser
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void WebView2_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            var wv = sender as WebView2;
            wv.CoreWebView2.NewWindowRequested += (sender, e) =>
                e.NewWindow = wv.CoreWebView2;
        }
    }
}