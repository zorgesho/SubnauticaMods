using System.Collections.Generic;

using UnityEngine;
using Harmony;
using Common;

namespace HabitatPlatform
{
	//[HarmonyPatch(typeof(Builder), "UpdateAllowed")]
	static class Builder_UpdateAllowed_Patch2
	{
		static bool Prefix(ref bool __result)
		{
			Builder.SetDefaultPlaceTransform(ref Builder.placePosition, ref Builder.placeRotation);
			ConstructableBase componentInParent = Builder.ghostModel.GetComponentInParent<ConstructableBase>();
			bool flag2 = true;
			if (componentInParent != null)
			{
				Transform transform = componentInParent.transform;
				transform.position = Builder.placePosition;
				transform.rotation = Builder.placeRotation;
				flag2 = componentInParent.UpdateGhostModel(Builder.GetAimTransform(), Builder.ghostModel, default(RaycastHit), out bool flag, componentInParent);
				Builder.placePosition = transform.position;
				Builder.placeRotation = transform.rotation;
				//if (flag)
				//{
				//	Builder.renderers = MaterialExtensions.AssignMaterial(Builder.ghostModel, Builder.ghostStructureMaterial);
				//	Builder.InitBounds(Builder.ghostModel);
				//}
			}
			else
			{
				flag2 = Builder.CheckAsSubModule();
			}
			//if (flag2)
			//{
			//	List<GameObject> list = new List<GameObject>();
			//	Builder.GetObstacles(Builder.placePosition, Builder.placeRotation, Builder.bounds, list);
			//	flag2 = (list.Count == 0);
			//	list.Clear();
			//}
			
			__result = flag2;

			return false;
		}
	}



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


	//[HarmonyPatch(typeof(Builder), "Begin")]
	//static class Builder_Begin_Patch
	//{
	//	static void Postfix(GameObject modulePrefab)
	//	{
	//		modulePrefab.dump("!prefab");
	//	}
	//}

	//[HarmonyPatch(typeof(BaseAddCellGhost), "UpdatePlacement")]
	static class BaseAddCellGhost_UpdatePlacement_Patch1
	{
		static void Prefix(BaseAddCellGhost __instance)
		{
			__instance.minHeightFromTerrain = 0f;
		}
	}

	//public static void GetObstacles(Vector3 position, Quaternion rotation, List<OrientedBounds> localBounds, List<GameObject> results)
	[HarmonyPatch(typeof(Builder), "GetObstacles")]
	static class BaseAddCellGhost_UpdatePlacemensdft_Patch
	{
		static bool Prefix()
		{
			return false;
		}
	}

