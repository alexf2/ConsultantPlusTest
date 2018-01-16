using System;
using System.Text;
using System.Threading;

using System.Collections.Concurrent;
using Ifx.FoundationHelpers.General;

namespace Ifx.FoundationHelpers.Threading
{
	/// <summary>
	/// Реализует последовательное асинхронное выполнение задач используя ThreadPool.
	/// </summary>
	/// <typeparam name="T">Класс задачи</typeparam>
	public abstract class ThreadPoolSequentialExecuter<T>: IDisposable, IMonitorable<BasicMonitoredActivities>
	{
		const int WaitOnDisposeMs = 45 * 1000; //время ожидания завершения задач при диспозе
		const int CommandPostponeTimeMS = 30 * 1000; //время задержки команды
	
		protected byte _disposed; //1 - если вызван Dispose
		readonly ConcurrentQueue<T> _tasks = new ConcurrentQueue<T>();
		int _requestsCount; //счётчик невыполненных задач (это то, что стоит в очереди, плюс)
		ManualResetEventSlim _queueFinished = new ManualResetEventSlim(true);
		readonly protected IAbstractLogger _logger;

		readonly ConcurrentQueue<T> _postponedTasks = new ConcurrentQueue<T>();
		Timer _tmPostoned;

#if DEBUG
		int _entranceCount;
#endif


		public ThreadPoolSequentialExecuter (IAbstractLogger logger, string name = null)
		{
			_logger = logger;
			Name = string.IsNullOrEmpty(name) ? "Unnamed":name;
			_tmPostoned = new Timer(processPostponed, null, 2 * CommandPostponeTimeMS, CommandPostponeTimeMS);
		}
	
		public string Name
		{
			get;
			private set;
		}

		public bool IsDisposed
		{
			get {
				return _disposed != 0;
			}
		}

		/// <summary>
		/// Вызывается для выставления сигнала завершения выполнения всех задач из очереди
		/// </summary>		
		void signalQueueFinished (bool isFinished)
		{
			Thread.MemoryBarrier();
			ManualResetEventSlim queueFinished = _queueFinished;
		
			if (queueFinished != null)
			{
				try {
					if (isFinished)
					{
						queueFinished.Set();
					}
					else
					{
						queueFinished.Reset();
					}
				}
				catch {}
			}
		}

		/// <summary>
		/// Возвращает true, если задача, соотвествующая условию, уже есть в очереди.
		/// </summary>		
		protected bool isAlreadyQueued (T task, Func<T, T, bool> cmp)
		{
			foreach (T t in _tasks)
			{
				if (cmp(task, t))
				{
					return true;
				}
			}
			return false;
		}
		protected bool isAlreadyQueuedEither (T task, Func<T, T, bool> cmp)
		{
			foreach (T t in _tasks)
			{
				if (cmp(task, t))
				{
					return true;
				}
			}
			foreach (T t in _postponedTasks)
			{
				if (cmp(task, t))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Ставит в очередь задачу.
		/// </summary>		
		public virtual bool QueueTask (T task)
		{
			if (Thread.VolatileRead(ref _disposed) != 0)
			{
				return false;
			}

			if (TaskHasSomeWork(task))
			{
				_tasks.Enqueue(task);
				if (Interlocked.Increment(ref _requestsCount) == 1)
				{
					signalQueueFinished(false);
					startNextTask();
				}
				return true;
			}

			return false;
		}

		public virtual bool QueueTaskTimeout (T task)
		{
			if (Thread.VolatileRead(ref _disposed) != 0)
			{
				return false;
			}

			if (TaskHasSomeWork(task))
			{
				_postponedTasks.Enqueue(task);
				return true;
			}
			return false;
		}

		void processPostponed (object state)
		{
			if (Thread.VolatileRead(ref _disposed) != 0)
			{
				return;
			}

			T task;
			while (_postponedTasks.TryDequeue(out task))
			{
				QueueTask(task);
			}
		}

		/// <summary>
		/// Ставит в ThreadPool на выполнение очредную задачу и главы очереди.
		/// </summary>
		void startNextTask ()
		{
			/*if (Thread.VolatileRead(ref _disposed) != 0)
			{
				T tmp;
				while (_tasks.TryDequeue(out tmp));//очистка очереди
				_requestsCount = 0;
				
				signalQueueFinished(true);
				return;
			}*/

			T task;
			if (_tasks.TryDequeue(out task))
			{
				signalQueueFinished(false);
				if (!ThreadPool.UnsafeQueueUserWorkItem(executer, task)) //если проблема с пулом потоков, то пробуем после задержки
				{
					Thread.Sleep(150);
					trySpawnNextTask();
				}
			}
		}
	
		/// <summary>
		/// Выполняет задачу.
		/// </summary>		
		void executer (object task)
		{
#if DEBUG
		int ec = Interlocked.Increment(ref _entranceCount);
		if (ec > 1)
		{
			_logger.Error("ThreadPoolSequentialExecuter: task parallel execution is detected '{0}'", ec);
		}
#endif
			if (Thread.VolatileRead(ref _disposed) == 0)
			{
				T tt = (T)task;
				Exception excKeep = null;
				try {
					ExecuteTask(tt);
				}
				catch (Exception ex)
				{
					excKeep = ex;
					_logger.Exception("Error of a task execution", ex);
				}

				try {
					onTaskFinished(tt, excKeep);
				}
				catch (Exception ex)
				{
					_logger.Exception("Error of a task execution/finishing", ex);
				}
			}

#if DEBUG
		Interlocked.Decrement(ref _entranceCount);
#endif

			trySpawnNextTask();		
		}

		void trySpawnNextTask ()
		{
			//пробуем запустить следующую задачу, либо выставляем флаг завершения
			if (Interlocked.Decrement(ref _requestsCount) > 0)
			{
				startNextTask();
			}
			else
			{
				signalQueueFinished(true);
			}
		}

		protected abstract void ExecuteTask (T task);
		protected virtual void onTaskFinished (T task, Exception ex)
		{
		}
		protected abstract bool TaskHasSomeWork (T task);

		#region IMonitorable
		/// <summary>
		/// Проверяет не зависла ли операция
		/// </summary>
		public virtual void PerformMonitoring (BasicMonitoredActivities monitoringActivity)
		{
		}
		#endregion IMonitorable

		public void Dispose ()
		{
			if (Thread.VolatileRead(ref _disposed) == 0)
			{				
				Dispose(true);
			}
		}

		protected virtual void Dispose (bool disposing)
		{
			Thread.VolatileWrite(ref _disposed, 1);

			if (disposing)
			{
				SafeReleasers.ReleaseTimer(ref _tmPostoned, null, true);

				if (!_queueFinished.Wait(WaitOnDisposeMs))
				{
					_logger.Warn("Timeout '{0}' ms was reached at disposing '{1}', queued tasks {2}", WaitOnDisposeMs, Name, _requestsCount);
					dumpCurrentRequests();
				}
				try {_queueFinished.Dispose();} catch {}
				Interlocked.Exchange(ref _queueFinished, null);
			}
		}

		void dumpCurrentRequests ()
		{
			StringBuilder bld = new StringBuilder();
			bld.Append("Taks in queue: ");
			int cnt = 0;
			foreach (T task in _tasks)
			{
				if (cnt++ != 0)
					bld.Append(";   ");
				bld.Append(task.ToString());
			}
			_logger.Warn(bld.ToString());
		}
	};

}
