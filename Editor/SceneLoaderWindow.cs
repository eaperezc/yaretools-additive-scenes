using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YareTools.EditorTools
{
    public class SceneLoaderWindow : EditorWindow
    {
        private string masterPath;
        private List<string> scenePaths = new List<string>();
        private Vector2 scroll;

        [MenuItem("Window/Scene Loader")]
        public static void Open()
        {
            GetWindow<SceneLoaderWindow>("Scene Loader");
        }

        private void OnEnable() => Refresh();
        private void OnInspectorUpdate() => Repaint();

        private void Refresh()
        {
            masterPath = null;
            scenePaths.Clear();
            foreach (var s in EditorBuildSettings.scenes)
            {
                if (!s.enabled) continue;
                if (string.IsNullOrEmpty(s.path)) continue;
                if (masterPath == null) masterPath = s.path;
                else scenePaths.Add(s.path);
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Scenes (from Build Settings)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "First enabled scene in Build Settings is the Master and is excluded from Unload All. " +
                "In edit mode, opens the first scene as Single if nothing is loaded yet.",
                MessageType.None);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Load All")) LoadAll();
                if (GUILayout.Button("Unload All")) UnloadAll();
                if (GUILayout.Button("Refresh", GUILayout.Width(80))) Refresh();
            }

            EditorGUILayout.Space(6);

            if (masterPath == null && scenePaths.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "No scenes enabled in Build Settings. Add scenes via File > Build Settings.",
                    MessageType.Info);
                return;
            }

            if (masterPath != null)
            {
                EditorGUILayout.LabelField("Master", EditorStyles.boldLabel);
                DrawRow(masterPath, isMaster: true);
                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField("Rooms", EditorStyles.boldLabel);
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (var path in scenePaths)
                DrawRow(path, isMaster: false);
            EditorGUILayout.EndScrollView();
        }

        private void DrawRow(string path, bool isMaster)
        {
            string label = Path.GetFileNameWithoutExtension(path);
            bool exists = File.Exists(path);
            Scene s = SceneManager.GetSceneByPath(path);
            bool loaded = exists && s.IsValid() && s.isLoaded;

            using (new EditorGUILayout.HorizontalScope())
            {
                var prevBg = GUI.backgroundColor;
                GUI.backgroundColor = loaded ? new Color(0.55f, 0.85f, 0.55f) : prevBg;
                EditorGUILayout.LabelField(label, isMaster ? EditorStyles.boldLabel : EditorStyles.label, GUILayout.Width(180));
                GUI.backgroundColor = prevBg;

                EditorGUILayout.LabelField(loaded ? "loaded" : (exists ? "unloaded" : "missing"), GUILayout.Width(70));

                using (new EditorGUI.DisabledScope(!exists))
                {
                    if (loaded)
                    {
                        if (GUILayout.Button("Unload", GUILayout.Width(70)))
                            UnloadScene(path);
                    }
                    else
                    {
                        if (GUILayout.Button("Load", GUILayout.Width(70)))
                            LoadScene(path, isMaster);
                    }

                    if (GUILayout.Button("Ping", GUILayout.Width(50)))
                    {
                        var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                        if (asset != null) EditorGUIUtility.PingObject(asset);
                    }
                }
            }
        }

        private void LoadScene(string path, bool isMaster)
        {
            if (Application.isPlaying)
            {
                SceneManager.LoadSceneAsync(path, LoadSceneMode.Additive);
                return;
            }

            var mode = isMaster && SceneManager.sceneCount == 0 ? OpenSceneMode.Single
                : SceneManager.sceneCount == 0 ? OpenSceneMode.Single
                : OpenSceneMode.Additive;
            EditorSceneManager.OpenScene(path, mode);
        }

        private void UnloadScene(string path)
        {
            Scene s = SceneManager.GetSceneByPath(path);
            if (!s.IsValid() || !s.isLoaded) return;

            if (Application.isPlaying)
            {
                SceneManager.UnloadSceneAsync(s);
                return;
            }

            if (SceneManager.sceneCount <= 1)
            {
                Debug.LogWarning("[SceneLoader] Refusing to close the only open scene.");
                return;
            }

            if (s.isDirty)
            {
                if (!EditorSceneManager.SaveModifiedScenesIfUserWantsTo(new[] { s }))
                    return;
            }
            EditorSceneManager.CloseScene(s, removeScene: true);
        }

        private void LoadAll()
        {
            if (masterPath != null)
            {
                Scene m = SceneManager.GetSceneByPath(masterPath);
                if (!m.IsValid() || !m.isLoaded)
                    LoadScene(masterPath, isMaster: true);
            }

            foreach (var path in scenePaths)
            {
                Scene s = SceneManager.GetSceneByPath(path);
                if (s.IsValid() && s.isLoaded) continue;
                LoadScene(path, isMaster: false);
            }
        }

        private void UnloadAll()
        {
            foreach (var path in scenePaths)
                UnloadScene(path);
        }
    }
}