	[HarmonyPatch(typeof(BaseAddCellGhost), "UpdatePlacement")]
	static class BaseAddCellGhost_UpdatePlacement_Patch
	{
		static bool Prefix(BaseAddCellGhost __instance, ref bool __result, Transform camera, float placeMaxDistance, out bool positionFound, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase)
		{
			//"plac".log();
			
			positionFound = false;
			geometryChanged = false;
			BaseAddCellGhost.overrideFaces.Clear();
			float placeDefaultDistance = ghostModelParentConstructableBase.placeDefaultDistance;
			Int3 @int = Base.CellSize[(int)__instance.cellType];
			Vector3 direction = Vector3.Scale((@int - 1).ToVector3(), Base.halfCellSize);
			Vector3 position = camera.position;
			Vector3 forward = camera.forward;

			//__instance.targetBase = Player_Update_Patch.lastBase; //!!!!!!!!!!!!!!

			//Vector3 lastPos = Player_Update_Patch.lastPos;


			if (__instance.cellType == Base.CellType.Moonpool)
			{
				__instance.targetBase = BaseGhost.FindBase(camera, 30f);
			}
			else
			{
				__instance.targetBase = BaseGhost.FindBase(camera, 20f);
			}

			bool flag;
			if (__instance.targetBase != null)
			{
				positionFound = true;
				flag = true;
				Vector3 a = position + forward * placeDefaultDistance;

			//	$"POS: {lastPos} {a}".log();
				Vector3 b = __instance.targetBase.transform.TransformDirection(direction);
				Int3 int2 = __instance.targetBase.WorldToGrid(a - b);
				Int3 maxs = int2 + @int - 1;
				Int3.Bounds bounds = new Int3.Bounds(int2, maxs);
				foreach (Int3 cell in bounds)
				{
					if (__instance.targetBase.GetCell(cell) != Base.CellType.Empty || __instance.targetBase.IsCellUnderConstruction(cell))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					if (__instance.cellType == Base.CellType.Foundation)
					{
						Int3.Bounds bounds2 = __instance.targetBase.Bounds;
						int y = bounds2.mins.y;
						int y2 = bounds2.maxs.y;
						Int3.Bounds bounds3 = new Int3.Bounds(int2, new Int3(maxs.x, int2.y, maxs.z));
						foreach (Int3 int3 in bounds3)
						{
							Int3 cell2 = int3;
							for (int i = int2.y - 1; i >= y; i--)
							{
								cell2.y = i;
								if (__instance.targetBase.IsCellUnderConstruction(cell2) || __instance.targetBase.GetCell(cell2) != Base.CellType.Empty)
								{
									flag = false;
									break;
								}
							}
							if (!flag)
							{
								break;
							}
							for (int j = maxs.y + 1; j <= y2; j++)
							{
								cell2.y = j;
								Base.CellType cell3 = __instance.targetBase.GetCell(cell2);
								if (__instance.targetBase.IsCellUnderConstruction(cell2) || cell3 == Base.CellType.Foundation || cell3 == Base.CellType.Moonpool)
								{
									flag = false;
									break;
								}
								if (j == maxs.y + 1 && (cell3 == Base.CellType.Observatory || cell3 == Base.CellType.MapRoom || cell3 == Base.CellType.MapRoomRotated))
								{
									flag = false;
									break;
								}
							}
							if (!flag)
							{
								break;
							}
						}
					}
					else if (__instance.cellType == Base.CellType.Moonpool)
					{
						Int3.Bounds bounds4 = new Int3.Bounds(Int3.zero, @int - 1);
						foreach (Int3 v in bounds4)
						{
							Base.CellType cell4 = __instance.targetBase.GetCell(Base.GetAdjacent(int2 + v, Base.Direction.Above));
							Base.CellType cell5 = __instance.targetBase.GetCell(Base.GetAdjacent(int2 + v, Base.Direction.Below));
							flag &= (cell4 == Base.CellType.Empty && cell5 == Base.CellType.Empty);
						}
					}
					else if (__instance.cellType == Base.CellType.Room)
					{
						__instance.connectionMask = 15;
						Int3 adjacent = Base.GetAdjacent(int2, Base.Direction.Below);
						bool flag2 = __instance.targetBase.GetRawCellType(adjacent) == Base.CellType.Room;
						bool flag3 = __instance.targetBase.CompareRoomCellTypes(adjacent, Base.CellType.Empty, false);
						bool flag4 = __instance.targetBase.CompareRoomCellTypes(adjacent, Base.CellType.Foundation, true);
						flag &= (flag2 || flag3 || flag4);
						Int3 adjacent2 = Base.GetAdjacent(int2, Base.Direction.Above);
						bool flag5 = __instance.targetBase.GetRawCellType(adjacent2) == Base.CellType.Room;
						bool flag6 = __instance.targetBase.CompareRoomCellTypes(adjacent2, Base.CellType.Empty, false);
						flag &= (flag5 || flag6);
						if (flag)
						{
							if (flag5)
							{
								BaseAddCellGhost.overrideFaces.Add(new KeyValuePair<Base.Face, Base.FaceType>(new Base.Face(Int3.zero, Base.Direction.Above), Base.FaceType.Hole));
							}
							if (flag2)
							{
								BaseAddCellGhost.overrideFaces.Add(new KeyValuePair<Base.Face, Base.FaceType>(new Base.Face(Int3.zero, Base.Direction.Below), Base.FaceType.Hole));
							}
							
							$"room: 2:{flag2} 3:{flag3} 4:{flag4} 5:{flag5} 6:{flag6}	'{adjacent}' '{adjacent2}'".log();
							//[HabitatPlatform] INFO: room: 2:False 3:False 4:True 5:False 6:True    '-1,0,0' '-1,2,0'.
							//[HabitatPlatform] INFO: room: 2:False 3:False 4:True 5:False 6:True    '-1,0,0' '-1,2,0'.

						}
					}
					else if (__instance.cellType == Base.CellType.Observatory)
					{
						flag &= (__instance.targetBase.GetCell(Base.GetAdjacent(int2, Base.Direction.Below)) == Base.CellType.Empty && __instance.targetBase.GetCell(Base.GetAdjacent(int2, Base.Direction.Above)) == Base.CellType.Empty);
						if (flag)
						{
							bool flag7 = __instance.targetBase.IsValidObsConnection(Base.GetAdjacent(int2, Base.Direction.North), Base.Direction.South);
							bool flag8 = __instance.targetBase.IsValidObsConnection(Base.GetAdjacent(int2, Base.Direction.East), Base.Direction.West);
							bool flag9 = __instance.targetBase.IsValidObsConnection(Base.GetAdjacent(int2, Base.Direction.South), Base.Direction.North);
							bool flag10 = __instance.targetBase.IsValidObsConnection(Base.GetAdjacent(int2, Base.Direction.West), Base.Direction.East);
							flag = (flag7 || flag8 || flag9 || flag10);
							if (flag7)
							{
								__instance.connectionMask = 1;
							}
							else if (flag8)
							{
								__instance.connectionMask = 4;
							}
							else if (flag9)
							{
								__instance.connectionMask = 2;
							}
							else if (flag10)
							{
								__instance.connectionMask = 8;
							}
						}
					}
				}
				if (__instance.targetOffset != int2)
				{
					__instance.targetOffset = int2;
					__instance.ghostBase.SetCell(Int3.zero, __instance.cellType);
					for (int k = 0; k < BaseAddCellGhost.overrideFaces.Count; k++)
					{
						KeyValuePair<Base.Face, Base.FaceType> keyValuePair = BaseAddCellGhost.overrideFaces[k];
						__instance.ghostBase.SetFace(keyValuePair.Key, keyValuePair.Value);
					}
					BaseAddCellGhost.overrideFaces.Clear();
					__instance.RebuildGhostGeometry();
					geometryChanged = true;
				}
				ghostModelParentConstructableBase.transform.position = __instance.targetBase.GridToWorld(int2);
				ghostModelParentConstructableBase.transform.rotation = __instance.targetBase.transform.rotation;
			}
			else
			{
				Vector3 offsetWorld = ghostModelParentConstructableBase.transform.TransformDirection(direction);
				Vector3 position2;
				flag = __instance.PlaceWithBoundsCast(position, forward, placeDefaultDistance, offsetWorld, __instance.minHeightFromTerrain, __instance.maxHeightFromTerrain, out position2);
				ghostModelParentConstructableBase.transform.position = position2;
				if (flag)
				{
					__instance.targetOffset = Int3.zero;
				}
			}
			__result = flag;
			//$"{__result}".log();
			
			return false;
		}
	}

		


