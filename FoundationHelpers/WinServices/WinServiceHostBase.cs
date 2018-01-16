using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Threading;

using Ifx.FoundationHelpers.RuntimeModel;
using Ifx.FoundationHelpers.General;
using Ifx.FoundationHelpers.StateMachine;


namespace Ifx.FoundationHelpers.WinServices
{
	public class WinServiceHostBase: ServiceBase, IRuntimeHost
	{
		const int DefaultSartStopTimeoutSeconds = 60;
		const int DefaultPauseResumeTimeoutSeconds = 30;

		readonly protected IAbstractLogger _logger;
		readonly IEnumerable<IRuntimeService> _services;
		ServiceDebugForm _dbgForm;
		bool _testMode;

		readonly StateMachine<RuntimeServiceState> _stateMachine;


		public WinServiceHostBase (string serviceName, IEnumerable<IRuntimeService> services, IAbstractLogger logger)
		{
			_logger = logger;
			WinServiceName = ServiceName = serviceName;
			_services = services;

			_stateMachine = StateMachine<RuntimeServiceState>.Create().SetName(serviceName).UseLogger(logger).UseStateDef(new RuntimeServiceStateDef())
				.TransitionTable(RuntimeServiceState.Stopped)
					.AddTrans(RuntimeServiceState.Started, start)
					.AddTrans(RuntimeServiceState.Closed, close)										
					.Build()

				.TransitionTable(RuntimeServiceState.Started)
					.AddTrans(RuntimeServiceState.Stopped, stop)
					.AddTrans(RuntimeServiceState.Suspended, suspend)					
					.Build()

				.TransitionTable(RuntimeServiceState.Suspended)
					.AddTrans(RuntimeServiceState.Stopped, close)
					.AddTrans(RuntimeServiceState.Started, resume)
					.Build()
				.Build();

			_stateMachine.StateChanging += stateChangingHndl;
			_stateMachine.StateChanged += stateChangedHandler;
		}

		#region IRuntimeHost
		protected override void Dispose (bool disposing)
		{
			if (disposing)
			{
				if (!_stateMachine.IsInFinalState)
					try {
						_stateMachine.Transit(_stateMachine.FinalState, CancellationToken.None).Wait(startStopTimeoutMS);
					}
					catch (Exception ex)
					{
						_logger.Exception(string.Format("At closing host '{0}'", WinServiceName), ex);
					}
			}

			base.Dispose(disposing);
		}
		#endregion IRuntimeHost

		#region IRuntimeHost
		public void Run (bool testMode, string[] commandLineArgs)
		{
			_testMode = testMode;
			if (testMode)
			{
				using (ServiceDebugForm form = new ServiceDebugForm())
				{
					_dbgForm = form;
					form.Text = WinServiceName;
					form.SupportsResume = base.CanPauseAndContinue;
					form.Stop += debugStopHandler;
					form.Suspend += debugSuspendHandler;
					_logger.Info("Service '{0}' is about to start in TEST mode", WinServiceName);
					form.ShowState(_stateMachine.StartState);
					try {	
						form.Show();
						OnStart(commandLineArgs);
						form.Visible = false;
						form.ShowDialog();
					}
					finally {
						form.Suspend -= debugSuspendHandler;
						form.Stop -= debugStopHandler;
						_dbgForm = null;
					}
					_logger.Info("Service '{0}' has been started in TEST mode", WinServiceName);
				}
			}
			else
			{
				_logger.Info("Service '{0}' is about to start in PRODUCTION mode", WinServiceName);
				ServiceBase.Run(this);
				_logger.Info("Service '{0}': production mode has been stopped", WinServiceName);
			}
		}

		public string WinServiceName
		{
			get;
			private set;			
		}

		public int? AdditionalStatStopTimeInSeconds
		{
			get;
			set;
		}

		public int? AdditionalPauseResumeTimeInSeconds
		{
			get;
			set;
		}

