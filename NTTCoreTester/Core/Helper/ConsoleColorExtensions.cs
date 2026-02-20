namespace NTTCoreTester.Core.Helper;

public static class ConsoleColorExtensions
{
    public static void WriteLine(this ConsoleColor fg, string value)
    {
        var origFg = Console.ForegroundColor;
        Console.ForegroundColor = fg;
        Console.WriteLine(value);
        Console.ForegroundColor = origFg;
    }

    public static void Write(this ConsoleColor fg, string value)
    {
        var origFg = Console.ForegroundColor;
        Console.ForegroundColor = fg;
        Console.Write(value);
        Console.ForegroundColor = origFg;
    }

    public static void WriteLine(this (ConsoleColor fg, ConsoleColor bg) colors, string value)
    {
        var origFg = Console.ForegroundColor;
        var origBg = Console.BackgroundColor;

        Console.ForegroundColor = colors.fg;
        Console.BackgroundColor = colors.bg;

        Console.WriteLine(value);

        Console.ForegroundColor = origFg;
        Console.BackgroundColor = origBg;
    }

    public static void Success(this string message)
    => ConsoleColor.Green.WriteLine($"[SUCCESS] {message}");

    public static void Info(this string message)
        => ConsoleColor.Cyan.WriteLine($"[INFO]    {message}");

    public static void Warn(this string message)
        => ConsoleColor.Yellow.WriteLine($"[WARN]    {message}");

    public static void Error(this string message)
        => ConsoleColor.Red.WriteLine($"[ERROR]   {message}");

    public static void Debug(this string message)
        => ConsoleColor.DarkGray.WriteLine($"[DEBUG]   {message}");
     
}