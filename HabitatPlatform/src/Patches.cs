using System.Collections.Generic;

using UnityEngine;
using Harmony;

using Common;

namespace HabitatPlatform
{
	[HarmonyPatch(typeof(Constructor), "GetItemSpawnPoint")]
	static class Constructor_GetItemSpawnPoint_Patch
	{
		static bool Prefix(Constructor __instance, TechType techType, ref Transform __result)
		{
			if (techType != HabitatPlatform.TechType)
				return true;

			__result = __instance.GetItemSpawnPoint(TechType.RocketBase);
			return false;
		}
	}
	//------------------------------------------------


	static class BuilderStuff
	{
		public static bool dirty = true;

		public static Base lastBase = null;

		public static bool isHabitatPlatform
		{
			get
			{
				if (dirty)
				{
					_isHabitatPlatform = Builder.ghostModel?.GetComponent<BaseGhost>()?.targetBase?.gameObject.getComponentInHierarchy<Rocket>();
					dirty = false;
				}

				return _isHabitatPlatform;
			}
		}
		static bool _isHabitatPlatform = false;

	}

	[HarmonyPatch(typeof(BaseGhost), "FindBase")]
	static class Builder_GetObstacles_Patchdsf
	{
		static void Postfix(Base __result)
		{
			if (BuilderStuff.lastBase != __result)
				BuilderStuff.dirty = true;
		}
	}

	//public static void GetObstacles(Vector3 position, Quaternion rotation, List<OrientedBounds> localBounds, List<GameObject> results)
	[HarmonyPatch(typeof(Builder), "GetObstacles")]
	static class Builder_GetObstacles_Patch
	{
		static void Postfix(Vector3 position, Quaternion rotation, List<OrientedBounds> localBounds, List<GameObject> results)
		{
			$"{results.Count}".onScreen("obstacles");

			if (results.Count > 0)
				$"{results[0].name}".onScreen("obstacles1");
		}
	}

	[HarmonyPatch(typeof(Builder), "GetOverlappedColliders")]
	static class Builder_GetObstacles_Patch11
	{
		static void Postfix(Vector3 position, Quaternion rotation, Vector3 extents, List<Collider> results)
		{
			//if (BuilderStuff.isHabitatPlatform)
			//{
			//	"PLATFORM".onScreen("builder");
			//	return;
			//}

			//"NOT PLATFORM".onScreen("builder");
			
			//return;
			//"----------".log();
			//foreach (var collider in results)
			//	collider.name.log();

			results.Clear();
		}
	}


	//[HarmonyPatch(typeof(Builder), "UpdateAllowed")]
	//static class Builder_UpdateAllowed_Patch2
	//{
	//	static bool Prefix(ref bool __result)
	//	{
	//		Builder.SetDefaultPlaceTransform(ref Builder.placePosition, ref Builder.placeRotation);
	//		ConstructableBase componentInParent = Builder.ghostModel.GetComponentInParent<ConstructableBase>();
	//		bool flag2 = true;
	//		if (componentInParent != null)
	//		{
	//			Transform transform = componentInParent.transform;
	//			transform.position = Builder.placePosition;
	//			transform.rotation = Builder.placeRotation;
	//			flag2 = componentInParent.UpdateGhostModel(Builder.GetAimTransform(), Builder.ghostModel, default(RaycastHit), out bool flag, componentInParent);
	//			Builder.placePosition = transform.position;
	//			Builder.placeRotation = transform.rotation;
	//			//if (flag)
	//			//{
	//			//	Builder.renderers = MaterialExtensions.AssignMaterial(Builder.ghostModel, Builder.ghostStructureMaterial);
	//			//	Builder.InitBounds(Builder.ghostModel);
	//			//}
	//		}
	//		else
	//		{
	//			flag2 = Builder.CheckAsSubModule();
	//		}
	//		//if (flag2)
	//		//{
	//		//	List<GameObject> list = new List<GameObject>();
	//		//	Builder.GetObstacles(Builder.placePosition, Builder.placeRotation, Builder.bounds, list);
	//		//	flag2 = (list.Count == 0);
	//		//	list.Clear();
	//		//}
			
	//		__result = flag2;

	//		return false;
	//	}
	//}


	//[HarmonyPatch(typeof(BaseAddCellGhost), "UpdatePlacement")]
	//static class BaseAddCellGhost_UpdatePlacement_Patch1
	//{
	//	static void Prefix(BaseAddCellGhost __instance)
	//	{
	//		__instance.minHeightFromTerrain = 0f;
	//	}
	//}


