﻿//Copyright (c) Microsoft Corporation.  All rights reserved.  Distributed under the Microsoft Public License (MS-PL)

using System.IO;
using Microsoft.WindowsAPICodePack.COMNative.Shell;
using Microsoft.WindowsAPICodePack.Win32Native.Shell;
using Microsoft.WindowsAPICodePack.Win32Native.Shell.Resources;

namespace Microsoft.WindowsAPICodePack.Shell
{
    /// <summary>
    /// A folder in the Shell Namespace
    /// </summary>
    public class ShellFileSystemFolder : ShellFolder
    {
        #region Internal Constructor
        internal ShellFileSystemFolder() { /* Left empty. */ }

        internal ShellFileSystemFolder(IShellItem2 shellItem) => nativeShellItem = shellItem;
        #endregion

        #region Public Methods
        /// <summary>
        /// Constructs a new ShellFileSystemFolder object given a folder path
        /// </summary>
        /// <param name="path">The folder path</param>
        /// <remarks>ShellFileSystemFolder created from the given folder path.</remarks>
        public static ShellFileSystemFolder FromFolderPath(string path)
        {
            // Get the absolute path
            string absPath = WinCopies.Util.IO.Path.GetAbsolutePath(path);

            // Make sure this is valid
            if (!Directory.Exists(absPath))

                throw new DirectoryNotFoundException(
                    string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    LocalizedMessages.FilePathNotExist, path));

            var folder = new ShellFileSystemFolder();

            try
            {
                folder.ParsingName = absPath;

                return folder;
            }

            catch
            {
                folder.Dispose();

                throw;
            }
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// The path for this Folder
        /// </summary>
        public virtual string Path => ParsingName;
        #endregion
    }
}