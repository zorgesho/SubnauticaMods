using UnityEngine;

namespace Common.UnityDebug
{
	class DrawSphere: DrawWire
	{
		public void setProps(Vector3 center, float radius)
		{
			position = center;
			scale = Vector3.one * radius;
		}

		protected override void Awake()
		{
			base.Awake();

			void _addCircle(Color color, Vector3 angles) => LineHelper.drawCircle(addLine(color)).transform.localEulerAngles = angles;

			_addCircle(Color.green, Vector3.zero);
			_addCircle(Color.red, new Vector3(0f, 0f, 90f));
			_addCircle(Color.blue, new Vector3(90f, 0f, 0f));
		}
	}
}