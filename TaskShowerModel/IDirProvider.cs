using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace ConsPlus.TaskShowerModel
{
    public enum ItemType
    {
        Drive, Dir, File
    };
    public struct FileSysItem : IEquatable<FileSysItem>
    {
        public string Sysname;
        public string DisplayName;
        public ItemType DevType;

        public override bool Equals (object obj)
        {
            if (obj == null || !(obj is FileSysItem))
                return false;
            else
                return Equals((FileSysItem)obj);
        }

        public bool Equals (FileSysItem o)
        {
            return Sysname == o.Sysname && DevType == o.DevType;
        }
        public static bool operator == (FileSysItem a1, FileSysItem a2)
        {
            return a1.Equals(a2);
        }
        public static bool operator != (FileSysItem a1, FileSysItem a2)
        {
            return !a1.Equals(a2);
        }

        public override int GetHashCode ()
        {
            int c1 = Sysname == null ? 0 : Sysname.GetHashCode();
            int c2 = DisplayName == null ? 0 : DisplayName.GetHashCode();
            int c3 = DevType.GetHashCode();

            return ((17 + c1) * 23 + c2) * 23 + c3;
        }
    }

    public interface IDirProvider
    {
        ObservableCollection<FileSysItem> GetDir(string path, bool withFiles);
    }
}
