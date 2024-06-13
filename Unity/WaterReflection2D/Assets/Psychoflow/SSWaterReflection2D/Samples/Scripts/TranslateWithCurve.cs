using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Psychoflow.SSWaterReflection2D.Samples {
    public class TranslateWithCurve : MonoBehaviour {
		public AnimationCurve curve;
		public float duration = 1f;
		public Vector3 movement = new Vector3(0f, -3f, 0f);
		public KeyCode triggerHotkey = KeyCode.Return;


		private bool m_Triggered = false;
		private Vector3 m_OriginalPosition;
		private float m_Timer;
		private void Update() {
			if (m_Triggered) {
				m_Timer += Time.deltaTime;
				Vector3 newPosition = m_OriginalPosition + curve.Evaluate(m_Timer / duration) * movement;
				this.transform.position = newPosition;
				if (m_Timer >= duration) {
					m_Triggered = false;
				}
			} else {
				if (Input.GetKeyDown(triggerHotkey)) {
					m_Triggered = true;
					m_OriginalPosition = this.transform.position;
					m_Timer = 0f;
				}
			}
		}


		private void OnDrawGizmosSelected() {
			Gizmos.color = Color.yellow;
			Gizmos.DrawRay(this.transform.position, movement);
		}
	}
}