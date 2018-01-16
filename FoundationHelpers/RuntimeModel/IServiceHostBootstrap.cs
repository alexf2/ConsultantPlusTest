using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ifx.FoundationHelpers.RuntimeModel
{
	/// <summary>
	/// Ключ командной строки.
	/// </summary>
	public struct CommandSwitch
	{
		public string Name
		{
			get;
			internal set;
		}
		public string Description
		{
			get;
			internal set;
		}

		/// <summary>
		/// Действие, которое по выполняется, если ключ есть в командной строке.
		/// </summary>
		public Action<string> Verb
		{
			get;
			internal set;
		}
	};

	/// <summary>
	/// Представляет интерфейс загрузчика Windows-сервиса.
	/// </summary>
	public interface IServiceHostBootstrap
	{
		/// <summary>
		/// Предварительная инициализация.
		/// </summary>
		void Setup ();
		
		/// <summary>
		/// Запуск Windows-сервиса, который хостит данный хост.
		/// </summary>
		int Run (string[] commandLineArgs = null);

		/// <summary>
		/// Очитска ресурсов.
		/// </summary>
		void Close ();		

		/// <summary>
		/// Имя Windows-сервиса.
		/// </summary>
		string ServiceName
		{
			get;
		}

		/// <summary>
		/// Команды, поддерживаемые сервисом.
		/// </summary>
		IEnumerable<CommandSwitch> Commands
		{
			get;
		}
	}
}