		public string GetStates ()
		{
			StringBuilder bld = new StringBuilder();
			foreach (var srv in _services)
			{
				if (bld.Length != 0)
					bld.Append(", ");
				bld.AppendFormat("{0} in {1}", srv.Name, srv.State.ToString());
			}
			return bld.ToString();
		}
		#endregion IRuntimeHost

		#region Overrides
		protected override void OnContinue ()
		{
			base.OnContinue();	
		
			base.OnPause();

			CancellationTokenSource src = new CancellationTokenSource();

			_logger.Info("Service '{0}' recieved OnContinue command", WinServiceName);
			Task<RuntimeServiceState> tsk = _stateMachine.Transit(RuntimeServiceState.Started, src.Token);
			tsk.ContinueWith( (t) => src.Dispose() );

			bool res = false;
			System.Diagnostics.Stopwatch w = System.Diagnostics.Stopwatch.StartNew();

			bool hasError = false;
			try {
				res = tsk.Wait(pauseResumeTimeoutMS);
			}
			catch (AggregateException ex)
			{						
				hasError = true;
				_logger.Error("Service '{0}' failed to process OnContinue command", WinServiceName);
				ex.Handle( (err) => {
					if (err is OperationCanceledException)
					{
						_logger.Warn("Service '{0}' OnContinue was cancelled", WinServiceName);
						if (_testMode)
							Console.WriteLine(string.Format("Service '{0}' OnContinue was cancelled", WinServiceName));
						return true;
					}							
					else
					{
						_logger.Exception(string.Format("Service '{0}' failed to process OnContinue command", WinServiceName), err);
						if (_testMode)
							Console.WriteLine(err.ToString());
					}
					return true;
				});
				
			}
			catch (OperationCanceledException)
			{
				hasError = true;
				_logger.Warn("Service '{0}' OnContinue was cancelled", WinServiceName);
				if (_testMode)
					Console.WriteLine("Resuming has been cancelled");
				
			}
			catch (Exception ex)
			{
				hasError = true;
				_logger.Exception(string.Format("Service '{0}' failed to process OnContinue command", WinServiceName), ex);
				if (_testMode)
					Console.WriteLine(ex.ToString());
				
			}
			finally {
				w.Stop();
			}

			if (!res)
			{
				try {src.Cancel();} catch {}
				if (!hasError)
				{
					string msg = string.Format("Timeout of resuming service '{0}', elapsed: {1:hh\\:mm\\:ss\\.ff}", WinServiceName, w.Elapsed);
					_logger.Error(msg);
					if (_testMode)
						Console.WriteLine(msg);
				}


#pragma warning disable 4014
				_logger.Info("Requesting additional time and stop...");				
				requestPauseResumeTime();
				//try {_stateMachine.Transit(RuntimeServiceState.Stopped, CancellationToken.None).Wait(startStopTimeoutMS);} catch {}
				//часть сервисов могла запуститься, поэтому состояние хоста не целостное (сервисы в разных состояниях), и мы всё останавливаем
				try {stop(_stateMachine.State, RuntimeServiceState.Stopped, CancellationToken.None);} catch {}
#pragma warning restore 4014

				throw new Exception(hasError ? "OnContinue rolled back by error":"OnContinue rolled back by timeout");
			}

			_logger.Info("Service '{0}' successfully processed OnContinue command, elapsed: {1:hh\\:mm\\:ss\\.ff}", WinServiceName, w.Elapsed);
		}
		protected override void OnPause ()
		{
			base.OnPause();

			CancellationTokenSource src = new CancellationTokenSource();

			_logger.Info("Service '{0}' recieved OnPause command", WinServiceName);
			Task<RuntimeServiceState> tsk = _stateMachine.Transit(RuntimeServiceState.Suspended, src.Token);
			tsk.ContinueWith( (t) => src.Dispose() );

			bool res = false;
			System.Diagnostics.Stopwatch w = System.Diagnostics.Stopwatch.StartNew();

			bool hasError = false;
			try {
				res = tsk.Wait(pauseResumeTimeoutMS);
			}
			catch (AggregateException ex)
			{			
				hasError = true;
				_logger.Error("Service '{0}' failed to process OnPause command", WinServiceName);
				ex.Handle( (err) => {
					if (err is OperationCanceledException)
					{
						_logger.Warn("Service '{0}' OnPause was cancelled", WinServiceName);
						if (_testMode)
							Console.WriteLine(string.Format("Service '{0}' OnPause was cancelled", WinServiceName));
						return true;
					}							
					else
					{
						_logger.Exception(string.Format("Service '{0}' failed to process OnPause command", WinServiceName), err);
						if (_testMode)
							Console.WriteLine(err.ToString());
					}
					return true;
				});				
			}
			catch (OperationCanceledException)
			{
				hasError = true;
				_logger.Warn("Service '{0}' OnPause was cancelled", WinServiceName);
				if (_testMode)
					Console.WriteLine("Pausing has been cancelled");				
			}
			catch (Exception ex)
			{
				hasError = true;
				_logger.Exception(string.Format("Service '{0}' failed to process OnPause command", WinServiceName), ex);
				if (_testMode)
					Console.WriteLine(ex.ToString());				
			}
			finally {
				w.Stop();
			}

			if (!res)
			{
				try {src.Cancel();} catch {}
				if (!hasError)
				{
					string msg = string.Format("Timeout of pausing service '{0}', elapsed: {1:hh\\:mm\\:ss\\.ff}", WinServiceName, w.Elapsed);
					_logger.Error(msg);
					if (_testMode)
						Console.WriteLine(msg);
				}

				

#pragma warning disable 4014
				_logger.Info("Requesting additional time and resuming...");				
				requestPauseResumeTime();
				//try {_stateMachine.Transit(RuntimeServiceState.Started, CancellationToken.None).Wait(pauseResumeTimeoutMS);} catch {}
				//часть сервисов могла приостановиться, поэтому состояние хоста не целостное (сервисы в разных состояниях), и мы всё запускаем
				try {resume(_stateMachine.State, RuntimeServiceState.Started, CancellationToken.None);} catch {}
#pragma warning restore 4014

				throw new Exception(hasError ? "OnPause rolled back by error":"OnPause rolled back by timeout");
			}

			_logger.Info("Service '{0}' successfully processed OnPause command, elapsed: {1:hh\\:mm\\:ss\\.ff}", WinServiceName, w.Elapsed);
		}
		protected override void OnShutdown ()
		{
			base.OnShutdown();
		}
		protected override void OnStart (string[] args)
		{			
			base.OnStart(args);

			CancellationTokenSource src = new CancellationTokenSource();

			_logger.Info("Service '{0}' recieved OnStart command", WinServiceName);
			Task<RuntimeServiceState> tsk = _stateMachine.Transit(RuntimeServiceState.Started, src.Token, args);
			tsk.ContinueWith( (t) => src.Dispose() );

			bool res = false;
			System.Diagnostics.Stopwatch w = System.Diagnostics.Stopwatch.StartNew();

			bool hasError = false;
			try {
				res = tsk.Wait(startStopTimeoutMS);
			}
			catch (AggregateException ex)
			{			
				hasError = true;
				_logger.Error("Service '{0}' failed to process OnStart command", WinServiceName);
				ex.Handle( (err) => {
					if (err is OperationCanceledException)
					{
						_logger.Warn("Service '{0}' OnStart was cancelled", WinServiceName);
						if (_testMode)
							Console.WriteLine(string.Format("Service '{0}' OnStart was cancelled", WinServiceName));
						return true;
					}							
					else
					{
						_logger.Exception(string.Format("Service '{0}' failed to process OnStart command", WinServiceName), err);
						if (_testMode)
							Console.WriteLine(err.ToString());
					}
					return true;
				});
				
			}
			catch (OperationCanceledException)
			{
				hasError = true;
				_logger.Warn("Service '{0}' OnStart was cancelled", WinServiceName);
				if (_testMode)
					Console.WriteLine("Starting has been cancelled");				
			}
			catch (Exception ex)
			{
				hasError = true;
				_logger.Exception(string.Format("Service '{0}' failed to process OnStart command", WinServiceName), ex);
				if (_testMode)
					Console.WriteLine(ex.ToString());
			}
			finally {
				w.Stop();
			}

			if (!res)
			{
				try {src.Cancel();} catch {}
				if (!hasError)
				{
					string msg = string.Format("Timeout of starting service '{0}', elapsed: {1:hh\\:mm\\:ss\\.ff}", WinServiceName, w.Elapsed);
					_logger.Error(msg);
					if (_testMode)
						Console.WriteLine(msg);
				}

#pragma warning disable 4014
				_logger.Info("Requesting additional time and stopping...");
				requestStartStopTime();
				//try {_stateMachine.Transit(RuntimeServiceState.Stopped, CancellationToken.None).Wait(startStopTimeoutMS);} catch {}				
				//часть сервисов могла запуститься, поэтому состояние хоста не целостное (сервисы в разных состояниях), и мы всё останавливаем
				try{stop(RuntimeServiceState.Stopped, RuntimeServiceState.Stopped, CancellationToken.None).Wait(startStopTimeoutMS);} catch {}
#pragma warning restore 4014

				throw new Exception(hasError ? "OnStart rolled back by error":"OnStart rolled back by timeout");
			}

			_logger.Info("Service '{0}' successfully processed OnStart command, elapsed: {1:hh\\:mm\\:ss\\.ff}", WinServiceName, w.Elapsed);
		}
		protected override void OnStop ()
		{			
			base.OnStop();
			
			_logger.Info("Service '{0}' recieved OnStop command", WinServiceName);
			Task<RuntimeServiceState> tsk = _stateMachine.Transit(RuntimeServiceState.Stopped, CancellationToken.None);

			bool res = false;
			System.Diagnostics.Stopwatch w = System.Diagnostics.Stopwatch.StartNew();

			try {
				res = tsk.Wait(startStopTimeoutMS);
			}
			catch (AggregateException ex)
			{						
				_logger.Error("Service '{0}' failed to process OnStop command", WinServiceName);
				ex.Handle( (err) => {
					_logger.Exception(string.Format("Service '{0}' failed to process OnStop command", WinServiceName), err);
					if (_testMode)
						Console.WriteLine(err.ToString());

					return true;
				});
			}
			catch (Exception ex)
			{
				_logger.Exception(string.Format("Service '{0}' failed to process OnStop command", WinServiceName), ex);
				if (_testMode)
					Console.WriteLine(ex.ToString());
				throw;
			}
			finally {
				w.Stop();
			}

			if (!res)
			{
				string msg = string.Format("Timeout of stopping service '{0}', elapsed: {1:hh\\:mm\\:ss\\.ff}", WinServiceName, w.Elapsed);
				_logger.Error(msg);
				if (_testMode)
					Console.WriteLine(msg);

				_logger.Info("Requesting additional time and waiting...");
				requestStartStopTime();
				try {tsk.Wait(startStopTimeoutMS);} catch {}
				_logger.Info("Waiting finished, exitting the service");
			}
			else
				_logger.Info("Service '{0}' successfully processed OnStop command, elapsed: {1:hh\\:mm\\:ss\\.ff}", WinServiceName, w.Elapsed);
		}
		#endregion Overrides

