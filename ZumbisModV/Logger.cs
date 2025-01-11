using System;
using System.IO;

namespace ZumbisModV
{
    public static class Logger
    {
        private static readonly string logFilePath = "./scripts/ZumbisModV.log";

        static Logger()
        {
            if (File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
            }
        }

        public static void LogInfo(string message)
        {
            Log(message, "INFO");
        }

        public static void LogWarning(string message)
        {
            Log(message, "WARNING");
        }

        public static void LogError(string message)
        {
            Log(message, "ERROR");
        }

        private static void Log(string message, string level)
        {
            string value = string.Format(
                "{0:dd-MM-yyyy HH:mm:ss} [{1}] {2}",
                DateTime.Now,
                level,
                message
            );
            using (StreamWriter streamWriter = File.AppendText(logFilePath))
            {
                streamWriter.WriteLine(value);
            }
        }
    }
}
