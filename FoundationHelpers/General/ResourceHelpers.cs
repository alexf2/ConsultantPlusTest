using System;
using System.IO;
using System.Reflection;

namespace Ifx.FoundationHelpers.General
{
	public static class ResourceHelpers
	{
		public static string GetStringResource (string rcName, string namespaceName = null)
		{
			if (string.IsNullOrEmpty(namespaceName))
			{
				namespaceName = Assembly.GetCallingAssembly().GetName().Name;
			}
			string resourceName = string.Format("{0}.{1}", namespaceName, rcName);
			Stream stm = Assembly.GetCallingAssembly().GetManifestResourceStream(resourceName);
			if (stm == null)
			{
				throw new Exception(string.Format("Resource '{0}' isn't found in assembly '{1}'", resourceName, Assembly.GetCallingAssembly().GetName().Name));
			}
			using (StreamReader reader = new StreamReader(stm))
			{
				return reader.ReadToEnd();
			}
		}

        public static Stream GetStringResourceAsStream (string rcName, string namespaceName = null)
        {
            if (string.IsNullOrEmpty(namespaceName))
            {
                namespaceName = Assembly.GetCallingAssembly().GetName().Name;
            }
            string resourceName = string.Format("{0}.{1}", namespaceName, rcName);
            Stream stm = Assembly.GetCallingAssembly().GetManifestResourceStream(resourceName);
            if (stm == null)
            {
                throw new Exception(string.Format("Resource '{0}' isn't found in assembly '{1}'", resourceName, Assembly.GetCallingAssembly().GetName().Name));
            }
            return stm;
        }

		public static byte[] GetRawResource (string rcName, string namespaceName = null)
		{
			if (string.IsNullOrEmpty(namespaceName))
			{
				namespaceName = Assembly.GetCallingAssembly().GetName().Name;
			}
			string resourceName = string.Format("{0}.{1}", namespaceName, rcName);
			Stream stm = Assembly.GetCallingAssembly().GetManifestResourceStream(resourceName);
			if (stm == null)
			{
				throw new Exception(string.Format("Resource '{0}' isn't found in assembly '{1}'", resourceName, Assembly.GetCallingAssembly().GetName().Name));
			}
			using (BinaryReader reader = new BinaryReader(stm))
			{
				return reader.ReadBytes((int)stm.Length);
			}
		}


	}
}