		#region Transitions
		async Task<RuntimeServiceState> start (RuntimeServiceState from, RuntimeServiceState to, CancellationToken cts, object prms = null)
		{
			requestStartStopTime();

			/*List<Task<RuntimeServiceState>> waits = new List<Task<RuntimeServiceState>>(_services.Count());
			foreach (IRuntimeService srv in _services.OrderBy(s => s.StartingOrder))
				waits.Add(srv.Transit(RuntimeServiceState.Started, cts));
			
			if (waits.Count == 0)
			{
				_logger.Warn("Host '{0}': no services for starting", WinServiceName);
				return to;
			}
			else
				return await Task.WhenAll(waits).WithAllExceptionsArray().ConfigureAwait(false);*/

			int count = 0;
			foreach (IRuntimeService srv in _services.OrderBy(s => s.StartingOrder))
			{
				++count;
				await srv.Transit(RuntimeServiceState.Started, cts).WithAllExceptions().ConfigureAwait(false);
			}
			if (count == 0)
				_logger.Warn("Host '{0}': no services for starting", WinServiceName);
			return RuntimeServiceState.Started;
		}

		async Task<RuntimeServiceState> stop (RuntimeServiceState from, RuntimeServiceState to, CancellationToken cts, object prms = null)
		{
			requestStartStopTime();
			/*List<Task<RuntimeServiceState>> waits = new List<Task<RuntimeServiceState>>(_services.Count());
			foreach (IRuntimeService srv in _services.Where(s => s.State != RuntimeServiceState.Stopped && !s.IsInFinalState).OrderByDescending(s => s.StartingOrder))
				waits.Add(srv.Transit(RuntimeServiceState.Stopped, cts));
			
			if (waits.Count == 0)
			{
				_logger.Warn("Host '{0}': no services for stopping.\r\n{1}", WinServiceName, GetStates());
				return to;
			}
			else
				return await Task.WhenAll(waits).WithAllExceptionsArray().ConfigureAwait(false);*/
			int count = 0;
			foreach (IRuntimeService srv in _services.Where(s => s.State != RuntimeServiceState.Stopped && !s.IsInFinalState).OrderByDescending(s => s.StartingOrder))
			{
				++count;
				await srv.Transit(RuntimeServiceState.Stopped, cts).WithAllExceptions().ConfigureAwait(false);
			}
			if (count == 0)
				_logger.Warn("Host '{0}': no services for stoping", WinServiceName);
			return RuntimeServiceState.Stopped;
		}

