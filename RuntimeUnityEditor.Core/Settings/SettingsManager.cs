using System.IO;
using UnityEngine;

namespace RuntimeUnityEditor.Core.Settings
{
    internal static class SettingsManager
    {
        internal static readonly string SETTINGS_FILE_NAME = "settings.json";

        internal static void Save(SettingsData settingsData)
        {
            string json = JsonUtility.ToJson(settingsData, true);
            File.WriteAllText(GetSettingsPath(), json);
        }

        internal static SettingsData LoadOrCreate()
        {
            string path = GetSettingsPath();
            if (File.Exists(path))
                return JsonUtility.FromJson<SettingsData>(File.ReadAllText(path));
            else
                return new SettingsData();
        }

        private static string GetSettingsPath()
        {
            string assemblyLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location);
            return Path.Combine(assemblyLocation, SETTINGS_FILE_NAME);
        }
    }
}
