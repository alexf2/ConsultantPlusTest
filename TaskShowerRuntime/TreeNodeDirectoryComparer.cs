using System.Collections;
using System.Windows.Forms;

namespace ConsPlus.TaskShowerRuntime
{
    sealed class TreeNodeDirectoryComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            TreeNode t1 = (TreeNode)x;
            TreeNode t2 = (TreeNode)y;
            if (t1 == null && t2 == null)
                return 0;
            if (t1 == null && t1 != t2)
                return 1;
            if (t2 == null && t1 != t2)
                return -1;

            return t1.Name.CompareTo(t2.Name);
        }
    }
}
