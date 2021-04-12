using UnityEngine;

namespace Common.UnityDebug
{
	class DrawCapsule: DrawWire
	{
		bool dirty = true;

		float radius = 1f;
		float height = 1f;

		public void setProps(Vector3 center, float radius, float height, int dir)
		{
			position = center;

			this.radius = radius;
			this.height = height;

			(rotation, color) = dir switch
			{
				1 => (Quaternion.Euler(0f, 0f, 90f), Color.green),
				2 => (Quaternion.Euler(0f, 90f, 0f), Color.blue),
				_ => (Quaternion.identity, Color.red)
			};

			dirty = true;
		}

		void redrawCapsule()
		{
			float length = Mathf.Max(0f, height / 2 - radius); // distance between the centers of the hemispheres

			var posHemi = new Vector3( length, 0f, 0f);
			var negHemi = new Vector3(-length, 0f, 0f);

			void _drawCircle(int index, Vector3 pos, int angle)
			{
				var line = getLine(index);
				line.transform.localPosition = pos;
				LineHelper.drawCircle(line, angle, radius);
			}

			_drawCircle(0, posHemi, 360);
			_drawCircle(1, posHemi, 180);
			_drawCircle(2, posHemi, 180);

			_drawCircle(3, negHemi, 360);
			_drawCircle(4, negHemi, 180);
			_drawCircle(5, negHemi, 180);

			if (length > 0f)
			{
				void _setPoints(int index, float y, float z) =>
					getLine(index).setPoints(new Vector3(length, y, z), new Vector3(-length, y, z));

				_setPoints(6, -radius, 0f);
				_setPoints(7,  radius, 0f);
				_setPoints(8, 0f, -radius);
				_setPoints(9, 0f,  radius);
			}
			else
			{
				for (int i = 6; i <= 9; i++)
					getLine(i).enabled = false;
			}
		}

		protected override void Awake()
		{
			base.Awake();

			void _addLine(Vector3 angles) => addLine(Color.red).transform.localEulerAngles = angles;

			// positive hemisphere
			_addLine(new Vector3(0f, 0f, 90f)); // 0
			_addLine(new Vector3(90f, 0f, 0f)); // 1
			_addLine(Vector3.zero); // 2

			// negative hemisphere
			_addLine(new Vector3(0f, 0f, 90f)); // 3
			_addLine(new Vector3(0f, 180f, 0f)); // 4
			_addLine(new Vector3(90f, 180f, 0f)); // 5

			// connection lines between the hemispheres (6-9)
			for (int i = 0; i < 4; i++)
				addLine(Color.red);
		}

		void Update()
		{
			if (dirty && !(dirty = false))
				redrawCapsule();
		}
	}
}