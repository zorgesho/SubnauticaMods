using UnityEngine;
using Common;

#if DEBUG
using System.Linq;
using System.Collections;
#endif

namespace HabitatPlatform
{
	class ConsoleCommands: PersistentConsoleCommands
	{
		const float defPosY = -0.0465f;

		GameObject _findPlatform() => UnityHelper.findNearestToCam<HabitatPlatform.Tag>()?.gameObject;

		public void hbpl_move(float dx, float dy)
		{
			if (_findPlatform() is GameObject platform)
				platform.transform.position += Main.config.stepMove * (dx * platform.transform.right - dy * platform.transform.forward);
		}

		public void hbpl_rotate(float angle)
		{
			if (_findPlatform() is GameObject platform)
				platform.transform.rotation *= Quaternion.AngleAxis(Main.config.stepRotate * angle, Vector3.up);
		}

		public void hbpl_reset_angles()
		{
			if (_findPlatform() is GameObject platform)
			{
				platform.transform.rotation = Quaternion.identity;
				platform.transform.position = platform.transform.position.setY(defPosY);
			}
		}

		public void hbpl_warp(float? x, float? y)
		{
			const float distance = 50f;

			if (_findPlatform() is GameObject platform)
			{
				Vector3 pos = MainCamera.camera.transform.position + distance * MainCamera.camera.transform.forward;
				platform.transform.position = new Vector3(x ?? pos.x, platform.transform.position.y, y ?? pos.z);
			}
		}

		#region debug console commands
#if DEBUG
		public void hbpl_dump(int parent = 0)
		{
			_findPlatform()?.dump("platform", parent);
		}

		public void hbpl_physics(bool? enabled)
		{
			if (_findPlatform() is GameObject platform)
			{
				var rb = platform.GetComponent<Rigidbody>();
				rb.isKinematic = !enabled ?? !rb.isKinematic;
				$"Platform physics is {(rb.isKinematic? "off": "on")}".onScreen();
			}
		}

		public void hbpl_debug(bool enabled)
		{
			const float delay = 0.5f;

			if (enabled)
				StartCoroutine(_dbg());
			else
				StopAllCoroutines();

			static IEnumerator _dbg()
			{
				while (true)
				{
					$"{FindObjectsOfType<Base>().Length}".onScreen("bases count");
					$"{FindObjectsOfType<BaseFoundationPiece>().Length}".onScreen("foundation count");

					yield return new WaitForSeconds(delay);
				}
			}
		}

		void _printVec(Vector3 vec, string prefix) => vec.ToString("F4").onScreen(prefix).logDbg();
		GameObject _findPlatformFloor() => _findPlatform()?.GetComponentInChildren<PlatformInitializer.FloorTag>()?.gameObject;

		public void hbpl_movefloor(float dx, float dy, float dz)
		{
			if (_findPlatformFloor() is GameObject floor)
			{
				floor.transform.localPosition += new Vector3(dx, dy, dz) * Main.config.stepMove;
				_printVec(floor.transform.localPosition, "floor pos");
			}
		}

		public void hbpl_scalefloor(float dx, float dz)
		{
			if (_findPlatformFloor() is GameObject floor)
			{
				floor.transform.localScale += new Vector3(dx, 0f, dz) * Main.config.stepMove;
				_printVec(floor.transform.localScale, "floor scale");
			}
		}

		public void hbpl_moveengines(float x, float y)
		{
			if (_findPlatform() is GameObject platform)
			{
				GameObject platformBase = platform.getChild("Base/rocketship_platform/Rocket_Geo/Rocketship_platform/");

				Vector3[] pos = new[] { new Vector3(x, -y, 0f), new Vector3(x, y, 0f), new Vector3(-x, y, 0f), new Vector3(-x, -y, 0f) };
				for (int i = 1; i <= 4; i++)
					platformBase.transform.Find($"Rocketship_platform_power_0{i}").localPosition = pos[i - 1];
			}
		}

		public void hbpl_lightmap(string texName)
		{
			if (_findPlatform() is GameObject platform)
			{
				Texture2D lightmap = AssetsHelper.loadTexture(texName);
				GameObject platformBase = platform.getChild("Base/rocketship_platform/Rocket_Geo/Rocketship_platform/Rocketship_platform_base-1/Rocketship_platform_base_MeshPart0");

				foreach (var m in platformBase.GetComponent<MeshRenderer>().materials)
					m.SetTexture("_Lightmap", lightmap);
			}
		}

		public void hbpl_movebase(float dx, float dy, float dz)
		{
			if (_findPlatform()?.GetComponentInChildren<Base>()?.gameObject is GameObject baseGo)
			{
				baseGo.transform.localPosition += new Vector3(dx, dy, dz) * Main.config.stepMove;
				_printVec(baseGo.transform.localPosition, "foundation pos");
			}
		}

		public void hbpl_printpos()
		{
			if (_findPlatform() is GameObject platform)
				_printVec(platform.transform.position, "platform pos");
		}

		public void hbpl_toggle_foundations()
		{
			_findPlatform()?.GetComponentsInChildren<BaseFoundationPiece>().
							 Select(fpiece => fpiece.gameObject.getChild("models")).OfType<GameObject>().
							 SelectMany(models => models.GetComponentsInChildren<Renderer>()).
							 ForEach(rend => rend.enabled = !rend.enabled);
		}
#endif // DEBUG
	#endregion
	}
}