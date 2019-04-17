#if UNITY_STANDALONE_LINUX || UNITY_EDITOR
using System;
using UnityEngine;

namespace Crosstales.FB.Wrapper
{
    /// <summary>File browser implementation for Linux (GTK).</summary>
    public class FileBrowserLinux : FileBrowserBase
    {
        #region Variables

        private static Action<string[]> _openFileCb;
        private static Action<string[]> _openFolderCb;
        private static Action<string> _saveFileCb;

        private const char splitChar = (char)28;


        #endregion


        #region Constructor

        public FileBrowserLinux()
        {
            Linux.NativeMethods.DialogInit();
        }

        #endregion


        #region Implemented methods

        public override bool canOpenMultipleFiles
        {
            get
            {
                return true;
            }
        }

        public override bool canOpenMultipleFolders
        {
            get
            {
                return true;
            }
        }

        public override string[] OpenFiles(string title, string directory, ExtensionFilter[] extensions, bool multiselect)
        {
            string paths = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(Linux.NativeMethods.DialogOpenFilePanel(title, directory, getFilterFromFileExtensionList(extensions), multiselect));
            return paths.Split(splitChar);
        }

        public override string[] OpenFolders(string title, string directory, bool multiselect)
        {
            string paths = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(Linux.NativeMethods.DialogOpenFolderPanel(title, directory, multiselect));
            return paths.Split(splitChar);
        }

        public override string SaveFile(string title, string directory, string defaultName, ExtensionFilter[] extensions)
        {
            return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(Linux.NativeMethods.DialogSaveFilePanel(title, directory, defaultName, getFilterFromFileExtensionList(extensions)));
        }

        public override void OpenFilesAsync(string title, string directory, ExtensionFilter[] extensions, bool multiselect, Action<string[]> cb)
        {
            _openFileCb = cb;
            Linux.NativeMethods.DialogOpenFilePanelAsync(
                title,
                directory,
                getFilterFromFileExtensionList(extensions),
                multiselect,
                (string result) => { _openFileCb.Invoke(result.Split(splitChar)); });
        }

        public override void OpenFoldersAsync(string title, string directory, bool multiselect, Action<string[]> cb)
        {
            _openFolderCb = cb;
            Linux.NativeMethods.DialogOpenFolderPanelAsync(
                title,
                directory,
                multiselect,
                (string result) => { _openFolderCb.Invoke(result.Split(splitChar)); });
        }

        public override void SaveFileAsync(string title, string directory, string defaultName, ExtensionFilter[] extensions, Action<string> cb)
        {
            _saveFileCb = cb;
            Linux.NativeMethods.DialogSaveFilePanelAsync(
                title,
                directory,
                defaultName,
                getFilterFromFileExtensionList(extensions),
                (string result) => { _saveFileCb.Invoke(result); });
        }

        #endregion


        #region Private methods

        private static string getFilterFromFileExtensionList(ExtensionFilter[] extensions)
        {
            if (extensions != null && extensions.Length > 0)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                ExtensionFilter filter;

                for (int xx = 0; xx < extensions.Length; xx++)
                {
                    filter = extensions[xx];

                    sb.Append(filter.Name);
                    sb.Append(";");

                    for (int ii = 0; ii < filter.Extensions.Length; ii++)
                    {
                        sb.Append(filter.Extensions[ii]);

                        if (ii + 1 < filter.Extensions.Length)
                            sb.Append(",");
                    }

                    if (xx + 1 < extensions.Length)
                        sb.Append("|");
                }

                if (Util.Config.DEBUG)
                    Debug.Log("getFilterFromFileExtensionList: " + sb.ToString());

                return sb.ToString();
            }

            return string.Empty;
        }

        #endregion
    }
}

namespace Crosstales.FB.Wrapper.Linux
{
    /// <summary>Native methods (bridge to Linux).</summary>
    internal static class NativeMethods
    {
        [System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall)]
        public delegate void AsyncCallback(string path);

        [System.Runtime.InteropServices.DllImport("StandaloneFileBrowser")]
        internal static extern void DialogInit();

        [System.Runtime.InteropServices.DllImport("StandaloneFileBrowser")]
        internal static extern IntPtr DialogOpenFilePanel(string title, string directory, string extension, bool multiselect);

        [System.Runtime.InteropServices.DllImport("StandaloneFileBrowser")]
        internal static extern IntPtr DialogOpenFolderPanel(string title, string directory, bool multiselect);

        [System.Runtime.InteropServices.DllImport("StandaloneFileBrowser")]
        internal static extern IntPtr DialogSaveFilePanel(string title, string directory, string defaultName, string extension);

        [System.Runtime.InteropServices.DllImport("StandaloneFileBrowser")]
        internal static extern void DialogOpenFilePanelAsync(string title, string directory, string extension, bool multiselect, AsyncCallback callback);

        [System.Runtime.InteropServices.DllImport("StandaloneFileBrowser")]
        internal static extern void DialogOpenFolderPanelAsync(string title, string directory, bool multiselect, AsyncCallback callback);

        [System.Runtime.InteropServices.DllImport("StandaloneFileBrowser")]
        internal static extern void DialogSaveFilePanelAsync(string title, string directory, string defaultName, string extension, AsyncCallback callback);
    }
}
#endif
// © 2019 crosstales LLC (https://www.crosstales.com)