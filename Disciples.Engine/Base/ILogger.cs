using System;

namespace Disciples.Engine.Base
{
    public interface ILogger
    {
        void Log(string message);

        void LogError(string message, Exception e);
    }
}