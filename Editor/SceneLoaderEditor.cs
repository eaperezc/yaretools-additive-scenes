using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Eaperezc.AdditiveScenes.EditorTools
{
    [CustomEditor(typeof(SceneLoader))]
    public class SceneLoaderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(6);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Refresh from Build Settings"))
                    RefreshFromBuildSettings((SceneLoader)target);

                if (GUILayout.Button("Clear"))
                    ClearList((SceneLoader)target);
            }
        }

        private static void RefreshFromBuildSettings(SceneLoader loader)
        {
            var names = new List<string>();
            foreach (var s in EditorBuildSettings.scenes)
            {
                if (!s.enabled) continue;
                if (string.IsNullOrEmpty(s.path)) continue;
                names.Add(Path.GetFileNameWithoutExtension(s.path));
            }
            Undo.RecordObject(loader, "Refresh Scene Names");
            loader.SetSceneNames(names);
            EditorUtility.SetDirty(loader);
        }

        private static void ClearList(SceneLoader loader)
        {
            Undo.RecordObject(loader, "Clear Scene Names");
            loader.SetSceneNames(null);
            EditorUtility.SetDirty(loader);
        }
    }
}
