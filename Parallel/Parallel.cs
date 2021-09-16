using System.Collections.Generic;
using static System.Environment;
using static System.Math;
using static System.Threading.Interlocked;
using static System.Threading.SpinWait;
using static System.Threading.ThreadPool;

namespace System.Threading
{
    /// <summary>
    /// A lightweight parallel library not based on tasks.
    /// </summary>
    public static class Parallel
    {
        /// <summary>
        /// A lightweight implementation parallel invoke not based on tasks.
        /// </summary>
        /// <param name="actions">The collection of actions to execute in parallel.</param>
        /// <param name="useSpinWait">Prefer to use a spin wait mechanism instead of polling.</param>
        public static void Invoke(Action[] actions, bool useSpinWait = false)
        {
            int threads = 0, finishedThreads = 0, min = Min(ProcessorCount, actions?.Length ?? 0), idx = -1, spawn =1;

            while (CompareExchange(ref spawn, 0, 0) == 1 && threads < min)
            {
                if (QueueUserWorkItem(_ =>
                {
                    try
                    {
                        int crntIdx;
                        while ((crntIdx = Increment(ref idx)) < actions.Length)
                            actions[crntIdx]?.Invoke();
                    }
                    finally
                    {
                        _ = Exchange(ref spawn, 0);
                        _ = Increment(ref finishedThreads);
                    }
                }))
                {
                    threads++;
                }
            }

            if (useSpinWait)
                SpinUntil(() => CompareExchange(ref finishedThreads, 0, threads) == threads);
            else
                while (CompareExchange(ref finishedThreads, 0, threads) != threads) ;
        }

        /// <summary>
        /// A lightweight implementation parallel for loop not based on tasks.
        /// </summary>
        /// <param name="fromInclusive">The starting index.</param>
        /// <param name="toExclusive">The exclusive ending index.</param>
        /// <param name="body">The action to execute for each iteration.</param>
        /// <param name="increment">The increment for each iteration.</param>
        /// <param name="useSpinWait">Prefer to use a spin wait mechanism instead of polling.</param>
        public static void For(int fromInclusive, int toExclusive, Action<int> body, int increment = 1, bool useSpinWait = false)
        {
            if (toExclusive <= fromInclusive)
                throw new ArgumentOutOfRangeException("toExclusive must be greater than fromInclusive.");
            if (increment < 1)
                throw new ArgumentOutOfRangeException(nameof(increment), $"{nameof(increment)} must be greater than 0.");
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            int lowerBound = fromInclusive;
            int threads = 0, finishedThreads = 0, spawn = 1;

            fromInclusive -= increment;
            while (CompareExchange(ref spawn, 0, 0) == 1 && threads < ProcessorCount)
            {
                if (QueueUserWorkItem(_ =>
                {
                    try
                    {
                        int crntIteration;
                        while ((crntIteration = Add(ref fromInclusive, increment)) < toExclusive && crntIteration >= lowerBound)
                            body.Invoke(crntIteration);
                    }
                    finally
                    {
                        _ = Exchange(ref spawn, 0);
                        _ = Increment(ref finishedThreads);
                    }
                }))
                {
                    threads++;
                }
            }

            if (useSpinWait)
                SpinUntil(() => CompareExchange(ref finishedThreads, 0, threads) == threads);
            else
                while (CompareExchange(ref finishedThreads, 0, threads) != threads) ;
        }

