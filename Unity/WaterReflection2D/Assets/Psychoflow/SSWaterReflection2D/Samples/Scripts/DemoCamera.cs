using UnityEngine;

namespace Psychoflow.SSWaterReflection2D.Samples {
    public class DemoCamera : MonoBehaviour {
        public float keyboardSpeed = 10f;

        public Vector2 boundMin = new Vector2(-10, -10);
        public Vector2 boundMax = new Vector2(10, 10);

		private Vector3 m_DragOrigin;

		public void Update() {
			KeyboradMoveInput();
			MouseDragMoveInput();

			BoundPosition();
        }

        void KeyboradMoveInput() {
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
                transform.Translate(new Vector3(keyboardSpeed * Time.deltaTime, 0, 0));
            }
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
                transform.Translate(new Vector3(-keyboardSpeed * Time.deltaTime, 0, 0));
            }
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) {
                transform.Translate(new Vector3(0, keyboardSpeed * Time.deltaTime, 0));
            }
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) {
                transform.Translate(new Vector3(0, -keyboardSpeed * Time.deltaTime, 0));
            }
        }

		void MouseDragMoveInput() {
			if (Input.GetMouseButtonDown(0)) {
				m_DragOrigin = Input.mousePosition;
				return;
			}

			if (!Input.GetMouseButton(0)) {
				return;
			}
			Camera mainCamera = Camera.main;
			float depth = -mainCamera.transform.position.z; // for perspective camera focus on detph-0 plane.
			Vector3 dragWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(m_DragOrigin.x, m_DragOrigin.y, depth));
			Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, depth));
			Vector3 offset =  dragWorldPosition - mouseWorldPosition;
			Vector3 move = new Vector3(offset.x, offset.y, 0f);
			m_DragOrigin = Input.mousePosition;

			transform.Translate(move, Space.World);
		}

        void BoundPosition() {
            Vector3 position = this.transform.position;
            if (position.x > boundMax.x) {
                position.x = boundMax.x;
            }
            if (position.y > boundMax.y) {
                position.y = boundMax.y;
            }
            if (position.x < boundMin.x) {
                position.x = boundMin.x;
            }
            if (position.y < boundMin.y) {
                position.y = boundMin.y;
            }

            this.transform.position = position;
        }

        private void OnDrawGizmosSelected() {
            Vector3 center = (boundMin + boundMax) / 2;
            Vector3 size = boundMax - boundMin;
            size.z = 1f;
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(center, size);
        }
    }
}
