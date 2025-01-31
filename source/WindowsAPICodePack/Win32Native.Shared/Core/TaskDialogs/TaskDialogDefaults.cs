//Copyright (c) Microsoft Corporation.  All rights reserved.  Distributed under the Microsoft Public License (MS-PL)

using Microsoft.WindowsAPICodePack.Win32Native.Resources;

namespace Microsoft.WindowsAPICodePack.Win32Native.Dialogs
{
    public static class TaskDialogDefaults
    {
        public static string Caption => LocalizedMessages.TaskDialogDefaultCaption;
        public static string MainInstruction => LocalizedMessages.TaskDialogDefaultMainInstruction;
        public static string Content => LocalizedMessages.TaskDialogDefaultContent;

        public const int ProgressBarMinimumValue = 0;
        public const int ProgressBarMaximumValue = 100;

        public const int IdealWidth = 0;

        // For generating control ID numbers that won't 
        // collide with the standard button return IDs.
        public const int MinimumDialogControlId =
            (int)TaskDialog.TaskDialogCommonButtonReturnIds.Close + 1;
    }
}