        /// <summary>
        /// A lightweight implementation parallel for loop not based on tasks.
        /// </summary>
        /// <param name="fromInclusive">The starting index.</param>
        /// <param name="toExclusive">The exclusive ending index.</param>
        /// <param name="body">The action to execute for each iteration.</param>
        /// <param name="increment">The increment for each iteration.</param>
        /// <param name="useSpinWait">Prefer to use a spin wait mechanism instead of polling.</param>
        public static void For(long fromInclusive, long toExclusive, Action<long> body, long increment = 1, bool useSpinWait = false)
        {
            if (toExclusive <= fromInclusive)
                throw new ArgumentOutOfRangeException("toExclusive must be greater than fromInclusive.");
            if (increment < 1)
                throw new ArgumentOutOfRangeException(nameof(increment), $"{nameof(increment)} must be greater than 0.");
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            long lowerBound = fromInclusive;
            int threads = 0, finishedThreads = 0, spawn = 1;

            fromInclusive -= increment;
            while (CompareExchange(ref spawn, 0, 0) == 1 && threads < ProcessorCount)
            {
                if (QueueUserWorkItem(_ =>
                {
                    try
                    {
                        long crntIteration;
                        while ((crntIteration = Add(ref fromInclusive, increment)) < toExclusive && crntIteration >= lowerBound)
                            body.Invoke(crntIteration);
                    }
                    finally
                    {
                        _ = Exchange(ref spawn, 0);
                        _ = Increment(ref finishedThreads);
                    }
                }))
                {
                    threads++;
                }
            }

            if (useSpinWait)
                SpinUntil(() => CompareExchange(ref finishedThreads, 0, threads) == threads);
            else
                while (CompareExchange(ref finishedThreads, 0, threads) != threads) ;
        }

        /// <summary>
        /// A lightweight implementation parallel for loop not based on tasks.
        /// </summary>
        /// <param name="fromInclusive">The starting index.</param>
        /// <param name="toExclusive">The exclusive ending index.</param>
        /// <param name="body">The action to execute for each iteration.</param>
        /// <param name="increment">The increment for each iteration.</param>
        /// <param name="useSpinWait">Prefer to use a spin wait mechanism instead of polling.</param>
        public static void For(uint fromInclusive, uint toExclusive, Action<uint> body, int increment = 1, bool useSpinWait = false)
        {
            if (toExclusive <= fromInclusive)
                throw new ArgumentOutOfRangeException("toExclusive must be greater than fromInclusive.");
            if (increment < 1)
                throw new ArgumentOutOfRangeException(nameof(increment), $"{nameof(increment)} must be greater than 0.");
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            static int ToIntShift(uint num) => num > 2147483647 ? (int)(num - 2147483648) : -(int)(2147483648 - num);
            static uint ToUintShift(int num) => num > -1 ? (uint)num + 2147483648 : 2147483648 - (uint)-num;

            int lowerBound = ToIntShift(fromInclusive);
            int threads = 0, finishedThreads = 0, from = lowerBound - increment, to = ToIntShift(toExclusive), spawn = 1;

            while (CompareExchange(ref spawn, 0, 0) == 1 && threads < ProcessorCount)
            {
                if (QueueUserWorkItem(_ =>
                {
                    try
                    {
                        int crntIteration;
                        while ((crntIteration = Add(ref from, increment)) < to && crntIteration >= lowerBound)
                            body.Invoke(ToUintShift(crntIteration));
                    }
                    finally
                    {
                        _ = Exchange(ref spawn, 0);
                        _ = Increment(ref finishedThreads);
                    }
                }))
                {
                    threads++;
                }
            }

            if (useSpinWait)
                SpinUntil(() => CompareExchange(ref finishedThreads, 0, threads) == threads);
            else
                while (CompareExchange(ref finishedThreads, 0, threads) != threads) ;
        }

