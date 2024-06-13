using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine;

namespace Psychoflow.SSWaterReflection2D {
	[CustomEditor(typeof(SSWR2DReadme))]
	[InitializeOnLoad]
	internal class SSWR2DReadmeEditor : Editor {
		const string ShowedReadmeEditorPrefName = "Psychoflow.SSWaterReflection2D.ShowedReadme_V1_0";

		const float LINK_SPACE_SIZE = 16f;

		static SSWR2DReadmeEditor() {
			EditorApplication.delayCall += SelectReadmeAutomatically;
		}

		static void SelectReadmeAutomatically() {
			if (!SessionState.GetBool(ShowedReadmeEditorPrefName, false)) {
				SessionState.SetBool(ShowedReadmeEditorPrefName, true);
				var readme = FindReadme();
				if (!readme.hasViewed) {
					Debug.Log($"Welcome to SS Water Reflection 2D! You can see Readme from top menu Help/SS Water Relfection 2D");
					Selection.objects = new UnityEngine.Object[] { readme };
					readme.hasViewed = true;
					EditorUtility.SetDirty(readme);
				}
			}
		}

		[MenuItem("Help/SS Water Relfection 2D")]
		static SSWR2DReadme SelectReadme() {
			SSWR2DReadme readme = FindReadme();
			if (readme != null) {
				Selection.objects = new UnityEngine.Object[] { readme };
			}
			return readme;
		}

		static SSWR2DReadme FindReadme() {
			var ids = AssetDatabase.FindAssets("Readme t:SSWR2DReadme");
			if (ids.Length == 1) {
				var readmeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(ids[0]));

				return (SSWR2DReadme)readmeObject;
			} else {
				Debug.Log("Couldn't find a readme");
				return null;
			}
		}

		protected override void OnHeaderGUI() {
			var readme = (SSWR2DReadme)target;
			Init();

			var iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth - 0f, readme.iconMaxWidth);
			float ratio = readme.icon.height / (float)readme.icon.width;
			var iconHeight = iconWidth * ratio;
			GUILayout.BeginHorizontal("In BigTitle");
			{
				GUILayout.FlexibleSpace();
				GUILayout.Label(readme.icon, GUILayout.Width(iconWidth), GUILayout.Height(iconHeight));
				GUILayout.FlexibleSpace();
				//GUILayout.Label(readme.title, TitleStyle);
			}
			GUILayout.EndHorizontal();
		}

		void DrawLine() {

			EditorGUILayout.Space(12);
			Rect rect = EditorGUILayout.GetControlRect(false, 1 );
			rect.height = 1;
			EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
			EditorGUILayout.Space(12);

		}

		public override void OnInspectorGUI() {
			//base.OnInspectorGUI();
			var readme = (SSWR2DReadme)target;
			Init();

			GUILayout.Label(readme.gettingStartedSection.heading, HeadingStyle);
			GUILayout.Label(readme.gettingStartedSection.text, BodyStyle);

			var buttonsPerRow = Mathf.CeilToInt(EditorGUIUtility.currentViewWidth / 256);
			//if (buttonsPerRow % 2 == 1) {
			//	buttonsPerRow--;
			//}

			var rendered = 0;
			while (rendered <= readme.gettingStartedSection.examples.Count) {
				var start = rendered;
				var end = start + buttonsPerRow;
				rendered = end;
				if (end >= readme.gettingStartedSection.examples.Count) {
					end = readme.gettingStartedSection.examples.Count;
				}

				GUILayout.Space(15);
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				for (var i = start; i < end; i++) {
					var sceneButton = readme.gettingStartedSection.examples[i];
					GUILayout.Label(sceneButton.title, GUILayout.Width(128));
					GUILayout.Space(10);
				}

				GUILayout.FlexibleSpace();

				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				for (var i = start; i < end; i++) {
					var sceneButton = readme.gettingStartedSection.examples[i];

					if (GUILayout.Button(new GUIContent(sceneButton.icon), GUILayout.Width(128), GUILayout.Height(128))) {
						EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
						EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(sceneButton.sceneFile));
					}

					GUILayout.Space(10);

				}

				GUILayout.FlexibleSpace();

				GUILayout.EndHorizontal();
			}

			DrawLine();

			GUILayout.Label(readme.docSection.heading, HeadingStyle);
			GUILayout.Label(readme.docSection.text, BodyStyle);

			foreach (var section in readme.docSection.links) {
				if (!string.IsNullOrEmpty(section.linkText)) {
					GUILayout.Space(LINK_SPACE_SIZE / 2);
					if (LinkLabel(new GUIContent(section.linkText))) {
						Application.OpenURL(section.url);
					}
					GUILayout.Label(section.about, BodyStyle);

				}

				GUILayout.Space(LINK_SPACE_SIZE);
			}
			DrawLine();

		}


		bool m_Initialized;

		GUIStyle LinkStyle { get { return m_LinkStyle; } }
		[SerializeField] GUIStyle m_LinkStyle;

		GUIStyle TitleStyle { get { return m_TitleStyle; } }
		[SerializeField] GUIStyle m_TitleStyle;

		GUIStyle HeadingStyle { get { return m_HeadingStyle; } }
		[SerializeField] GUIStyle m_HeadingStyle;

		GUIStyle BodyStyle { get { return m_BodyStyle; } }
		[SerializeField] GUIStyle m_BodyStyle;

		void Init() {
			if (m_Initialized)
				return;
			m_BodyStyle = new GUIStyle(EditorStyles.label);
			m_BodyStyle.wordWrap = true;
			m_BodyStyle.fontSize = 14;

			m_TitleStyle = new GUIStyle(m_BodyStyle);
			m_TitleStyle.fontSize = 26;

			m_HeadingStyle = new GUIStyle(m_BodyStyle);
			m_HeadingStyle.fontSize = 18;
			m_HeadingStyle.fontStyle = FontStyle.Bold;

			m_LinkStyle = new GUIStyle(m_BodyStyle);
			// Match selection color which works nicely for both light and dark skins
			m_LinkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
			m_LinkStyle.stretchWidth = false;

			m_Initialized = true;
		}

		bool LinkLabel(GUIContent label, params GUILayoutOption[] options) {
			var position = GUILayoutUtility.GetRect(label, LinkStyle, options);

			Handles.BeginGUI();
			Handles.color = LinkStyle.normal.textColor;
			Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
			Handles.color = Color.white;
			Handles.EndGUI();

			EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

			return GUI.Button(position, label, LinkStyle);
		}
	}
}