using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace System.Windows.Controls
{
    public static class NotificationTemplates
    {
        public static NotifierInformation Information
        {
            get
            {
                return new NotifierInformation
                {
                    TitleColor = Colors.Blue
                };
            }
        }

        public static NotifierInformation Error
        {
            get
            {
                return new NotifierInformation
                {
                    TitleColor = Colors.Red
                };
            }
        }
    }

    public static class Notifier
    {
        private static async Task DispatchAsync(Action action)
        { await Application.Current.Dispatcher.InvokeAsync(action); }

        private static void Dispatch(Action action)
        { Application.Current.Dispatcher.Invoke(action); }

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

        private static void ShowNewWindow(Window parent, NotifierInformation info)
        {
            var display = info.GetDisplay();

            var window = new Window
            {
                Content = display,
                WindowStyle = WindowStyle.None,
                Background = Brushes.Transparent,
                AllowsTransparency = true,
                Topmost = true,
                Owner = parent,
                ShowInTaskbar = false
            };

            display.Closed += (o, e) => window.Close();

            window.ShowDialog();
        }

        private static void ShowEmbedded<T, R>(Panel parent, T overlay)
            where T : UIElement
                    , ICloseableControl<R>
        {
            //Assert that parent is never null at this point    
            var layoutRoot = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(0x55, 0, 0, 0)),
                Child = overlay
            };

            int zIndex = parent.Children
                               .OfType<UIElement>()
                               .Max(Panel.GetZIndex);

            Panel.SetZIndex(layoutRoot, zIndex);

            parent.Children.Add(layoutRoot);
            overlay.Closed += (o, e) => parent.Children.Remove(layoutRoot);
        }

        private static void ShowEmbedded(NotifierInformation info)
        {
            //Assert that parent is never null at this point    
            var display = info.GetDisplay();

            var layoutRoot = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(0x55, 0, 0, 0)),
                Child = display
            };

            var parent = info.ParentPanel;
            int zIndex = parent.Children
                               .OfType<UIElement>()
                               .Max(Panel.GetZIndex);

            Panel.SetZIndex(layoutRoot, zIndex);

            parent.Children.Add(layoutRoot);
            display.Closed += (o, e) => parent.Children.Remove(layoutRoot);
        }

        private static void InterruptCore(NotifierInformation info)
        {
            var window = Application.Current?.MainWindow;
            if (info.ParentPanel != null)
            { ShowEmbedded(info); }
            else if ((window?.IsVisible == false)
                  || (window?.WindowState == WindowState.Minimized))
            { ShowNewWindow(window, info); }
            else
            {
                info.ParentPanel = window.FindVisualChildren<Panel>()
                                         .FirstOrDefault();

                ShowEmbedded(info);
            }
        }

        public static async Task<int> InterruptAsync(this NotifierInformation info)
        {
            using (var semaphore = new SemaphoreSlim(0, 1))
            {
                int result = -1;

                info.Completed += (o, e) =>
                {
                    result = e;
                    semaphore.Release();
                };

                await DispatchAsync(() => InterruptCore(info));
                await semaphore.WaitAsync();

                return result;
            }
        }

        public static async Task<R> EmbedAsync<T, R>(this Panel parentPanel, T overlay)
            where T : UIElement
                    , ICloseableControl<R>
        {
            using (var semaphore = new SemaphoreSlim(0, 1))
            {
                R result = default(R);
                overlay.Closed += (o, e) =>
                {
                    result = e;
                    semaphore.Release();
                };

                await DispatchAsync(() => ShowEmbedded<T, R>(parentPanel, overlay));
                await semaphore.WaitAsync();

                return result;
            }
        }

        public static async Task<R> EmbedAsync<T, R>(this Panel parentPanel)
            where T : UIElement
                    , ICloseableControl<R>
                    , new()
        {
            R result = default(R);
            using (var semaphore = new SemaphoreSlim(0, 1))
            {
                Dispatch(() =>
                {
                    var overlay = new T();

                    overlay.Closed += (o, e) =>
                    {
                        result = e;
                        semaphore.Release();
                    };

                    ShowEmbedded<T, R>(parentPanel, overlay);
                });
                await semaphore.WaitAsync();
            }
            return result;
        }

        public static Task<int> InterruptAsync(this Panel parentPanel, string message, NotifierInformation template = null, NotificationButtons buttons = null, string title = "")
        {
            template = template ?? NotificationTemplates.Information;

            template.Message = message;
            template.Title = title;
            template.ParentPanel = parentPanel;
            template.Buttons = buttons ?? NotificationButtons.OK;

            return template.InterruptAsync();
        }

        public static void Interrupt(this NotifierInformation info)
        { Dispatch(() => InterruptCore(info)); }

        public static void Interrupt(this Panel parentPanel, string message, Action<int> callback, NotifierInformation template = null, NotificationButtons buttons = null, string title = "")
        {
            template = template ?? NotificationTemplates.Information;

            template.Message = message;
            template.Title = title;
            template.ParentPanel = parentPanel;
            template.Buttons = buttons ?? NotificationButtons.OK;

            template.Completed += (o, e) => callback(e);

            Dispatch(() => InterruptCore(template));
        }

        public static void Ask(this Panel parentPanel, string message, Action<int> callback, string title = "")
        {
            var info = new NotifierInformation
            {
                Message = message,
                Title = title,
                ParentPanel = parentPanel,
                Buttons = NotificationButtons.YesNo
            };

            info.Interrupt();
        }

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

        public static Task<int> AskAsync(this Panel parentPanel, string message, string title = "")
        {
            var info = new NotifierInformation
            {
                Message = message,
                Title = title,
                ParentPanel = parentPanel,
                Buttons = NotificationButtons.YesNo
            };

            return info.InterruptAsync();
        }
    }
}
