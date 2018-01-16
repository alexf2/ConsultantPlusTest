using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ifx.FoundationHelpers.General
{
	/// <summary>
	/// Представляет вспомогательные классы для работы с await.
	/// </summary>
	public static class AwaitHelpers
	{
		/// <summary>
		/// Данный вызов нужно присоединять справа к таску, на котором используется await, чтобы получить AggregatedException, а не распакованное первое исключение.
		/// </summary>
		/// <typeparam name="T">Тип ReturnValue таска.</typeparam>
		/// <param name="task">Таск, на котором выполняется await</param>		
		public static Task<T> WithAllExceptions<T> (this Task<T> task)
		{
			TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();

			task.ContinueWith(ignored =>
			{
				switch (task.Status)
				{
					case TaskStatus.Canceled:
						tcs.SetCanceled();
						break;
					case TaskStatus.RanToCompletion:
						tcs.SetResult(task.Result);
						break;
					case TaskStatus.Faulted:
						// SetException will automatically wrap the original AggregateException
						// in another one. The new wrapper will be removed in TaskAwaiter, leaving
						// the original intact.
						tcs.SetException(task.Exception);
						break;
					default:
						tcs.SetException(new InvalidOperationException("Continuation called illegally."));
						break;
				}
			});

			return tcs.Task;
		}

		/// <summary>
		/// Используется с Task.WhenAll, чтобы вернуть таск, совместимый по типу ReturnValue из композитного метода (где ожидаются сразу несколько тасков).
		/// </summary>
		/// <typeparam name="T">Тип ReturnValue таска.</typeparam>
		/// <param name="task">Таск, на котором выполняется await</param>
		/// <returns></returns>
		public static Task<T> WithAllExceptionsArray<T> (this Task<T[]> task)
		{
			TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();

			task.ContinueWith(ignored =>
			{
				switch (task.Status)
				{
					case TaskStatus.Canceled:
						tcs.SetCanceled();
						break;
					case TaskStatus.RanToCompletion:
						tcs.SetResult(task.Result.Length > 0 ? task.Result[0]:default(T));
						break;
					case TaskStatus.Faulted:
						// SetException will automatically wrap the original AggregateException
						// in another one. The new wrapper will be removed in TaskAwaiter, leaving
						// the original intact.
						tcs.SetException(task.Exception);
						break;
					default:
						tcs.SetException(new InvalidOperationException("Continuation called illegally."));
						break;
				}
			});

			return tcs.Task;
		} 
	}
}
