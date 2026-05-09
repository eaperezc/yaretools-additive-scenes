using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eaperezc.AdditiveScenes
{
    public class SceneLoader : MonoBehaviour
    {
        [Tooltip("Scene names to manage. Use the inspector's Refresh button to populate from Build Settings.")]
        [SerializeField] private List<string> sceneNames = new List<string>();

        public IReadOnlyList<string> SceneNames => sceneNames;

        public bool IsLoading { get; private set; }
        public float Progress { get; private set; }
        public string CurrentSceneName { get; private set; }

        public void LoadAll(Action onComplete = null)
        {
            StartCoroutine(LoadAllRoutine(onComplete));
        }

        public void UnloadAll(Action onComplete = null)
        {
            StartCoroutine(UnloadAllRoutine(onComplete));
        }

        public AsyncOperation Load(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return null;
            Scene existing = SceneManager.GetSceneByName(sceneName);
            if (existing.IsValid() && existing.isLoaded) return null;
            return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        }

        public AsyncOperation Unload(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return null;
            Scene s = SceneManager.GetSceneByName(sceneName);
            if (!s.IsValid() || !s.isLoaded) return null;
            return SceneManager.UnloadSceneAsync(s);
        }

        private IEnumerator LoadAllRoutine(Action onComplete)
        {
            IsLoading = true;
            Progress = 0f;

            int total = 0;
            for (int i = 0; i < sceneNames.Count; i++)
                if (!string.IsNullOrEmpty(sceneNames[i])) total++;

            int doneCount = 0;
            for (int i = 0; i < sceneNames.Count; i++)
            {
                string sceneName = sceneNames[i];
                if (string.IsNullOrEmpty(sceneName)) continue;

                CurrentSceneName = sceneName;

                Scene existing = SceneManager.GetSceneByName(sceneName);
                if (existing.IsValid() && existing.isLoaded)
                {
                    doneCount++;
                    Progress = total > 0 ? (float)doneCount / total : 1f;
                    continue;
                }

                var load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                while (load != null && !load.isDone)
                {
                    float perScene = Mathf.Clamp01(load.progress / 0.9f);
                    Progress = total > 0 ? (doneCount + perScene) / total : 1f;
                    yield return null;
                }
                doneCount++;
                Progress = total > 0 ? (float)doneCount / total : 1f;
            }

            CurrentSceneName = null;
            Progress = 1f;
            IsLoading = false;
            onComplete?.Invoke();
        }

        private IEnumerator UnloadAllRoutine(Action onComplete)
        {
            IsLoading = true;
            Progress = 0f;

            for (int i = 0; i < sceneNames.Count; i++)
            {
                string sceneName = sceneNames[i];
                if (string.IsNullOrEmpty(sceneName)) continue;

                Scene s = SceneManager.GetSceneByName(sceneName);
                if (!s.IsValid() || !s.isLoaded) continue;

                CurrentSceneName = sceneName;
                var op = SceneManager.UnloadSceneAsync(s);
                while (op != null && !op.isDone) yield return null;
            }

            CurrentSceneName = null;
            Progress = 1f;
            IsLoading = false;
            onComplete?.Invoke();
        }

        public void SetSceneNames(IEnumerable<string> names)
        {
            sceneNames.Clear();
            if (names == null) return;
            foreach (var n in names)
                if (!string.IsNullOrEmpty(n)) sceneNames.Add(n);
        }
    }
}
