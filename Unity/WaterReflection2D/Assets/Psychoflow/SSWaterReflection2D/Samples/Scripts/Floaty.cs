using UnityEngine;

namespace Psychoflow.SSWaterReflection2D.Samples {
	/// <summary>
	/// Make the gameObject floaty along y axis.
	/// </summary>
    public class Floaty : MonoBehaviour {
		public float duration = 1f;

		public float height = 1f;
		public AnimationCurve moveCurve;
		public bool reversed = false;

		private float m_Time;
		private float m_OriginalPositionY;

		private void Start() {
			m_OriginalPositionY = this.transform.position.y;
			m_Time = 0f;
		}

		private void Update() {
			m_Time += Time.deltaTime;

			Vector3 position = this.transform.position;
			float sign = reversed ? -1f : 1f;
			position.y = m_OriginalPositionY + Mathf.Lerp(0f, sign * height, moveCurve.Evaluate(Mathf.PingPong(m_Time, duration) / duration));
			this.transform.position = position;
		}

		private void OnDrawGizmosSelected() {
			Vector3 pos = this.transform.position;
			if (Application.isPlaying) {
				pos.y = m_OriginalPositionY;
			}
			Gizmos.color = Color.blue;
			float sign = reversed ? -1f : 1f;
			Vector3 center = pos + new Vector3(0f, sign * height / 2, 0f);
			Gizmos.DrawWireCube(center, new Vector3(1f, height, 1f));
		}
	}
}