using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Controls
{
    public class NotificationButtons
    {
        private List<Func<Button>> _ButtonCreationFunctions = new List<Func<Button>>();

        private static IEnumerable<Func<Button>> FromNames(IEnumerable<string> names)
        {
            foreach (var name in names)
            { yield return () => new Button { Content = name }; }
        }

        public NotificationButtons()
        { }

        private static Func<T> Delay<T>(T item)
        { return () => item; }

        private static IEnumerable<Func<T>> Delay<T>(IEnumerable<T> items)
        { return items.Select(Delay); }

        public NotificationButtons(IEnumerable<Button> buttons)
        { _ButtonCreationFunctions.AddRange(Delay(buttons)); }

        public NotificationButtons(IEnumerable<string> buttonNames)
        { _ButtonCreationFunctions.AddRange(FromNames(buttonNames)); }

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
