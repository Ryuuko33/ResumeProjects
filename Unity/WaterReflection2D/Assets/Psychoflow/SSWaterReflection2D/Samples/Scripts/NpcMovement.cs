using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Psychoflow.SSWaterReflection2D.Samples {
	[RequireComponent(typeof(SpriteRenderer))]
    public class NpcMovement : MonoBehaviour {
		public float speed = 2f;
		public Vector2 patrolPositionOffset = new Vector2(5f, 0f);
		public bool faceRight = true;

		private Vector2 m_StartPosition;
		private Vector2 GoalPosition {
			get => m_GoBack ? m_StartPosition : m_StartPosition + patrolPositionOffset;
		}

		private bool m_GoBack = false;
		private SpriteRenderer m_SpriteRenderer;

		private void Start() {
			m_SpriteRenderer = GetComponent<SpriteRenderer>();
			m_StartPosition = this.transform.position;
			m_GoBack = false;
		}

		private void Update() {
			Vector2 newPosition = Vector2.MoveTowards(this.transform.position, GoalPosition, speed * Time.deltaTime);
			this.transform.position = newPosition;
			if (newPosition == GoalPosition) {
				m_GoBack = !m_GoBack;
				m_SpriteRenderer.flipX = faceRight ? m_GoBack : !m_GoBack;
			}
		}

		private void OnDrawGizmosSelected() {
			Gizmos.color = Color.yellow;

			Gizmos.DrawRay(this.transform.position, patrolPositionOffset);
		}
	}
}