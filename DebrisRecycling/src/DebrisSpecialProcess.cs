using System;
using System.Collections.Generic;

using UnityEngine;

using Common;

namespace DebrisRecycling
{
	static class DebrisSpecialProcess
	{
		class DebrisProcessed: MonoBehaviour {}

		static readonly Dictionary<string, Action<GameObject>> debrisSpecial = new Dictionary<string, Action<GameObject>>()
		{
			{"078b41f8-968e-4ca3-8a7e-4e3d7d98422c", process_SubmarineLocker05},				// submarine_locker_05
			{"8ce870ba-b559-45d7-9c10-a5477967db24", process_TechLight},						// tech_light_deco
			{"0f779340-8064-4308-8baa-6be9324a1e05", process_TechBox},							// Starship_tech_box_01_02
			{"4e8f6009-fc9c-4774-9ddc-27a6b0081dde", process_Room06Wreck},						// room_06_wreck
			{"40e2a610-19dc-4ae8-b0c1-816230ab1ce3", process_VendingMachine},					// VendingMachine
			{"386f311e-0d93-44cf-a180-f388820cb35b", process_descent_trashcans_01},				// descent_trashcans_01

			// check for size
			{"5cd34124-935f-4628-b694-a266bc2f5517", process_Starship_exploded_debris_01},		// Starship_exploded_debris_01
			{"df36cdfb-abee-41f1-bdc6-fec6566d3557", process_Starship_exploded_debris_06},		// Starship_exploded_debris_06
			{"d88147fb-007c-481f-aa75-ebcbab24e4a8", process_Starship_exploded_debris_19},		// Starship_exploded_debris_19
			{"72437ebc-7d61-49b8-bac4-cb7f3af3af8e", process_Starship_exploded_debris_22},		// Starship_exploded_debris_22
		};

		public static void tryProcessSpecial(PrefabIdentifier prefabID)
		{
			if (debrisSpecial.TryGetValue(prefabID.ClassId, out Action<GameObject> processFunc))
			{																					$"Special processing {prefabID.gameObject.name}".logDbg();
				processFunc(prefabID.gameObject);
				prefabID.gameObject.ensureComponent<DebrisProcessed>();
			}
		}

		static void process_Room06Wreck(GameObject go)
		{
			if (!go.GetComponent<DebrisProcessed>())
			{
				GameObject model = UnityEngine.Object.Instantiate(go);
				model.name = "model";
				model.destroyChild("Cube (13)");
				model.destroyComponent<WorldForces>();
				model.destroyComponent<PrefabIdentifier>();
				model.destroyComponent<Rigidbody>();
				model.destroyComponent<Constructable>();
				model.destroyComponent<LargeWorldEntity>();
				model.destroyComponent<ResourceTracker>();

				model.transform.parent = go.transform;
				model.transform.localPosition = Vector3.zero;
				model.transform.localEulerAngles = Vector3.zero;

				go.destroyComponent<MeshFilter>();
				go.destroyComponent<MeshRenderer>();
			}

			go.GetComponent<Constructable>().model = go.getChild("model");
		}

		static void process_TechBox(GameObject go)
		{
			go.GetComponent<Constructable>().model = go.getChild("Starship_tech_box_01_02");
		}

		static void process_TechLight(GameObject go)
		{
			go.destroyChild("x_TechLight_Cone");
			go.GetComponent<Constructable>().model = go.getChild("model");
		}

		static void process_VendingMachine(GameObject go)
		{
			go.GetComponent<Constructable>().model = go.getChild("Vending_machine");
		}

		static void process_descent_trashcans_01(GameObject go)
		{
			go.GetComponent<Constructable>().model = go.getChild("descent_trashcan_01");
		}

		static void process_SubmarineLocker05(GameObject go)
		{
			if (!go.GetComponent<DebrisProcessed>())
			{
				GameObject modelRoot = new GameObject("modelroot");
				modelRoot.transform.parent = go.transform;

				go.getChild("mirror").transform.parent = modelRoot.transform;
				go.getChild("paper_01").transform.parent = modelRoot.transform;
				go.getChild("paper_02").transform.parent = modelRoot.transform;
				go.getChild("girl_photo").transform.parent = modelRoot.transform;
				go.getChild("submarine_locker_05").transform.parent = modelRoot.transform;
				go.getChild("submarine_locker_03_door_01/Cube (1)").transform.parent = go.getChild("collision").transform;
				go.getChild("submarine_locker_03_door_01").transform.parent = modelRoot.transform;
			}

			go.GetComponent<Constructable>().model = go.getChild("modelroot");
		}


		static bool checkIfTooBig(GameObject go, float sizeTooBig)
		{
			if (go.transform.localScale.x <= sizeTooBig)
				return false;
																		$"{go.name} is too big, removing Constructable".logDbg();
			DebrisPatcher.unpatchObject(go, false);
			return true;
		}

		static void process_Starship_exploded_debris_01(GameObject go)
		{
			if (!checkIfTooBig(go, 1.51f) && go.transform.localScale.x > 1.29f)
				go.GetComponent<Constructable>().resourceMap.add(ScrapMetalSmall.TechType, (go.transform.localScale.x > 1.4f? 2: 1));  // add additional resources
		}

		static void process_Starship_exploded_debris_06(GameObject go)
		{
			if (!checkIfTooBig(go, 1.3f) && go.transform.localScale.x > 1.1f)
				go.GetComponent<Constructable>().resourceMap.add(ScrapMetalSmall.TechType, 2);  // add additional resources
		}

		static void process_Starship_exploded_debris_19(GameObject go) => checkIfTooBig(go, 1.21f);

		static void process_Starship_exploded_debris_22(GameObject go) => checkIfTooBig(go, 0.41f);
	}
}