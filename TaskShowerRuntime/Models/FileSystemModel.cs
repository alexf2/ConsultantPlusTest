using System;
using System.Collections.Generic;
using System.Threading;
using ConsPlus.TaskShowerModel;
using System.IO;
using System.Diagnostics;

namespace ConsPlus.TaskShowerRuntime.Models
{
    public sealed class FileSystemModel : IFileSystemModel
    {
        bool _disposed;
        readonly Dictionary<string, FsoDescriptorBase> _descriptors = new Dictionary<string, FsoDescriptorBase>(StringComparer.OrdinalIgnoreCase);       
        

        #region IFileSystemModel
        public IDescriptorBase GetDir(string path, bool withFiles)
        {
            if (string.IsNullOrEmpty(path))
                path = ".";            

            FsoDescriptorBase res;
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

        public void Dispose ()
        {
            if (!_disposed)
            {
                _disposed = true;
                foreach (var kv in _descriptors)
                    kv.Value.Dispose();
                _descriptors.Clear();
            }
        }
        #endregion IFileSystemModel
    }
}
