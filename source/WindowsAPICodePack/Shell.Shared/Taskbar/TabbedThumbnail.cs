﻿//Copyright (c) Microsoft Corporation.  All rights reserved.  Distributed under the Microsoft Public License (MS-PL)

using Microsoft.WindowsAPICodePack.Shell.Resources;
using Microsoft.WindowsAPICodePack.Win32Native;
using Microsoft.WindowsAPICodePack.Win32Native.GDI;
using Microsoft.WindowsAPICodePack.Win32Native.Shell.DesktopWindowManager;

using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Microsoft.WindowsAPICodePack.Taskbar
{
    /// <summary>
    /// Represents a tabbed thumbnail on the taskbar for a given window or a control.
    /// </summary>
    public class TabbedThumbnail : IDisposable
    {
        private bool _addedToTaskbar;
        private string _title = string.Empty;
        private string _tooltip = string.Empty;
        private Rectangle? _clippingRectangle;

        #region Internal members
        // Control properties
        internal IntPtr WindowHandle { get; set; }

        internal IntPtr ParentWindowHandle { get; set; }

        // WPF properties
        internal UIElement WindowsControl { get; set; }

        internal Window WindowsControlParentWindow { get; set; }

        private TaskbarWindow _taskbarWindow;

        internal TaskbarWindow TaskbarWindow
        {
            get => _taskbarWindow;

            set
            {
                _taskbarWindow = value;

                // If we have a TaskbarWindow assigned, set it's icon
                if (_taskbarWindow != null && _taskbarWindow.TabbedThumbnailProxyWindow != null)

                    _taskbarWindow.TabbedThumbnailProxyWindow.Icon = Icon;
            }
        }

        internal bool AddedToTaskbar
        {
            get => _addedToTaskbar;

            set
            {
                _addedToTaskbar = value;

                // The user has updated the clipping region, so invalidate our existing preview
                if (ClippingRectangle != null)

                    TaskbarWindowManager.InvalidatePreview(TaskbarWindow);
            }
        }

        internal bool RemovedFromTaskbar { get; set; }
        #endregion

        #region Public Properties
        /// <summary>
        /// Title for the window shown as the taskbar thumbnail.
        /// </summary>
        public string Title
        {
            get => _title;

            set
            {
                if (_title != value)
                {
                    _title = value;

                    TitleChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Tooltip to be shown for this thumbnail on the taskbar. 
        /// By default this is full title of the window shown on the taskbar.
        /// </summary>
        public string Tooltip
        {
            get => _tooltip;

            set
            {
                if (_tooltip != value)
                {
                    _tooltip = value;
                    TooltipChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Sets the window icon for this thumbnail preview
        /// </summary>
        /// <param name="icon">System.Drawing.Icon for the window/control associated with this preview</param>
        public void SetWindowIcon(Icon icon)
        {
            Icon = icon;

            // If we have a TaskbarWindow assigned, set its icon
            if (TaskbarWindow?.TabbedThumbnailProxyWindow != null)

                TaskbarWindow.TabbedThumbnailProxyWindow.Icon = Icon;
        }

        /// <summary>
        /// Sets the window icon for this thumbnail preview
        /// </summary>
        /// <param name="iconHandle">Icon handle (hIcon) for the window/control associated with this preview</param>
        /// <remarks>This method will not release the icon handle. It is the caller's responsibility to release the icon handle.</remarks>
        public void SetWindowIcon(IntPtr iconHandle)
        {
            Icon = iconHandle != IntPtr.Zero ? Icon.FromHandle(iconHandle) : null;

            if (TaskbarWindow?.TabbedThumbnailProxyWindow != null)

                TaskbarWindow.TabbedThumbnailProxyWindow.Icon = Icon;
        }

        /// <summary>
        /// Specifies that only a portion of the window's client area
        /// should be used in the window's thumbnail.
        /// <para>A value of null will clear the clipping area and use the default thumbnail.</para>
        /// </summary>
        public Rectangle? ClippingRectangle
        {
            get => _clippingRectangle;

            set
            {
                _clippingRectangle = value;

                // The user has updated the clipping region, so invalidate our existing preview
                TaskbarWindowManager.InvalidatePreview(TaskbarWindow);
            }
        }

        internal IntPtr CurrentHBitmap { get; set; }

        internal Icon Icon { get; private set; }

        /// <summary>
        /// Override the thumbnail and peek bitmap. 
        /// By providing this bitmap manually, Thumbnail Window manager will provide the 
        /// Desktop Window Manager (DWM) this bitmap instead of rendering one automatically.
        /// Use this property to update the bitmap whenever the control is updated and the user
        /// needs to be shown a new thumbnail on the taskbar preview (or aero peek).
        /// </summary>
        /// <param name="bitmap">The image to use.</param>
        /// <remarks>
        /// If the bitmap doesn't have the right dimensions, the DWM may scale it or not 
        /// render certain areas as appropriate - it is the user's responsibility
        /// to render a bitmap with the proper dimensions.
        /// </remarks>
        public void SetImage(Bitmap bitmap) => SetImage(bitmap == null ? IntPtr.Zero : bitmap.GetHbitmap());

        /// <summary>
        /// Override the thumbnail and peek bitmap. 
        /// By providing this bitmap manually, Thumbnail Window manager will provide the 
        /// Desktop Window Manager (DWM) this bitmap instead of rendering one automatically.
        /// Use this property to update the bitmap whenever the control is updated and the user
        /// needs to be shown a new thumbnail on the taskbar preview (or aero peek).
        /// </summary>
        /// <param name="bitmapSource">The image to use.</param>
        /// <remarks>
        /// If the bitmap doesn't have the right dimensions, the DWM may scale it or not 
        /// render certain areas as appropriate - it is the user's responsibility
        /// to render a bitmap with the proper dimensions.
        /// </remarks>
        public void SetImage(BitmapSource bitmapSource)
        {
            if (bitmapSource == null)
            {
                SetImage(IntPtr.Zero);

                return;
            }

            var encoder = new BmpBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

#if CS8
            using var memoryStream = new MemoryStream();
            encoder.Save(memoryStream);
            memoryStream.Position = 0;

            using var bmp = new Bitmap(memoryStream);
            SetImage(bmp.GetHbitmap());
#else
            using (MemoryStream memoryStream = new MemoryStream())
            {
                encoder.Save(memoryStream);
                memoryStream.Position = 0;

                using (Bitmap bmp = new Bitmap(memoryStream))

                    SetImage(bmp.GetHbitmap());
            }
#endif
        }

        /// <summary>
        /// Override the thumbnail and peek bitmap. 
        /// By providing this bitmap manually, Thumbnail Window manager will provide the 
        /// Desktop Window Manager (DWM) this bitmap instead of rendering one automatically.
        /// Use this property to update the bitmap whenever the control is updated and the user
        /// needs to be shown a new thumbnail on the taskbar preview (or aero peek).
        /// </summary>
        /// <param name="hBitmap">A bitmap handle for the image to use.
        /// <para>When the TabbedThumbnail is finalized, this class will delete the provided hBitmap.</para></param>
        /// <remarks>
        /// If the bitmap doesn't have the right dimensions, the DWM may scale it or not 
        /// render certain areas as appropriate - it is the user's responsibility
        /// to render a bitmap with the proper dimensions.
        /// </remarks>
        internal void SetImage(IntPtr hBitmap)
        {
            // Before we set a new bitmap, dispose the old one
            if (CurrentHBitmap != IntPtr.Zero)

                _ = GDI.DeleteObject(CurrentHBitmap);

            // Set the new bitmap
            CurrentHBitmap = hBitmap;

            // Let DWM know to invalidate its cached thumbnail/preview and ask us for a new one            
            TaskbarWindowManager.InvalidatePreview(TaskbarWindow);
        }

        /// <summary>
        /// Specifies whether a standard window frame will be displayed
        /// around the bitmap.  If the bitmap represents a top-level window,
        /// you would probably set this flag to <b>true</b>.  If the bitmap
        /// represents a child window (or a frameless window), you would
        /// probably set this flag to <b>false</b>.
        /// </summary>
        public bool DisplayFrameAroundBitmap { get; set; }

        /// <summary>
        /// Invalidate any existing thumbnail preview. Calling this method
        /// will force DWM to request a new bitmap next time user previews the thumbnails
        /// or requests Aero peek preview.
        /// </summary>
        public void InvalidatePreview() =>
            // clear current image and invalidate
            SetImage(IntPtr.Zero);

        /// <summary>
        /// Gets or sets the offset used for displaying the peek bitmap. This setting is
        /// recomended for hidden WPF controls as it is difficult to calculate their offset.
        /// </summary>
        public Vector? PeekOffset { get; set; }
        #endregion

        #region Events
        /// <summary>
        /// This event is raised when the Title property changes.
        /// </summary>
        public event EventHandler TitleChanged;

        /// <summary>
        /// This event is raised when the Tooltip property changes.
        /// </summary>
        public event EventHandler TooltipChanged;

        /// <summary>
        /// The event that occurs when a tab is closed on the taskbar thumbnail preview.
        /// </summary>
        public event EventHandler<TabbedThumbnailClosedEventArgs> TabbedThumbnailClosed;

        /// <summary>
        /// The event that occurs when a tab is maximized via the taskbar thumbnail preview (context menu).
        /// </summary>
        public event EventHandler<TabbedThumbnailEventArgs> TabbedThumbnailMaximized;

        /// <summary>
        /// The event that occurs when a tab is minimized via the taskbar thumbnail preview (context menu).
        /// </summary>
        public event EventHandler<TabbedThumbnailEventArgs> TabbedThumbnailMinimized;

        /// <summary>
        /// The event that occurs when a tab is activated (clicked) on the taskbar thumbnail preview.
        /// </summary>
        public event EventHandler<TabbedThumbnailEventArgs> TabbedThumbnailActivated;

        /// <summary>
        /// The event that occurs when a thumbnail or peek bitmap is requested by the user.
        /// </summary>
        public event EventHandler<TabbedThumbnailBitmapRequestedEventArgs> TabbedThumbnailBitmapRequested;

        internal void OnTabbedThumbnailMaximized()
        {
            if (TabbedThumbnailMaximized == null)

                // No one is listening to these events.
                // Forward the message to the main window
                _ = Core.SendMessage(ParentWindowHandle, WindowMessage.SystemCommand, new IntPtr((int)SystemMenuCommands.Maximize), IntPtr.Zero);

            else

                TabbedThumbnailMaximized(this, GetTabbedThumbnailEventArgs());
        }

        internal void OnTabbedThumbnailMinimized()
        {
            if (TabbedThumbnailMinimized == null)

                // No one is listening to these events.
                // Forward the message to the main window
                _ = Core.SendMessage(ParentWindowHandle, WindowMessage.SystemCommand, new IntPtr((int)SystemMenuCommands.Minimize), IntPtr.Zero);

            else

                TabbedThumbnailMinimized(this, GetTabbedThumbnailEventArgs());
        }

        /// <summary>
        /// Returns true if the thumbnail was removed from the taskbar; false if it was not.
        /// </summary>
        /// <returns>Returns true if the thumbnail was removed from the taskbar; false if it was not.</returns>
        internal bool OnTabbedThumbnailClosed()
        {
            EventHandler<TabbedThumbnailClosedEventArgs> closedHandler = TabbedThumbnailClosed;

            if (closedHandler == null)

                // No one is listening to these events. Forward the message to the main window
                _ = Core.SendMessage(ParentWindowHandle, WindowMessage.NCDestroy, IntPtr.Zero, IntPtr.Zero);

            else
            {
                TabbedThumbnailClosedEventArgs closingEvent = GetTabbedThumbnailClosingEventArgs();

                closedHandler(this, closingEvent);

                if (closingEvent.Cancel) return false;
            }

            // Remove it from the internal list as well as the taskbar
            TaskbarManager.Instance.TabbedThumbnail.RemoveThumbnailPreview(this);
            return true;
        }

        internal void OnTabbedThumbnailActivated()
        {
            if (TabbedThumbnailActivated == null)

                // No one is listening to these events.
                // Forward the message to the main window
                _ = Core.SendMessage(ParentWindowHandle, WindowMessage.ActivateApplication, new IntPtr(1), new IntPtr(Thread.CurrentThread.GetHashCode()));

            else

                TabbedThumbnailActivated(this, GetTabbedThumbnailEventArgs());
        }

        internal void OnTabbedThumbnailBitmapRequested()
        {
            if (TabbedThumbnailBitmapRequested != null)
            {
                TabbedThumbnailBitmapRequestedEventArgs eventArgs = null;

                if (WindowHandle != IntPtr.Zero)

                    eventArgs = new TabbedThumbnailBitmapRequestedEventArgs(WindowHandle);

                else if (WindowsControl != null)

                    eventArgs = new TabbedThumbnailBitmapRequestedEventArgs(WindowsControl);

                TabbedThumbnailBitmapRequested(this, eventArgs);
            }
        }

        private TabbedThumbnailClosedEventArgs GetTabbedThumbnailClosingEventArgs()
        {
            TabbedThumbnailClosedEventArgs eventArgs = null;

            if (WindowHandle != IntPtr.Zero)

                eventArgs = new TabbedThumbnailClosedEventArgs(WindowHandle);

            else if (WindowsControl != null)

                eventArgs = new TabbedThumbnailClosedEventArgs(WindowsControl);

            return eventArgs;
        }

        private TabbedThumbnailEventArgs GetTabbedThumbnailEventArgs()
        {
            TabbedThumbnailEventArgs eventArgs = null;

            if (WindowHandle != IntPtr.Zero)

                eventArgs = new TabbedThumbnailEventArgs(WindowHandle);

            else if (WindowsControl != null)

                eventArgs = new TabbedThumbnailEventArgs(WindowsControl);

            return eventArgs;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new TabbedThumbnail with the given window handle of the parent and
        /// a child control/window's handle (e.g. TabPage or Panel)
        /// </summary>
        /// <param name="parentWindowHandle">Window handle of the parent window. 
        /// This window has to be a top-level window and the handle cannot be null or IntPtr.Zero</param>
        /// <param name="windowHandle">Window handle of the child control or window for which a tabbed 
        /// thumbnail needs to be displayed</param>
        public TabbedThumbnail(IntPtr parentWindowHandle, IntPtr windowHandle)
        {
            if (parentWindowHandle == IntPtr.Zero)

                throw new ArgumentException(LocalizedMessages.TabbedThumbnailZeroParentHandle, nameof(parentWindowHandle));

            if (windowHandle == IntPtr.Zero)

                throw new ArgumentException(LocalizedMessages.TabbedThumbnailZeroChildHandle, nameof(windowHandle));

            WindowHandle = windowHandle;

            ParentWindowHandle = parentWindowHandle;
        }

        /// <summary>
        /// Creates a new TabbedThumbnail with the given window handle of the parent and
        /// a child control (e.g. TabPage or Panel)
        /// </summary>
        /// <param name="parentWindowHandle">Window handle of the parent window. 
        /// This window has to be a top-level window and the handle cannot be null or IntPtr.Zero</param>
        /// <param name="control">Child control for which a tabbed thumbnail needs to be displayed</param>
        /// <remarks>This method can also be called when using a WindowsFormHost control in a WPF application.
        ///  Call this method with the main WPF Window's handle, and windowsFormHost.Child control.</remarks>
        public TabbedThumbnail(IntPtr parentWindowHandle, Control control)
        {
            if (parentWindowHandle == IntPtr.Zero)

                throw new ArgumentException(LocalizedMessages.TabbedThumbnailZeroParentHandle, nameof(parentWindowHandle));

            if (control == null)

                throw new ArgumentNullException(nameof(control));

            WindowHandle = control.Handle;

            ParentWindowHandle = parentWindowHandle;
        }

        /// <summary>
        /// Creates a new TabbedThumbnail with the given window handle of the parent and
        /// a WPF child Window. For WindowsFormHost control, use TabbedThumbnail(IntPtr, Control) overload and pass
        /// the WindowsFormHost.Child as the second parameter.
        /// </summary>
        /// <param name="parentWindow">Parent window for the UIElement control. 
        /// This window has to be a top-level window and the handle cannot be null</param>
        /// <param name="windowsControl">WPF Control (UIElement) for which a tabbed thumbnail needs to be displayed</param>
        /// <param name="peekOffset">Offset point used for displaying the peek bitmap. This setting is
        /// recomended for hidden WPF controls as it is difficult to calculate their offset.</param>
        public TabbedThumbnail(Window parentWindow, UIElement windowsControl, Vector peekOffset)
        {
            WindowHandle = IntPtr.Zero;

            WindowsControl = windowsControl ?? throw new ArgumentNullException(nameof(windowsControl));
            WindowsControlParentWindow = parentWindow ?? throw new ArgumentNullException(nameof(parentWindow));
            ParentWindowHandle = new WindowInteropHelper(parentWindow).Handle;
            PeekOffset = peekOffset;
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// 
        /// </summary>
        ~TabbedThumbnail() => Dispose(false);

        /// <summary>
        /// Release the native objects.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Release the native objects.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _taskbarWindow = null;

                if (Icon != null)
                {
                    Icon.Dispose();
                    Icon = null;
                }

                _title = null;
                _tooltip = null;
                WindowsControl = null;
            }

            if (CurrentHBitmap != IntPtr.Zero)
            {
                _ = GDI.DeleteObject(CurrentHBitmap);

                CurrentHBitmap = IntPtr.Zero;
            }
        }
        #endregion
    }
}
