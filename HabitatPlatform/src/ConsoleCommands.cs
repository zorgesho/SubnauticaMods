#if DEBUG
using System.Linq;
using System.Collections;
#endif
using UnityEngine;
using Common;

namespace HabitatPlatform
{
	class ConsoleCommands: PersistentConsoleCommands
	{
		GameObject _findPlatform() => UnityHelper.findNearestToCam<HabitatPlatform.Tag>()?.gameObject;

		void OnConsoleCommand_hbpl_platform_move(NotificationCenter.Notification n)
		{
			if (_findPlatform() is GameObject platform)
			{
				Vector3 pos = platform.transform.position;
				pos += Main.config.stepMove * n.getArg<float>(0) * (Quaternion.AngleAxis(90, Vector3.up) * platform.transform.forward);
				pos += Main.config.stepMove * n.getArg<float>(1) * (Quaternion.AngleAxis(90, Vector3.up) * platform.transform.right);
				platform.transform.position = pos;
			}
		}

		void OnConsoleCommand_hbpl_platform_rotate(NotificationCenter.Notification n)
		{
			if (_findPlatform() is GameObject platform)
				platform.transform.rotation *= Quaternion.AngleAxis(Main.config.stepRotate * n.getArg<float>(0), Vector3.up);
		}

		#region debug console commands
#if DEBUG
		void OnConsoleCommand_hbpl_dump(NotificationCenter.Notification n)
		{
			_findPlatform()?.dump("platform", n.getArg<int>(0));
		}

		void OnConsoleCommand_hbpl_physics(NotificationCenter.Notification n)
		{
			if (_findPlatform() is GameObject platform)
			{
				var rb = platform.GetComponent<Rigidbody>();
				rb.isKinematic = n.getArg(0, !rb.isKinematic);
				$"Platform physics is {(rb.isKinematic? "off": "on")}".onScreen();
			}
		}

		void OnConsoleCommand_hbpl_debug(NotificationCenter.Notification n)
		{
			const float delay = 0.5f;

			if (n.getArg<bool>(0))
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

		void OnConsoleCommand_hbpl_movefloor(NotificationCenter.Notification n)
		{
			if (_findPlatformFloor() is GameObject floor && n.getArgCount() == 3)
			{
				var args = n.getArgs<float>();
				floor.transform.localPosition += new Vector3(args[0], args[1], args[2]) * Main.config.stepMove;
				_printVec(floor.transform.localPosition, "floor pos");
			}
		}

		void OnConsoleCommand_hbpl_scalefloor(NotificationCenter.Notification n)
		{
			if (_findPlatformFloor() is GameObject floor && n.getArgCount() == 2)
			{
				var args = n.getArgs<float>();
				floor.transform.localScale += new Vector3(args[0], 0f, args[1]) * Main.config.stepMove;
				_printVec(floor.transform.localScale, "floor scale");
			}
		}

		void OnConsoleCommand_hbpl_moveengines(NotificationCenter.Notification n)
		{
			if (_findPlatform() is GameObject platform)
			{
				GameObject platformBase = platform.getChild("Base/rocketship_platform/Rocket_Geo/Rocketship_platform/");

				float dx = n.getArg<float>(0);
				float dy = n.getArg<float>(1);

				Vector3[] pos = new[] { new Vector3(dx, -dy, 0f), new Vector3(dx, dy, 0f), new Vector3(-dx, dy, 0f), new Vector3(-dx, -dy, 0f) };
				for (int i = 1; i <= 4; i++)
					platformBase.transform.Find($"Rocketship_platform_power_0{i}").localPosition = pos[i - 1];
			}
		}

		void OnConsoleCommand_hbpl_lightmap(NotificationCenter.Notification n)
		{
			if (_findPlatform() is GameObject platform)
			{
				Texture2D lightmap = AssetsHelper.loadTexture(n.getArg(0));
				GameObject platformBase = platform.getChild("Base/rocketship_platform/Rocket_Geo/Rocketship_platform/Rocketship_platform_base-1/Rocketship_platform_base_MeshPart0");

				foreach (var m in platformBase.GetComponent<MeshRenderer>().materials)
					m.SetTexture("_Lightmap", lightmap);
			}
		}

		void OnConsoleCommand_hbpl_movebase(NotificationCenter.Notification n)
		{
			if (_findPlatform()?.GetComponentInChildren<Base>()?.gameObject is GameObject baseGo && n.getArgCount() == 3)
			{
				var args = n.getArgs<float>();
				baseGo.transform.localPosition += new Vector3(args[0], args[1], args[2]) * Main.config.stepMove;
				_printVec(baseGo.transform.localPosition, "foundation pos");
			}
		}

		void OnConsoleCommand_hbpl_toggle_foundations(NotificationCenter.Notification _)
		{
			_findPlatform()?.GetComponentsInChildren<BaseFoundationPiece>().
							 Select(fpiece => fpiece.gameObject.getChild("models")).OfType<GameObject>().
							 SelectMany(models => models.GetComponentsInChildren<Renderer>()).
							 ForEach(rend => rend.enabled = !rend.enabled);
		}
#endif
	#endregion
	}
}