	//[HarmonyPatch(typeof(BaseAddCellGhost), "UpdatePlacement")]
	//static class BaseAddCellGhost_UpdatePlacement_Patch
	//{
	//	static bool Prefix(BaseAddCellGhost __instance, ref bool __result, Transform camera, float placeMaxDistance, out bool positionFound, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase)
	//	{
	//		//"plac".log();
			
	//		positionFound = false;
	//		geometryChanged = false;
	//		BaseAddCellGhost.overrideFaces.Clear();
	//		float placeDefaultDistance = ghostModelParentConstructableBase.placeDefaultDistance;
	//		Int3 @int = Base.CellSize[(int)__instance.cellType];
	//		Vector3 direction = Vector3.Scale((@int - 1).ToVector3(), Base.halfCellSize);
	//		Vector3 position = camera.position;
	//		Vector3 forward = camera.forward;

	//		//__instance.targetBase = Player_Update_Patch.lastBase; //!!!!!!!!!!!!!!

	//		//Vector3 lastPos = Player_Update_Patch.lastPos;


	//		if (__instance.cellType == Base.CellType.Moonpool)
	//		{
	//			__instance.targetBase = BaseGhost.FindBase(camera, 30f);
	//		}
	//		else
	//		{
	//			__instance.targetBase = BaseGhost.FindBase(camera, 20f);
	//		}

	//		bool flag;
	//		if (__instance.targetBase != null)
	//		{
	//			positionFound = true;
	//			flag = true;
	//			Vector3 a = position + forward * placeDefaultDistance;

	//		//	$"POS: {lastPos} {a}".log();
	//			Vector3 b = __instance.targetBase.transform.TransformDirection(direction);
	//			Int3 int2 = __instance.targetBase.WorldToGrid(a - b);
	//			Int3 maxs = int2 + @int - 1;
	//			Int3.Bounds bounds = new Int3.Bounds(int2, maxs);
	//			foreach (Int3 cell in bounds)
	//			{
	//				if (__instance.targetBase.GetCell(cell) != Base.CellType.Empty || __instance.targetBase.IsCellUnderConstruction(cell))
	//				{
	//					flag = false;
	//					break;
	//				}
	//			}
	//			if (flag)
	//			{
	//				if (__instance.cellType == Base.CellType.Foundation)
	//				{
	//					Int3.Bounds bounds2 = __instance.targetBase.Bounds;
	//					int y = bounds2.mins.y;
	//					int y2 = bounds2.maxs.y;
	//					Int3.Bounds bounds3 = new Int3.Bounds(int2, new Int3(maxs.x, int2.y, maxs.z));
	//					foreach (Int3 int3 in bounds3)
	//					{
	//						Int3 cell2 = int3;
	//						for (int i = int2.y - 1; i >= y; i--)
	//						{
	//							cell2.y = i;
	//							if (__instance.targetBase.IsCellUnderConstruction(cell2) || __instance.targetBase.GetCell(cell2) != Base.CellType.Empty)
	//							{
	//								flag = false;
	//								break;
	//							}
	//						}
	//						if (!flag)
	//						{
	//							break;
	//						}
	//						for (int j = maxs.y + 1; j <= y2; j++)
	//						{
	//							cell2.y = j;
	//							Base.CellType cell3 = __instance.targetBase.GetCell(cell2);
	//							if (__instance.targetBase.IsCellUnderConstruction(cell2) || cell3 == Base.CellType.Foundation || cell3 == Base.CellType.Moonpool)
	//							{
	//								flag = false;
	//								break;
	//							}
	//							if (j == maxs.y + 1 && (cell3 == Base.CellType.Observatory || cell3 == Base.CellType.MapRoom || cell3 == Base.CellType.MapRoomRotated))
	//							{
	//								flag = false;
	//								break;
	//							}
	//						}
	//						if (!flag)
	//						{
	//							break;
	//						}
	//					}
	//				}
	//				else if (__instance.cellType == Base.CellType.Moonpool)
	//				{
	//					Int3.Bounds bounds4 = new Int3.Bounds(Int3.zero, @int - 1);
	//					foreach (Int3 v in bounds4)
	//					{
	//						Base.CellType cell4 = __instance.targetBase.GetCell(Base.GetAdjacent(int2 + v, Base.Direction.Above));
	//						Base.CellType cell5 = __instance.targetBase.GetCell(Base.GetAdjacent(int2 + v, Base.Direction.Below));
	//						flag &= (cell4 == Base.CellType.Empty && cell5 == Base.CellType.Empty);
	//					}
	//				}
	//				else if (__instance.cellType == Base.CellType.Room)
	//				{
	//					__instance.connectionMask = 15;
	//					Int3 adjacent = Base.GetAdjacent(int2, Base.Direction.Below);
	//					bool flag2 = __instance.targetBase.GetRawCellType(adjacent) == Base.CellType.Room;
	//					bool flag3 = __instance.targetBase.CompareRoomCellTypes(adjacent, Base.CellType.Empty, false);
	//					bool flag4 = __instance.targetBase.CompareRoomCellTypes(adjacent, Base.CellType.Foundation, true);
	//					flag &= (flag2 || flag3 || flag4);
	//					Int3 adjacent2 = Base.GetAdjacent(int2, Base.Direction.Above);
	//					bool flag5 = __instance.targetBase.GetRawCellType(adjacent2) == Base.CellType.Room;
	//					bool flag6 = __instance.targetBase.CompareRoomCellTypes(adjacent2, Base.CellType.Empty, false);
	//					flag &= (flag5 || flag6);
	//					if (flag)
	//					{
	//						if (flag5)
	//						{
	//							BaseAddCellGhost.overrideFaces.Add(new KeyValuePair<Base.Face, Base.FaceType>(new Base.Face(Int3.zero, Base.Direction.Above), Base.FaceType.Hole));
	//						}
	//						if (flag2)
	//						{
	//							BaseAddCellGhost.overrideFaces.Add(new KeyValuePair<Base.Face, Base.FaceType>(new Base.Face(Int3.zero, Base.Direction.Below), Base.FaceType.Hole));
	//						}
							
