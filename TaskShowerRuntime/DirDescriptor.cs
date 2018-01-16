using System;
using System.IO;
using ConsPlus.TaskShowerModel;
using System.Diagnostics;
using System.Threading;

namespace ConsPlus.TaskShowerRuntime
{
    sealed class DirDescriptor : FsoDescriptorBase
    {
        readonly bool _withFiles;
        FileSystemWatcher _w;
        readonly SynchronizationContext _syncCtx;

        public DirDescriptor(string path, bool withFiles)
        {
            Path = path;
            _withFiles = withFiles;

            _syncCtx = SynchronizationContext.Current;

            foreach (var dir in Directory.EnumerateDirectories(path))
                Add(new FileSysItem(this, dir.Substring(dir.LastIndexOf("\\") + 1)) { ItemType = ItemType.Dir });

            if (_withFiles)
                foreach (var f in Directory.EnumerateFiles(path))
                    Add(new FileSysItem(this, f.Substring(f.LastIndexOf("\\") + 1)) { ItemType = ItemType.File });

            _w = new FileSystemWatcher(path) { IncludeSubdirectories = false, NotifyFilter = NotifyFilters.DirectoryName | (withFiles ? NotifyFilters.FileName : 0) };
            _w.Created += watcher_Created;
            _w.Deleted += watcher_Deleted;
            _w.Renamed += watcher_Renamed;
        }

        protected override void Dispose(bool disposing)
        {
            FileSystemWatcher w = _w;
            if (!_disposed && disposing && w != null)
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

        public override void Activatewatching(bool watchChanges)
        {
            _w.EnableRaisingEvents = watchChanges;
        }

        void watcher_Created(object sender, FileSystemEventArgs e)
        {
            threadSafeExec(() =>
            {
                FileSysItem item = new FileSysItem(this, e.Name);
                int i = IndexOf(item);
                if (i < 0)
                    Add(item);
            });
        }
        void watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            threadSafeExec(() =>
            {
                FileSysItem item = new FileSysItem(this, e.Name);
                int i = IndexOf(item);
                if (i > -1)
                    RemoveAt(i);
            });
        }
        void watcher_Renamed(object sender, RenamedEventArgs e)
        {
            threadSafeExec(() =>
            {
                FileSysItem item = new FileSysItem(this, e.OldName);
                int i = IndexOf(item);
                if (i > -1)
                    this[i] = new FileSysItem(this, e.Name);
            });
        }

        void threadSafeExec(Action act)
        {
            _syncCtx.Post((st) => act(), null);
        }
    }
}
