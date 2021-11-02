using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace ElmishBrowser
{
    public partial class BindableWebView : UserControl
    {
        public BindableWebView()
        {
            InitializeComponent();

            DependencyPropertyDescriptor.FromProperty(IsDisposedProperty, typeof(BindableWebView))
                .AddValueChanged(this, (sender, e) =>
                {
                    if (IsDisposed == true)
                    {
                        selfWv.Dispose();
                    }
                }); // regist property changed event of a denpendency property. 
            selfWv.CoreWebView2InitializationCompleted += (sender, e) =>
                selfWv.CoreWebView2.NewWindowRequested += (sender, e) =>
                {
                    e.Handled = true;
                    selfWv.Tag = e.Uri;
                    RaiseEvent(new(newWindowRequest, selfWv));
                };
        }
        public static readonly RoutedEvent newWindowRequest =
            EventManager.RegisterRoutedEvent("NewWindowRequest", RoutingStrategy.Bubble, typeof(RoutedEventArgs), typeof(BindableWebView));
        public event RoutedEventHandler NewWindowRequest
        {
            add => AddHandler(newWindowRequest, value);
            remove => RemoveHandler(newWindowRequest, value);
        }
        public string Address
        {
            get { return (string)GetValue(AddressProperty); }
            set { SetValue(AddressProperty, value); }
        }
        public static readonly DependencyProperty AddressProperty =
            DependencyProperty.Register("Address", typeof(string), typeof(BindableWebView), new PropertyMetadata("https://www.bing.com"));
        public bool IsDisposed
        {
            get { return (bool)GetValue(IsDisposedProperty); }
            set { SetValue(IsDisposedProperty, value); }
        }
        public static readonly DependencyProperty IsDisposedProperty =
            DependencyProperty.Register("IsDisposed", typeof(bool), typeof(BindableWebView), new PropertyMetadata(false));
        public Color DefaultBackground
        {
            get { return (Color)GetValue(DefaultBackgroundProperty); }
            set { SetValue(DefaultBackgroundProperty, value); }
        }
        public static readonly DependencyProperty DefaultBackgroundProperty =
            DependencyProperty.Register("DefaultBackground", typeof(Color), typeof(BindableWebView), new PropertyMetadata(Color.Transparent));
        private void selfWv_SourceChanged(object sender, Microsoft.Web.WebView2.Core.CoreWebView2SourceChangedEventArgs e) =>
            Address = selfWv.Source.OriginalString;
    }
}