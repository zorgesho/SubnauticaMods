using Harmony;
using UnityEngine;

namespace MiscPrototypes
{
	//static class CraftingStuff
	//{
	//	public static GameObject lastCrafted = null;
	//	public static float moveStep = 0.01f;

	//	public static void moveObject(int direction)
	//	{
	//		if (!lastCrafted)
	//			return;
			
	//		Vector3 v = lastCrafted.transform.localRotation * Vector3.forward * moveStep;

	//		v *= direction;
			
	//		lastCrafted.transform.localPosition += v;
	//	}
	//}

	//[HarmonyPatch(typeof(Player), "Update")]
	//class Player_Update_Patch
	//{
	//	static void Postfix()
	//	{
	//		if (Input.GetKeyDown(KeyCode.PageUp))
	//			CraftingStuff.moveObject(1);

	//		if (Input.GetKeyDown(KeyCode.PageDown))
	//			CraftingStuff.moveObject(-1);
	//	}
	//}
}