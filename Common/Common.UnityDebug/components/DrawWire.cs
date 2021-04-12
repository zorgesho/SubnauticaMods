using System.Collections.Generic;
using UnityEngine;

namespace Common.UnityDebug
{
	abstract class DrawWire: MonoBehaviour
	{
		GameObject linesParent;
		readonly List<LineRenderer> lines = new List<LineRenderer>();

		protected LineRenderer addLine(Color color)
		{
			var lr = LineHelper.addLine(linesParent, color);
			lines.Add(lr);

			return lr;
		}

		protected LineRenderer getLine(int index)
		{
			return lines[index];
		}

		protected virtual void Awake()
		{
			linesParent = new GameObject("wire");
			linesParent.setParent(gameObject);
		}

		void OnDestroy()
		{
			Destroy(linesParent);
		}


		public Vector3 position
		{
			get => linesParent.transform.localPosition;
			set => linesParent.transform.localPosition = value;
		}
		public Quaternion rotation
		{
			get => linesParent.transform.localRotation;
			set => linesParent.transform.localRotation = value;
		}
		public Vector3 scale
		{
			get => linesParent.transform.localScale;
			set => linesParent.transform.localScale = value;
		}

		public float lineWidth
		{
			get => _lineWidth;

			set
			{
				_lineWidth = value;
				lines.ForEach(line => line.setWidth(_lineWidth));
			}
		}
		protected float _lineWidth = LineHelper.defaultLineWidth;

		public Color color
		{
			set => lines.ForEach(line => line.setColor(value));
		}
	}
}