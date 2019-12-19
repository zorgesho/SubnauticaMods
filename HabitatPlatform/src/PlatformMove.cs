using System.Collections;

using UnityEngine;

using Common;

namespace HabitatPlatform
{
	class PlatformMove: MonoBehaviour
	{
		bool moving = false;
		
		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Alpha9))
				moving = !moving;

			if (moving)
			{
				Vector3 v = gameObject.transform.position;
				v += Main.config.step * (Quaternion.AngleAxis(90, Vector3.up) * gameObject.transform.forward);
				gameObject.transform.localPosition = v;
				$"{gameObject.transform.localPosition}".onScreen("platform pos");

				if (Input.GetKeyDown(KeyCode.LeftArrow))
				{
					Quaternion qq = gameObject.transform.rotation;
					qq *= Quaternion.AngleAxis(Main.config.stepAngle, Vector3.up);
					gameObject.transform.rotation = qq;
				}

				if (Input.GetKeyDown(KeyCode.RightArrow))
				{
					Quaternion qq = gameObject.transform.rotation;
					qq *= Quaternion.AngleAxis(-Main.config.stepAngle, Vector3.up);
					gameObject.transform.rotation = qq;
				}
			}
		}
	}
}