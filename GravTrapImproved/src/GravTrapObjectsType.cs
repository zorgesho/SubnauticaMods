using System.IO;

using UnityEngine;
using Oculus.Newtonsoft.Json;

namespace GravTrapImproved
{
	class GravTrapObjectsType: MonoBehaviour, IProtoEventListener
	{
		class SaveData
		{
			public ObjectsType trapObjType;
		}
	
		static public string GetSavePathDir()
		{
			var savePathDir = Path.Combine(@".\SNAppData\SavedGames\", Utils.GetSavegameDir());
			return Path.Combine(savePathDir, "GravTrapImproved");
		}

		static readonly int[] techIndex = new int[] {0, 26, 82, 122}; // indexes in GravSphere.allowedTechTypes[]
		
		public enum ObjectsType {All = 0, Creatures = 1, Resources = 2, Eggs = 3, count}; // also used in UI

		public ObjectsType ObjType
		{
			get => objType;
			
			set
			{
				objType = value;

				if (objType < 0)
					objType = ObjectsType.count - 1;

				if (objType >= ObjectsType.count)
					objType = 0;
			}
		}
		ObjectsType objType = ObjectsType.All;

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

			// if target object is CrashFish we want to pull it out
			r.gameObject.GetComponent<Crash>()?.AttackLastTarget();
		}
		

		public bool isValidTarget(GameObject obj)
		{
			if (obj.GetComponentInParent<SinkingGroundChunk>() || obj.name.Contains("TreaderShale"))
				return (objType == ObjectsType.All || objType == ObjectsType.Resources);
		
			TechType techType = CraftData.GetTechType(obj);

			if (techType == TechType.Crash)
				return (objType == ObjectsType.All || objType == ObjectsType.Creatures);
			
			if (techType == TechType.CrashPowder)
				return false;
			
			Pickupable component = obj.GetComponent<Pickupable>();

			if (!component || !component.attached)
			{
				int begin = (objType == ObjectsType.All)? 0: techIndex[(int)objType - 1];
				int end = (objType == ObjectsType.All)? Gravsphere.allowedTechTypes.Length: techIndex[(int)objType];
		
				for (int i = begin; i < end; ++i)
					if (Gravsphere.allowedTechTypes[i] == techType)
						return true;
			}
		
			return false;
		}
	}
}