	[HarmonyPatch(typeof(Player), "Update")]
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

			Rocket rrr = Object.FindObjectOfType<Rocket>();
			$"{rrr?.gameObject.getChild("Base/RocketConstructorPlatform").activeSelf}".onScreen("AAA");

			if (Input.GetKeyDown(KeyCode.Alpha7))
			{
				uGUI_RocketBuildScreen ss = rrr.GetComponentInChildren<uGUI_RocketBuildScreen>();

				if (ss)
				{
					ss.buildScreen.SetActive(false);
					ss.customizeScreen.SetActive(true);
					ss.buildAnimationScreen.SetActive(false);

				}
			}

			

			if (Input.GetKeyDown(KeyCode.Alpha8))
			{
				Rocket r = Object.FindObjectOfType<Rocket>();
				r.gameObject.getChild("Base/RocketConstructorPlatform").SetActive(true);
			}

			
			if (Input.GetKeyDown(KeyCode.Alpha9))
				move = !move;

			if (Input.GetKeyDown(KeyCode.Insert))
			{
				BaseCell[] cells = Object.FindObjectsOfType<BaseCell>();

				$"{cells.Length}".log();
				foreach (var cell in cells)
				{
					if (cell.GetComponentInChildren<BaseFoundationPiece>())
					{
						MeshRenderer[] meshes = cell.gameObject.GetAllComponentsInChildren<MeshRenderer>();

						$"{meshes.Length}".log();

						meshes.forEach(mesh => mesh.enabled = false);
					}
				}



			}
			
