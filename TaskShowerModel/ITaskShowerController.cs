using System;

namespace ConsPlus.TaskShowerModel
{
    /// <summary>
    /// Интерфейс контроллера просмоторщика XML
    /// </summary>
    public interface ITaskShowerController: IDisposable
    {
        /// <summary>
        /// Возвращает текущую вьюшку
        /// </summary>
        IShowerView View
        {
            get;
        }
    }
}
