using System;

namespace ConsPlus.TaskShowerModel
{
    
    public sealed class RequestDirArgs: EventArgs
    {
        public string Path
        {
            get;
            set;
        }
    }
    public delegate void RequestDirHandler (IShowerView sender, RequestDirArgs args);

    public sealed class ItemPickedArgs : EventArgs
    {
        public FileSysItem Item
        {
            get;
            set;
        }
    }
    public delegate void ItemPickedHandler (IShowerView sender, ItemPickedArgs args);

    /// <summary>
    /// Интерфейс просмоторщика XML-файлов задач.
    /// </summary>
    public interface IShowerView
    {
        /// <summary>
        /// Запрос вложенных контейнеров
        /// </summary>
        event RequestDirHandler RequestDir;        
        /// <summary>
        /// Запрос содержимого контейнера
        /// </summary>
        event RequestDirHandler RequestDetails;
        /// <summary>
        /// Выбран обобщённый файловый элемент
        /// </summary>
        event ItemPickedHandler ItemPicked;

        /// <summary>
        /// Устанавливает заполненный файловый контейнер в дереве каталогов
        /// </summary>        
        void SetDirDescriptor(IDescriptorBase descr);
        /// <summary>
        /// Устанавливает детальную информацию о каталоге
        /// </summary>        
        void SetDirDetails(IDescriptorBase descr);
        /// <summary>
        /// очищает вьювер XML
        /// </summary>
        void ClearDocument();
        /// <summary>
        /// Загружает во вьювер сообщение, которое отображаетcя крупным шрифтом по-середине
        /// </summary>        
        void ShowDocumentMsg(string msg);
        /// <summary>
        /// Загружает HTML во вьювер
        /// </summary>        
        void ShowHtml(string msg);
        /// <summary>
        /// Загружает HTML во вьювер
        /// </summary>        
        void ShowHtml(System.IO.Stream stm);
    }
}
