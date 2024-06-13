using UnityEngine;

namespace Psychoflow.SSWaterReflection2D {
    internal class ShaderPropertyIDs {
		public readonly static int UnscaledTime = Shader.PropertyToID("_UnscaledTime");

		public readonly static int ReferenceWorldY = Shader.PropertyToID("_ReferenceWorldY");
		public readonly static int ScreenBoundFadeOut = Shader.PropertyToID("_ScreenBoundFadeOut");
		public readonly static int WaterScale = Shader.PropertyToID("_WaterScale");

		public readonly static int DisplacementTex = Shader.PropertyToID("_DisplacementTex");
		public readonly static int DisplacementSpeed = Shader.PropertyToID("_DisplacementSpeed");
		public readonly static int DisplacementCompositeTex = Shader.PropertyToID("_DisplacementCompositeTex");
		public readonly static int DisplacementCompositeSpeedOffset = Shader.PropertyToID("_DisplacementCompositeSpeedOffset");

		public readonly static int ReflectionTint = Shader.PropertyToID("_ReflectionTint");
		public readonly static int ReflectionDisplacementAmount = Shader.PropertyToID("_ReflectionDisplacementAmount");
		public readonly static int ReflectionFoamWeight = Shader.PropertyToID("_ReflectionFoamWeight");

		public readonly static int RefractionTint = Shader.PropertyToID("_RefractionTint");
		public readonly static int RefractionDisplacementAmount = Shader.PropertyToID("_RefractionDisplacementAmount");
		public readonly static int RefractionFoamWeight = Shader.PropertyToID("_RefractionFoamWeight");

		public readonly static int FoamColor = Shader.PropertyToID("_FoamColor");
		public readonly static int FoamThreshold = Shader.PropertyToID("_FoamThreshold");
		public readonly static int FoamEdgeThreshold = Shader.PropertyToID("_FoamEdgeThreshold");

		public readonly static int PerspectiveCorrectionStrength = Shader.PropertyToID("_PerspectiveCorrectionStrength");
		public readonly static int PerspectiveCorrectionContrast = Shader.PropertyToID("_PerspectiveCorrectionContrast");
		public readonly static int PerspectiveCorrectionMaxLength = Shader.PropertyToID("_PerspectiveCorrectionMaxLength");
		public readonly static int PerspectiveReflectionTiltY = Shader.PropertyToID("_PerspectiveReflectionTiltY");

		public readonly static int VertexDisplacementTex = Shader.PropertyToID("_VertexDisplacementTex");
		public readonly static int VertexDisplacementAmount = Shader.PropertyToID("_VertexDisplacementAmount");
		public readonly static int VertexDisplacementSpeed = Shader.PropertyToID("_VertexDisplacementSpeed");

		public readonly static int VoronoiAngleOffset = Shader.PropertyToID("_VoronoiAngleOffset");
		public readonly static int VoronoiAngleSpeed = Shader.PropertyToID("_VoronoiAngleSpeed");
		public readonly static int VoronoiCellDensity = Shader.PropertyToID("_VoronoiCellDensity");
		public readonly static int VoronoiPower = Shader.PropertyToID("_VoronoiPower");
	}
}