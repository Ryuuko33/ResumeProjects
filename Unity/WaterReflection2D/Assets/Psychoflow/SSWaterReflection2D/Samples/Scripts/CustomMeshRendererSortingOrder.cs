using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Psychoflow.SSWaterReflection2D.Samples {
	/// <summary>
	/// This scripts expose the hidden properties sortingLayerID and sortingOrder in UnityEngine.MeshRenderer.
	/// </summary>
	[RequireComponent(typeof(MeshRenderer))]
	public class CustomMeshRendererSortingOrder : MonoBehaviour {
		public int sortingLayerID;
		public int sortingOrder;

		private void Reset() {
			MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
			sortingLayerID = meshRenderer.sortingLayerID;
			sortingOrder = meshRenderer.sortingOrder;
		}

		private void OnValidate() {
			MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
			meshRenderer.sortingLayerID = sortingLayerID;
			meshRenderer.sortingOrder = sortingOrder;
		}

		private void Start() {
			MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
			meshRenderer.sortingLayerID = sortingLayerID;
			meshRenderer.sortingOrder = sortingOrder;
		}
	}
}