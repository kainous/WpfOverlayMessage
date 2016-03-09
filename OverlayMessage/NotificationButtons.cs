using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Controls
{
    public class NotificationButtons : List<Button>
    {
        private static IEnumerable<Button> FromNames(IEnumerable<string> names)
        {
            foreach (var name in names)
            { yield return new Button { Content = name }; }
        }

        public NotificationButtons()
        { }

        public NotificationButtons(IEnumerable<Button> buttons)
            : base(buttons)
        { }

        public NotificationButtons(IEnumerable<string> buttonNames)
            : this(FromNames(buttonNames))
        { }

        public NotificationButtons(params string[] buttonNames)
            : this((IEnumerable<string>)buttonNames)
        { }

        public static NotificationButtons OK { get; } = 
            new NotificationButtons("OK");
        public static NotificationButtons OKCancel { get; } =
            new NotificationButtons("OK", "Cancel");
        public static NotificationButtons YesNo { get; } =
            new NotificationButtons("Yes", "No");
        public static NotificationButtons YesNoCancel { get; } =
            new NotificationButtons("Yes", "No", "Cancel");
    }
}
