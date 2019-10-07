using System.IO;

using UnityEngine;
using Oculus.Newtonsoft.Json;

namespace GravTrapImproved
{
	class GravTrapObjectsType: MonoBehaviour, IProtoEventListener
	{
		class SaveData
		{
			public int trapObjType;
		}
	
		static public string GetSavePathDir()
		{
			var savePathDir = Path.Combine(@".\SNAppData\SavedGames\", Utils.GetSavegameDir());
			return Path.Combine(savePathDir, "GravTrapImproved");
		}

		static int[] techIndex = new int[] {0, 26, 82, 122}; // indexes in GravSphere.allowedTechTypes[]
		static int maxObjectsType = 3;
		static string[] strObjTypes = { "All", "Creatures", "Resources", "Eggs"};
		
		int objType = 0;
		string id;
		bool inited = false;

		// we need to add this component while gameobject is inactive (while in inventory), so Awake is not called
		static public GravTrapObjectsType getFrom(GameObject go)
		{
			GravTrapObjectsType cmp = go.GetComponent<GravTrapObjectsType>();
			if (!cmp)
			{
				cmp = go.AddComponent<GravTrapObjectsType>();
				cmp.init();
			}
			
			return cmp;
		}
		
		public void Awake()
		{
			init();
		}
		
		public void init()
		{
			if (!inited)
			{
				inited = true;
				id = GetComponent<PrefabIdentifier>().Id;
				OnProtoDeserialize(null);
			}
		}

		public void OnProtoDeserialize(ProtobufSerializer serializer)
		{
			var savePathDir = GetSavePathDir();
			var saveFile = Path.Combine(savePathDir, id + ".json");

			if (File.Exists(saveFile))
			{
				var json = File.ReadAllText(saveFile);
				var saveData = JsonConvert.DeserializeObject<SaveData>(json);

				objType = saveData.trapObjType;
			}
			else
				objType = 0;
		}
		
		
		public void OnProtoSerialize(ProtobufSerializer serializer)
		{
			var savePathDir = GetSavePathDir();
			var saveFile = Path.Combine(savePathDir, id + ".json");

			if (!Directory.Exists(savePathDir))
				Directory.CreateDirectory(savePathDir);

			var saveData = new SaveData()
			{
				trapObjType = objType
			};

			var json = JsonConvert.SerializeObject(saveData, Formatting.None);
			File.WriteAllText(saveFile, json);
		}

		
		public void switchObjectsType(int objTypeForced = -1)
		{
			objType = (objTypeForced == -1)? (objType + 1) % (maxObjectsType + 1): objTypeForced;
		}
		
		
		public string getObjectsTypeAsString()
		{
			return strObjTypes[objType];
		}

		
		public void handleAttracted(Rigidbody r)
		{
			SinkingGroundChunk sgc = r.gameObject.GetComponent<SinkingGroundChunk>();
		
			if (sgc)
			{
				UnityEngine.Object.Destroy(sgc);

				BoxCollider c = r.gameObject.AddComponent<BoxCollider>();
				c.size = new Vector3(0.736f,0.51f,0.564f);
				c.center = new Vector3(0.076f,0.224f,0.012f);
				
				r.gameObject.FindChild("models").transform.localPosition = Vector3.zero;

				return;
			}

			Crash crashFish = r.gameObject.GetComponent<Crash>();
			
			if (crashFish)
				crashFish.AttackLastTarget();
		}
		

		public bool isValidTarget(GameObject obj)
		{
			if (obj.GetComponentInParent<SinkingGroundChunk>() || obj.name.Contains("TreaderShale"))
				return (objType == 0 || objType == 2);
		
			TechType techType = CraftData.GetTechType(obj);

			if (techType == TechType.Crash)
				return (objType == 0 || objType == 1);
			
			if (techType == TechType.CrashPowder)
				return false;
			
			Pickupable component = obj.GetComponent<Pickupable>();

			if (!component || !component.attached)
			{
				int begin = (objType == 0)? 0: GravTrapObjectsType.techIndex[objType - 1];
				int end = (objType == 0)? Gravsphere.allowedTechTypes.Length: GravTrapObjectsType.techIndex[objType];
		
				for (int i = begin; i < end; ++i)
					if (Gravsphere.allowedTechTypes[i] == techType)
						return true;
			}
		
			return false;
		}
	}
}
