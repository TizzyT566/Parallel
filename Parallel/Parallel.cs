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
            int threads = 0, finishedThreads = 0, min = Min(ProcessorCount, actions.Length), idx = -1;
            for (int i = 0; i < min; i++)
            {
                if (QueueUserWorkItem(_ =>
                {
                    try
                    {
                        int crntIdx;
                        while ((crntIdx = Increment(ref idx)) < actions.Length)
                            actions[crntIdx].Invoke();
                    }
                    finally
                    {
                        _ = Increment(ref finishedThreads);
                    }
                }))
                {
                    threads++;
                }
            }
            if (useSpinWait)
                SpinUntil(() => finishedThreads == threads);
            else
                while (finishedThreads != threads) ;
        }

        /// <summary>
        /// A lightweight implementation parallel for loop not based on tasks.
        /// </summary>
        /// <param name="from">The starting index.</param>
        /// <param name="toExclusive">The exclusive ending index.</param>
        /// <param name="body">The action to execute for each iteration.</param>
        /// <param name="useSpinWait">Prefer to use a spin wait mechanism instead of polling.</param>
        public static void For(int from, int toExclusive, Action<int> body, bool useSpinWait = false)
        {
            int threads = 0, finishedThreads = 0, min = Min(ProcessorCount, toExclusive - from);
            for (int i = 0; i < min; i++)
            {
                if (QueueUserWorkItem(_ =>
                {
                    try
                    {
                        int crntIteration;
                        while ((crntIteration = Increment(ref from)) <= toExclusive)
                            body.Invoke(crntIteration - 1);
                    }
                    finally
                    {
                        _ = Increment(ref finishedThreads);
                    }
                }))
                {
                    threads++;
                }
            }
            if (useSpinWait)
                SpinUntil(() => finishedThreads == threads);
            else
                while (finishedThreads != threads) ;
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
            int threads = 0, finishedThreads = 0, _lock = 0;
            using (IEnumerator<T> enumerator = source.GetEnumerator())
            {
                for (int i = 0; i < ProcessorCount; i++)
                {
                    if (QueueUserWorkItem(_ =>
                    {
                        try
                        {
                            while (true)
                            {
                                while (CompareExchange(ref _lock, 1, 0) == 1) ;
                                if (enumerator.MoveNext())
                                {
                                    T value = enumerator.Current;
                                    _ = Exchange(ref _lock, 0);
                                    body.Invoke(value);
                                }
                                else
                                {
                                    _ = Exchange(ref _lock, 0);
                                    break;
                                }
                            }
                        }
                        finally
                        {
                            _ = Increment(ref finishedThreads);
                        }
                    }))
                    {
                        threads++;
                    }
                }
                if (useSpinWait)
                    SpinUntil(() => finishedThreads == threads);
                else
                    while (finishedThreads != threads) ;
            }
        }
    }
}
