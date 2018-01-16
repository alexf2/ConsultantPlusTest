using System;
using System.Collections.Generic;
using System.Collections.Specialized;


namespace ConsPlus.TaskShowerModel
{
    /// <summary>
    /// Интрфейс обобщённого контейнера объектов файловой системы.
    /// </summary>
    public interface IDescriptorBase : IEnumerable<FileSysItem>, IDisposable
    {
        /// <summary>
        /// Полный путь контейнера в системе.
        /// </summary>
        string Path
        {
            get;            
        }

        /// <summary>
        /// Если true, то является корневым контейнером.
        /// </summary>
        bool IsRoot
        { 
            get;
        }

        /// <summary>
        /// используется для включения/отключения мониторинга за изменениями в контейнере (появлении новых элементов...).
        /// </summary>        
        void Activatewatching(bool watchChanges);

        /// <summary>
        /// Уведомляет об изменении содержимого контейнера.
        /// </summary>
        event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}
