using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Ifx.FoundationHelpers.Security
{
	public class UserPrincipalBase<TRole, TUserId>: IPrincipal
	{
		readonly Dictionary<string, TRole> _roles = new Dictionary<string, TRole>(StringComparer.OrdinalIgnoreCase); //userName, role
		readonly Func<TRole, string> _roleIdExtractor;
		bool _isClean = true;

		public UserPrincipalBase (UserIdentityBase<TUserId> userIdentity, Func<TRole, string> roleIdExtractor)
		{
			Identity = userIdentity;
			_roleIdExtractor = roleIdExtractor;
		}

		protected void Authorize (IEnumerable<TRole> roles)
		{
			if (roles != null)
				AddRoles(roles);
			_isClean = false;
		}

		public bool IsClean
		{
			get {return _isClean;}
		}

		public bool IsAuthorized
		{
			get;
			set;
		}

		public string AuthorizationErrorMsg
		{
			get;
			set;
		}
		
		#region IPrincipal
		public IIdentity Identity
		{ 
			get;
			private set;
		}

		public virtual bool IsInRole (string role)
		{
			return _roles.ContainsKey(role);
		}
		#endregion IPrincipal

		void AddRole (TRole role)
		{
			_roles.Add(_roleIdExtractor(role), role);
		}

		void AddRoles (IEnumerable<TRole> roles)
		{
			foreach (TRole r in roles)
				AddRole(r);
		}
	}
}
