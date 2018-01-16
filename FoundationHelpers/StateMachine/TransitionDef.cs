using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ifx.FoundationHelpers.StateMachine
{
	public struct TransitionDef<TState>: IEquatable<TransitionDef<TState>>, IComparable<TransitionDef<TState>>, IComparable
	{
		static TransitionDef<TState> _empty = new TransitionDef<TState>();
		public static TransitionDef<TState> Empty
		{
			get {
				return _empty;
			}
		}

		public TState NewState;
		public Func<TState, TState, CancellationToken, object, Task<TState>> StateActivityAsync;

		public bool Equals (TransitionDef<TState> other)
		{
			return EqualityComparer<TState>.Default.Equals(NewState, other.NewState) && StateActivityAsync == other.StateActivityAsync;
		}

		public override bool Equals (object obj)
		{
			if (!(obj is TransitionDef<TState>))
				return false;
			return Equals( (TransitionDef<TState>)obj );
		}
		public override int GetHashCode ()
		{
			int res = 381 ^ NewState.GetHashCode();
			if (StateActivityAsync != null)
				res ^= StateActivityAsync.GetHashCode();
			return res;
		}

		public static bool operator == (TransitionDef<TState> n1, TransitionDef<TState> n2)
		{
			return n1.Equals (n2);
		}
		public static bool operator != (TransitionDef<TState> n1, TransitionDef<TState> n2)
		{
			return !(n1 == n2);
		}

		public int CompareTo (TransitionDef<TState> other)
		{
			if (Equals(other))
				return 0;
			return Comparer<TState>.Default.Compare(NewState, other.NewState);
		}
		int IComparable.CompareTo (object other)
		{
			if (!(other is TransitionDef<TState>))
				throw new InvalidOperationException ("CompareTo: Not a TransitionDef<T>");
			return CompareTo( (TransitionDef<TState>)other );
		}
		public static bool operator < (TransitionDef<TState> n1, TransitionDef<TState> n2)
		{
			return n1.CompareTo(n2) < 0;
		}
		public static bool operator > (TransitionDef<TState> n1, TransitionDef<TState> n2)
		{
			return n1.CompareTo(n2) > 0;
		}
	}
}
