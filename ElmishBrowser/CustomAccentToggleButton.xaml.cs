using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ElmishBrowser
{
    public partial class CustomAccentToggleButton : UserControl
    {
        /* New Content Prop */
        public object ContentElement
        {
            get { return GetValue(ContentElementProperty); }
            set { SetValue(ContentElementProperty, value); }
        }
        public static readonly DependencyProperty ContentElementProperty =
            DependencyProperty.Register("ContentElement", typeof(object), typeof(CustomAccentToggleButton), new PropertyMetadata(null));
        /* Custom IsChecked Prop */
        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }
        public static readonly DependencyProperty IsCheckedProperty = // fail to use callbacks
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(CustomAccentToggleButton), new PropertyMetadata(false));
        /* RoutedEvent Instanece */
        public static readonly RoutedEvent checkChanged =
            EventManager.RegisterRoutedEvent("CheckChange", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(CustomAccentToggleButton));
        public event RoutedEventHandler CheckChange
        {
            add => AddHandler(checkChanged, value);
            remove => RemoveHandler(checkChanged, value);
        }
        /* Constructor */
        public CustomAccentToggleButton()
        {
            InitializeComponent(); // init 
            
            /* this regist a property changed event */
            DependencyPropertyDescriptor.FromProperty(IsCheckedProperty, typeof(CustomAccentToggleButton))
                .AddValueChanged(this, (sender, e) => RaiseEvent(new(checkChanged, Self)));
        }
        /* event methods */
        private void Self_Click(object sender, RoutedEventArgs e)
        {
            if (IsChecked)
            {
                (e.OriginalSource as Button).SetValue(Button.BackgroundProperty,
                            Brushes.Transparent);
            }
            else
            {
                (e.OriginalSource as Button).SetValue(Button.BackgroundProperty,
                            Application.Current.Resources["ImmersiveSystemAccentBrush"]);
            }
            IsChecked = !IsChecked;
        }
    }
}
