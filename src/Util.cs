using System;

public static class Util
{
    public static void Assert(bool condition)
    {
#if DEBUG
        if (!condition)
        {
            throw new Exception();
        }
#endif
    }

    public static void Log(string log, params object[] args) =>
        Console.WriteLine(string.Format(log, args));
}
