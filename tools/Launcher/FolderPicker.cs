// GPL-3.0. Windows common-item folder dialog: address bar, search and Quick Access.
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Marmo.Launcher
{
    public static class FolderPicker
    {
        public static string Choose(IWin32Window owner, string title, string initial)
        {
            object native = null; IShellItem start = null, result = null;
            try
            {
                native = new FileOpenDialog(); var dialog = (IFileDialog)native;
                uint options; dialog.GetOptions(out options);
                dialog.SetOptions(options | 0x20u | 0x40u | 0x800u | 0x8u);
                dialog.SetTitle(title); dialog.SetOkButtonLabel("Select folder");
                while (!String.IsNullOrEmpty(initial) && !Directory.Exists(initial)) initial = Path.GetDirectoryName(initial);
                if (!String.IsNullOrEmpty(initial))
                {
                    var iid = typeof(IShellItem).GUID;
                    SHCreateItemFromParsingName(initial, IntPtr.Zero, ref iid, out start);
                    dialog.SetFolder(start);
                }
                int hr = dialog.Show(owner.Handle);
                if (hr == unchecked((int)0x800704C7)) return null;
                Marshal.ThrowExceptionForHR(hr); dialog.GetResult(out result);
                IntPtr value; result.GetDisplayName(0x80058000, out value);
                try { return Marshal.PtrToStringUni(value); } finally { Marshal.FreeCoTaskMem(value); }
            }
            catch (COMException)
            {
                using (var fallback = new FolderBrowserDialog { Description=title, SelectedPath=initial ?? "", ShowNewFolderButton=true })
                    return fallback.ShowDialog(owner)==DialogResult.OK ? fallback.SelectedPath : null;
            }
            finally
            {
                if (result != null) Marshal.ReleaseComObject(result);
                if (start != null) Marshal.ReleaseComObject(start);
                if (native != null) Marshal.ReleaseComObject(native);
            }
        }
        [DllImport("shell32.dll", CharSet=CharSet.Unicode, PreserveSig=false)]
        static extern void SHCreateItemFromParsingName(string path, IntPtr binding, ref Guid iid, [MarshalAs(UnmanagedType.Interface)] out IShellItem item);
        [ComImport, Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7")] class FileOpenDialog { }
        [ComImport, Guid("42F85136-DB7E-439C-85F1-E4075D135FC8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IFileDialog
        {
            [PreserveSig] int Show(IntPtr parent);
            void SetFileTypes(uint count, IntPtr types); void SetFileTypeIndex(uint index); void GetFileTypeIndex(out uint index);
            void Advise(IntPtr events, out uint cookie); void Unadvise(uint cookie);
            void SetOptions(uint options); void GetOptions(out uint options);
            void SetDefaultFolder(IShellItem item); void SetFolder(IShellItem item); void GetFolder(out IShellItem item);
            void GetCurrentSelection(out IShellItem item);
            void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string name); void GetFileName(out IntPtr name);
            void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string title);
            void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string text);
            void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string text);
            void GetResult(out IShellItem item); void AddPlace(IShellItem item, uint alignment);
            void SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string extension); void Close(int result);
            void SetClientGuid(ref Guid guid); void ClearClientData(); void SetFilter(IntPtr filter);
        }
        [ComImport, Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IShellItem
        {
            void BindToHandler(IntPtr binding, ref Guid handler, ref Guid iid, out IntPtr value);
            void GetParent(out IShellItem item); void GetDisplayName(uint kind, out IntPtr name);
            void GetAttributes(uint mask, out uint attributes); void Compare(IShellItem other, uint hint, out int order);
        }
    }
}
