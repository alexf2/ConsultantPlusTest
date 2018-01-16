using System;
using System.Threading;


namespace Ifx.FoundationHelpers.General
{
	public enum BasicMonitoredActivities
	{
		AsyncTaskExecution,
		Performance,
		Load,
		ThroughPut,
		ClientDetails
	}

	/// <summary>
	/// Реализует периодический вызов некого метода
	/// </summary>
	public sealed class ServerMonitor<T>
	{		
		Timer _t;
		readonly IMonitorable<T>[] _controlables;
		readonly IAbstractLogger _logger;
		readonly int _monitorPeriodMS;
		readonly T _monitorActivity;

		public ServerMonitor (int monitorPeriodMS, T monitorActivity, IAbstractLogger logger, params IMonitorable<T>[] controlables)
		{
			_monitorPeriodMS = monitorPeriodMS;
			_monitorActivity = monitorActivity;
			_logger = logger;
			_controlables = controlables;
		}

		public void Start ()
		{
			Timer t = new Timer(wkfn, null, _monitorPeriodMS, Timeout.Infinite);
			Timer oldVal = Interlocked.CompareExchange(ref _t, t, null);
			if (oldVal != null)
			{
				t.Dispose();
			}
		}

		public void Stop (bool wait = true)
		{
			Timer t = Interlocked.Exchange(ref _t, null);
			if (t != null)
			{
				if (wait)
				{
					using (ManualResetEvent ev = new ManualResetEvent(false))
					{
						t.Dispose(ev);
						ev.WaitOne();
					}
				}
				else
				{
					t.Dispose();
				}
			}
		}		

		void wkfn (object st)
		{
			try {
				foreach (IMonitorable<T> drb in _controlables)
				{
					drb.PerformMonitoring(_monitorActivity);
				}
			}
			catch (Exception ex)
			{
				_logger.Exception("ThreadMonitor: CheckHungingUp", ex);
			}

			Timer t = _t;
			if (t != null)
			{
				try {t.Change(_monitorPeriodMS, Timeout.Infinite);} catch {}
			}
		}
	}
}
