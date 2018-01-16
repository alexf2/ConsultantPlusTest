using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ifx.FoundationHelpers.StateMachine;

namespace Ifx.FoundationHelpers.RuntimeModel
{
	/// <summary>
	/// Стандартные состояния сервиса.
	/// </summary>
	public enum RuntimeServiceState
	{		
		//Stable states
		Stopped,		
		Started,		
		Suspended,
		Closed, //final

		//Transit states
		Suspending,
		Resuming,
		Starting,
		Stopping,
		Closing
	};

	/// <summary>
	/// Представляет сервис, управляемый через конечный автомат.
	/// </summary>
	public interface IRuntimeService: IDisposable, IStateMachine<RuntimeServiceState>
	{				
		/// <summary>
		/// Позволяет задать последовательность запуска сервисов. Остановка будет в обратном порядке.
		/// </summary>
		int StartingOrder
		{
			get;
		}
	}
}
