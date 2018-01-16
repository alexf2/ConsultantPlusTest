using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Ifx.FoundationHelpers.General;

namespace Ifx.FoundationHelpers.StateMachine
{
	public sealed class StateMachineBuilder<TState> where TState: struct
	{
		string _name;
		IAbstractLogger _logger;
		StateDef<TState> _stateDef;
		bool _logTansitions = true;
		Dictionary<TState, TransitionDef<TState>[]> _dic = new Dictionary<TState, TransitionDef<TState>[]>();

		public StateMachineBuilder<TState> SetName (string name)
		{
			_name = name;
			return this;
		}
		public StateMachineBuilder<TState> UseLogger (IAbstractLogger logger)
		{
			_logger = logger;
			return this;
		}

		public StateMachineBuilder<TState> UseStateDef (StateDef<TState> def)
		{
			_stateDef = def;
			return this;
		}

		public StateBuilder TransitionTable (TState state)
		{
			return new StateBuilder(this, state);
		}

		public StateMachine<TState> Build ()
		{
			check (_name, "SetName");
			check (_logger, "UseLogger");
			check (_stateDef, "UseStateDef");
			if (_dic.Count == 0)
				throw new Exception("States are not defined");

			return new StateMachine<TState>(_name, _logger, _stateDef, _dic, _logTansitions);
		}

		void check<T> (T val, string name)
		{
			if (object.Equals(val, default(T)))
				throw new Exception(string.Format("'{0}' was not called on the builder", name));
		}

		public sealed class StateBuilder
		{
			readonly TState _state;
			readonly List<TransitionDef<TState>> _trans = new List<TransitionDef<TState>>();
			readonly StateMachineBuilder<TState> _parent;

			internal StateBuilder (StateMachineBuilder<TState> parent, TState st)
			{
				_parent = parent;
				_state = st;
			}

			public StateBuilder AddTrans (TState st, Func<TState, TState, CancellationToken, object, Task<TState>> action)
			{
				_trans.Add(new TransitionDef<TState>(){NewState = st, StateActivityAsync = action});
				return this;
			}

			public StateMachineBuilder<TState>  Build ()
			{
				_parent._dic.Add(_state, _trans.ToArray());
				return _parent;
			}
		};
	}
}
