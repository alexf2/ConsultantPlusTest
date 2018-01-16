using System;
using System.Threading;

namespace Ifx.FoundationHelpers.General
{
	public static class SafeReleasers
	{
		/// <summary>
		/// Безопасным образом освобождает таймер.
		/// </summary>
		/// <param name="timer">Таймер.</param>
		/// <param name="tLock">Объект для блокировки таймера.</param>
		/// <param name="wait">Ждать завершения таймера.</param>
		/// <returns>true - если таймер был не null.</returns>
		public static bool ReleaseTimer (ref Timer timer, object tLock = null, bool wait = false)
		{
			Timer t = Interlocked.Exchange(ref timer, null);
			Action actRel = () => {
				if (wait)
				{
					using (ManualResetEvent ev = new ManualResetEvent(false))
					{
						if (t.Dispose(ev))
						{
							ev.WaitOne();
						}
					}
				}
				else
				{
					t.Dispose();
				}
			};

			if (t != null)
			{
				if (tLock != null)
				{
					lock (tLock)
					{
						actRel();
					}
				}
				else
				{
					actRel();
				}
			}

			return t != null;
		}

		public static void DisposeObj<T> (ref T obj) where T:class, IDisposable
		{
			T tmp = Interlocked.Exchange(ref obj, null);
			if (tmp != null)
			{
				tmp.Dispose();
			}			
		}
	}
}