			if (Input.GetKeyDown(KeyCode.Home))
			{
				//Base[] bb = Object.FindObjectsOfType<Base>();

				//for (int i = 0; i < bb.Length; i++)
				//	bb[i].gameObject.dump("!!base" + i);

				//Builder.ghostModel?.dump("!ghost_model");
				floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
				floor.transform.localScale = new Vector3(12.0f, 1.0f, 12.0f);
				floor.GetComponent<Renderer>().material.color = Color.gray;
				//floor.destroyComponent<Collider>(false);
				
				//debugSphere.SetActive(false);
				
				
				Rocket r = Object.FindObjectOfType<Rocket>();
				//Base b = Object.FindObjectOfType<Base>();
				//GameObject baseGo = b.gameObject;

				floor.transform.parent = r.gameObject.transform;
				floor.transform.localPosition = new Vector3(0, 3, 0);
				floor.transform.localScale = new Vector3(43f, 0.1f, 35f);
					//baseGo.transform.localPosition = new Vector3(11.7f, -1f, 7.5f);
				floor.transform.localEulerAngles = Vector3.zero;

			}

			if (Input.GetKeyDown(KeyCode.PageUp))
			{
				Builder.Begin(CraftData.GetPrefabForTechType(TechType.BaseFoundation));
			}

			if (false && Input.GetKeyDown(KeyCode.Delete))
			{
				// parenting to platform
				{
					Rocket r = Object.FindObjectOfType<Rocket>();
					Base b = Object.FindObjectOfType<Base>();
					GameObject baseGo = b.gameObject;

					//if (baseGo.transform.parent != r.gameObject.getChild("Base").transform)
					//{
					//	baseGo.transform.parent = r.gameObject.getChild("Base").transform;
					//	//baseGo.transform.localPosition = new Vector3(12.5f, -1f, 9f);
					//	baseGo.transform.localPosition = new Vector3(11.7f, -1.4f, 7.5f);
					//	baseGo.transform.localEulerAngles = Vector3.zero;
					//}
					if (baseGo.transform.parent != r.gameObject.transform)
					{
						baseGo.transform.parent = r.gameObject.transform;
						//baseGo.transform.localPosition = new Vector3(12.5f, -1f, 9f);
						baseGo.transform.localPosition = new Vector3(11.7f, -1.4f, 7.5f);
						baseGo.transform.localEulerAngles = Vector3.zero;
					}
				}
			}

