using System.Collections.Generic;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System;

using Ifx.FoundationHelpers.General;

namespace Ifx.FoundationHelpers.StateMachine
{
	/// <summary>
	/// Базовый класс для определения дескриптора состояний конечного автомата.
	/// </summary>
	/// <typeparam name="TState">Enum, используемый для состояний.</typeparam>
	public abstract class StateDef<TState>: IEnumerable<TState>, ICloneable where TState: struct
	{
		static readonly HashSet<TState> _validStates = new HashSet<TState>();

		static StateDef ()
		{
			RegisterValues(EnumUtils.GetValues<TState>());
		}

		static void RegisterValues (IEnumerable<TState> statesDef)
		{
			lock (_validStates)
			{
				if (_validStates.Count != 0)
					throw new Exception(string.Format("Values for '{0}' already has been registered", typeof(TState).Name));
				

				foreach (TState v in statesDef)
					_validStates.Add(v);
			}
		}		

		/// <summary>
		/// По переходу состояний возвращает транзитное состояние.
		/// </summary>
		/// <param name="from">Исходное устойчивое состояние.</param>
		/// <param name="to">Целевое устойчивое состояние.</param>
		/// <returns></returns>
		public abstract TState GetTransitionalState (TState from, TState to);
		/// <summary>
		/// Начальное состояние.
		/// </summary>
		public abstract TState Start
		{
			get;
		}
		/// <summary>
		/// Конечное состояние (из него невозможен переход ни в какие другие состояния).
		/// </summary>
		public abstract TState Final
		{
			get;
		}						

		object ICloneable.Clone ()
		{
			return MemberwiseClone();
		}
		

		IEnumerator<TState> IEnumerable<TState>.GetEnumerator ()
		{
			return _validStates.GetEnumerator();
		}
	
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return (IEnumerator)_validStates.GetEnumerator();
		}

		/// <summary>
		/// Проверяет состояние state на допустимое по типу enum TState.
		/// </summary>		
		public static bool IsValid (TState state)
		{
			return _validStates.Contains(state);
		}
		
		/// <summary>
		/// Проверяет состояние на допустимость по типу enum TState и выбрасывает исключение, если оно не допустимо.
		/// </summary>
		public static void Validate (TState val)
		{
			if (!_validStates.Contains(val))
				throw new Exception(string.Format("Value '{0}' is invalid for '{0}'", val, typeof(TState).Name));
		}
	}
}
