using Ifx.FoundationHelpers.General;
using System.ServiceModel;
using System.ServiceModel.Security;

namespace Ifx.FoundationHelpers.Security
{
	public abstract class CustomUserAuthenticator<TRole, TUserId>
	{
		protected readonly IAbstractLogger _logger;

		public CustomUserAuthenticator (IAbstractLogger logger)
		{
			_logger = logger;
		}

		public static string MakeKey (string name)
		{
			return string.IsNullOrEmpty(name) ? name:name.ToLowerInvariant();
		}

		public UserPrincipalBase<TRole, TUserId> Identificate (string userName, string password)
		{
			UserPrincipalBase<TRole, TUserId> principal = null;
			string userKey = MakeKey(userName);

			//надо идентифицировать пользователя
			lock (UsersCache.Instance)
			{
				//проверяем условие гонок потоков (чтобы не идентифицировать одного юзера два раза)
				principal = (UserPrincipalBase<TRole, TUserId>)UsersCache.Instance.Get(userKey);
				if (principal == null) //если никто ещё на идентифицировал юзера, то идентифицируем и добавлем в кэш
				{
					string err;
					principal = AuthenticateUser (userName, password, out err);

					UsersCache.Instance.Add(new System.Runtime.Caching.CacheItem(userKey, principal), UsersCache.Policy);

					if (!principal.Identity.IsAuthenticated)
					{
						_logger.Do( (log) => log.Error(err) );
						ThrowSecurity(err);
					}
				} //если пользователя уже идентифицировал другой поток, то проверяем пароль
				else if (((IExtendedUserIdentity)principal.Identity).Passowrd != password)
				{
					//сюда попадать не должно
					string msg = MarkHlp.MarkGuid(string.Format("User's '{0}' password '{1}' mismatches to the cached user (second try)", userName, password));
					_logger.Do( (log) => log.Error(msg) );					
					ThrowSecurity(msg);
				}				
				else if (!principal.Identity.IsAuthenticated)
				{
					string msg = MarkHlp.MarkGuid(string.Format("User '{0}': access denied", userName));
					_logger.Do( (log) => log.Error(msg) );
					ThrowSecurity(msg);
				}
			}

			return principal;
		}

		public static void ThrowSecurity (string msg)
		{
			throw new FaultException<SecurityAccessDeniedException>(new SecurityAccessDeniedException(msg), msg);			
		}

		public abstract UserPrincipalBase<TRole, TUserId> AuthenticateUser (string userName, string password, out string errorMsg);
	}
}
