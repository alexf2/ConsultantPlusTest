using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ifx.FoundationHelpers.RuntimeModel
{
	/// <summary>
	/// Представляет интерфейс хоста Windows-сервисов.
	/// </summary>
	public interface IRuntimeHost: IDisposable
	{
		/// <summary>
		/// Используется для запуска жизненного цикла хоста.
		/// </summary>
		/// <param name="testMode">Ести true, то в тестовом режиме, который предусматривает диалоговое окно со статусом сервиса.</param>
		/// <param name="commandLineArgs">Аргументы командной строки.</param>
		void Run (bool testMode, string[] commandLineArgs);

		/// <summary>
		/// Имя сервиса.
		/// </summary>
		string WinServiceName
		{
			get;
		}

		/// <summary>
		/// Дополнительное время для запуска и остановки сервиса.
		/// </summary>
		int? AdditionalStatStopTimeInSeconds
		{
			get;
			set;
		}

		/// <summary>
		/// Дополнительное время для приостановки и возобновления сервиса.
		/// </summary>
		int? AdditionalPauseResumeTimeInSeconds
		{
			get;
			set;
		}

		string GetStates ();
	}
}
