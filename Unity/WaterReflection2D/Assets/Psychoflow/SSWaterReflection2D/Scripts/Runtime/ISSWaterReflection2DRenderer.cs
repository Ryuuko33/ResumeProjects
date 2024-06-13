using System.Collections;
using System.Collections.Generic;

namespace Psychoflow.SSWaterReflection2D {
	/// <summary>
	/// Reflection Renderer interface that will be notified if the provider's parameter changed.
	/// </summary>
	internal interface ISSWaterReflection2DRenderer {
		/// <summary>
		/// Notify by provider that its parameter has been changed.
		/// </summary>
		void OnParameterChanged();
	}
}