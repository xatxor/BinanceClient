using Nancy.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BinanceCore.Services
{
    public class AppSettings<T> where T : new()
    {
        private const string DEFAULT_FILENAME = "settings.json";

        public void Save(string fileName = DEFAULT_FILENAME)
        {
            File.WriteAllText(fileName, (new JavaScriptSerializer()).Serialize(this));
        }

        public static void Save(T pSettings, string fileName = DEFAULT_FILENAME)
        {
            File.WriteAllText(fileName, (new JavaScriptSerializer()).Serialize(pSettings));
        }

        public static T Load(string fileName = DEFAULT_FILENAME)
        {
            T t = new T();
            if (File.Exists(fileName))
                return new JavaScriptSerializer().Deserialize<T>(File.ReadAllText(fileName));
            else throw new Exception($"Config file '{fileName}' not found");
        }
    }
}
