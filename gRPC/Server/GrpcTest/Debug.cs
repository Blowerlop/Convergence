public static class Debug
{
    private const ConsoleColor _LOG_COLOR = ConsoleColor.White;
    private const ConsoleColor _LOG_WARNING_COLOR = ConsoleColor.Yellow;
    private const ConsoleColor _LOG_ERROR_COLOR = ConsoleColor.Red;
    public const ConsoleColor SEPARATOR_COLOR = ConsoleColor.Green;


    public static void Log(object message, ConsoleColor customColor = _LOG_COLOR)
    {
        Console.ForegroundColor = customColor;
        Console.WriteLine(message);
    }

    public static void LogWarning(object message, ConsoleColor customColor = _LOG_WARNING_COLOR)
    {
        Console.ForegroundColor = customColor;
        Console.WriteLine(message);
    }

    public static void LogError(object message, ConsoleColor customColor = _LOG_ERROR_COLOR)
    {
        Console.ForegroundColor = customColor;
        Console.WriteLine(message);
    }
}