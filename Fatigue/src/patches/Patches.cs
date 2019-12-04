using System;
using System.Diagnostics;
using System.Collections.Generic;

using Harmony;

using Common;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.EventSystems;

namespace Fatigue
{
	[HarmonyPatch(typeof(Player), "Awake")]
	static class Player_Awake_Patch
	{
		static void Postfix(Player __instance)
		{
			__instance.gameObject.addComponentIfNeeded<PlayerSleep>();
			__instance.gameObject.addComponentIfNeeded<EnergySurvival>();
		}
	}
	
	[HarmonyPatch(typeof(uGUI_SceneHUD), "Awake")]
	static class uGUI_Awake_Patch
	{
		static void Postfix()
		{
			uGUI_EnergyBar.create();
		}
	}

	[HarmonyPatch(typeof(uGUI_SceneHUD), "UpdateElements")]
	static class uGUISceneHUD_UpdateElements_Patch
	{
		static void Postfix(uGUI_SceneHUD __instance)
		{
			__instance.barOxygen.transform.parent.Find("EnergyBar")?.gameObject.SetActive(__instance._active && __instance._mode == 2);
		}
	}



	/// <summary>
	/// //////////////////////////////////////////////////////////////////////////////
	/// </summary>
	
	class HHH: MonoBehaviour
	{
		void Update()
		{
			ErrorMessage.AddDebug("pp " + UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject());

			if (Input.GetKeyDown(KeyCode.PageUp))
			{
				//EnergyGUI.init();
				SleepGUI.create();

//				Common.ObjectDumper.Dump(uGUI_MainMenu.main.gameObject);

				SleepGUI.main.start();

				//Common.ObjectDumper.Dump(GameObject.FindObjectOfType<uGUI_OptionsPanel>().sliderOptionPrefab);

				//foreach (var bar in bars)
				//{
				//	Console.WriteLine(bar.gameObject.GetFullHierarchyPath());
				//}
							//	Common.ObjectDumper.Dump(uGUI_MainMenu.main.gameObject);

			}
			if (Input.GetKeyDown(KeyCode.PageDown))
			{
				SleepGUI.main.stop();
		



			}
			
			
			if (Input.GetKeyDown(KeyCode.Home))
			{
				//SleepGUI.main.canva.alpha = 0.5f;

				//uGUI_OptionsPanel options = GameObject.FindObjectOfType<uGUI_OptionsPanel>();

				//Console.WriteLine(options.gameObject.GetFullHierarchyPath());
				//Common.ObjectDumper.Dump(options.gameObject);
			//	uGUI_MainMenu.main.gameObject.destroyComponent<uGUI_GraphicRaycaster>();
			}

			if (Input.GetKeyDown(KeyCode.End))
			{
				//uGUI_MainMenu.main.gameObject.destroyComponent<uGUI_GraphicRaycaster>();

				PointerEventData ped = new PointerEventData(EventSystem.current);
				ped.position =  Input.mousePosition;
				List<RaycastResult> hits = new List<RaycastResult>();
				EventSystem.current.RaycastAll(ped, hits);
 
				foreach (RaycastResult hit in hits)
				{
					GameObject go = hit.gameObject;
					Console.WriteLine($"--------------------{go.name}  {go.GetFullHierarchyPath()} ");
				}
				//foreach (RaycastResult hit in hits)
				//{
				//	GameObject go = hit.gameObject;
				//	Console.WriteLine($"-----------------------{go.name}  {go.GetFullHierarchyPath()} ");
				//	Common.ObjectDumper.Dump(go);
				//}
			}
		}
		
	}


	//[HarmonyPatch(typeof(Text))]
	//[HarmonyPatch("set_text")]
	//class Player_Update_Patsdfdfsdfdsfchsdfsdf
	//{
	//	private static void Prefix(Text __instance, string value)
	//	{
	//		if (__instance.name == "Caption")
	//		{
	//			Console.WriteLine("---" + __instance.name + " " + value);
	//			Common.Debug.printStack();
	//		}
			
	//	}
	//}

