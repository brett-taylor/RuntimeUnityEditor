using Harmony;
using RuntimeUnityEditor.Core;
using System.Reflection;
using UnityEngine;

namespace RuntimeUnityEditorForSubnautica
{
    public static class EntryPoint
    {
        public static RuntimeUnityEditorCore INSTANCE { get; private set; }

        public static void Entry()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("taylor.brett.RuntimeUnityEditorForSubnautica.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        internal static void CreateRuntimeEditor(GameObject objectToAddTo)
        {
            if (INSTANCE == null)
            {
                INSTANCE = objectToAddTo.AddComponent<RuntimeUnityEditorCore>();
                INSTANCE.Setup(
                    new RuntimeEditorLogger(), 
                    showCursor => {
                        UWE.Utils.alwaysLockCursor = !showCursor;
                        UWE.Utils.lockCursor = !showCursor;
                    }
                );
            }
        }
    }
}
