using System;
using System.IO;

namespace Ifx.FoundationHelpers.General
{
	public static class PathHelper
	{
		public static string ExtendPath (string p)
		{
			Uri uri;
			if (!Uri.TryCreate(p, UriKind.RelativeOrAbsolute, out uri))
			{
				throw new Exception(string.Format("Path '{0}' is not valid", p));
			}
			else if (!uri.IsAbsoluteUri)
			{
				return Path.GetFullPath(p);
			}
			
			return p;
		}

		public static void EnsureDirectoryExists (string path)
		{
			string[] directories = path.Split(new char[]{Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar}, StringSplitOptions.RemoveEmptyEntries);

			string previousEntry = string.Empty;
			if (null != directories)
			{
				foreach (string direc in directories)
				{
					string newEntry = previousEntry == string.Empty ? direc:previousEntry + Path.DirectorySeparatorChar + direc;
					if (!string.IsNullOrEmpty(newEntry))
					{
						if (!newEntry.Equals(Convert.ToString(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
						{
							if (!newEntry.EndsWith(":"))
							{
								if (!Directory.Exists(newEntry))
									Directory.CreateDirectory(newEntry);
							}
							previousEntry = newEntry;
						}
					}
				}
			}
		}
	}
}
