#if DEBUG
using System.Collections;
#endif
using UnityEngine;
using Common;

namespace HabitatPlatform
{
	class ConsoleCommands: PersistentConsoleCommands
	{
		void OnConsoleCommand_hbpl_platform_move(NotificationCenter.Notification n)
		{
			if (UnityHelper.findNearestToCam<HabitatPlatform.Tag>()?.gameObject is GameObject platform)
			{
				Vector3 pos = platform.transform.position;
				pos += Main.config.stepMove * n.getArg<float>(0) * (Quaternion.AngleAxis(90, Vector3.up) * platform.transform.forward);
				pos += Main.config.stepMove * n.getArg<float>(1) * (Quaternion.AngleAxis(90, Vector3.up) * platform.transform.right);
				platform.transform.position = pos;
			}
		}

		void OnConsoleCommand_hbpl_platform_rotate(NotificationCenter.Notification n)
		{
			if (UnityHelper.findNearestToCam<HabitatPlatform.Tag>()?.gameObject is GameObject platform)
				platform.transform.rotation *= Quaternion.AngleAxis(Main.config.stepRotate * n.getArg<float>(0), Vector3.up);
		}

		#region debug console commands
#if DEBUG
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

		GameObject _getFloor()
		{
			GameObject platform = UnityHelper.findNearestToCam<HabitatPlatform.Tag>()?.gameObject;
			return platform?.GetComponentInChildren<PlatformInitializer.FloorTag>()?.gameObject;
		}

		void OnConsoleCommand_hbpl_movefloor(NotificationCenter.Notification n)
		{
			if (_getFloor() is GameObject floor)
			{
				var vecParam = new Vector3(n.getArg<float>(0), n.getArg<float>(1), n.getArg<float>(2));
				(floor.transform.localPosition += vecParam * Main.config.stepMove).ToString("F4").onScreen("floor pos").logDbg();
			}
		}

		void OnConsoleCommand_hbpl_scalefloor(NotificationCenter.Notification n)
		{
			if (_getFloor() is GameObject floor)
			{
				var vecParam = new Vector3(n.getArg<float>(0), 0f, n.getArg<float>(1));
				(floor.transform.localScale += vecParam * Main.config.stepMove).ToString("F4").onScreen("floor scale").logDbg();
			}
		}

		void OnConsoleCommand_hbpl_movebase(NotificationCenter.Notification n)
		{
			if (UnityHelper.findNearestToCam<Base>()?.gameObject is GameObject baseGo)
			{
				var vecParam = new Vector3(n.getArg<float>(0), n.getArg<float>(1), n.getArg<float>(2));
				(baseGo.transform.localPosition += vecParam * Main.config.stepMove).ToString("F4").onScreen("foundation pos").logDbg();
			}
		}

		void OnConsoleCommand_hbpl_toggle_foundations(NotificationCenter.Notification _)
		{
			if (UnityHelper.findNearestToCam<HabitatPlatform.Tag>()?.gameObject is GameObject platform)
			{
				foreach (var fpiece in platform.GetComponentsInChildren<BaseFoundationPiece>())
				{
					GameObject models = fpiece.gameObject.getChild("models");

					for (int i = 0; i < models.transform.childCount; i++)
					{
						var rend = models.transform.GetChild(i).GetComponent<MeshRenderer>();

						if (rend)
							rend.enabled = !rend.enabled;
					}
				}
			}
		}
#endif
	#endregion
	}
}