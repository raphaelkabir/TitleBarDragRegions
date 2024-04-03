using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.Graphics;
using WinRT.Interop;

namespace TitleBarDrag
{
    public class DragRegions
    {
        private AppWindow AppWindow;
        private FrameworkElement TitleBar;
        private bool Extended { get { return AppWindow.TitleBar.ExtendsContentIntoTitleBar; } }
        private FrameworkElement[] nonDragElements;
        public FrameworkElement[] NonDragElements 
        { 
            get 
            {
                return nonDragElements;
            } 
            set 
            {
                nonDragElements = value;
                UpdateTitleBarDragRegions();
            } 
        }

        public DragRegions(Window mainWindow, FrameworkElement titleBar)
        {
            AppWindow = GetAppWindow(mainWindow);
            TitleBar = titleBar;

            AppWindow.Changed += AppWindow_Changed;
            TitleBar.Loaded += TitleBar_Loaded;
            TitleBar.SizeChanged += TitleBar_SizeChanged;
        }

        ~DragRegions() 
        {
            AppWindow.Changed -= AppWindow_Changed;
            TitleBar.Loaded -= TitleBar_Loaded;
            TitleBar.SizeChanged -= TitleBar_SizeChanged;
        }

        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
        {
            if (!args.DidPresenterChange)
                return;

            switch (sender.Presenter.Kind)
            {
                case AppWindowPresenterKind.CompactOverlay:
                    TitleBar.Visibility = Visibility.Collapsed;
                    sender.TitleBar.ResetToDefault();
                    break;

                case AppWindowPresenterKind.FullScreen:
                    TitleBar.Visibility = Visibility.Collapsed;
                    sender.TitleBar.ExtendsContentIntoTitleBar = true;
                    break;

                case AppWindowPresenterKind.Overlapped:
                    TitleBar.Visibility = Visibility.Visible;
                    sender.TitleBar.ExtendsContentIntoTitleBar = true;
                    break;

                default:
                    sender.TitleBar.ResetToDefault();
                    break;
            }
        }

        private void TitleBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Extended) UpdateTitleBarDragRegions();
        }

        private void TitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            if (Extended) UpdateTitleBarDragRegions();
        }

        private static AppWindow GetAppWindow(object mainWindow)
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(mainWindow);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(wndId);
        }

        private void UpdateTitleBarDragRegions()
        {
            var rects = new List<RectInt32>();
            var xamlRoot = TitleBar.XamlRoot;
            if (xamlRoot == null) 
                return;

            double scaleAdjustment = xamlRoot.RasterizationScale;

            foreach (FrameworkElement item in NonDragElements)
            {
                GeneralTransform transform = item.TransformToVisual(null);
                Rect bounds = transform.TransformBounds(
                    new Rect(0, 0, item.ActualWidth, item.ActualHeight));
                rects.Add(GetRect(bounds, scaleAdjustment));
            }

            var rectArray = rects.ToArray();
            InputNonClientPointerSource nonClientInputSrc
                = InputNonClientPointerSource.GetForWindowId(AppWindow.Id);
            nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, rectArray);
        }

        private RectInt32 GetRect(Rect bounds, double scale)
        {
            return new RectInt32(
                _X: (int)Math.Round(bounds.X * scale),
                _Y: (int)Math.Round(bounds.Y * scale),
                _Width: (int)Math.Round(bounds.Width * scale),
                _Height: (int)Math.Round(bounds.Height * scale)
            );
        }
    }
}
