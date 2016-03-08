using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OverlayMessage.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnShowInterruption(object sender, RoutedEventArgs e)
        {
            Task.Run(() => test.InterruptAsync("Critical"));            
        }

        private async void OnAskAsync(object sender, RoutedEventArgs e)
        {
            MessageBox.Show((await test.AskAsync("Asking asynchronously")).ToString());
        }

        private void OnAskCallback(object sender, RoutedEventArgs e)
        {
            test.Ask("Asking with callback", a => MessageBox.Show(a.ToString()));
        }
    }
}
