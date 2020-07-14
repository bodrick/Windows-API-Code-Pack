﻿//Copyright (c) Microsoft Corporation.  All rights reserved.  Distributed under the Microsoft Public License (MS-PL)

using System;
using System.Runtime.InteropServices;
using Microsoft.WindowsAPICodePack.Controls;
using Microsoft.WindowsAPICodePack.Controls.WindowsForms;
using Microsoft.WindowsAPICodePack.Win32Native.Controls;
using Microsoft.WindowsAPICodePack.Win32Native;
using static Microsoft.WindowsAPICodePack.NativeAPI.Consts.Controls.ExplorerBrowserViewDispatchIds;
using Microsoft.WindowsAPICodePack.COMNative.Controls;

namespace Microsoft.WindowsAPICodePack.Internal
{
    /// <summary>
    /// This provides a connection point container compatible dispatch interface for
    /// hooking into the ExplorerBrowser view.
    /// </summary>    
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    internal class ExplorerBrowserViewEvents : IDisposable
    {
        #region implementation
        private uint viewConnectionPointCookie;
        private object viewDispatch;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
        private IntPtr nullPtr = IntPtr.Zero;

        private Guid IID_DShellFolderViewEvents = new Guid(NativeAPI.Guids.Shell.ExplorerBrowser.DShellFolderViewEvents);
        private Guid IID_IDispatch = new Guid(NativeAPI.Guids.Shell.ExplorerBrowser.IDispatch);
        private readonly ExplorerBrowser parent;
        #endregion

        #region contstruction
        /// <summary>
        /// Default constructor for ExplorerBrowserViewEvents
        /// </summary>
        public ExplorerBrowserViewEvents() : this(null) { }

        internal ExplorerBrowserViewEvents(ExplorerBrowser parent)
        {
            this.parent = parent;
        }
        #endregion

        #region operations
        internal void ConnectToView(IShellView psv)
        {
            DisconnectFromView();

            HResult hr = psv.GetItemObject(
                ShellViewGetItemObject.Background,
                ref IID_IDispatch,
                out viewDispatch);

            if (hr == HResult.Ok)
            {
                hr = ExplorerBrowserNativeMethods.ConnectToConnectionPoint(
                    this,
                    ref IID_DShellFolderViewEvents,
                    true,
                    viewDispatch,
                    ref viewConnectionPointCookie,
                    ref nullPtr);

                if (hr != HResult.Ok)

                    _ = Marshal.ReleaseComObject(viewDispatch);
            }
        }

        internal void DisconnectFromView()
        {
            if (viewDispatch != null)
            {
                ExplorerBrowserNativeMethods.ConnectToConnectionPoint(
                    IntPtr.Zero,
                    ref IID_DShellFolderViewEvents,
                    false,
                    viewDispatch,
                    ref viewConnectionPointCookie,
                    ref nullPtr);

                _ = Marshal.ReleaseComObject(viewDispatch);
                viewDispatch = null;
                viewConnectionPointCookie = 0;
            }
        }
        #endregion

        #region IDispatch events
        // These need to be public to be accessible via AutoDual reflection

        /// <summary>
        /// The view selection has changed
        /// </summary>
        [DispId(SelectionChanged)]
        public void ViewSelectionChanged() => parent.FireSelectionChanged();

        /// <summary>
        /// The contents of the view have changed
        /// </summary>
        [DispId(ContentsChanged)]
        public void ViewContentsChanged() => parent.FireContentChanged();

        /// <summary>
        /// The enumeration of files in the view is complete
        /// </summary>
        [DispId(FileListEnumDone)]
        public void ViewFileListEnumDone() => parent.FireContentEnumerationComplete();

        /// <summary>
        /// The selected item in the view has changed (not the same as the selection has changed)
        /// </summary>
        [DispId(SelectedItemChanged)]
        public void ViewSelectedItemChanged() => parent.FireSelectedItemChanged();
        #endregion

        /// <summary>
        /// Finalizer for ExplorerBrowserViewEvents
        /// </summary>
        ~ExplorerBrowserViewEvents()
        {
            Dispose(false);
        }

        #region IDisposable Members

        /// <summary>
        /// Disconnects and disposes object.
        /// </summary>        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disconnects and disposes object.
        /// </summary>
        /// <param name="disposed"></param>
        protected virtual void Dispose(bool disposed)
        {
            if (disposed)
            
                DisconnectFromView();
                    }

        #endregion
    }
}
