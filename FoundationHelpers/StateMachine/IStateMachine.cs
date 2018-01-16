using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ifx.FoundationHelpers.StateMachine
{
	/// <summary>
	/// Обработчик транзитного состояния.
	/// </summary>
	/// <typeparam name="TState">Enum, описывающий состояния.</typeparam>
	/// <param name="sender">Конечный автомат</param>
	/// <param name="state">Транзитное состояние, в которое вошёл сервис</param>
	public delegate void StateChangingHandler<TState> (object sender, TState state);

	/// <summary>
	/// Обработчик стойивого состояния.
	/// </summary>
	/// <typeparam name="TState">Enum, описывающий состояния.</typeparam>
	/// <param name="sender">Конечный автомат</param>
	/// <param name="oldState">Старое состояние</param>
	/// <param name="newState">Новое состояние</param>
	/// <param name="ex">Исключение, если переход не удался</param>
	public delegate void StateChangedHandler<TState> (object sender, TState oldState, TState newState, Exception ex = null);	

	/// <summary>
	/// Представляет конечный автомат.
	/// </summary>
	/// <typeparam name="TState">Enum, описывающий состояния.</typeparam>
	public interface IStateMachine<TState> where TState: struct
	{		
		/// <summary>
		/// Вызывает асинхронный переход автомата из текущего состояния в состояние newState.
		/// Если в таблице состояний перехода нет, то кидается исключение.
		/// </summary>
		/// <param name="newState">Новое состояние.</param>
		/// <param name="cts">Токен для отмены перехода.</param>
		/// <param name="args">Опциальные аргументы для перехода.</param>
		/// <returns>Задача, на которой можно ожидать завершения перехода.</returns>
		Task<TState> Transit (TState newState, CancellationToken cts, object args = null);

		/// <summary>
		/// Имя конечного автомата.
		/// </summary>
		string Name
		{
			get;			
		}

		/// <summary>
		/// Текущее состояние автомата.
		/// </summary>
		TState State
		{
			get;
		}
		/// <summary>
		/// Транзитное состояние (если автомат в процессе перехода).
		/// </summary>
		TState? TransitionState
		{
			get;
		}

		/// <summary>
		/// Если true, то автомат в начальном состоянии.
		/// </summary>
		bool IsInFinalState
		{
			get;
		}

		/// <summary>
		/// Коллекция всех возможных состояний автомата.
		/// </summary>
		IEnumerable<TState> AllStates
		{
			get;
		}

		/// <summary>
		/// Начальное состояние автомата.
		/// </summary>
		TState StartState
		{
			get;
		}

		/// <summary>
		/// Конечное состояние автомата.
		/// </summary>
		TState FinalState
		{
			get;
		}

		/// <summary>
		/// Вызывается, когда автомат переходит в транзитное состояние.
		/// </summary>
		event StateChangingHandler<TState> StateChanging;
		/// <summary>
		/// Вызывается, когда автомат завершил переход в устойчивое состояние.
		/// </summary>
		event StateChangedHandler<TState> StateChanged;
	}
}
