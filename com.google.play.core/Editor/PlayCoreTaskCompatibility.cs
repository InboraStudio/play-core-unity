using UnityEngine;
using UnityEditor;

namespace Google.Play.Core.Editor
{
    [InitializeOnLoad]
    public class PlayCoreTaskCompatibility
    {
        private const string CUSTOM_GRADLE_TEMPLATE_WARNING = "Custom Gradle template should be enabled for Unity 6000+ compatibility";
        private const string MEMORY_MANAGEMENT_WARNING = "Ensure proper disposal of AndroidJavaObjects in your Play Core implementations";
        
        static PlayCoreTaskCompatibility()
        {
            #if UNITY_6000_OR_NEWER
            CheckCompatibility();
            #endif
        }

        private static void CheckCompatibility()
        {
            Debug.Log("Checking Play Core Task compatibility for Unity 6000+");
            
            // Check Gradle template using the correct PlayerSettings API
            #if UNITY_ANDROID
            if (!EditorUserBuildSettings.androidBuildSystem.Equals(AndroidBuildSystem.Gradle))
            {
                Debug.LogWarning("Android build system must be set to Gradle for Play Core functionality.");
            }
            
            var gradleProp = typeof(EditorUserBuildSettings).GetProperty("exportAsGradleProject", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            
            if (gradleProp == null || !(bool)gradleProp.GetValue(null))
            {
                Debug.LogWarning(CUSTOM_GRADLE_TEMPLATE_WARNING);
            }
            #endif

            // Memory management warning
            Debug.LogWarning(MEMORY_MANAGEMENT_WARNING);
        }

        // Moved menu item to separate method
        [MenuItem("Google Play Core/Check Memory Management")]
        public static void ShowMemoryManagementCheck()
        {
            EditorUtility.DisplayDialog("Memory Management Check",
                "Unity 6000+ Requirements:\n\n" +
                "1. Use 'using' statements for AndroidJavaObject\n" +
                "2. Explicitly dispose of JNI references\n" +
                "3. Avoid storing AndroidJavaObject long-term\n" +
                "4. Check for memory leaks in Play Core callbacks",
                "OK");
        }

        public static void ValidateAndroidJavaObjectUsage(AndroidJavaObject obj, string context)
        {
            #if UNITY_6000_OR_NEWER
            if (obj != null && !obj.GetRawObject().Equals(System.IntPtr.Zero))
            {
                Debug.LogWarning($"Active AndroidJavaObject detected in {context}. Ensure proper disposal.");
            }
            #endif
        }
    }

    // Separate example class moved outside of PlayCoreTaskCompatibility
    public class PlayCoreExamples
    {
        public void SomePlayCoreMethod(AndroidJavaObject javaObject)
        {
            #if UNITY_6000_OR_NEWER
            using (javaObject)
            {
                PlayCoreTaskCompatibility.ValidateAndroidJavaObjectUsage(javaObject, "SomePlayCoreMethod");
            }
            #else
            // Legacy code
            #endif
        }
    }
}