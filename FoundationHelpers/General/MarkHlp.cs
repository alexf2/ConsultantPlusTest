using System;

namespace Ifx.FoundationHelpers.General
{
	public static class MarkHlp
	{
		public static string MarkGuid (string str)
		{			
			return string.Format("{0}, mark {1}", str, Guid.NewGuid().ToString("D"));
		}
	}
}
