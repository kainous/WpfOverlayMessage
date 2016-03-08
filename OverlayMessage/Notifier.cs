using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Windows.Controls
{
    public static class Notifier
    {
        private static void InterruptCore(Panel parentPanel, string message, Action callback = null)
        {
            var display = new OverlayMessage(parentPanel);
            display.Content = message;

            if (callback != null)
            { display.Closed += (o, e) => callback?.Invoke(); }

            parentPanel.Children.Add(display);
            int zIndex = parentPanel.Children
                                    .OfType<UIElement>()
                                    .Max(Panel.GetZIndex) + 1;

            Panel.SetZIndex(display, zIndex);
        }

        public static async Task InterruptAsync(this Panel parentPanel, string message)
        {
            await Application.Current.Dispatcher.InvokeAsync(() => InterruptCore(parentPanel, message));
        }

        public static void InterruptAsync(this Panel parentPanel, string message, Action callback)
        { Application.Current.Dispatcher.Invoke(() => InterruptCore(parentPanel, message, callback)); }

        public static void Ask(this Panel parentPanel, string message, Action<int> callback)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var display = new OverlayMessage(parentPanel);

                display.Content = message;

                display.Closed += (o, e) => callback?.Invoke(e.Data);
                parentPanel.Children.Add(display);
                int zIndex = parentPanel.Children
                                        .OfType<UIElement>()
                                        .Max(Panel.GetZIndex) + 1;

                Panel.SetZIndex(display, zIndex);
            });
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

        public static async Task<int> AskAsync(this Panel parentPanel, string message)
        {
            int result = -1;
            var display = new OverlayMessage(parentPanel);
            display.Content = message;

            using (var semaphore = new SemaphoreSlim(0, 1))
            {
                display.Closed += (o, e) =>
                {
                    result = e.Data;
                    semaphore.Release();
                };
                parentPanel.Children.Add(display);
                int zIndex = parentPanel.Children
                                        .OfType<UIElement>()
                                        .Max(Panel.GetZIndex) + 1;

                Panel.SetZIndex(display, zIndex);
                await semaphore.WaitAsync();
            }
            return result;
        }
    }
}
