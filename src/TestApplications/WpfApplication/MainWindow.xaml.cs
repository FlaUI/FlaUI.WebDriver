using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Window1 _subWindow;

        public MainWindow()
        {
            InitializeComponent();
            var vm = new MainViewModel();
            DataContext = vm;
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.AddedItems)
            {
                var textBlock = (TextBlock)item;
                if (textBlock.Text == "Item 4")
                {
                    MessageBox.Show("Do you really want to do it?");
                }
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            DpiScaling.Text = VisualTreeHelper.GetDpi(this).DpiScaleX.ToString();
            base.OnRenderSizeChanged(sizeInfo);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _subWindow?.Close();
            base.OnClosing(e);
        }

        private void OnShowLabel(object sender, RoutedEventArgs e)
        {
            MenuItem menuitem = sender as MenuItem;
            if (menuitem == null) { return; }

            if (menuitem.IsChecked == true)
            {
                lblMenuChk.Visibility = Visibility.Visible;
            }
            else
            {
                lblMenuChk.Visibility = Visibility.Hidden;
            }
        }

        private void OnDisableForm(object sender, RoutedEventArgs e)
        {
            textBox.IsEnabled = false;
            passwordBox.IsEnabled = false;
            editableCombo.IsEnabled = false;
            nonEditableCombo.IsEnabled = false;
            listBox.IsEnabled = false;
            checkBox.IsEnabled = false;
            threeStateCheckbox.IsEnabled = false;
            radioButton1.IsEnabled = false;
            radioButton2.IsEnabled = false;
            slider.IsEnabled = false;
            invokableButton.IsEnabled = false;
            PopupToggleButton1.IsEnabled = false;
            lblMenuChk.IsEnabled = false;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            _subWindow = new Window1();
            _subWindow.Show();
        }

        private void LabelWithHover_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            lblHover.Content = "Hovered!";
        }

        private void LabelWithHover_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            lblHover.Content = "Please hover over me";
        }
    }
}
