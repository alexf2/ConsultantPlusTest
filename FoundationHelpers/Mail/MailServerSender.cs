using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Ifx.FoundationHelpers.General;

namespace Ifx.FoundationHelpers.Mail
{
	public sealed class MailServerSender
	{
		const int WarnTimeInSec = 10;

		sealed class QueuedMail
		{
			public IMailMessage Msg;
			public int RetryCount;
			public Exception LastError;
		};

		readonly ISmtpClientFactory _clifac;
		readonly int _maxRetry, _retryTimeSeconds, _maxParallelMessages;
		readonly IAbstractLogger _logger;

		readonly ConcurrentQueue<QueuedMail> _mainQueue = new ConcurrentQueue<QueuedMail>();
		readonly ConcurrentQueue<QueuedMail> _retryQueue = new ConcurrentQueue<QueuedMail>();

		CancellationTokenSource _cancelSrc;
		CancellationToken _token;
		int _mailsActiveCount, _activeRetryCount;
		bool _stopped;

		public MailServerSender (IAbstractLogger logger, int maxRetry, int retryTimeSeconds, int maxParallelMessages, ISmtpClientFactory clifac)
		{
			_maxRetry = maxRetry;
			_retryTimeSeconds = retryTimeSeconds;
			_maxParallelMessages = maxParallelMessages;
			_clifac = clifac;
			_logger = logger;

			_cancelSrc = new CancellationTokenSource();
			_token = _cancelSrc.Token;
		}

		public void EnqueueMail (IMailMessage msg)
		{
			if (_stopped)
				return;

			_mainQueue.Enqueue(new QueuedMail{Msg = msg});			
			startSending();
		}

		public void Stop ()
		{
			if (!_stopped)
			{
				_stopped = true;
				_cancelSrc.Cancel();
				_cancelSrc.Dispose();

				QueuedMail m;
				while (_mainQueue.TryDequeue(out m))
				{
					m.Msg.Dispose();
				}
				while (_retryQueue.TryDequeue(out m))
				{
					m.Msg.Dispose();
				}
			}
		}

		void startSending ()
		{			
			Task.Run( () => processMail(_token), _token );
		}
		void startRetrying ()
		{
			Task.Delay(TimeSpan.FromSeconds(_retryTimeSeconds), _token).ContinueWith( (t) => processRetryMail(_token), _token );
		}

		void processMail (CancellationToken token)
		{
			if (_mailsActiveCount > _maxParallelMessages)
				return;
			
			Interlocked.Increment(ref _mailsActiveCount);
			QueuedMail msg = null;
			try {				
				if (!token.IsCancellationRequested && _mainQueue.TryDequeue(out msg))
				{
					DateTime dt = DateTime.UtcNow;
					using (ISmpClient cli = _clifac.Create())
					{
						cli.Send(msg.Msg);
					}

					if ((DateTime.UtcNow - dt).TotalSeconds > WarnTimeInSec)
						_logger.Warn("Sending mail '{0}', to {1} took extratime: {2}s", msg.Msg.Subject, msg.Msg.Recipients, (DateTime.UtcNow - dt).TotalSeconds);

					msg.Msg.Dispose();
				}
			}
			catch (Exception ex)
			{
				_logger.Exception(string.Format("On sending news, queue size = {0}", _mainQueue.Count), ex);
				if (!token.IsCancellationRequested)
				{
					msg.LastError = ex;
					_retryQueue.Enqueue(msg);
					startRetrying();
				}
			}
			finally {
				Interlocked.Decrement(ref _mailsActiveCount);
			}

			if (_mainQueue.Count > 0 && !token.IsCancellationRequested)
				startSending();
		}

		void processRetryMail (CancellationToken token)
		{
			if (_activeRetryCount > _maxParallelMessages)
				return;
			
			Interlocked.Increment(ref _activeRetryCount);
			QueuedMail msg = null;
			try {				
				if (!token.IsCancellationRequested && _retryQueue.TryDequeue(out msg))
				{
					if (++msg.RetryCount > _maxRetry)
					{
						_logger.Error("Sending mail '{0}' to {1}: maxretry has been exceeded {2}\r\nLast error is: {3}", msg.Msg.Subject, msg.Msg.Recipients, _maxRetry, msg.LastError.ToString());
						msg.Msg.Dispose();
					}
					else
					{
						DateTime dt = DateTime.UtcNow;
						using (ISmpClient cli = _clifac.Create())
						{
							cli.Send(msg.Msg);
						}

						if ((DateTime.UtcNow - dt).TotalSeconds > WarnTimeInSec)
							_logger.Warn("Sending mail '{0}', to {1} took extratime: {2}s", msg.Msg.Subject, msg.Msg.Recipients, (DateTime.UtcNow - dt).TotalSeconds);

						msg.Msg.Dispose();
					}
				}
			}
			catch (Exception ex)
			{
				_logger.Exception(string.Format("On sending news, queue size = {0}", _retryQueue.Count), ex);
				if (!token.IsCancellationRequested)
				{
					msg.LastError = ex;
					_retryQueue.Enqueue(msg);
				}
			}
			finally {
				Interlocked.Decrement(ref _activeRetryCount);
			}

			if (_retryQueue.Count > 0 && !token.IsCancellationRequested)
				startRetrying();
		}
	}
}
