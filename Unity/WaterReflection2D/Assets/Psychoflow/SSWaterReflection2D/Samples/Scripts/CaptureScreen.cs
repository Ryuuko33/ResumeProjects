using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Psychoflow.SSWaterReflection2D.Samples {
    public class CaptureScreen : MonoBehaviour {
		public bool captureOnStart = true;
		public int superResolution = 1;
		public void Start() {
			if (captureOnStart) {
				Capture();
			}
		}

		[ContextMenu("Capture")]
		public void Capture() {
			ScreenCapture.CaptureScreenshot("Screenshot.png", superResolution);
		}
	}
}