	//[HarmonyPatch(typeof(Bed))]
	//[HarmonyPatch("Update")]
	//class Player_Update_Psdfatsdfdfsdfdsfchsdfsdf
	//{
	//	private static bool Prefix(Bed __instance)
	//	{
	//		Bed.InUseMode inUseMode = __instance.inUseMode;
	//		if (inUseMode != Bed.InUseMode.None)
	//		{
	//			if (inUseMode == Bed.InUseMode.Sleeping)
	//			{
	//				if (!DayNightCycle.main.IsInSkipTimeMode() || __instance.currentPlayer == null)
	//				{
	//					__instance.ExitInUseMode(__instance.currentPlayer, true);
	//				}
	//			}
	//		}
	//		else if (__instance.currentPlayer != null)
	//		{
	//			__instance.Subscribe(__instance.currentPlayer, false);
	//			__instance.currentPlayer = null;
	//		}
			
		
	//		return false;
	//	}
	//}


//	[HarmonyPatch(typeof(UWE.Utils))]
//	[HarmonyPatch("UpdateCusorLockState")]
//	class Player_Update_Patsdfdsfchsdfsdf
//	{
//		static bool lastLockedState = false;
		
//		private static void Postfix()
//		{
			
////			Console.WriteLine("---------------cursor " + UWE.Utils.lockCursor);
//	//		Common.Debug.printStack();
			
//		}
//	}
	
	//[HarmonyPatch(typeof(UWE.Utils))]
	//[HarmonyPatch("set_lockCursor")]
	//class Player_Update_Patsdfdsfchsdfsdfsdfdsf
	//{
	//	static bool lastLockedState = false;
		
	//	private static void Postfix(bool value)
	//	{
	//		Console.WriteLine("---------------set lock cursor " + value);
	//		Common.Debug.printStack();
	//	}
	//}

	//[HarmonyPatch(typeof(AvatarInputHandler))]
	//[HarmonyPatch("OnEnable")]
	//class Player_Update_Patsdfsdfdsfchsdfsdfsdfdsf
	//{
	//	private static void Postfix(AvatarInputHandler __instance)
	//	{
	//		Console.WriteLine("---------------AvatarInputHandler ENABLED");
	//		Common.Debug.printStack();
	//	}
	//}
	//[HarmonyPatch(typeof(AvatarInputHandler))]
	//[HarmonyPatch("OnDisable")]
	//class Player_Update_Patsdfsdfdsfchsdfsdfsdfdsfsdfs
	//{
	//	private static void Postfix(AvatarInputHandler __instance)
	//	{
	//		Console.WriteLine("---------------AvatarInputHandler DISABLED");
	//		Common.Debug.printStack();
	//	}
	//}

	

	
	//[HarmonyPatch(typeof(FPSInputModule))]
	//[HarmonyPatch("UpdateCursor")]
	//class Player_Update_Patsdfdsfch
	//{
	//	private static void Postfix(FPSInputModule  __instance)
	//	{
	//		__instance.cursor.SetActive(true);
			
