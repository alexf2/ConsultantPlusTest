using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Ifx.FoundationHelpers.Security
{
	public interface IExtendedUserIdentity
	{
		string Address
		{
			get;
			set;
		}

		string Passowrd
		{
			get;
		}
	}

	public class UserIdentityBase<T>: GenericIdentity, IExtendedUserIdentity
	{
		T _userID;
		string _pwd;
		readonly DateTime _dtCreated;
		string _addr;

		public UserIdentityBase (string name, string pwd): base(name, "CustomCredentials")
		{
			_pwd = pwd;
			_dtCreated = DateTime.UtcNow;
		}
		public UserIdentityBase (string name, string pwd, string type = "CustomCredentials"): base(name, type)
		{
			_pwd = pwd;
			_dtCreated = DateTime.UtcNow;
		}

		public override bool IsAuthenticated {
			get {
				return !EqualityComparer<T>.Default.Equals(_userID, default(T));
			}
		}

		public void Authenticate (T id)
		{
			if (EqualityComparer<T>.Default.Equals(id, default(T)))
				throw new System.ServiceModel.FaultException("UserIdentityBase<T>.Authenticate: user id can't be 0");

			UserID = id;
		}

		public override string ToString ()
		{
			return string.Format("name '{0}'/{3}, id '{1}'", Name, UserID, Address);
		}

		public T UserID
		{
			get {
				return _userID;
			}
			protected set {
				_userID = value;
			}
		}

		public string Address
		{
			get {return _addr;}			
		}

		string IExtendedUserIdentity.Address
		{
			get {return _addr;}			
			set {_addr = value;}			
		}

		string IExtendedUserIdentity.Passowrd
		{
			get {
				return _pwd;
			}			
		}
	}
}
