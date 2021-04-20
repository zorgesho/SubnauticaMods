using UnityEngine;

namespace Common.UnityDebug
{
	class DrawBox: DrawWire
	{
		static readonly Vector3[] verts =
		{
			new Vector3(-0.5f, -0.5f, -0.5f), // 0
			new Vector3( 0.5f, -0.5f, -0.5f), // 1
			new Vector3( 0.5f, -0.5f,  0.5f), // 2
			new Vector3(-0.5f, -0.5f,  0.5f), // 3

			new Vector3(-0.5f,  0.5f, -0.5f), // 4
			new Vector3( 0.5f,  0.5f, -0.5f), // 5
			new Vector3( 0.5f,  0.5f,  0.5f), // 6
			new Vector3(-0.5f,  0.5f,  0.5f)  // 7
		};

		public void setProps(Vector3 center, Vector3 size)
		{
			position = center;
			scale = size;
		}

		protected override void Awake()
		{
			base.Awake();

			void _addLine(int vert0, int vert1) => addLine(Color.red).setPoints(verts[vert0], verts[vert1]);

			// using separate lines instead of rects because of buggy LineRenderer
			_addLine(0, 1); _addLine(1, 2); _addLine(2, 3); _addLine(3, 0); // bottom rect
			_addLine(4, 5); _addLine(5, 6); _addLine(6, 7); _addLine(7, 4); // top rect
			_addLine(0, 4); _addLine(1, 5); _addLine(2, 6); _addLine(3, 7); // vertical connections
		}
	}
}