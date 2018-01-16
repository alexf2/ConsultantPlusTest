using System;

namespace Ifx.FoundationHelpers.General
{
	/// <summary>
	/// Абстрактные уровни логирования
	/// </summary>
	public enum AbsLogLevel
	{
		Trace, Debug, Info, Warning, Error, Fatal
	};

	/// <summary>
	/// Интерфейс общей обёртки логгеров.
	/// </summary>
	public interface IAbstractLogger
	{
		void Trace (string message, params object[] args);
        void Debug (string message, params object[] args);
        void Info (string message, params object[] args);
        void Warn (string message, params object[] args);
        void Error (string message, params object[] args);
        void Fatal (string message, params object[] args);
        void Exception (string message, Exception ex);

		void Log (AbsLogLevel level, string msg, params object[] args);

		bool IsDebugEnabled
		{
			get;
		}
	}
}
