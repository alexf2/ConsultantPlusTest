using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ifx.FoundationHelpers.StateMachine;

namespace Ifx.FoundationHelpers.RuntimeModel
{
	/// <summary>
	/// Определение состояний для Windows сервиса.
	/// </summary>
	public sealed class RuntimeServiceStateDef: StateDef<RuntimeServiceState>
	{
		public override RuntimeServiceState GetTransitionalState (RuntimeServiceState from, RuntimeServiceState to)
		{
			if (from == RuntimeServiceState.Stopped && to == RuntimeServiceState.Started)
				return RuntimeServiceState.Starting;

			if (from == RuntimeServiceState.Started && to == RuntimeServiceState.Suspended)
				return RuntimeServiceState.Suspending;

			if (from == RuntimeServiceState.Suspended && to == RuntimeServiceState.Started)
				return RuntimeServiceState.Resuming;

			if (from == RuntimeServiceState.Started && to == RuntimeServiceState.Stopped)
				return RuntimeServiceState.Stopping;

			if (from == RuntimeServiceState.Stopped && to == RuntimeServiceState.Closed)
				return RuntimeServiceState.Closing;

			throw new Exception(string.Format("Transition {0} --> {1} is invalid", from, to));
		}
		/// <summary>
		/// Начальное состояние сервиса.
		/// </summary>
		public override RuntimeServiceState Start
		{
			get {
				return RuntimeServiceState.Stopped;
			}
		}
		/// <summary>
		/// Заключительное состояние сервиса.
		/// </summary>
		public override RuntimeServiceState Final
		{
			get {
				return RuntimeServiceState.Closed;
			}
		}
	}
}
