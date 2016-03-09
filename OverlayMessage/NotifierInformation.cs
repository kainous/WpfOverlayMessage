using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Controls
{
    public class NotifierInformation
    {
        //private static ConcurrentDictionary<Panel, OverlayMessage>;

        public NotificationButtons Buttons { get; set; }
        public object Title { get; set; }
        public object Message { get; set; }
        public Panel ParentPanel { get; set; }
        public event EventHandler<DataEventArgs<int>> Completed;

        internal OverlayMessage GetDisplay()
        {
            var display = new OverlayMessage(ParentPanel)
            {
                Header = Title,
                Content = Message                
            };

            display.Closed += Completed;

            return display;
        }
    }
}
