using System.Diagnostics;
using static System.Threading.Parallel;

// Parallel Invoke
Action[] actions = new Action[]
{
    () => {
        for(int i = 0; i < 10; i += 2)
            Console.WriteLine(i);
    },
    () => {
        for(int i = 1; i < 10; i += 2)
            Console.WriteLine(i);
    }
};
Invoke(actions);

// Another example

Invoke(() =>
{
    for (int i = 0; i < 10; i += 2)
        Console.WriteLine(i);
}, () =>
{
    for (int i = 1; i < 10; i += 2)
        Console.WriteLine(i);
});
// Parallel Invoke



// Parallel For (int)
void someAction1(int i)
{
    Console.WriteLine(i);
}
For(-5, 5, someAction1);

// Another example

For(-5, 5, i => Console.WriteLine(i));

// Another example

For(-5, 5, i => Console.WriteLine(i), 1); // Optional increment argument
// Parallel For (int)



// Parallel For (long)
void someAction2(long i)
{
    Console.WriteLine(i);
}
For(long.MinValue, long.MinValue + 10, someAction2);

// Another example

For(long.MinValue, long.MinValue + 10, i => Console.WriteLine(i));

// Another example

For(long.MinValue, long.MinValue + 10, i => Console.WriteLine(i), 1); // Optional increment argument
// Parallel For (long)



// Parallel For (uint)
void someAction3(uint i)
{
    Console.WriteLine(i);
}
For(uint.MaxValue - 10, uint.MaxValue, someAction3);

// Another example

For(uint.MaxValue - 10, uint.MaxValue, i => Console.WriteLine(i));

// Another example

For(uint.MaxValue - 10, uint.MaxValue, i => Console.WriteLine(i), 1); // Optional increment argument
// Parallel For (uint)



// Parallel For (ulong)
void someAction4(ulong i)
{
    Console.WriteLine(i);
}
For(ulong.MaxValue - 10, ulong.MaxValue, someAction4);

// Another example

For(ulong.MaxValue - 10, ulong.MaxValue, i => Console.WriteLine(i));

// Another example

For(ulong.MaxValue - 10, ulong.MaxValue, i => Console.WriteLine(i), 1); // Optional increment argument
// Parallel For (ulong)



// Parallel Foreach
object[] collection1 = new object[10];
void someAction5(object o)
{
    // Do some work on the object
}
ForEach(collection1, someAction5);

// Another example

object[] collection2 = new object[10];
ForEach(collection2, o =>
{
    // Do some work on the object
});
// Parallel Foreach



// Parallel While
int flag1 = 0;
int workCount1 = 0;
bool conditonFunc()
{
    return flag1 == 0;
}
void someAction6()
{
    Interlocked.Increment(ref workCount1);
}
// 1 second wait
ThreadPool.QueueUserWorkItem(_ =>
{
    long endTime = Stopwatch.GetTimestamp() + Stopwatch.Frequency;
    while (Stopwatch.GetTimestamp() < endTime) ;
    Interlocked.Exchange(ref flag1, 1);
});
While(conditonFunc, someAction6);
Console.WriteLine(workCount1);

// Another example

int flag2 = 0;
int workCount2 = 0;
// 1 second wait
ThreadPool.QueueUserWorkItem(_ =>
{
    long endTime = Stopwatch.GetTimestamp() + Stopwatch.Frequency;
    while (Stopwatch.GetTimestamp() < endTime) ;
    Interlocked.Exchange(ref flag2, 1);
});
While(() => flag2 == 0, () => Interlocked.Increment(ref workCount2));
Console.WriteLine(workCount2);
// Parallel While