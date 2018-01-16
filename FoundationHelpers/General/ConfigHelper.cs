using System;
using System.Configuration;
using System.Globalization;


namespace Ifx.FoundationHelpers.General
{
	public static class ConfigHelper
	{
		public static T GetAppSetM<T> (string name)
		{
			string val = ConfigurationManager.AppSettings[ name ];
			val = val.Do( v => v.Trim() );
			if (string.IsNullOrEmpty(val))
			{
				throw new ConfigurationErrorsException(string.Format("Mandatory appSettings '{0}' isn't found or empty", name));
			}

			try {
				return (T)System.Convert.ChangeType(val, typeof(T), CultureInfo.InvariantCulture);
			}
			catch (Exception ex)
			{
				throw new ConfigurationErrorsException(string.Format("Can't parse mandatory appSettings '{0}'", name), ex);
			}
		}

		public static T GetAppSetO<T> (string name, T defVal = default(T))
		{
			string val = ConfigurationManager.AppSettings[ name ];
			val = val.Do( v => v.Trim() );
			if (string.IsNullOrEmpty(val))
			{
				return defVal;
			}

			try {
				return (T)System.Convert.ChangeType(val, typeof(T), CultureInfo.InvariantCulture);
			}
			catch (Exception ex)
			{
				throw new ConfigurationErrorsException(string.Format("Can't parse mandatory appSettings '{0}'", name), ex);
			}
		}
	}
}
