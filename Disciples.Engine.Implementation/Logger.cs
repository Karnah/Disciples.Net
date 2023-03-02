using System;
using System.IO;
using Disciples.Engine.Base;

namespace Disciples.Engine.Implementation;

public class Logger : ILogger
{
    /// <summary>
    /// Имя файла с логами.
    /// </summary>
    private const string LOG_FILE_NAME = "Log.txt";

    /// <inheritdoc />
    public void Log(string message)
    {
        File.AppendAllText(LOG_FILE_NAME, $"{DateTime.UtcNow:dd.MM.yyyy HH:mm:ss:fff} {message}{Environment.NewLine}");
    }

    /// <inheritdoc />
    public void LogError(string message, Exception e)
    {
        File.AppendAllText(LOG_FILE_NAME, $"{DateTime.UtcNow:dd.MM.yyyy HH:mm:ss:fff} {message}" +
                                          $"{Environment.NewLine}{e}{Environment.NewLine}");
    }
}