			if (true)
			{
								GameObject baseGo = Object.FindObjectOfType<Base>()?.gameObject;
				//GameObject baseGo = Object.FindObjectOfType<Rocket>()?.gameObject;

				if (move)
				{
					Vector3 v = baseGo.transform.position;
					v += Main.config.step * (Quaternion.AngleAxis(90, Vector3.up) * baseGo.transform.forward);
					baseGo.transform.localPosition = v;
					$"{baseGo.transform.localPosition}".onScreen("foundation pos");

					if (Input.GetKeyDown(KeyCode.LeftArrow))
					{
						Quaternion qq = baseGo.transform.rotation;
						qq *= Quaternion.AngleAxis(Main.config.stepAngle, Vector3.up);
						baseGo.transform.rotation = qq;
						//$"{baseGo.transform.localPosition}".onScreen("foundation pos");

					}
					if (Input.GetKeyDown(KeyCode.RightArrow))
					{
						Quaternion qq = baseGo.transform.rotation;
						qq *= Quaternion.AngleAxis(-Main.config.stepAngle, Vector3.up);
						baseGo.transform.rotation = qq;

					}



				}
				else
				if (true)
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





			if (false && Input.GetKeyDown(KeyCode.PageDown))
			{
				//private static void SetDefaultPlaceTransform(ref Vector3 position, ref Quaternion rotation)
				try
				{
					Builder.Begin(CraftData.GetPrefabForTechType(TechType.BaseFoundation));
					Builder.TryPlace();
					Builder.Update();

					ConstructableBase componentInParent = Builder.ghostModel?.GetComponentInParent<ConstructableBase>();

					//componentInParent?.Start();
					if (componentInParent != null)
					{
						BaseGhost component = Builder.ghostModel.GetComponent<BaseGhost>();
						component.Place();

						if (component.TargetBase != null)
							componentInParent.transform.SetParent(component.TargetBase.transform, true);

						componentInParent._constructed = false;
						componentInParent.constructedAmount = 0f;
						componentInParent.InitializeModelCopy();
						componentInParent.SetupRenderers();
						componentInParent.NotifyConstructedChanged(false);

						componentInParent.UpdateMaterial();


						componentInParent.constructedAmount = 1f;
						componentInParent.UpdateMaterial();
						componentInParent.model.GetComponent<BaseGhost>().Finish();
						
							lastBase = componentInParent.model.GetComponent<BaseGhost>().targetBase; //!!!!!!!!!!!
						lastPos = componentInParent.model.GetComponent<BaseGhost>().gameObject.transform.position;

						//// parenting to platform
						//{
						//	Rocket r = Object.FindObjectOfType<Rocket>();
						//	GameObject go = componentInParent.model.GetComponent<BaseGhost>().gameObject;

						//	go.transform.parent = r.gameObject.getChild("Base").transform;
						//	go.transform.localPosition = Vector3.zero;
						//	go.transform.localEulerAngles = Vector3.zero;

						//}


						componentInParent._constructed = true;
						componentInParent.constructedAmount = 1f;
						if (componentInParent.ghostOverlay != null)
						{
							componentInParent.ghostOverlay.RemoveOverlay();
						}
						if (componentInParent.modelCopy != null)
						{
							//componentInParent.modelCopy.AddComponent<BuiltEffectController>();
							componentInParent.modelCopy = null;
						}

									//lastBase = componentInParent.model.GetComponent<Base>(); //!!!!!!!!!!!
						//$"111 --- {lastBase}".log();
						///componentInParent.model.dump("model");

						componentInParent.NotifyConstructedChanged(true);
						componentInParent.SetupRenderers();

						UnityEngine.Object.Destroy(componentInParent.gameObject);


						Builder.ghostModel = null;
						Builder.prefab = null;
						Builder.canPlace = false;

						Builder.End();
						Builder.Update();
					}
				}
				catch (System.Exception e)
				{
					Log.msg(e);
				}
			}
		}
	}
}


