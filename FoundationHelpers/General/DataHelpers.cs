using System.Data;

namespace Ifx.FoundationHelpers.General
{
	public static class DataHelpers
	{
		public static T GetValOrDefault<T> (this IDataRecord row, string dbFieldName)
		{
			return row.GetValOrDefault<T>(row.GetOrdinal(dbFieldName));
		}

		public static T GetValOrDefault<T> (this IDataRecord row, int dbFieldOrdinal)
		{
			return row.IsDBNull(dbFieldOrdinal) ? default(T) : (T)row.GetValue(dbFieldOrdinal);
		}
	}
}
