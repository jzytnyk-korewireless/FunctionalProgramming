﻿using System;
using System.Collections.Generic;
using System.Linq;
using FunctionalProgramming.Monad;

using BF = FunctionalProgramming.Basics.BasicFunctions;

namespace FunctionalProgramming.Basics
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Sequence takes a list of computations of type IMaybe 'T, and builds from them a computation which will
        /// run each in turn and produce a list of the results.
        /// </summary>
        /// <typeparam name="T">Type of value yielded by each computation</typeparam>
        /// <param name="maybeTs">The list of computations</param>
        /// <returns>A single IMaybe computation of type IEnumerable 'T</returns>
        public static IMaybe<IEnumerable<T>> Sequence<T>(this IEnumerable<IMaybe<T>> maybeTs)
        {
            return BF.If(maybeTs.Any(),
                () =>
                    maybeTs.First()
                        .SelectMany(t => maybeTs.Skip(1).Sequence().SelectMany(ts => ((new[] {t}).Concat(ts)).ToMaybe())),
                () => Enumerable.Empty<T>().ToMaybe());
        }

        /// <summary>
        /// Sequence takes a list of computations of type Io 'T, and builds from them a computation which will
        /// run each in turn and produce a list of the results.
        /// </summary>
        /// <typeparam name="T">Type of value yielded by each computation</typeparam>
        /// <param name="ioTs">The list of computations</param>
        /// <returns>A single Io computation of type IEnumerable 'T</returns>
        public static Io<IEnumerable<T>> Sequence<T>(this IEnumerable<Io<T>> ioTs)
        {
            return BF.If(ioTs.Any(),
                () =>
                    ioTs.First()
                        .SelectMany(
                            t =>
                                ioTs.Skip(1)
                                    .Sequence()
                                    .SelectMany(ts => Io<IEnumerable<T>>.Apply(() => (new[] {t}).Concat(ts)))),
                () => Io<IEnumerable<T>>.Apply(Enumerable.Empty<T>));
        }

        /// <summary>
        /// ZipWithIndex takes a collection and pairs each element with its index in the collection
        /// </summary>
        /// <typeparam name="T">The type of elements in the IEnumerable</typeparam>
        /// <param name="xs">The IEnumerable to zip</param>
        /// <returns>An IEnumerable of Tuples where the first element of the tuple is the corresponding element from `xs` and the second element of the tuple is the index of that element</returns>
        public static IEnumerable<Tuple<T, int>> ZipWithIndex<T>(this IEnumerable<T> xs)
        {
            var i = 0;
            foreach (var x in xs)
            {
                yield return new Tuple<T, int>(x, i);
                i++;
            }
        }

        public static IEnumerable<T> LiftEnumerable<T>(this T t)
        {
            return new[] { t };
        }

        public static IMaybe<T> HeadOption<T>(this IEnumerable<T> xs)
        {
            return xs.FirstOrDefault().ToMaybe();
        }

        public static IMaybe<IEnumerable<T>> TailOption<T>(this IEnumerable<T> xs)
        {
            return xs.Any() ? xs.Skip(1).ToMaybe() : MaybeExtensions.Nothing<IEnumerable<T>>();
        }

        public static IEnumerable<T> Cons<T>(this T t, IEnumerable<T> xs)
        {
            return t.LiftEnumerable().Concat(xs);
        }

        public static T2 Match<T1, T2>(this IEnumerable<T1> xs, Func<T1, IEnumerable<T1>, T2> cons, Func<T2> nil)
        {
            return (from h in xs.HeadOption()
                    from t in xs.TailOption()
                    select cons(h, t)).GetOrElse(nil);
        }

        public static string MkString(this IEnumerable<char> chars)
        {
            return chars.Aggregate("", (str, c) => str + c.ToString());
        }
    }
}