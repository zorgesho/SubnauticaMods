using UnityEngine;
using UnityEngine.Rendering;

namespace Common.UnityDebug
{
	static class LineHelper
	{
		public const float defaultLineWidth = 0.01f;

		static readonly Material lineMaterial = new(Shader.Find("Hidden/Internal-Colored"));

		public static LineRenderer addLine(GameObject parent, Color color)
		{
			var lineGO = new GameObject("line");
			lineGO.setParent(parent);

			var lr = lineGO.AddComponent<LineRenderer>();
			lr.material = lineMaterial;
			lr.useWorldSpace = false;
			lr.setWidth(defaultLineWidth);
			lr.setColor(color);
			lr.receiveShadows = false;
			lr.shadowCastingMode = ShadowCastingMode.Off;

			return lr;
		}

		public static LineRenderer setPoints(this LineRenderer lr, params Vector3[] points)
		{
			lr.enabled = true;
			lr.positionCount = points.Length;
			lr.SetPositions(points);

			return lr;
		}

		public static void setColor(this LineRenderer lr, Color color)
		{
			lr.startColor = lr.endColor = color;
		}

		public static void setWidth(this LineRenderer lr, float width)
		{
			lr.startWidth = lr.endWidth = width;
		}

		public static LineRenderer drawCircle(LineRenderer lr, int angle = 360, float radius = 1f)
		{
			var points = new Vector3[angle + 1];

			for (int i = 0; i < points.Length; i++)
			{
				float rad = Mathf.Deg2Rad * i;
				points[i] = new Vector3(Mathf.Sin(rad) * radius, 0f, Mathf.Cos(rad) * radius);
			}

			lr.setPoints(points);

			return lr;
		}
	}
}