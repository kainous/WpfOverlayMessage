using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace System.Windows.Controls
{
    public class NotifierInformation
    {
        //private static ConcurrentDictionary<Panel, OverlayMessage>;

        public NotificationButtons Buttons { get; set; }
        public object Title { get; set; }
        public object Message { get; set; }
        public Color TitleColor { get; set; }
        public Panel ParentPanel { get; set; }
        public event EventHandler<int> Completed;

        public Color StartGradient { get { return TitleColor; } }
        public Color EndGradient { get { return TitleColor - Color.FromArgb(0x33, 0, 0, 0); } }

        internal OverlayMessage GetDisplay()
        {
            var display = new OverlayMessage(ParentPanel)
            {
                Header = Title,
                Content = Message,
                DataContext = this
            };

            display.Closed += Completed;

            return display;
        }
    }
}
