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

namespace System.Windows.Controls
{
    /// <summary>
    /// Interaction logic for ExampleCustomizedOverlay.xaml
    /// </summary>
    public partial class ExampleCustomizedOverlay : UserControl, ICloseableControl<string>
    {
        public ExampleCustomizedOverlay()
        { InitializeComponent(); }

        public event EventHandler<string> Closed;

        private void OnAccept(object sender, RoutedEventArgs e)
        { Closed?.Invoke(this, "Accepted"); }

        private void OnReject(object sender, RoutedEventArgs e)
        { Closed?.Invoke(this, "Rejected"); }

        private void OnCancel(object sender, RoutedEventArgs e)
        { Closed?.Invoke(this, "Cancelled"); }
    }
}
