using System;

namespace Ifx.FoundationHelpers.General
{
    public static class MonadExtentions
    {
        public delegate TResult Func<in T, out TResult>(T input);

        public static TResult With<TInput, TResult>(this TInput input, Func<TInput, TResult> evaluator)
            where TResult : class
            where TInput : class
        {
            if (input == null)
            {
                return null;
            }

            return evaluator(input);
        }

        public static TResult Return<TInput, TResult>(this TInput o,
                                                      Func<TInput, TResult> evaluator, TResult failureValue)
            where TInput : class
        {
            if (o == null) return failureValue;

            return evaluator(o);
        }


        public static TInput If<TInput>(this TInput o, Func<TInput, bool> evaluator)
            where TInput : class
        {
            if (o == null) return null;

            return evaluator(o) ? o : null;
        }

        public static TResult Cast<TResult>(this object o)
            where TResult : class
        {
            if (o == null) return null;

            return o as TResult;
        }

        public static TResult DoCast<TResult>(this object o, Action<TResult> action)
            where TResult : class
        {
            if (o == null) return null;

            return Do(o as TResult, action);
        }


        public static TInput Unless<TInput>(this TInput o, Func<TInput, bool> evaluator)
            where TInput : class
        {
            if (o == null) return null;

            return evaluator(o) ? null : o;
        }

        public static TInput Do<TInput>(this TInput o, Action<TInput> action)
            where TInput : class
        {
            if (o == null) return null;

            action(o);

            return o;
        }
    }
}