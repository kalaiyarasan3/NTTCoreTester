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
    => ConsoleColor.Green.WriteLine(message);

    public static void Info(this string message)
        => ConsoleColor.Cyan.WriteLine(message);

    public static void Warn(this string message)
        => ConsoleColor.Yellow.WriteLine(message);

    public static void Error(this string message)
        => ConsoleColor.Red.WriteLine(message);

    public static void Debug(this string message)
        => ConsoleColor.DarkGray.WriteLine(message);

}