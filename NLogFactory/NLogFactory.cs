using NLog;

using Ifx.FoundationHelpers.General;

namespace Ifx.NLogFactory
{
	/// <summary>
	/// Представляет фабрику для получения обёрток логгеров NLog.
	/// </summary>
    public sealed class NLogFactory: IAbstractLoggerFactory
    {
		/// <summary>
		/// Возвращает обёрнутый логгер по имени.
		/// </summary>
		/// <param name="name">Имя логгера</param>		
		public IAbstractLogger GetLogger (string name)
		{
			return new NLogAdapter(LogManager.GetLogger(name));
		}

		/// <summary>
		/// Возвращает обёрнутый логгер по некому ID, который конвертируется в строку.
		/// </summary>
		/// <typeparam name="T">Тип id</typeparam>		
		public IAbstractLogger GetLogger<T> (T id)
		{
			return new NLogAdapter(LogManager.GetLogger(id.ToString()));
		}
    }
}
