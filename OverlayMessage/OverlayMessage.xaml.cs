using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class DataEventArgs<T> : EventArgs
    {
        public T Data { get; private set; }
        public DataEventArgs(T data)
        { Data = data; }
    }

    /// <summary>
    /// Interaction logic for OverlayMessage.xaml
    /// </summary>
    public partial class OverlayMessage : HeaderedContentControl
    {
        private readonly Panel _Panel;

        public EventHandler<DataEventArgs<int>> Closed;

        public OverlayMessage(Panel panel)
        {
            _Panel = panel;
            InitializeComponent();
        }

        private void OnClose(object sender, RoutedEventArgs e)
        {
            _Panel.Children.Remove(this);
            Closed?.Invoke(this, new DataEventArgs<int>(0));
            Closed = null;  //Clear all subscribers
        }
    }
}
