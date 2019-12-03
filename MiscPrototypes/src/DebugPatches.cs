using Harmony;
using UnityEngine;

using Common;

namespace MiscPrototypes
{
	class DbgCmp: MonoBehaviour { }

	[HarmonyPatch(typeof(Player), "Update")]
	class Player_Update_Patch
	{
		static void Postfix()
		{
			DbgCmp[] cc = Object.FindObjectsOfType<DbgCmp>();
			$"DbgCmp count {cc.Length}".onScreen();

			if (Input.GetKeyDown(KeyCode.PageDown))
			{
				foreach (var d in cc)
					$"Distance to DbgCmp {(SNCameraRoot.main.transform.position -  d.gameObject.transform.position).magnitude}".onScreen();
			}

			GameObject trg = Player.main.GetComponent<GUIHand>().activeTarget;

			if (Input.GetKeyDown(KeyCode.PageUp))
				trg?.addComponentIfNeeded<DbgCmp>();

			if (Input.GetKeyDown(KeyCode.Insert))
			{
				foreach (var d in cc)
					foreach (var r in d.gameObject.GetAllComponentsInChildren<Renderer>())
						r.enabled = false;
			}

			if (Input.GetKeyDown(KeyCode.Home))
			{
				foreach (var d in cc)
					foreach (var r in d.gameObject.GetAllComponentsInChildren<Renderer>())
						r.enabled = true;
			}
		}
	}
}