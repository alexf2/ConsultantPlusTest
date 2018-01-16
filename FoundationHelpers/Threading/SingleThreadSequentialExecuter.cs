using System;
using System.Collections.Concurrent;
using System.Threading;
using Ifx.FoundationHelpers.General;

namespace Ifx.FoundationHelpers.Threading
{
	/// <summary>
	/// Реализует упорядоченное исполнение заявок на отдельном потоке.
	/// </summary>
	public abstract class SingleThreadSequentialExecuter<T>: IMonitorable<BasicMonitoredActivities>
	{
		const int StopTimeoutMS = 25 * 1000;
		const int OperationTimeoutMS = 120 * 1000;

		readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
		readonly IAbstractLogger _logger;
		long _operationStarted; //время начала последней операции
		Thread _thread;
		ManualResetEventSlim _evStop;
		AutoResetEvent _evHasData;

		public SingleThreadSequentialExecuter (String name, IAbstractLogger logger)
		{
			Name = name;
			_logger = logger;
			createThread();
		}

		void createThread ()
		{
			_thread = new Thread(wkfn);
			_thread.IsBackground = false;
			_evStop = new ManualResetEventSlim(false);
			_evHasData = new AutoResetEvent(_queue.Count > 0);
		}

		public string Name
		{
			get;
			private set;
		}

		public void Start ()
		{
			if (_thread == null)
			{
				throw new Exception(string.Format("Can't start thread '{0}': it isn't created", Name));
			}
			if ((_thread.ThreadState & ThreadState.Unstarted) == 0)
			{
				throw new Exception(string.Format("Can't start thread '{0}': it is in inpropriate state '{1}", Name, _thread.ThreadState));
			}
			_operationStarted = 0;
			_evStop.Reset();
			_thread.Start();
		}

		public void Stop ()
		{
			if (_thread == null)
			{
				throw new Exception(string.Format("Can't stop thread '{0}': it isn't created", Name));
			}

			if ((_thread.ThreadState & ThreadState.Unstarted) != 0)
			{
				throw new Exception(string.Format("Can't stop thread '{0}': it is in inpropriate state '{1}", Name, _thread.ThreadState));
			}

			_evStop.Set();
			if (!_thread.Join(StopTimeoutMS))
			{
				_logger.Warn("Thread '{0}' was timeot at stopping: aborting", Name);
				_thread.Interrupt();
				_thread.Abort();
			}

			_thread = null;
			_evStop.Dispose();
			_evStop = null;
			_evHasData.Dispose();
			_evHasData = null;
		}

		#region IMonitorable
		/// <summary>
		/// Проверяет не зависла ли операция
		/// </summary>
		public virtual void PerformMonitoring (BasicMonitoredActivities monitoringActivity)
		{
			long opSta = _operationStarted;
			if (opSta != 0 && (DateTime.UtcNow - DateTime.FromBinary(opSta)).TotalMilliseconds > OperationTimeoutMS)
			{
				_logger.Warn("Thread '{0}': operation timeout was detected. Restarting...", Name);
				Stop();
				createThread();
				Start();
				_logger.Warn("Thread '{0}': Restarted", Name);
				return;
			}
		}
		#endregion IMonitorable

		void wkfn ()
		{
			try {
				WaitHandle[] handles = new WaitHandle[]{_evHasData, _evStop.WaitHandle};
				while (true)
				{
					_operationStarted = 0;
					int idx = WaitHandle.WaitAny(handles);
					if (idx == 0)
					{
						T workUnit;
						while (_queue.TryDequeue(out workUnit))
						{
							Interlocked.Exchange(ref _operationStarted,  DateTime.UtcNow.ToBinary());

							try {
								if (null != workUnit)
								{
									try {										
										ExecuteTask(workUnit);
									}
									catch (Exception ex)
									{
										_logger.Exception(string.Format("AT processing {0}", GetTaskDescription(workUnit)), ex);
									}
								}
								else
								{
									_logger.Error("Thread {0}: null work in queue.", Name);
								}
							}
							finally {
								_operationStarted = 0;
							}
						}
					}
					else
					{
						break;
					}
				}
			}			
			catch (ThreadAbortException)
            {
                Thread.ResetAbort();			
            }
			catch (Exception ex)
			{
				_logger.Exception(string.Format("Main wkfn of '{0}' was broken", Name), ex);
			}
		}

		public void Add (T work) 
        {
            if (work == null)
			{
                throw new ArgumentNullException("work");
			}

            if (TaskHasSomeWork(work))
            {
                _queue.Enqueue(work);
                _logger.Trace("Put {0}, queue={1}", work, _queue.Count);

				AutoResetEvent ev = _evHasData;
				if (ev != null)
				{
					ev.Set();
				}
            }
        }

		protected abstract void ExecuteTask (T task);
		protected abstract string GetTaskDescription (T task);
		protected abstract bool TaskHasSomeWork (T task);

		public override string ToString ()
        {
            return Name;
        }		
	}

}
