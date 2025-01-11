using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ZumbisModV.Static
{
    public static class Serializer
    {
        private const string DefaultCrashLogPath = "./scripts/ZumbisModVCrashLog.txt";

        public static T Deserialize<T>(string path)
        {
            if (!File.Exists(path))
            {
                return default(T);
            }
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    BinaryFormatter serializer = new BinaryFormatter();
                    return (T)serializer.Deserialize(fs);
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                return default(T);
            }
        }

        public static void Serialize<T>(string path, T obj)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    BinaryFormatter serializer = new BinaryFormatter();
                    serializer.Serialize(fs, obj);
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        private static void LogError(Exception ex)
        {
            File.AppendAllText(
                DefaultCrashLogPath,
                string.Format("\n[{0}] {1}", DateTime.UtcNow.ToShortDateString(), ex.Message)
            );
        }
    }
}
