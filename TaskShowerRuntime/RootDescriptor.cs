using System;
using System.IO;
using ConsPlus.TaskShowerModel;
using System.Diagnostics;

namespace ConsPlus.TaskShowerRuntime
{
    sealed class RootDescriptor : FsoDescriptorBase
    {
        public RootDescriptor()
        {
            Path = ".";
            foreach (var drive in DriveInfo.GetDrives())
                Add(new FileSysItem(this, drive.Name.TrimEnd('\\')) { DisplayName = getLbl(drive), ItemType = ItemType.Drive });
        }

        [DebuggerStepThrough]
        static string getLbl(DriveInfo info)
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
}
