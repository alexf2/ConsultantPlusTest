using System;

namespace ConsPlus.TaskShowerModel
{    
    /// <summary>
    /// Интерфейс модели файловой системы.
    /// </summary>
    public interface IFileSystemModel: IDisposable
    {
        /// <summary>
        /// Возвращает описатель контейнера файловой системы.
        /// </summary>
        /// <param name="path">Полный путь контейнера.</param>
        /// <param name="withFiles">Если true, то возвращается вместе с файлами иначе только с вложеными контейнерами.</param>        
        IDescriptorBase GetDir(string path, bool withFiles);
    }
}
