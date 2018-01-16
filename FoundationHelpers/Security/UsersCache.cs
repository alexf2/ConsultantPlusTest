using System;
using System.Runtime.Caching;
using Ifx.FoundationHelpers.General;

namespace Ifx.FoundationHelpers.Security
{
	public static class UsersCache
	{
		static readonly MemoryCache _cache;
		static readonly CacheItemPolicy _policy = new CacheItemPolicy(){SlidingExpiration = TimeSpan.Parse(ConfigHelper.GetAppSetO<string>("Security_UsersCache_SlidingExpiration", "24:00:00"))};		

		static UsersCache ()
		{
			_cache = new MemoryCache("Security_UsersCache");
		}

		public static MemoryCache Instance
		{
			get {
				return _cache;
			}
		}

		public static CacheItemPolicy Policy
		{
			get {
				return _policy;
			}
		}
	}
}
