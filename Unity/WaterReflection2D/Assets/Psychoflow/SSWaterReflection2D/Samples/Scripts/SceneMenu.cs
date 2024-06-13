using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Psychoflow.SSWaterReflection2D.Samples {
    public class SceneMenu : MonoBehaviour {
		public string startSceneName;
		public Text sceneNameText;
		public Text sceneNameTextBorder;
		private Scene m_ActiveScene;

		public void Start() {
			LoadScene(startSceneName);
		}

		public void LoadScene(string sceneName) {
			if (sceneName == m_ActiveScene.name) { return; }
			if (m_ActiveScene.IsValid() && m_ActiveScene.isLoaded) {
				Debug.Log($"Unload {m_ActiveScene.name}");
				SceneManager.UnloadSceneAsync(m_ActiveScene);
			}
			SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
			m_ActiveScene = SceneManager.GetSceneByName(sceneName);
			if (!m_ActiveScene.IsValid()) {
				Debug.LogError($"Cannot find {sceneName}");
			} else {
				sceneNameText.text = m_ActiveScene.name;
				sceneNameTextBorder.text = m_ActiveScene.name;
				Debug.Log($"m_Active = {m_ActiveScene.name}");
			}
		}
    }
}