	//						$"room: 2:{flag2} 3:{flag3} 4:{flag4} 5:{flag5} 6:{flag6}	'{adjacent}' '{adjacent2}'".log();
	//						//[HabitatPlatform] INFO: room: 2:False 3:False 4:True 5:False 6:True    '-1,0,0' '-1,2,0'.
	//						//[HabitatPlatform] INFO: room: 2:False 3:False 4:True 5:False 6:True    '-1,0,0' '-1,2,0'.

	//					}
	//				}
	//				else if (__instance.cellType == Base.CellType.Observatory)
	//				{
	//					flag &= (__instance.targetBase.GetCell(Base.GetAdjacent(int2, Base.Direction.Below)) == Base.CellType.Empty && __instance.targetBase.GetCell(Base.GetAdjacent(int2, Base.Direction.Above)) == Base.CellType.Empty);
	//					if (flag)
	//					{
	//						bool flag7 = __instance.targetBase.IsValidObsConnection(Base.GetAdjacent(int2, Base.Direction.North), Base.Direction.South);
	//						bool flag8 = __instance.targetBase.IsValidObsConnection(Base.GetAdjacent(int2, Base.Direction.East), Base.Direction.West);
	//						bool flag9 = __instance.targetBase.IsValidObsConnection(Base.GetAdjacent(int2, Base.Direction.South), Base.Direction.North);
	//						bool flag10 = __instance.targetBase.IsValidObsConnection(Base.GetAdjacent(int2, Base.Direction.West), Base.Direction.East);
	//						flag = (flag7 || flag8 || flag9 || flag10);
	//						if (flag7)
	//						{
	//							__instance.connectionMask = 1;
	//						}
	//						else if (flag8)
	//						{
	//							__instance.connectionMask = 4;
	//						}
	//						else if (flag9)
	//						{
	//							__instance.connectionMask = 2;
	//						}
	//						else if (flag10)
	//						{
	//							__instance.connectionMask = 8;
	//						}
	//					}
	//				}
	//			}
	//			if (__instance.targetOffset != int2)
	//			{
	//				__instance.targetOffset = int2;
	//				__instance.ghostBase.SetCell(Int3.zero, __instance.cellType);
	//				for (int k = 0; k < BaseAddCellGhost.overrideFaces.Count; k++)
	//				{
	//					KeyValuePair<Base.Face, Base.FaceType> keyValuePair = BaseAddCellGhost.overrideFaces[k];
	//					__instance.ghostBase.SetFace(keyValuePair.Key, keyValuePair.Value);
	//				}
	//				BaseAddCellGhost.overrideFaces.Clear();
	//				__instance.RebuildGhostGeometry();
	//				geometryChanged = true;
	//			}
	//			ghostModelParentConstructableBase.transform.position = __instance.targetBase.GridToWorld(int2);
	//			ghostModelParentConstructableBase.transform.rotation = __instance.targetBase.transform.rotation;
	//		}
	//		else
	//		{
	//			Vector3 offsetWorld = ghostModelParentConstructableBase.transform.TransformDirection(direction);
	//			Vector3 position2;
	//			flag = __instance.PlaceWithBoundsCast(position, forward, placeDefaultDistance, offsetWorld, __instance.minHeightFromTerrain, __instance.maxHeightFromTerrain, out position2);
	//			ghostModelParentConstructableBase.transform.position = position2;
	//			if (flag)
	//			{
	//				__instance.targetOffset = Int3.zero;
	//			}
	//		}
	//		__result = flag;
	//		//$"{__result}".log();
			
