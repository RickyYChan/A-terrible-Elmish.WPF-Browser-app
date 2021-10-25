using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ElmishBrowser
{
    public partial class CustomAccentToggleButton : UserControl
    {
        /* New Content Prop */
        public object ElementContent
        {
            get { return GetValue(ElementContentProperty); }
            set { SetValue(ElementContentProperty, value); }
        }
        public static readonly DependencyProperty ElementContentProperty =
            DependencyProperty.Register("ElementContent", typeof(object), typeof(CustomAccentToggleButton), new PropertyMetadata(null));
        /* New background */
        public Brush ElementBackground
        {
            get { return (Brush)GetValue(ElementBackgroundProperty); }
            set { SetValue(ElementBackgroundProperty, value); }
        }
        public static readonly DependencyProperty ElementBackgroundProperty =
            DependencyProperty.Register("ElementBackground", typeof(Brush), typeof(CustomAccentToggleButton), new PropertyMetadata(Brushes.Transparent));
        /* New border thickness */
        public Thickness ElementBorderThickness
        {
            get { return (Thickness)GetValue(ElementBorderThicknessProperty); }
            set { SetValue(ElementBorderThicknessProperty, value); }
        }
        public static readonly DependencyProperty ElementBorderThicknessProperty =
            DependencyProperty.Register("ElementBorderThickness", typeof(Thickness), typeof(CustomAccentToggleButton), new PropertyMetadata(new Thickness(0)));
        /* New border brush */
        public Brush ElementBorderBrush
        {
            get { return (Brush)GetValue(ElementBorderBrushProperty); }
            set { SetValue(ElementBorderBrushProperty, value); }
        }
        public static readonly DependencyProperty ElementBorderBrushProperty =
            DependencyProperty.Register("ElementBorderBrush", typeof(Brush), typeof(CustomAccentToggleButton), new PropertyMetadata(Brushes.Transparent));
        /* New padding */
        public Thickness ElementPadding
        {
            get { return (Thickness)GetValue(ElementPaddingProperty); }
            set { SetValue(ElementPaddingProperty, value); }
        }
        public static readonly DependencyProperty ElementPaddingProperty =
            DependencyProperty.Register("ElementPadding", typeof(Thickness), typeof(CustomAccentToggleButton), new PropertyMetadata(new Thickness(10, 0, 10, 0)));
        /* 
            Custom IsChecked Prop 
        */
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
