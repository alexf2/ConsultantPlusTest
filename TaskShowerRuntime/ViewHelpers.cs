using ConsPlus.TaskShowerModel;
using System;
using System.Windows.Forms;

namespace ConsPlus.TaskShowerRuntime
{
    static class ViewHelpers
    {
        internal static void execWithWaitCursor(Action act)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                act();
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        internal static string correctPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return ".";

            return path.EndsWith(":") ? path += "\\" : path;
        }

        internal static string makeDescription(FileSysItem item)
        {
            string s = item.DisplayName;
            if (s != null)
                s = item.DisplayName.Trim();

            return string.IsNullOrEmpty(s) ? item.Sysname : string.Format("{0} ({1})", item.DisplayName, item.Sysname);
        }

        internal static int getImageIndexByType(FileSysItem item)
        {
            switch (item.ItemType)
            {
                case ItemType.Dir:
                case ItemType.Drive:
                    return 0;
                default:
                    return item.IsXml ? 3 : 2;
            }
        }
    }
}
