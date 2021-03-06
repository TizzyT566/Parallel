using System.Collections.Generic;
using static System.Environment;
using static System.Math;
using static System.Threading.Interlocked;
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
        public static void Invoke(params Action[] actions)
        {
            if (actions == null)
                throw new ArgumentNullException(nameof(actions));
            if (actions.Length == 0)
                throw new ArgumentException($"{nameof(actions)} cannot be empty.");

            int threads = 1, finishedThreads = 0, min = Min(ProcessorCount, actions.Length), idx = -1, spawn = 1;

            void work(object obj = default)
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
            }

            while (threads < min && Volatile.Read(ref spawn) == 1)
                if (QueueUserWorkItem(work))
                    threads++;

            work();

            while (Volatile.Read(ref finishedThreads) < threads) ;
        }

        /// <summary>
        /// A lightweight implementation parallel for loop not based on tasks.
        /// </summary>
        /// <param name="fromInclusive">The starting index.</param>
        /// <param name="toExclusive">The exclusive ending index.</param>
        /// <param name="action">The action to execute for each iteration.</param>
        /// <param name="increment">The increment for each iteration.</param>
        public static void For(int fromInclusive, int toExclusive, Action<int> action, int increment = 1)
        {
            if (toExclusive <= fromInclusive)
                throw new ArgumentOutOfRangeException(nameof(toExclusive), $"{nameof(toExclusive)} must be greater than {nameof(fromInclusive)}.");
            if (increment < 1)
                throw new ArgumentOutOfRangeException(nameof(increment), $"{nameof(increment)} must be greater than 0.");
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            int lowerBound = fromInclusive;
            int threads = 1, finishedThreads = 0, spawn = 1;

            fromInclusive -= increment;

            void work(object obj = default)
            {
                try
                {
                    int crntIteration;
                    while ((crntIteration = Add(ref fromInclusive, increment)) < toExclusive && crntIteration >= lowerBound)
                        action(crntIteration);
                }
                finally
                {
                    _ = Exchange(ref spawn, 0);
                    _ = Increment(ref finishedThreads);
                }
            }

            while (threads < ProcessorCount && Volatile.Read(ref spawn) == 1)
                if (QueueUserWorkItem(work))
                    threads++;

            work();

            while (Volatile.Read(ref finishedThreads) < threads) ;
        }

        /// <summary>
        /// A lightweight implementation parallel for loop not based on tasks.
        /// </summary>
        /// <param name="fromInclusive">The starting index.</param>
        /// <param name="toExclusive">The exclusive ending index.</param>
        /// <param name="action">The action to execute for each iteration.</param>
        /// <param name="increment">The increment for each iteration.</param>
        public static void For(long fromInclusive, long toExclusive, Action<long> action, long increment = 1)
        {
            if (toExclusive <= fromInclusive)
                throw new ArgumentOutOfRangeException(nameof(toExclusive), $"{nameof(toExclusive)} must be greater than {nameof(fromInclusive)}.");
            if (increment < 1)
                throw new ArgumentOutOfRangeException(nameof(increment), $"{nameof(increment)} must be greater than 0.");
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            long lowerBound = fromInclusive;
            int threads = 1, finishedThreads = 0, spawn = 1;

            fromInclusive -= increment;

            void work(object obj = default)
            {
                try
                {
                    long crntIteration;
                    while ((crntIteration = Add(ref fromInclusive, increment)) < toExclusive && crntIteration >= lowerBound)
                        action(crntIteration);
                }
                finally
                {
                    _ = Exchange(ref spawn, 0);
                    _ = Increment(ref finishedThreads);
                }
            }

            while (threads < ProcessorCount && Volatile.Read(ref spawn) == 1)
                if (QueueUserWorkItem(work))
                    threads++;

            work();

            while (Volatile.Read(ref finishedThreads) < threads) ;
        }

        /// <summary>
        /// A lightweight implementation parallel for loop not based on tasks.
        /// </summary>
        /// <param name="fromInclusive">The starting index.</param>
        /// <param name="toExclusive">The exclusive ending index.</param>
        /// <param name="action">The action to execute for each iteration.</param>
        /// <param name="increment">The increment for each iteration.</param>
        public static void For(uint fromInclusive, uint toExclusive, Action<uint> action, int increment = 1)
        {
            if (toExclusive <= fromInclusive)
                throw new ArgumentOutOfRangeException(nameof(toExclusive), $"Must be greater than {nameof(fromInclusive)}.");
            if (increment < 1)
                throw new ArgumentOutOfRangeException(nameof(increment), $"Must be greater than 0.");
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            static int ToIntShift(uint num) => num > 2147483647 ? (int)(num - 2147483648) : -(int)(2147483648 - num);
            static uint ToUintShift(int num) => num > -1 ? (uint)num + 2147483648 : 2147483648 - (uint)-num;

            int lowerBound = ToIntShift(fromInclusive);
            int threads = 1, finishedThreads = 0, from = lowerBound - increment, to = ToIntShift(toExclusive), spawn = 1;

            void work(object obj = default)
            {
                try
                {
                    int crntIteration;
                    while ((crntIteration = Add(ref from, increment)) < to && crntIteration >= lowerBound)
                        action(ToUintShift(crntIteration));
                }
                finally
                {
                    _ = Exchange(ref spawn, 0);
                    _ = Increment(ref finishedThreads);
                }
            }

            while (threads < ProcessorCount && Volatile.Read(ref spawn) == 1)
                if (QueueUserWorkItem(work))
                    threads++;

            work();

            while (Volatile.Read(ref finishedThreads) < threads) ;
        }

        /// <summary>
        /// A lightweight implementation parallel for loop not based on tasks.
        /// </summary>
        /// <param name="fromInclusive">The starting index.</param>
        /// <param name="toExclusive">The exclusive ending index.</param>
        /// <param name="action">The action to execute for each iteration.</param>
        /// <param name="increment">The increment for each iteration.</param>
        public static void For(ulong fromInclusive, ulong toExclusive, Action<ulong> action, long increment = 1)
        {
            if (toExclusive <= fromInclusive)
                throw new ArgumentOutOfRangeException(nameof(toExclusive), $"Must be greater than {nameof(fromInclusive)}.");
            if (increment < 1)
                throw new ArgumentOutOfRangeException(nameof(increment), $"Must be greater than 0.");
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            static long ToLongShift(ulong num) => num > 9223372036854775807 ? (long)(num - 9223372036854775808) : -(long)(9223372036854775808 - num);
            static ulong ToULongShift(long num) => num > -1 ? (ulong)num + 9223372036854775808 : 9223372036854775808 - (ulong)-num;

            long lowerBound = ToLongShift(fromInclusive);
            int threads = 1, finishedThreads = 0, spawn = 1;
            long from = lowerBound - increment, to = ToLongShift(toExclusive);

            void work(object obj = default)
            {
                try
                {
                    long crntIteration;
                    while ((crntIteration = Add(ref from, increment)) < to && crntIteration >= lowerBound)
                        action(ToULongShift(crntIteration));
                }
                finally
                {
                    _ = Exchange(ref spawn, 0);
                    _ = Increment(ref finishedThreads);
                }
            }

            while (threads < ProcessorCount && Volatile.Read(ref spawn) == 1)
                if (QueueUserWorkItem(work))
                    threads++;

            work();

            while (Volatile.Read(ref finishedThreads) < threads) ;
        }

        /// <summary>
        /// A lightweight implementation parallel foreach loop not based on tasks.
        /// </summary>
        /// <typeparam name="T">The type contained in the enumerable.</typeparam>
        /// <param name="source">The enumberable to loop through.</param>
        /// <param name="body">The action to execute on each element in the enumerable.</param>
        public static void ForEach<T>(IEnumerable<T> source, Action<T> body)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            int threads = 1, finishedThreads = 0, @lock = 0, spawn = 1;

            using IEnumerator<T> enumerator = source.GetEnumerator();

            void work(object obj = default)
            {
                try
                {
                    while (true)
                    {
                        while (Exchange(ref @lock, 1) == 1) ;
                        if (enumerator.MoveNext())
                        {
                            T value = enumerator.Current;
                            _ = Exchange(ref @lock, 0);
                            body(value);
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
            }

            while (threads < ProcessorCount && Volatile.Read(ref spawn) == 1)
                if (QueueUserWorkItem(work))
                    threads++;

            work();

            while (Volatile.Read(ref finishedThreads) < threads) ;
        }

        /// <summary>
        /// A lightweight implementation parallel while loop not based on tasks.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="action">The action to execute.</param>
        /// <remarks>There is a potential for a race condition, do not use for accurate code.</remarks>
        public static void While(Func<bool> condition, Action action)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            int threads = 1, finishedThreads = 0, @lock = 0, spawn = 1;

            void work(object obj = default)
            {
                try
                {
                    while (true)
                    {
                        while (Exchange(ref @lock, 1) == 1) ;
                        bool result = condition();
                        _ = Exchange(ref @lock, 0);
                        if (!result)
                            break;
                        action();
                    }
                }
                finally
                {
                    _ = Exchange(ref spawn, 0);
                    _ = Increment(ref finishedThreads);
                }
            }

            while (threads < ProcessorCount && Volatile.Read(ref spawn) == 1)
                if (QueueUserWorkItem(work))
                    threads++;

            work();

            while (Volatile.Read(ref finishedThreads) < threads) ;
        }
    }
}
