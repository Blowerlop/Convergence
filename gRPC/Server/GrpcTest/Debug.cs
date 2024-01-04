public static class Debug
{
    public const ConsoleColor logColor = ConsoleColor.White;
    public const ConsoleColor logWarningColor = ConsoleColor.Yellow;
    public const ConsoleColor logErrorColor = ConsoleColor.Red;
    public const ConsoleColor separatorColor = ConsoleColor.Green;


    public static void Log(object message, ConsoleColor customColor = logColor)
    {
        Console.ForegroundColor = customColor;
        Console.WriteLine(message);
    }

    public static void LogWarning(object mesage, ConsoleColor customColor = logWarningColor)
    {
        Console.ForegroundColor = customColor;
        Console.WriteLine(mesage);
    }

    public static void LogError(object mesage, ConsoleColor customColor = logErrorColor)
    {
        Console.ForegroundColor = customColor;
        Console.WriteLine(mesage);
    }
}