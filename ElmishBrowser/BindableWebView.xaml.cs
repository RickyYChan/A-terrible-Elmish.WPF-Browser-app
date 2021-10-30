using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace ElmishBrowser
{
    public partial class BindableWebView : UserControl
    {
        public BindableWebView()
        {
            InitializeComponent(); // init 

            DependencyPropertyDescriptor.FromProperty(IsDisposedProperty, typeof(BindableWebView))
                .AddValueChanged(this, (sender, e) =>
                {
                    if (IsDisposed == true)
                    {
                        System.Console.WriteLine("\n[[[ Dispoing! ]]]\n");
                        selfWv.Dispose();
                    }
                }); // regist property changed event of a denpendency property. 

            selfWv.CoreWebView2InitializationCompleted += (sender, e) =>
                selfWv.CoreWebView2.NewWindowRequested += (sender, e) =>
                    e.NewWindow = selfWv.CoreWebView2;
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
    }
}
