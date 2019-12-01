using RuntimeUnityEditor.Core;

namespace RuntimeUnityEditorForSubnautica
{
    public class RuntimeEditorLogger : ILoggerWrapper
    {
        public void Log(LogLevel logLevel, object content)
        {
            if (logLevel == LogLevel.Warning || logLevel == LogLevel.Error || logLevel == LogLevel.Fatal)
            {
                ErrorMessage.AddMessage(content.ToString());
            }

            UnityEngine.Debug.Log($"[RuntimeEditor] [{logLevel}]: {content}");
        }
    }
}
