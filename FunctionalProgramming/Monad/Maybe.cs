﻿using System;

namespace FunctionalProgramming.Monad
{
    public interface IMaybe<out TValue>
    {
        bool IsEmpty { get; }
        TResult Match<TResult>(Func<TValue, TResult> just, Func<TResult> nothing);
    }

    /// <summary>
    /// Extension methods for monadic operations on Maybe&gt;T&lt;
    /// </summary>
    public static class Maybe
    {
        public static IMaybe<TValue> ToMaybe<TValue>(this TValue value)
        {
            return value == null ? Nothing<TValue>() : new Just<TValue>(value);
        }

        public static IMaybe<TValue> Nothing<TValue>()
        {
            return new Nadda<TValue>();
        }

        public static IMaybe<T> Where<T>(this IMaybe<T> m, Func<T, Boolean> predicate) 
        {
            return m.Match(
                just: value => predicate(value) ? new Just<T>(value) : Nothing<T>(),
                nothing: Nothing<T>);
        } 

        public static IMaybe<TResult> Select<TValue, TResult>(this IMaybe<TValue> m, Func<TValue, TResult> f)
        {
            return m.SelectMany(a => new Just<TResult>(f(a)));
        }

        public static IMaybe<TResult> SelectMany<TValue, TResult>(this IMaybe<TValue> m, Func<TValue, IMaybe<TResult>> f)
        {
            return m.Match(
                just: f,
                nothing: Nothing<TResult>);
        }

        /// <summary>
        /// Lifts function f from Tvalue =&lt; TResult to Maybe&gt;TValue&lt; =&lt; Maybe&gt;TResult&lt;, applies it, and then applies the lifted selector
        /// </summary>
        /// <typeparam name="TValue">The type wrapped in initial Maybe m </typeparam>
        /// <typeparam name="TResult">The type returned by f</typeparam>
        /// <typeparam name="TSelector">The type of the value returned by selector</typeparam>
        /// <param name="m">A Maybe of TValue</param>
        /// <param name="f">Function to lift</param>
        /// <param name="selector">Transformation function</param>
        /// <returns>
        /// IMaybe&gt;TSelector&lt;
        /// </returns>
        public static IMaybe<TSelector> SelectMany<TValue, TResult, TSelector>(this IMaybe<TValue> m, Func<TValue, IMaybe<TResult>> f, Func<TValue, TResult, TSelector> selector)
        {
            return m.SelectMany(a => f(a).SelectMany(b => new Just<TSelector>(selector(a, b)))); ;
        }

        public static T GetOrElse<T>(this IMaybe<T> m, Func<T> defaultValue)
        {
            return m.Match(
                just: arg => arg,
                nothing: defaultValue);
        }

        public static T GetOrError<T>(this IMaybe<T> m, Func<Exception> errorToThrow)
        {
            return m.Match(
                just: arg => arg,
                nothing: () => { throw errorToThrow(); });
        }

        private class Just<TValue> : IMaybe<TValue>
        {
            private readonly TValue _value;

            public Just(TValue value)
            {
                _value = value;
            }

            public bool IsEmpty { get { return false; } }

            public TResult Match<TResult>(Func<TValue, TResult> just, Func<TResult> nothing)
            {
                return just(_value);
            }

            public override bool Equals(object obj)
            {
                var retval = false;
                if (obj is Just<TValue>)
                {
                    var j = obj as Just<TValue>;
                    retval = _value.Equals(j._value);
                }
                return retval;
            }

            public override int GetHashCode()
            {
                return _value.GetHashCode();
            }

            public override string ToString()
            {
                return string.Format("Just({0})", _value);
            }
        }

        private class Nadda<TValue> : IMaybe<TValue>
        {
            public bool IsEmpty { get { return true; } }

            public TResult Match<TResult>(Func<TValue, TResult> just, Func<TResult> nothing)
            {
                return nothing();
            }

            public override bool Equals(object obj)
            {
                return obj is Nadda<TValue>;
            }

            public override int GetHashCode()
            {
                return 1;
            }

            public override string ToString()
            {
                return "Nothing";
            }
        }
    }

}
