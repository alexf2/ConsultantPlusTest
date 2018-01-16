
namespace Ifx.FoundationHelpers.General
{
	public interface IMonitorable<T>
	{
		/// <summary>
		/// Выполняет шаг мониторинга объекта.
		/// </summary>		
		void PerformMonitoring (T monitorActivity);
	}
}