	//		return false;
	//	}
	//}

		


	//[HarmonyPatch(typeof(Player), "Update")]
	static class Player_Update_Patch
	{
		public static Base lastBase = null;
		public static Vector3 lastPos = Vector3.zero;
		public static bool movePlatform = false;
		public static GameObject floor = null;

		static bool move = false;
		
		static void Prefix()
		{
			//if (Input.GetKeyDown(KeyCode.M))
			//{
			//	movePlatform = !movePlatform;
			//}

			//if (movePlatform)
			//{
			//	Rocket r = Object.FindObjectOfType<Rocket>();

			//	if (r != null)
			//	{
			//		Vector3 v = r.gameObject.transform.position;
			//		v.x += Main.config.step * 5;
			//		r.gameObject.transform.position = v;
			//	}
			//}

			//Rocket rrr = Object.FindObjectOfType<Rocket>();
			//$"{rrr?.gameObject.getChild("Base/RocketConstructorPlatform").activeSelf}".onScreen("AAA");

			//if (Input.GetKeyDown(KeyCode.Alpha7))
			//{
			//	uGUI_RocketBuildScreen ss = rrr.GetComponentInChildren<uGUI_RocketBuildScreen>();

			//	if (ss)
			//	{
			//		ss.buildScreen.SetActive(false);
			//		ss.customizeScreen.SetActive(true);
			//		ss.buildAnimationScreen.SetActive(false);

			//	}
			//}

			//if (Input.GetKeyDown(KeyCode.Alpha8))
			//{
			//	Rocket r = Object.FindObjectOfType<Rocket>();
			//	r.gameObject.getChild("Base/RocketConstructorPlatform").SetActive(true);
			//}

			
			//if (Input.GetKeyDown(KeyCode.Alpha9))
			//	move = !move;

			//if (false && Input.GetKeyDown(KeyCode.Delete))
			//{
			//	// parenting to platform
			//	{
			//		Rocket r = Object.FindObjectOfType<Rocket>();
			//		Base b = Object.FindObjectOfType<Base>();
			//		GameObject baseGo = b.gameObject;

			//		//if (baseGo.transform.parent != r.gameObject.getChild("Base").transform)
			//		//{
			//		//	baseGo.transform.parent = r.gameObject.getChild("Base").transform;
			//		//	//baseGo.transform.localPosition = new Vector3(12.5f, -1f, 9f);
			//		//	baseGo.transform.localPosition = new Vector3(11.7f, -1.4f, 7.5f);
			//		//	baseGo.transform.localEulerAngles = Vector3.zero;
			//		//}
			//		if (baseGo.transform.parent != r.gameObject.transform)
			//		{
			//			baseGo.transform.parent = r.gameObject.transform;
			//			//baseGo.transform.localPosition = new Vector3(12.5f, -1f, 9f);
			//			baseGo.transform.localPosition = new Vector3(11.7f, -1.4f, 7.5f);
			//			baseGo.transform.localEulerAngles = Vector3.zero;
			//		}
			//	}
			//}

			if (true)
			{
				GameObject baseGo = Object.FindObjectOfType<Base>()?.gameObject;
				//GameObject baseGo = Object.FindObjectOfType<Rocket>()?.gameObject;

				//if (move)
				//{
				//	Vector3 v = baseGo.transform.position;
				//	v += Main.config.step * (Quaternion.AngleAxis(90, Vector3.up) * baseGo.transform.forward);
				//	baseGo.transform.localPosition = v;
				//	$"{baseGo.transform.localPosition}".onScreen("foundation pos");

				//	if (Input.GetKeyDown(KeyCode.LeftArrow))
				//	{
				//		Quaternion qq = baseGo.transform.rotation;
				//		qq *= Quaternion.AngleAxis(Main.config.stepAngle, Vector3.up);
				//		baseGo.transform.rotation = qq;
				//		//$"{baseGo.transform.localPosition}".onScreen("foundation pos");

				//	}
				//	if (Input.GetKeyDown(KeyCode.RightArrow))
				//	{
				//		Quaternion qq = baseGo.transform.rotation;
				//		qq *= Quaternion.AngleAxis(-Main.config.stepAngle, Vector3.up);
				//		baseGo.transform.rotation = qq;

				//	}



				//}
				//else
				if (false)
				{
					if (Input.GetKeyDown(KeyCode.UpArrow))
					{
						Vector3 v = baseGo.transform.localPosition;
						v.x += Main.config.step;
						baseGo.transform.localPosition = v;
						$"{baseGo.transform.localPosition}".onScreen("foundation pos");
					}
					if (Input.GetKeyDown(KeyCode.DownArrow))
					{
						Vector3 v = baseGo.transform.localPosition;
						v.x -= Main.config.step;
						baseGo.transform.localPosition = v;
						$"{baseGo.transform.localPosition}".onScreen("foundation pos");

					}
					if (Input.GetKeyDown(KeyCode.LeftArrow))
					{
						Vector3 v = baseGo.transform.localPosition;
						v.z += Main.config.step;
						baseGo.transform.localPosition = v;
						$"{baseGo.transform.localPosition}".onScreen("foundation pos");

					}
					if (Input.GetKeyDown(KeyCode.RightArrow))
					{
						Vector3 v = baseGo.transform.localPosition;
						v.z -= Main.config.step;
						baseGo.transform.localPosition = v;
						$"{baseGo.transform.localPosition}".onScreen("foundation pos");

					}

					if (Input.GetKeyDown(KeyCode.P))
					{
						Vector3 v = baseGo.transform.localPosition;
						v.y += Main.config.step;
						baseGo.transform.localPosition = v;
						$"{baseGo.transform.localPosition}".onScreen("foundation pos");
					}

					if (Input.GetKeyDown(KeyCode.O))
					{
						Vector3 v = baseGo.transform.localPosition;
						v.y -= Main.config.step;
						baseGo.transform.localPosition = v;
						$"{baseGo.transform.localPosition}".onScreen("foundation pos");
					}
				}
				else
				{
					if (Input.GetKeyDown(KeyCode.UpArrow))
					{
						Vector3 v = baseGo.transform.localScale;
						v.x += Main.config.step;
						baseGo.transform.localScale = v;
						$"{baseGo.transform.localScale}".onScreen("foundation pos");
					}
					if (Input.GetKeyDown(KeyCode.DownArrow))
					{
						Vector3 v = baseGo.transform.localScale;
						v.x -= Main.config.step;
						baseGo.transform.localScale = v;
						$"{baseGo.transform.localScale}".onScreen("foundation pos");

					}
					if (Input.GetKeyDown(KeyCode.LeftArrow))
					{
						Vector3 v = baseGo.transform.localScale;
						v.z += Main.config.step;
						baseGo.transform.localScale = v;
						$"{baseGo.transform.localScale}".onScreen("foundation pos");

					}
					if (Input.GetKeyDown(KeyCode.RightArrow))
					{
						Vector3 v = baseGo.transform.localScale;
						v.z -= Main.config.step;
						baseGo.transform.localScale = v;
						$"{baseGo.transform.localScale}".onScreen("foundation pos");

					}

					if (Input.GetKeyDown(KeyCode.P))
					{
						Vector3 v = baseGo.transform.localScale;
						v.y += Main.config.step;
						baseGo.transform.localScale = v;
						$"{baseGo.transform.localScale}".onScreen("foundation pos");
					}

					if (Input.GetKeyDown(KeyCode.O))
					{
						Vector3 v = baseGo.transform.localScale;
						v.y -= Main.config.step;
						baseGo.transform.localScale = v;
						$"{baseGo.transform.localScale}".onScreen("foundation pos");
					}
				}
			}
		}
	}
}