		async Task<RuntimeServiceState> suspend (RuntimeServiceState from, RuntimeServiceState to, CancellationToken cts, object prms = null)
		{
			requestPauseResumeTime();
			/*List<Task<RuntimeServiceState>> waits = new List<Task<RuntimeServiceState>>(_services.Count());
			foreach (IRuntimeService srv in _services.Where(s => s.State != RuntimeServiceState.Suspended && !s.IsInFinalState).OrderByDescending(s => s.StartingOrder))
				waits.Add(srv.Transit(RuntimeServiceState.Suspended, cts));
			
			if (waits.Count == 0)
			{
				_logger.Warn("Host '{0}': no services for suspending.\r\n{1}", WinServiceName, GetStates());
				return to;
			}
			else
				return await Task.WhenAll(waits).WithAllExceptionsArray().ConfigureAwait(false);*/

			int count = 0;
			foreach (IRuntimeService srv in _services.Where(s => s.State != RuntimeServiceState.Suspended && !s.IsInFinalState).OrderByDescending(s => s.StartingOrder))
			{
				++count;
				await srv.Transit(RuntimeServiceState.Suspended, cts).WithAllExceptions().ConfigureAwait(false);
			}
			if (count == 0)
				_logger.Warn("Host '{0}': no services for suspending", WinServiceName);
			return RuntimeServiceState.Suspended;
		}