        /// <summary>
        /// A lightweight implementation parallel for loop not based on tasks.
        /// </summary>
        /// <param name="fromInclusive">The starting index.</param>
        /// <param name="toExclusive">The exclusive ending index.</param>
        /// <param name="body">The action to execute for each iteration.</param>
        /// <param name="increment">The increment for each iteration.</param>
        /// <param name="useSpinWait">Prefer to use a spin wait mechanism instead of polling.</param>
        public static void For(ulong fromInclusive, ulong toExclusive, Action<ulong> body, long increment = 1, bool useSpinWait = false)
        {
            if (toExclusive <= fromInclusive)
                throw new ArgumentOutOfRangeException("toExclusive must be greater than fromInclusive.");
            if (increment < 1)
                throw new ArgumentOutOfRangeException(nameof(increment), $"{nameof(increment)} must be greater than 0.");
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            static long ToLongShift(ulong num) => num > 9223372036854775807 ? (long)(num - 9223372036854775808) : -(long)(9223372036854775808 - num);
            static ulong ToULongShift(long num) => num > -1 ? (ulong)num + 9223372036854775808 : 9223372036854775808 - (ulong)-num;

            long lowerBound = ToLongShift(fromInclusive);
            int threads = 0, finishedThreads = 0, spawn = 1;
            long from = lowerBound - increment, to = ToLongShift(toExclusive);

            while (CompareExchange(ref spawn, 0, 0) == 1 && threads < ProcessorCount)
            {
                if (QueueUserWorkItem(_ =>
                {
                    try
                    {
                        long crntIteration;
                        while ((crntIteration = Add(ref from, increment)) < to && crntIteration >= lowerBound)
                            body.Invoke(ToULongShift(crntIteration));
                    }
                    finally
                    {
                        _ = Exchange(ref spawn, 0);
                        _ = Increment(ref finishedThreads);
                    }
                }))
                {
                    threads++;
                }
            }

            if (useSpinWait)
                SpinUntil(() => CompareExchange(ref finishedThreads, 0, threads) == threads);
            else
                while (CompareExchange(ref finishedThreads, 0, threads) != threads) ;
        }

        /// <summary>
        /// A lightweight implementation parallel foreach loop not based on tasks.
        /// </summary>
        /// <typeparam name="T">The type contained in the enumerable.</typeparam>
        /// <param name="source">The enumberable to loop through.</param>
        /// <param name="body">The action to execute on each element in the enumerable.</param>
        /// <param name="useSpinWait">Prefer to use a spin wait mechanism instead of polling.</param>
        public static void ForEach<T>(IEnumerable<T> source, Action<T> body, bool useSpinWait = false)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            int threads = 0, finishedThreads = 0, @lock = 0, spawn = 1;

            using IEnumerator<T> enumerator = source.GetEnumerator();
            while (CompareExchange(ref spawn, 0, 0) == 1 && threads < ProcessorCount)
            {
                if (QueueUserWorkItem(_ =>
                {
                    try
                    {
                        while (true)
                        {
                            while (CompareExchange(ref @lock, 1, 0) == 1) ;
                            if (enumerator.MoveNext())
                            {
                                T value = enumerator.Current;
                                _ = Exchange(ref @lock, 0);
                                body.Invoke(value);
                            }
                            else
                            {
                                _ = Exchange(ref @lock, 0);
                                break;
                            }
                        }
                    }
                    finally
                    {
                        _ = Exchange(ref spawn, 0);
                        _ = Increment(ref finishedThreads);
                    }
                }))
                {
                    threads++;
                }
            }

            if (useSpinWait)
                SpinUntil(() => CompareExchange(ref finishedThreads, 0, threads) == threads);
            else
                while (CompareExchange(ref finishedThreads, 0, threads) != threads) ;
        }

        /// <summary>
        /// Executes an action as long as a condition is true.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="body">The action to execute.</param>
        /// <param name="useSpinWait">Prefer to use a spin wait mechanism instead of polling.</param>
        /// <remarks>There is an inherent race condition, do not use for accurate code.</remarks>
        public static void While(Func<bool> condition, Action body, bool useSpinWait = false)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            int threads = 0, finishedThreads = 0, @lock = 0, spawn = 1;

            while (CompareExchange(ref spawn, 0, 0) == 1 && threads < ProcessorCount)
            {
                if (QueueUserWorkItem(_ =>
                {
                    try
                    {
                        while (true)
                        {
                            while (CompareExchange(ref @lock, 1, 0) == 1) ;
                            bool result = condition.Invoke();
                            _ = Exchange(ref @lock, 0);
                            if (result)
                            {
                                body.Invoke();
                                continue;
                            }
                            break;
                        }
                    }
                    finally
                    {
                        _ = Exchange(ref spawn, 0);
                        _ = Increment(ref finishedThreads);
                    }
                }))
                {
                    threads++;
                }
            }

            if (useSpinWait)
                SpinUntil(() => CompareExchange(ref finishedThreads, 0, threads) == threads);
            else
                while (CompareExchange(ref finishedThreads, 0, threads) != threads) ;
        }
    }
}