#if SCRAPS
				Builder.Begin(CraftData.GetPrefabForTechType(TechType.BaseFoundation));
				Builder.TryPlace();
				Builder.Update();
				
				ConstructableBase componentInParent = Builder.ghostModel?.GetComponentInParent<ConstructableBase>();

				componentInParent?.Start();
				if (componentInParent != null)
				{
					BaseGhost component = Builder.ghostModel.GetComponent<BaseGhost>();
					component.Place();

					if (component.TargetBase != null)
						componentInParent.transform.SetParent(component.TargetBase.transform, true);

					//componentInParent.SetState(false, true);
					//componentInParent.constructedAmount = 1f;
					//componentInParent.Construct();

					//componentInParent.SetState(false, true);
					{
						bool value = false;
						//bool setAmount = true;
						$"componentInParent._constructed = {componentInParent._constructed}".logDbg();
						//if (componentInParent._constructed != value && value)
						//{
						//	List<ConstructableBounds> list = new List<ConstructableBounds>();
						//	componentInParent.GetComponentsInChildren<ConstructableBounds>(true, list);
						//	List<GameObject> list2 = new List<GameObject>();
						//	for (int i = 0; i < list.Count; i++)
						//	{
						//		ConstructableBounds constructableBounds = list[i];
						//		OrientedBounds orientedBounds = OrientedBounds.ToWorldBounds(constructableBounds.transform, constructableBounds.bounds);
						//		list2.Clear();
						//		Builder.GetOverlappedObjects(orientedBounds.position, orientedBounds.rotation, orientedBounds.extents, list2);
						//		int j = 0;
						//		int count = list2.Count;
						//		while (j < count)
						//		{
						//			GameObject gameObject = list2[j];
						//			if (Builder.CanDestroyObject(gameObject))
						//			{
						//				UnityEngine.Object.Destroy(gameObject);
						//			}
						//			j++;
						//		}
						//	}
						//	componentInParent.model.GetComponent<BaseGhost>().Finish();
						//	//component;
						//}

						//bool result = (componentInParent as Constructable).SetState(value, setAmount);
						{
							componentInParent._constructed = value;
							//MonoBehaviour[] components = componentInParent.gameObject.GetComponents<MonoBehaviour>();
							//int i = 0;
							//int num = components.Length;
							//while (i < num)
							//{
							//	MonoBehaviour monoBehaviour = components[i];
							//	if (!(monoBehaviour == null) && !(monoBehaviour == componentInParent) && monoBehaviour.GetType() != typeof(SubModuleHandler))
							//	{
							//		components[i].enabled = componentInParent._constructed;
							//	}
							//	i++;
							//}
							//if (componentInParent.controlledBehaviours != null)
							//{
							//	int j = 0;
							//	int num2 = componentInParent.controlledBehaviours.Length;
							//	while (j < num2)
							//	{
							//		MonoBehaviour x = componentInParent.controlledBehaviours[j];
							//		if (!(x == null) && !(x == componentInParent))
							//		{
							//			componentInParent.controlledBehaviours[j].enabled = componentInParent._constructed;
							//		}
							//		j++;
							//	}
							//}
							//if (setAmount)
							{
								componentInParent.constructedAmount = 0f;// ((!componentInParent._constructed) ? 0f : 1f);
							}
							//if (componentInParent._constructed)
							//{
							//	componentInParent.DestroyModelCopy();
							//	componentInParent.NotifyConstructedChanged(true);
							//	componentInParent.SetupRenderers();
							//	//ItemGoalTracker.OnConstruct(componentInParent.techType);
							//}
							//else
							{
								componentInParent.InitializeModelCopy();
								componentInParent.SetupRenderers();
								componentInParent.NotifyConstructedChanged(false);
							}
						}

						//if (componentInParent._constructed)
						//{
						//	UnityEngine.Object.Destroy(componentInParent.gameObject);
						//}
						//else
						{
							componentInParent.UpdateMaterial();
						}
					}


					componentInParent.constructedAmount = 1f;
					componentInParent.UpdateMaterial();
					//componentInParent.SetState(true, true);
					{
						bool value = true;
						bool setAmount = true;
						if (componentInParent._constructed != value && value)
						{
							//List<ConstructableBounds> list = new List<ConstructableBounds>();
							//componentInParent.GetComponentsInChildren<ConstructableBounds>(true, list);
							//List<GameObject> list2 = new List<GameObject>();
							//for (int i = 0; i < list.Count; i++)
							//{
							//	"11111111111111".log();
							//	ConstructableBounds constructableBounds = list[i];
							//	OrientedBounds orientedBounds = OrientedBounds.ToWorldBounds(constructableBounds.transform, constructableBounds.bounds);
							//	list2.Clear();
							//	Builder.GetOverlappedObjects(orientedBounds.position, orientedBounds.rotation, orientedBounds.extents, list2);
							//	int j = 0;
							//	int count = list2.Count;
							//	while (j < count)
							//	{
							//		GameObject gameObject = list2[j];
							//		if (Builder.CanDestroyObject(gameObject))
							//		{
							//			UnityEngine.Object.Destroy(gameObject);
							//		}
							//		j++;
							//	}
							//}
							componentInParent.model.GetComponent<BaseGhost>().Finish();
							//component;
						}

						//bool result = (componentInParent as Constructable).SetState(value, setAmount);
						if (true)
						{
							componentInParent._constructed = value;
							//MonoBehaviour[] components = componentInParent.gameObject.GetComponents<MonoBehaviour>();
							//int i = 0;
							//int num = components.Length;
							//while (i < num)
							//{
							//	MonoBehaviour monoBehaviour = components[i];
							//	if (!(monoBehaviour == null) && !(monoBehaviour == componentInParent) && monoBehaviour.GetType() != typeof(SubModuleHandler))
							//	{
							//		components[i].enabled = componentInParent._constructed;
							//	}
							//	i++;
							//}
							//if (componentInParent.controlledBehaviours != null)
							//{
							//	int j = 0;
							//	int num2 = componentInParent.controlledBehaviours.Length;
							//	while (j < num2)
							//	{
							//		MonoBehaviour x = componentInParent.controlledBehaviours[j];
							//		if (!(x == null) && !(x == componentInParent))
							//		{
							//			componentInParent.controlledBehaviours[j].enabled = componentInParent._constructed;
							//		}
							//		j++;
							//	}
							//}
							//if (setAmount)
							{
								componentInParent.constructedAmount = 1f;//((!componentInParent._constructed) ? 0f : 1f);
							}
							//if (componentInParent._constructed)
							{
								//(componentInParent as Constructable).DestroyModelCopy();
								{
									if (componentInParent.ghostOverlay != null)
									{
										componentInParent.ghostOverlay.RemoveOverlay();
									}
									if (componentInParent.modelCopy != null)
									{
										//componentInParent.modelCopy.AddComponent<BuiltEffectController>();
										componentInParent.modelCopy = null;
									}
									//if (componentInParent.builtBoxFX != null)
									//{
									//	componentInParent.GetComponentsInChildren<Renderer>(false, Constructable.sRenderers);
									//	Matrix4x4 worldToLocalMatrix = componentInParent.transform.worldToLocalMatrix;
									//	Vector3 position;
									//	Vector3 extents;
									//	OrientedBounds.EncapsulateRenderers(worldToLocalMatrix, Constructable.sRenderers, out position, out extents);
									//	Constructable.sRenderers.Clear();
									//	if (extents.x > 0f && extents.y > 0f && extents.z > 0f)
									//	{
									//		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(componentInParent.builtBoxFX);
									//		Transform transform = gameObject.transform;
									//		OrientedBounds localBounds = new OrientedBounds(position, Quaternion.identity, extents);
									//		OrientedBounds orientedBounds = OrientedBounds.ToWorldBounds(componentInParent.transform, localBounds);
									//		transform.position = orientedBounds.position;
									//		transform.rotation = orientedBounds.rotation;
									//		transform.localScale = orientedBounds.size;
									//	}
									//}

								}

								(componentInParent as Constructable).NotifyConstructedChanged(true);
								(componentInParent as Constructable).SetupRenderers();
								//Story.ItemGoalTracker.OnConstruct(componentInParent.techType);
							}
							//else
							//{
							//	componentInParent.InitializeModelCopy();
							//	componentInParent.SetupRenderers();
							//	componentInParent.NotifyConstructedChanged(false);
							//}
							//return true;

						}

						//if (componentInParent._constructed)
						{
							UnityEngine.Object.Destroy(componentInParent.gameObject);
						}
						//else
						//{
						//	componentInParent.UpdateMaterial();
						//}
					}
#endif