		async Task<RuntimeServiceState> resume (RuntimeServiceState from, RuntimeServiceState to, CancellationToken cts, object prms = null)
		{
			requestPauseResumeTime();
			/*List<Task<RuntimeServiceState>> waits = new List<Task<RuntimeServiceState>>(_services.Count());
			foreach (IRuntimeService srv in _services.Where(s => s.State != RuntimeServiceState.Started && !s.IsInFinalState).OrderBy(s => s.StartingOrder))
				waits.Add(srv.Transit(RuntimeServiceState.Started, cts));
			
			if (waits.Count == 0)
			{
				_logger.Warn("Host '{0}': no services for resuming.\r\n{1}", WinServiceName, GetStates());
				return to;
			}
			else
				return await Task.WhenAll(waits).WithAllExceptionsArray().ConfigureAwait(false);*/
			int count = 0;
			foreach (IRuntimeService srv in _services.Where(s => s.State != RuntimeServiceState.Started && !s.IsInFinalState).OrderBy(s => s.StartingOrder))
			{
				++count;
				await srv.Transit(RuntimeServiceState.Started, cts).WithAllExceptions().ConfigureAwait(false);
			}
			if (count == 0)
				_logger.Warn("Host '{0}': no services for resuming", WinServiceName);
			return RuntimeServiceState.Started;
		}

