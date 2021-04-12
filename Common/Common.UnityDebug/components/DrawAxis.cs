using UnityEngine;

namespace Common.UnityDebug
{
	class DrawAxis: DrawWire
	{
		protected override void Awake()
		{
			base.Awake();

			addLine(Color.red).setPoints(Vector3.zero, Vector3.right);
			addLine(Color.green).setPoints(Vector3.zero, Vector3.up);
			addLine(Color.blue).setPoints(Vector3.zero, Vector3.forward);
		}
	}
}