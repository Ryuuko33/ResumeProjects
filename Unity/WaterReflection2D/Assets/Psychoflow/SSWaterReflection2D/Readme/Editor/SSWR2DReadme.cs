using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Psychoflow.SSWaterReflection2D {
	internal class SSWR2DReadme : ScriptableObject {
		public Texture2D icon;
		public float iconMaxWidth = 128f;
		public string title;
		public GettingStartedSection gettingStartedSection;
		public DocumentationSection docSection;
		public bool hasViewed = false;

		[Serializable]
		public class DocumentationSection {
			public string heading;
			[TextArea]
			public string text;
			public List<DocLink> links;
		}

		[Serializable]
		public class DocLink {
			public string linkText;
			public string url;
			public string about;
		}

		[Serializable]
		public class GettingStartedSection {
			public string heading;
			[TextArea]
			public string text;
			public List<SceneButtonData> examples;
		}

		[Serializable]
		public class SceneButtonData {
			public Texture2D icon;
			public string title;
			public UnityEngine.Object sceneFile;
		}
	}
}