		async Task<RuntimeServiceState> close (RuntimeServiceState from, RuntimeServiceState to, CancellationToken cts, object prms = null)
		{			
			/*List<Task<RuntimeServiceState>> waits = new List<Task<RuntimeServiceState>>(_services.Count());
			foreach (IRuntimeService srv in _services.Where(s => !s.IsInFinalState).OrderByDescending(s => s.StartingOrder))
				waits.Add(srv.Transit(RuntimeServiceState.Closed, cts));

			if (waits.Count == 0)
			{
				_logger.Warn("Host '{0}': no services for closing.\r\n{1}", WinServiceName, GetStates());
				return to;
			}
			else
				return await Task.WhenAll(waits).WithAllExceptionsArray().ConfigureAwait(false);*/
			int count = 0;
			foreach (IRuntimeService srv in _services.Where(s => !s.IsInFinalState).OrderByDescending(s => s.StartingOrder))
			{
				++count;
				await srv.Transit(RuntimeServiceState.Closed, cts).WithAllExceptions().ConfigureAwait(false);
			}
			if (count == 0)
				_logger.Warn("Host '{0}': no services for closing", WinServiceName);
			return RuntimeServiceState.Closed;
		}
		#endregion Transitions

		void debugSuspendHandler (object sender, EventArgs args)
		{
			if (_stateMachine.State == RuntimeServiceState.Started)
				OnPause();
			else
				OnContinue();
		}
		void debugStopHandler (object sender, EventArgs args)
		{
			OnStop();			
		}
		int startStopTimeoutMS
		{
			get {
				return (AdditionalStatStopTimeInSeconds ?? DefaultSartStopTimeoutSeconds) * 1000;
			}
		}
		int pauseResumeTimeoutMS
		{
			get {
				return (AdditionalPauseResumeTimeInSeconds ?? DefaultPauseResumeTimeoutSeconds) * 1000;
			}
		}

		void requestPauseResumeTime ()
		{
			if (!_testMode && AdditionalPauseResumeTimeInSeconds != null)
				RequestAdditionalTime((int)TimeSpan.FromSeconds(AdditionalPauseResumeTimeInSeconds.Value).TotalMilliseconds);
		}
		void requestStartStopTime ()
		{
			if (!_testMode && AdditionalStatStopTimeInSeconds != null)
				RequestAdditionalTime((int)TimeSpan.FromSeconds(AdditionalStatStopTimeInSeconds.Value).TotalMilliseconds);
		}
		
		void stateChangingHndl (object sender, RuntimeServiceState state)
		{
			ServiceDebugForm form = _dbgForm;
			if (form != null)
				form.ShowState(state);
		}
		void stateChangedHandler (object sender, RuntimeServiceState oldState, RuntimeServiceState newState, Exception ex)
		{
			ServiceDebugForm form = _dbgForm;
			if (form != null)
			{
				if (ex == null)
					form.ShowState(newState);
				else
					form.ShowStateFailure(oldState, newState, ex);
			}
		}
		
	}
}