	//	}
	//}
	
	
	[HarmonyPatch(typeof(Player), "Update")]
	static class Player_Update_Patch
	{
		static void Prefix()
		{
			ErrorMessage.AddDebug("pp " + UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject());
		
					if (Input.GetKeyDown(KeyCode.Home))
			{
				uGUI_OptionsPanel options = GameObject.FindObjectOfType<uGUI_OptionsPanel>();

				Console.WriteLine(options.gameObject.GetFullHierarchyPath());
				//Common.ObjectDumper.Dump(options.gameObject);
			//	uGUI_MainMenu.main.gameObject.destroyComponent<uGUI_GraphicRaycaster>();
			}

			//GameObject barIcon = uGUI.main.gameObject.getChild("ScreenCanvas/HUD/Content/BarsPanel/WaterBar/");

			if (Input.GetKeyDown(KeyCode.Q))
			{
			
				GameObject barsPanel = uGUI.main.gameObject.getChild("ScreenCanvas/HUD/Content/BarsPanel/EnergyBar");
				barsPanel.SetActive(true);
		


					
			}
			if (Input.GetKeyDown(KeyCode.W))
			{
				GameObject back = uGUI.main.gameObject.getChild("ScreenCanvas/HUD/Content/BarsPanel/EnergyBar");
				Image image = back.GetComponent<Image>();
			
				Vector3 ll = back.transform.localPosition;
				ll.x += 0.5f;
				back.transform.localPosition = ll;
				Console.WriteLine($"------- pos {back.transform.localPosition}");
			
					
			}
			if (Input.GetKeyDown(KeyCode.E))
			{
				GameObject back = uGUI.main.gameObject.getChild("ScreenCanvas/HUD/Content/BarsPanel/EnergyBar");
				Image image = back.GetComponent<Image>();
			
				Vector3 ll = back.transform.localPosition;
				ll.y -= 0.5f;
				back.transform.localPosition = ll;
				Console.WriteLine($"------- pos {back.transform.localPosition}");
					
			}

			if (Input.GetKeyDown(KeyCode.R))
			{
				GameObject back = uGUI.main.gameObject.getChild("ScreenCanvas/HUD/Content/BarsPanel/EnergyBar");
				Image image = back.GetComponent<Image>();
			
				Vector3 ll = back.transform.localPosition;
				ll.y += 0.5f;
				back.transform.localPosition = ll;
				Console.WriteLine($"------- pos {back.transform.localPosition}");
					
			}

			//if (Input.GetKeyDown(KeyCode.W))
			//{
			//	GameObject back = uGUI.main.gameObject.getChild("ScreenCanvas/HUD/Content/BarsPanel/EnergyBar");
			//	Image image = back.GetComponent<Image>();
			
			//	Vector3 ll = image.rectTransform.localPosition;
			//	ll.x += 0.5f;
			//	image.rectTransform.localPosition = ll;
			//	Console.WriteLine($"------- pos {image.rectTransform.localPosition}");
			
					
			//}
			//if (Input.GetKeyDown(KeyCode.E))
			//{
			//	GameObject back = uGUI.main.gameObject.getChild("ScreenCanvas/HUD/Content/BarsPanel/EnergyBar");
			//	Image image = back.GetComponent<Image>();
			
			//	Vector3 ll = image.rectTransform.localPosition;
			//	ll.y += 0.5f;
			//	image.rectTransform.localPosition = ll;
			//	Console.WriteLine($"------- pos {image.rectTransform.localPosition}");
					
			//}
			//if (Input.GetKeyDown(KeyCode.R))
			//{
			//	GameObject back = uGUI.main.gameObject.getChild("ScreenCanvas/HUD/Content/BarsPanel/EnergyBar");
			//	Image image = back.GetComponent<Image>();
			
			//	Vector3 ll = image.rectTransform.localPosition;
			//	ll.y -= 0.5f;
			//	image.rectTransform.localPosition = ll;
			//	Console.WriteLine($"------- pos {image.rectTransform.localPosition}");
					
			//}

			//if (Input.GetKeyDown(KeyCode.T))
			//{
			//	uGUI.main.gameObject.getChild("ScreenCanvas/HUD/Content/BarsPanel/BackgroundQuad/Center").SetActive(false);
					
			//}
			//if (Input.GetKeyDown(KeyCode.Y))
			//{
			//	uGUI.main.gameObject.getChild("ScreenCanvas/HUD/Content/BarsPanel/BackgroundQuad").SetActive(false);
					
			//}
			//if (Input.GetKeyDown(KeyCode.U))
			//{
			//	uGUI.main.gameObject.getChild("ScreenCanvas/HUD/Content/BarsPanel").SetActive(false);
					
			//}

			if (Input.GetKeyDown(KeyCode.End))
			{
				//uGUI_MainMenu.main.gameObject.destroyComponent<uGUI_GraphicRaycaster>();
				Console.WriteLine($"----------------");
				PointerEventData ped = new PointerEventData(EventSystem.current);
				ped.position =  Input.mousePosition;
				List<RaycastResult> hits = new List<RaycastResult>();
				EventSystem.current.RaycastAll(ped, hits);
 
				foreach (RaycastResult hit in hits)
				{
					GameObject go = hit.gameObject;
					Console.WriteLine($"--------------------{go.name}  {go.GetFullHierarchyPath()} ");
				}
				//foreach (RaycastResult hit in hits)
				//{
				//	GameObject go = hit.gameObject;
				//	Console.WriteLine($"-----------------------{go.name}  {go.GetFullHierarchyPath()} ");
				//	Common.ObjectDumper.Dump(go);
				//}

			}
			
			if (Input.GetKeyDown(KeyCode.Insert))
				UWE.Utils.lockCursor = !UWE.Utils.lockCursor;

			if (Input.GetKeyDown(KeyCode.PageDown))
			{

				//Common.ObjectDumper.Dump(uGUI.main.gameObject);
				SleepGUI.main.stop();
			}
		
			//if (Input.GetKeyDown(KeyCode.PageUp))
			//{
			//	DayNightCycle.main.SkipTime(400, 5);
			//}


			if (Input.GetKeyDown(KeyCode.PageUp) && true)
			{
				//EnergyGUI.init();
				SleepGUI.create();

				//uGUI_PlayerSleep.main.StartSleepScreen();
				SleepGUI.main.start();

			}

			if (Input.GetKeyDown(KeyCode.PageUp) && false)
			{
				createBed();
			}

		}


