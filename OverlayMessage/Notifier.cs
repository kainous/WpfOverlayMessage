using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace System.Windows.Controls
{
    public static class Notifier
    {
        private static IEnumerable<T> FindVisualChildren<T>(this DependencyObject obj)
            where T : DependencyObject
        {
            if (obj != null)
            {
                var count = VisualTreeHelper.GetChildrenCount(obj);
                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(obj, i);

                    var typedChild = child as T;
                    if (typedChild != null)
                    { yield return typedChild; }

                    foreach (var subchild in FindVisualChildren<T>(child))
                    { yield return subchild; }
                }
            }
        }

        private static void ShowNewWindow(
            Window parent
          , OverlayMessage display
          , string message
          , Action<int> callback
          , bool bringToForeground
          , bool topMost)
        {
            var window = new Window
            {
                Content = display,
                WindowStyle = WindowStyle.None,
                Background = Brushes.Transparent,
                AllowsTransparency = true,
                Topmost = topMost,
                Owner = parent,
                ShowInTaskbar = false
            };

            display.Closed += (o, e) =>
            {
                window.Close();
                callback(e.Data);
            };

            window.ShowDialog();
        }

        private static void ShowEmbedded(Panel parent, object message, object title, Action<int> callback)
        {
            //Assert that parent is never null at this point            

            var display = new OverlayMessage(parent)
            {
                Content = message,
                Header = title
            };

            var layoutRoot = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(0x55, 0, 0, 0)),
                Child = display
            };            

            int zIndex = parent.Children
                               .OfType<UIElement>()
                               .Max(Panel.GetZIndex) + 1;

            Panel.SetZIndex(layoutRoot, zIndex);

            parent.Children.Add(layoutRoot);
            display.Closed += (o, e) =>
            {
                parent.Children.Remove(layoutRoot);
                callback?.Invoke(e.Data);
            };            
        }

        private static void InterruptCore(Panel parentPanel, string message, string title, Action<int> callback = null, bool bringToForeground = true)
        {
            if (parentPanel == null)
            {
                var window = Application.Current
                                        .MainWindow;

                if ((window?.IsVisible == true) && (window?.WindowState != WindowState.Minimized))
                {
                    parentPanel = window.FindVisualChildren<Panel>()
                                        .FirstOrDefault();

                    ShowEmbedded(parentPanel, message, title, callback);
                }
                else
                {
                    var display = new OverlayMessage(null)
                    {
                        Content = message,
                        Header = title
                    };
                    ShowNewWindow(window, display, message, callback, true, true);
                }
            }
            else
            { ShowEmbedded(parentPanel, message, title, callback); }
        }

        public static async Task InterruptAsync(this Panel parentPanel, string message, string title = "")
        { await Application.Current.Dispatcher.InvokeAsync(() => InterruptCore(parentPanel, message, title)); }

        public static void Interrupt(this Panel parentPanel, string message, Action<int> callback, string title = "")
        { Application.Current.Dispatcher.Invoke(() => InterruptCore(parentPanel, message, title, callback)); }

        public static void Ask(this Panel parentPanel, string message, Action<int> callback)
        { Application.Current.Dispatcher.Invoke(() => ShowEmbedded(parentPanel, message, "", callback)); }

        //Synchronous call must occur in message box style
        //public static int Ask(string message)
        //{
        //    var semaphore = new SemaphoreSlim(0, 1);
        //    int result = -1;
        //    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        var display = new OverlayMessage(parentPanel);

        //        display.Closed += (o, e) =>
        //        {
        //            result = e.Data;
        //            semaphore.Release();
        //        };
        //        parentPanel.Children.Add(display);
        //        int zIndex = parentPanel.Children
        //                                .OfType<UIElement>()
        //                                .Max(Panel.GetZIndex) + 1;

        //        Panel.SetZIndex(display, zIndex);
        //        semaphore.Wait();
        //    }));
        //    return result;
        //}

        public static async Task<int> AskAsync(this Panel parentPanel, string message)
        {
            int result = -1;
            using (var semaphore = new SemaphoreSlim(0, 1))
            {
                var callback = new Action<int>(a =>
                {
                    result = a;
                    semaphore.Release();
                });

                Application.Current.Dispatcher.Invoke(() => ShowEmbedded(parentPanel, message, "", callback));
                await semaphore.WaitAsync();
            }
            return result;
        }
    }
}
