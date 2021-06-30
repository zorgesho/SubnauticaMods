using UnityEngine;
using Common;

#if DEBUG
using System.Linq;
using System.Collections;

using Common.UnityDebug;
#endif

namespace HabitatPlatform
{
	class ConsoleCommands: PersistentConsoleCommands
	{
		static GameObject _platform => GameUtils.findNearestToCam<HabitatPlatform.Tag>()?.gameObject;

		public void hbpl_move(float dx, float dy)
		{
			if (_platform is GameObject platform)
				platform.transform.position += Main.config.stepMove * (dx * platform.transform.right - dy * platform.transform.forward);
		}

		public static void hbpl_rotate(float angle)
		{
			if (_platform is GameObject platform)
				platform.transform.rotation *= Quaternion.AngleAxis(Main.config.stepRotate * angle, Vector3.up);
		}

		public static void hbpl_fix()
		{
			PlatformCollisionFixer.fix(_platform);
		}

		public void hbpl_reset_angles()
		{
			if (_platform is not GameObject platform)
				return;

			platform.transform.rotation = Quaternion.identity;
			platform.transform.position = platform.transform.position.setY(Main.config.defPosY);
		}

		public void hbpl_warp(float? x, float? y)
		{
			const float distance = 50f;

			if (_platform is not GameObject platform)
				return;

			Vector3 pos = MainCamera.camera.transform.position + distance * MainCamera.camera.transform.forward;
			platform.transform.position = new Vector3(x ?? pos.x, platform.transform.position.y, y ?? pos.z);
		}

		#region debug console commands
#if DEBUG
		public void hbpl_dump(int parent = 0)
		{
			_platform?.dump("platform", parent);
		}

		public void hbpl_physics(bool? enabled)
		{
			if (_platform is not GameObject platform)
				return;

			var rb = platform.GetComponent<Rigidbody>();
			rb.isKinematic = !enabled ?? !rb.isKinematic;
			$"Platform physics is {(rb.isKinematic? "off": "on")}".onScreen();
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

					if (_platform is GameObject platform)
					{
						var rb = platform.GetComponent<Rigidbody>();
						$"pos: {platform.transform.position.ToString("F4")} rot: {platform.transform.eulerAngles} rb pos: {rb.position.ToString("F4")}".onScreen("nearest platform");
					}

					yield return new WaitForSeconds(delay);
				}
			}
		}

		public void hbpl_show_colliders(bool show)
		{
			if (_platform is not GameObject platform)
				return;

			if (show)
				platform.ensureComponent<DrawColliders>();
			else
				platform.destroyComponent<DrawColliders>(false);
		}

		void _printVec(Vector3 vec, string prefix) => vec.ToString("F4").onScreen(prefix).logDbg();
		GameObject _platformFloor => _platform?.GetComponentInChildren<PlatformInitializer.FloorTag>()?.gameObject;

		public void hbpl_movefloor(float dx, float dy, float dz)
		{
			if (_platformFloor is not GameObject floor)
				return;

			floor.transform.localPosition += new Vector3(dx, dy, dz) * Main.config.stepMove;
			_printVec(floor.transform.localPosition, "floor pos");
		}

		public void hbpl_scalefloor(float dx, float dz)
		{
			if (_platformFloor is not GameObject floor)
				return;

			floor.transform.localScale += new Vector3(dx, 0f, dz) * Main.config.stepMove;
			_printVec(floor.transform.localScale, "floor scale");
		}

		public void hbpl_moveengines(float x, float y)
		{
			if (_platform is not GameObject platform)
				return;

			GameObject platformBase = platform.getChild("Base/rocketship_platform/Rocket_Geo/Rocketship_platform/");

			Vector3[] pos = { new Vector3(x, -y, 0f), new Vector3(x, y, 0f), new Vector3(-x, y, 0f), new Vector3(-x, -y, 0f) };
			for (int i = 1; i <= 4; i++)
				platformBase.transform.Find($"Rocketship_platform_power_0{i}").localPosition = pos[i - 1];
		}

		public void hbpl_lightmap(string texName)
		{
			if (_platform is not GameObject platform)
				return;

			Texture2D lightmap = AssetsHelper.loadTexture(texName);
			GameObject platformBase = platform.getChild("Base/rocketship_platform/Rocket_Geo/Rocketship_platform/Rocketship_platform_base-1/Rocketship_platform_base_MeshPart0");

			foreach (var m in platformBase.GetComponent<MeshRenderer>().materials)
				m.SetTexture("_Lightmap", lightmap);
		}

		public void hbpl_movebase(float dx, float dy, float dz)
		{
			if (_platform?.GetComponentInChildren<Base>()?.gameObject is not GameObject baseGo)
				return;

			baseGo.transform.localPosition += new Vector3(dx, dy, dz) * Main.config.stepMove;
			_printVec(baseGo.transform.localPosition, "foundation pos");
		}

		public void hbpl_printpos()
		{
			if (_platform is GameObject platform)
				_printVec(platform.transform.position, "platform pos");
		}

		public void hbpl_toggle_foundations()
		{
			_platform?.GetComponentsInChildren<BaseFoundationPiece>().
					   Select(fpiece => fpiece.gameObject.getChild("models")).OfType<GameObject>().
					   SelectMany(models => models.GetComponentsInChildren<Renderer>()).
					   ForEach(rend => rend.enabled = !rend.enabled);
		}
#endif // DEBUG
	#endregion
	}
}