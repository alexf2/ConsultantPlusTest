using System.Windows.Forms;
using System.Collections;
using ConsPlus.TaskShowerModel;

namespace ConsPlus.TaskShowerRuntime
{
    sealed class ListViewDirectoryComparer : IComparer
    {
        readonly bool _asc;
        public ListViewDirectoryComparer(bool asc)
        {
            _asc = asc;
        }

        public int Compare(object x, object y)
        {
            int res;

            ListViewItem t1 = (ListViewItem)x;
            ListViewItem t2 = (ListViewItem)y;
            if (t1 == null && t2 == null)
                res = 0;
            else if (t1 == null && t1 != t2)
                res = 1;
            else if (t2 == null && t1 != t2)
                res = -1;
            else
            {

                FileSysItem i1 = (FileSysItem)t1.Tag;
                FileSysItem i2 = (FileSysItem)t2.Tag;

                if (i1.ItemType == i2.ItemType)
                    res = i1.Sysname.CompareTo(i2.Sysname);
                else
                    res = i1.ItemType == ItemType.Dir || i1.ItemType == ItemType.Drive ? -1 : 1;
            }

            return _asc ? res : -res;
        }
    }
}
