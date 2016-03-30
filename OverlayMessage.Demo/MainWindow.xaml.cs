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
using System.Windows.Threading;

namespace OverlayMessage.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _timer;
        private readonly Random _random = new Random();
        private readonly byte[] _updatingData = new byte[1024];

        public MainWindow()
        {
            InitializeComponent();
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            _timer.Tick += OnTick;            
            _timer.Start();
        }

        private void OnTick(object sender, EventArgs e)
        {
            _random.NextBytes(_updatingData);
            this.UpdateThis.Text = Convert.ToBase64String(_updatingData);
        }

        private void OnShowInterruption(object sender, RoutedEventArgs e)
        {
            Task.Run(() => test.InterruptAsync("Critical", GetTemplate()));
        }

        private async void OnAskAsync(object sender, RoutedEventArgs e)
        {
            //test.AskAsync("Hello", )                        
            MessageBox.Show((await test.AskAsync("Asking asynchronously")).ToString());
        }

        private void OnAskCallback(object sender, RoutedEventArgs e)
        {
            test.Ask("Asking with callback", a => MessageBox.Show(a.ToString()));
        }

        private NotifierInformation GetTemplate()
        {
            return Dispatcher.Invoke(() =>
            {
                if (InformationButton.IsChecked == true)
                { return NotificationTemplates.Information; }
                else if (ErrorButton.IsChecked == true)
                { return NotificationTemplates.Error; }

                return null;
            });
        }

        private void OnMessageBox(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            Notifier.Interrupt(null, "Test", _ => { }, GetTemplate());
            this.WindowState = WindowState.Normal;
        }

        private async void OnComplicatedBox(object sender, RoutedEventArgs e)
        {
            var ex = new ExampleCustomizedOverlay();
            var result = await test.EmbedAsync<ExampleCustomizedOverlay, string>(ex);
            MessageBox.Show(result);
        }
    }
}
