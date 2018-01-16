using System;
using System.Collections.ObjectModel;
using ConsPlus.TaskShowerModel;

namespace ConsPlus.TaskShowerRuntime
{
    public class FsoDescriptorBase : ObservableCollection<FileSysItem>, IDescriptorBase
    {
        protected bool _disposed;

        public string Path
        {
            get;
            protected set;
        }

        public bool IsRoot
        {
            get {
                return string.IsNullOrEmpty(Path) || Path == ".";
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            _disposed = true;
        }

        public virtual void Activatewatching(bool watchChanges)
        {
        }
    }
}
