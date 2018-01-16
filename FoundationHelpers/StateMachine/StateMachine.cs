using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Ifx.FoundationHelpers.General;

namespace Ifx.FoundationHelpers.StateMachine
{
	/// <summary>
	/// Представляет конечный автомат.
	/// </summary>
	/// <typeparam name="TState">Enum, определяющий состояния. Состояния делятся на транзитные и устойчивые.</typeparam>
	public class StateMachine<TState>: IStateMachine<TState> where TState: struct
	{		
		readonly Dictionary<TState, TransitionDef<TState>[]> _stateTransitions;
		readonly StateDef<TState> _state;
		readonly IAbstractLogger _logger;
		readonly bool _logTransitions;

		TState _current;
		TState? _transitionalState;

		int _isInTransition;		

		public StateMachine (string name, IAbstractLogger logger, StateDef<TState> stateDef, Dictionary<TState, TransitionDef<TState>[]> transitionMap, bool logTransitions = true)
		{
			if (logger == null)
				throw new ArgumentNullException("logger");

			_logger = logger;
			_stateTransitions = new Dictionary<TState, TransitionDef<TState>[]>();
			_logTransitions = logTransitions;
			foreach (var kv in transitionMap)
			{
				TransitionDef<TState>[] tmp = (TransitionDef<TState>[])kv.Value.Clone();
				Array.Sort<TransitionDef<TState>>(tmp);
				_stateTransitions.Add(kv.Key, tmp);
			}
			_state = stateDef;
			Name = name;
		}

		

		public static StateMachineBuilder<TState> Create ()
		{
			return new StateMachineBuilder<TState>();
		}

		#region IStateMachine
		public event StateChangingHandler<TState> StateChanging;
		public event StateChangedHandler<TState> StateChanged;

		public string Name
		{
			get;
			private set;
		}

		public TState State
		{
			get {Thread.MemoryBarrier(); return _current;}
			private set {
				StateDef<TState>.Validate(value);
				Thread.MemoryBarrier();
				_current = value;
			}
		}
		public TState? TransitionState
		{
			get {Thread.MemoryBarrier(); return _transitionalState;}
			private set {
				if (value != null)
					StateDef<TState>.Validate(value.Value);
				Thread.MemoryBarrier();
				_transitionalState = value;
			}
		}

		Task<TState> compliteWithError (Exception ex)
		{
			TaskCompletionSource<TState> src = new TaskCompletionSource<TState>();
			src.SetException(ex);
			return src.Task;
		}

		public async Task<TState> Transit (TState newState, CancellationToken cts, object args = null)
		{
			if (_logTransitions)
				_logger.Info("Machine '{0}' is about to transit {1} --> {2}", Name, State, newState);

			if (!StateDef<TState>.IsValid(newState))
				return await compliteWithError( new Exception(string.Format("State '{0}' is invalid for machine '{1}'", newState, Name)) );				

			if (IsInFinalState)
				return await compliteWithError( new Exception(string.Format("Machine '{0}' can't transit to state '{1}' because it is in final state '{2}'", Name, newState, _state.Final)) );

			int res = Interlocked.Exchange(ref _isInTransition, 1); //блокировка транзишенов до тех пор, пока не заевршится текущий транзишен
			if (res != 0)
			{
				if (EqualityComparer<TState>.Default.Equals(newState, FinalState)) //разрешаем только рекурсивный Close
					_logger.Warn("Machine '{0}' started reqursive transition {1} --> {2}", Name, State, newState);
				else
					return await compliteWithError( new Exception(string.Format("In machine '{0}' reqursive transition {1} --> {2} has been detected", Name, State, newState)) );
			}

			TState transitionResult = State;
			CancellationTokenRegistration reg = default(CancellationTokenRegistration);
			bool lockCleared = false;
			try {				
				TState stateFrom = State;
				TransitionDef<TState> transition = getTransition(stateFrom, newState);
				if (transition == TransitionDef<TState>.Empty)
					throw new Exception(string.Format("Machine '{0}' didn't find transition {1} --> {2}", Name, stateFrom, newState));

				if (_logTransitions)
					_logger.Info("Machine '{0}' is transiting {1} --> {2}", Name, stateFrom, newState);

				if (transition.StateActivityAsync != null)
				{
					try {
						TransitionState = _state.GetTransitionalState(State, newState);
						if (TransitionState != null)
							onStateChanging(TransitionState.Value);
				
						reg = cts.Register(() => {lockCleared = true; Interlocked.Exchange(ref _isInTransition, 0);});
						transitionResult = await transition.StateActivityAsync(stateFrom, newState, cts, args).WithAllExceptions().ConfigureAwait(false);

						cts.ThrowIfCancellationRequested();
						State = transitionResult;
						if (_logTransitions)
							_logger.Info("Machine '{0}' transited {1} --> {2}", Name, stateFrom, newState);
						onStateChanged(stateFrom, transitionResult);
					}
					catch (AggregateException ex)
					{												
						_logger.Error("Machine '{0}' failed to transit {1} --> {2}", Name, stateFrom, newState);
						onStateChanged(stateFrom, transitionResult, ex);
						ex.Handle( (err) => {
							if (err is OperationCanceledException)
							{
								_logger.Warn("Machine '{0}' transition {1} --> {2} was cancelled", Name, stateFrom, newState);
								return true;
							}							
							return false;
						});
					}
					catch (OperationCanceledException ce)
					{
						_logger.Warn("Machine '{0}' transition {1} --> {2} was cancelled", Name, stateFrom, newState);
						onStateChanged(stateFrom, transitionResult, ce);
					}
					catch (Exception ex)
					{
						_logger.Exception(string.Format("Machine '{0}' failed to transit {1} --> {2}", Name, stateFrom, newState), ex);
						onStateChanged(stateFrom, transitionResult, ex);
						throw;
					}
				}
				else
				{
					State = newState;
					transitionResult = newState;
					if (_logTransitions)
						_logger.Info("Machine '{0}' transited {1} --> {2} with no action", Name, stateFrom, newState);
					onStateChanged(stateFrom, newState);
				}
			}
			finally {
				if (!lockCleared)
				{
					Interlocked.Exchange(ref _isInTransition, 0);
					TransitionState = null;
				}
			}

			return transitionResult;				
		}

		public bool IsInFinalState
		{
			get {
				return EqualityComparer<TState>.Default.Equals(_current, _state.Final);
			}
		}

		public IEnumerable<TState> AllStates
		{
			get {
				return (IEnumerable<TState>)_state;
			}
		}

		public TState StartState
		{
			get {
				return _state.Start;
			}
		}

		public TState FinalState
		{
			get {
				return _state.Final;
			}
		}
		#endregion

		TransitionDef<TState> getTransition (TState stFrom, TState stTo)
		{
			TransitionDef<TState>[] tmp;
			if (!_stateTransitions.TryGetValue(stFrom, out tmp))
				return TransitionDef<TState>.Empty;

			TransitionDef<TState> arg = new TransitionDef<TState>(){NewState = stTo};
			int idx = Array.BinarySearch<TransitionDef<TState>>(tmp, arg);

			return idx < 0 ? TransitionDef<TState>.Empty:tmp[ idx ];
		}

		void onStateChanged (TState oldState, TState newState, Exception ex = null)
		{
			StateChangedHandler<TState> h = StateChanged;
 			if (h != null)
			{
				h(this, oldState, newState, ex);
			}
		}

		void onStateChanging (TState state)
		{
			StateChangingHandler<TState> h = StateChanging;
 			if (h != null)
			{
				h(this, state);
			}
		}
	}

}
