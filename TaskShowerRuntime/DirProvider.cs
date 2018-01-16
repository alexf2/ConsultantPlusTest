using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using ConsPlus.TaskShowerModel;
using System.IO;

namespace ConsPlus.TaskShowerRuntime
{
    public sealed class DirProvider : IDirProvider
    {
        class Descriptor : ObservableCollection<FileSysItem>, IDisposable
        {
            protected bool _disposed;

            public string Path
            {
                get;
                private set;
            }            

            public void Dispose()
            {
                Dispose(true);
            }

            protected virtual void Dispose (bool disposing)
            {
            }

            public virtual void Activatewatching (bool watchChanges)
            {
            }
        }

        sealed class RootDescriptor : Descriptor
        {
            public RootDescriptor ()
            {
                foreach (var drive in DriveInfo.GetDrives())
                    Add(new FileSysItem() { Sysname = drive.Name.TrimEnd('\\'), DisplayName = getLbl(drive), DevType = ItemType.Drive });
            }

            static string getLbl (DriveInfo info)
            {
                try
                {
                    return info.VolumeLabel;
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
        }

        sealed class DirDescriptor : Descriptor
        {
            readonly string _path;
            readonly bool _withFiles;
            FileSystemWatcher _w;

            public DirDescriptor(string path, bool withFiles)
            {
                _path = path;
                _withFiles = withFiles;

                foreach (var dir in Directory.EnumerateDirectories(_path))
                    Add(new FileSysItem() { Sysname = dir, DevType = ItemType.Dir });

                if (_withFiles)
                    foreach (var f in Directory.EnumerateFiles(_path))
                        Add(new FileSysItem() { Sysname = f, DevType = ItemType.File });

                _w = new FileSystemWatcher(_path) { IncludeSubdirectories = false, NotifyFilter = NotifyFilters.DirectoryName | (withFiles ? NotifyFilters.FileName:0) };
                _w.Created += watcher_Created;
                _w.Deleted += watcher_Deleted;
                _w.Renamed += watcher_Renamed;
            }

            protected override void Dispose (bool disposing)
            {
                FileSystemWatcher w = _w;
                if (disposing && w != null)
                {
                    _w.Created -= watcher_Created;
                    _w.Deleted -= watcher_Deleted;
                    _w.Renamed -= watcher_Renamed;

                    Activatewatching(false);
                    w.Dispose();
                    _w = null;
                }
                base.Dispose(disposing);
            }

            public override void Activatewatching (bool watchChanges)
            {
                _w.EnableRaisingEvents = watchChanges;
            }

            void watcher_Created(object sender, FileSystemEventArgs e)
            {                
            }
            void watcher_Deleted(object sender, FileSystemEventArgs e)
            {
            }
            void watcher_Renamed(object sender, FileSystemEventArgs e)
            {                
                int i = IndexOf(new FileSysItem() { Sysname = e.FullPath, DevType = ItemType.Dir });
            }
        }

        Dictionary<string, Descriptor> _descriptors = new Dictionary<string, Descriptor>(StringComparer.OrdinalIgnoreCase);

        public ObservableCollection<FileSysItem> GetDir (string path, bool withFiles)
        {
            Descriptor res;
            if (_descriptors.TryGetValue(path, out res))
                return res;

            if (string.IsNullOrEmpty(path) || path == ".")
            {
                res = new RootDescriptor();
                path = ".";
            }
            else
                res = new DirDescriptor(path, withFiles);

            _descriptors.Add(path, res);
            return res;
        }
    }
}