		static void createBed()
		{
			GameObject pod = EscapePod.main.gameObject;

			GameObject bed = GameObject.Instantiate(CraftData.GetPrefabForTechType(TechType.NarrowBed));

			bed.destroyChild("bed_narrow/bed_narrow");
			bed.destroyChild("bed_narrow/blanket_narrow");
			bed.destroyChild("Cube");
			bed.destroyChild("Cube (1)");
			bed.destroyChild("Cube (3)");

			bed.setParent(pod);

			bed.transform.localPosition = new Vector3(1.813f, -0.05f, 0.075f);
			bed.transform.localEulerAngles = new Vector3(0, 180, 0);

			bed.getChild("bed_narrow/matress_narrow").transform.localScale = new Vector3(0.7f, 0.9f, 1f);
			bed.getChild("bed_narrow/pillow_01").transform.localPosition = new Vector3(0f, 0.556f, -0.717f);
			bed.getChild("bed_narrow/pillow_01").transform.localScale = new Vector3(1f, 0.7f, 1f);
			bed.getChild("bed_narrow/end_position/right_end").transform.localPosition = new Vector3(1.0f, 1.6401f, 0.078f);

			//Common.ObjectDumper.Dump(bed);

			GameObject goal = new GameObject("BedGoal");

			//goal.setParent(pod);
			goal.transform.position = new Vector3(-1.726f, 1.023f, -1.514f) + pod.transform.position;
			GoalObject goalObj = goal.AddComponent<GoalObject>();
			goalObj.customGoal = "This is goal";
			goalObj.findRadius = 0.01f;

			WorldArrowManager.main.CreateArrow(pod.transform, new Vector3(-1.726f, 1.023f, -1.514f), true, "OLOLO", 0, WorldArrowManager.PointDirection.Down, .3f);
			


			//var collider = bed.getChild("Cube");
		
			

			//GameObject seatLeft = pod.getChild("models/Life_Pod_damaged_03/lifepod_damaged_03_geo/life_pod_seat_01_L");

				
			//GameObject debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			////debugSphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			//debugSphere.transform.position = seatLeft.transform.position;
			////debugSphere.transform.parent = port.parent;
			//debugSphere.GetComponent<Renderer>().material.color = Color.green;
			//debugSphere.destroyComponent<Collider>(false);
			////debugSphere.SetActive(false);
		
		
		
		
		}


	}
}

				//GameObject pod = EscapePod.main.gameObject;

				//GameObject seatLeft = pod.getChild("models/Life_Pod_damaged_03/lifepod_damaged_03_geo/life_pod_seat_01_L");

				
				//GameObject debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				////debugSphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
				//debugSphere.transform.position = seatLeft.transform.position;
				////debugSphere.transform.parent = port.parent;
				//debugSphere.GetComponent<Renderer>().material.color = Color.green;
				//debugSphere.destroyComponent<Collider>(false);
				////debugSphere.SetActive(false);
