using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ifx.FoundationHelpers.General
{
	/// <summary>
	/// Интерфейс фабрики для логеров.
	/// </summary>
	public interface IAbstractLoggerFactory
	{
		/// <summary>
		/// Получить логгер по имени.
		/// </summary>		
		IAbstractLogger GetLogger (string name);

		/// <summary>
		/// Получить логгер по  Id.
		/// </summary>		
		IAbstractLogger GetLogger<T> (T id);
	}
}
