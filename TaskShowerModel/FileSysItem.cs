using System;
using System.IO;

namespace ConsPlus.TaskShowerModel
{
    public enum ItemType
    {
        Undefined, Drive, Dir, File
    };


    /// <summary>
    /// Представляет универсальный элемент файловой системы.
    /// </summary>
    public struct FileSysItem : IEquatable<FileSysItem>
    {
        public string Sysname;
        public string DisplayName;
        public ItemType ItemType;
        readonly IDescriptorBase _parent;

        public FileSysItem(IDescriptorBase parent, string sysName)
        {
            Sysname = sysName;            
            DisplayName = null;
            _parent = parent;
            
            string path = Path.Combine(_parent.Path, sysName);
            ItemType = Directory.Exists(path) ? ItemType.Dir : ItemType.File;
        }

        public IDescriptorBase Parent
        {
            get { return _parent; }
        }        
        public bool IsXml
        {
            get
            {
                return Path.GetExtension(Sysname).ToLower().Contains(".xml");
            }
        }

        public string ItemPath
        {
            get { return Path.Combine(_parent.Path, Sysname); }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is FileSysItem))
                return false;
            else
                return Equals((FileSysItem)obj);
        }

        public bool Equals(FileSysItem o)
        {
            return Sysname == o.Sysname /*&& DevType == o.DevType*/;
        }
        public static bool operator ==(FileSysItem a1, FileSysItem a2)
        {
            return a1.Equals(a2);
        }
        public static bool operator !=(FileSysItem a1, FileSysItem a2)
        {
            return !a1.Equals(a2);
        }

        public override int GetHashCode()
        {
            /*int c1 = Sysname == null ? 0 : Sysname.GetHashCode();
            int c2 = DisplayName == null ? 0 : DisplayName.GetHashCode();
            int c3 = DevType.GetHashCode();

            return ((17 + c1) * 23 + c2) * 23 + c3;*/

            return Sysname.GetHashCode();
        }
    }
}
