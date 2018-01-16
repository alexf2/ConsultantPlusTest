using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Ifx.FoundationHelpers.RuntimeModel;
using Ifx.FoundationHelpers.StateMachine;
using Ifx.FoundationHelpers.General;

namespace Ifx.FoundationHelpers.RuntimeModel
{
	public class RuntimeServiceBase: IRuntimeService
	{
		const int DisposingTimeoutSeconds = 60;

		readonly protected IAbstractLogger _logger;
		readonly StateMachine<RuntimeServiceState> _stateMachine;

		public RuntimeServiceBase (string serviceName, int order, IAbstractLogger logger)
		{
			_logger = logger;
			StartingOrder = order;

			_stateMachine = StateMachine<RuntimeServiceState>.Create().SetName(serviceName).UseLogger(logger).UseStateDef(new RuntimeServiceStateDef())
				.TransitionTable(RuntimeServiceState.Stopped)
					.AddTrans(RuntimeServiceState.Started, Starting)
					.AddTrans(RuntimeServiceState.Closed, Closing)
					.Build()

				.TransitionTable(RuntimeServiceState.Started)
					.AddTrans(RuntimeServiceState.Stopped, Stopping)
					.AddTrans(RuntimeServiceState.Suspended, Suspending)
					.Build()

				.TransitionTable(RuntimeServiceState.Suspended)
					.AddTrans(RuntimeServiceState.Stopped, Closing)
					.AddTrans(RuntimeServiceState.Started, Resuming)
					.Build()
				.Build();
		}

		#region overrides
		public virtual Task<RuntimeServiceState> Starting (RuntimeServiceState from, RuntimeServiceState to, CancellationToken cts, object prms = null)
		{
			return Task<RuntimeServiceState>.FromResult(RuntimeServiceState.Started);
		}
		public virtual Task<RuntimeServiceState> Stopping (RuntimeServiceState from, RuntimeServiceState to, CancellationToken cts, object prms = null)
		{
			return Task<RuntimeServiceState>.FromResult(RuntimeServiceState.Stopped);
		}
		public virtual Task<RuntimeServiceState> Suspending (RuntimeServiceState from, RuntimeServiceState to, CancellationToken cts, object prms = null)
		{
			return Task<RuntimeServiceState>.FromResult(RuntimeServiceState.Suspended);
		}		
		public virtual Task<RuntimeServiceState> Resuming (RuntimeServiceState from, RuntimeServiceState to, CancellationToken cts, object prms = null)
		{
			return Task<RuntimeServiceState>.FromResult(RuntimeServiceState.Started);
		}
		public virtual Task<RuntimeServiceState> Closing (RuntimeServiceState from, RuntimeServiceState to, CancellationToken cts, object prms = null)
		{
			return Task<RuntimeServiceState>.FromResult(RuntimeServiceState.Closed);
		}
		#endregion overrides

		#region IRuntimeService
		public int StartingOrder
		{
			get;
			private set;
		}
		#endregion

		#region IStateMachine
		Task<RuntimeServiceState> IStateMachine<RuntimeServiceState>.Transit (RuntimeServiceState newState, CancellationToken cts, object args)
		{
			return _stateMachine.Transit(newState, cts, args);
		}

		string IStateMachine<RuntimeServiceState>.Name
		{
			get {
				return _stateMachine.Name;
			}
		}

		RuntimeServiceState IStateMachine<RuntimeServiceState>.State
		{
			get {
				return _stateMachine.State;
			}
		}

		RuntimeServiceState? IStateMachine<RuntimeServiceState>.TransitionState
		{
			get {
				return _stateMachine.TransitionState;
			}
		}

		bool IStateMachine<RuntimeServiceState>.IsInFinalState
		{
			get {
				return _stateMachine.IsInFinalState;
			}
		}

		IEnumerable<RuntimeServiceState> IStateMachine<RuntimeServiceState>.AllStates
		{
			get {
				return _stateMachine.AllStates;
			}
		}

		RuntimeServiceState IStateMachine<RuntimeServiceState>.StartState
		{
			get {
				return _stateMachine.StartState;
			}
		}

		RuntimeServiceState IStateMachine<RuntimeServiceState>.FinalState
		{
			get {
				return _stateMachine.FinalState;
			}
		}

		event StateChangingHandler<RuntimeServiceState> IStateMachine<RuntimeServiceState>.StateChanging
		{
			add {
				_stateMachine.StateChanging += value;
			}
			remove {
				_stateMachine.StateChanging -= value;
			}
		}
		event StateChangedHandler<RuntimeServiceState> IStateMachine<RuntimeServiceState>.StateChanged
		{
			add {
				_stateMachine.StateChanged += value;
			}
			remove {
				_stateMachine.StateChanged -= value;
			}
		}
		#endregion IStateMachine


		protected void CheckIfStarted (string actionName)
		{
			if (_stateMachine.State != RuntimeServiceState.Started)
				throw new Exception(string.Format("Can't perform '{0}': service {1} is not started. It is in '{2}' state", actionName, _stateMachine.Name, _stateMachine.State));
		}

		public void Dispose ()
		{
			if (!_stateMachine.IsInFinalState)
				try {
					_stateMachine.Transit(_stateMachine.FinalState, CancellationToken.None).Wait(DisposingTimeoutSeconds);
				}
				catch (Exception ex)
				{
					_logger.Exception(string.Format("At closing service '{0}'", _stateMachine.Name), ex);
				}
		}
		
	}
}

