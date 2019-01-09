using System;
using Disciples.Engine.Base;

namespace Disciples.Engine.Implementation
{
    public class Logger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine($"{DateTime.UtcNow:dd.MM.yyyy HH:mm:ss:fff} {message}");
        }
    }
}