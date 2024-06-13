using UnityEngine;


namespace Psychoflow.SSWaterReflection2D.Samples {
    /// <summary>
    /// This scripts expose the hidden properties TransparencySortMode in UnityEngine.Camera.
    /// It's useful to modify a perspective camera as orthgraphic transparency sort mode, which is usually used on perspective 2d game.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CustomCameraTransparencySortMode : MonoBehaviour {
        public TransparencySortMode transparencySortMode;
        public Vector3 transparencySortAxis;

        private void Reset() {
            Camera camera = GetComponent<Camera>();
            transparencySortMode = camera.transparencySortMode;
            transparencySortAxis = camera.transparencySortAxis;
        }

        private void OnValidate() {
            Camera camera = GetComponent<Camera>();
            camera.transparencySortMode = transparencySortMode;
            camera.transparencySortAxis = transparencySortAxis;
        }

        private void Start() {
            Camera camera = GetComponent<Camera>();
            camera.transparencySortMode = transparencySortMode;
            camera.transparencySortAxis = transparencySortAxis;
        }
    }
}
