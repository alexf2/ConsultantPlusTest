using System;
using NLog;

using Ifx.FoundationHelpers.General;

namespace Ifx.NLogFactory
{
	/// <summary>
	/// Представляет обёртку, используемую для логгера NLog. Предназначено для внутреннего использования фабрикой.
	/// </summary>
	sealed class NLogAdapter: IAbstractLogger
	{
		readonly Logger _logger;

		public NLogAdapter (Logger logger)
		{
			_logger = logger;
		}

		#region IAbstractLogger
		public void Trace (string message, params object[] args)
		{
			_logger.Log(LogLevel.Trace, message, args);
		}
        public void Debug (string message, params object[] args)
		{
			_logger.Log(LogLevel.Debug, message, args);
		}
        public void Info (string message, params object[] args)
		{
			_logger.Log(LogLevel.Info, message, args);
		}
        public void Warn (string message, params object[] args)
		{
			_logger.Log(LogLevel.Warn, message, args);
		}
        public void Error (string message, params object[] args)
		{
			_logger.Log(LogLevel.Error, message, args);
		}
        public void Fatal (string message, params object[] args)
		{
			_logger.Log(LogLevel.Fatal, message, args);
		}
        public void Exception (string message, Exception ex)
		{
			_logger.ErrorException(message, ex);
		}
		public bool IsDebugEnabled
		{
			get {
				return _logger.IsDebugEnabled;
			}
		}

		public void Log (AbsLogLevel level, string msg, params object[] args)
		{
			switch (level)
			{
				case AbsLogLevel.Debug:
					Debug(msg, args);
					break;
				case AbsLogLevel.Error:
					Error(msg, args);
					break;
				case AbsLogLevel.Fatal:
					Fatal(msg, args);
					break;
				case AbsLogLevel.Info:
					Info(msg, args);
					break;
				case AbsLogLevel.Trace:
					Trace(msg, args);
					break;
				case AbsLogLevel.Warning:
					Warn(msg, args);
					break;
			};
		}
		#endregion
	}

}
