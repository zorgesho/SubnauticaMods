using UnityEngine;

using Common;
using Common.GameSerialization;

namespace GravTrapImproved
{
	class GravTrapObjectsType: MonoBehaviour, IProtoEventListener
	{
		class SaveData { public ObjectsType trapObjType; }

		static readonly int[] techIndex = new int[] {0, 26, 82, 122}; // indexes in GravSphere.allowedTechTypes[]

		public enum ObjectsType {All = 0, Creatures = 1, Resources = 2, Eggs = 3, count}; // also used in UI

		string id;
		bool inited = false;

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

		public void OnProtoDeserialize(ProtobufSerializer serializer) => objType = SaveLoad.load<SaveData>(id)?.trapObjType ?? 0;
		
		public void OnProtoSerialize(ProtobufSerializer serializer) => SaveLoad.save(id, new SaveData { trapObjType = objType });

		// we may add this component while gameobject is inactive (while in inventory) and Awake for it is not calling, so we need init it that way
		public static GravTrapObjectsType getFrom(GameObject go)
		{
			GravTrapObjectsType cmp = go.GetComponent<GravTrapObjectsType>();
			if (!cmp)
			{
				cmp = go.AddComponent<GravTrapObjectsType>();
				cmp.init();
			}
			
			return cmp;
		}
		
		void Awake() => init();
		
		void init()
		{
			if (!inited)
			{
				inited = true;
				id = GetComponent<PrefabIdentifier>().Id;
				OnProtoDeserialize(null);
			}
		}

		public static void handleAttracted(Rigidbody rigidBody)
		{
			if (rigidBody.gameObject.GetComponent<Crash>() is Crash crash)
			{
				crash.AttackLastTarget(); // if target object is CrashFish we want to pull it out
			}
			else
			if (rigidBody.gameObject.GetComponent<SinkingGroundChunk>() is SinkingGroundChunk sgc)
			{
				Destroy(sgc);

				BoxCollider c = rigidBody.gameObject.AddComponent<BoxCollider>();
				c.size = new Vector3(0.736f,0.51f,0.564f);
				c.center = new Vector3(0.076f,0.224f,0.012f);

				rigidBody.gameObject.FindChild("models").transform.localPosition = Vector3.zero;
			}
		}

		public bool isValidTarget(GameObject obj)
		{
			if (obj.GetComponent<Pickupable>()?.attached ?? false)
				return false;

			if (obj.GetComponentInParent<SinkingGroundChunk>() || obj.name.Contains("TreaderShale"))
				return (objType == ObjectsType.All || objType == ObjectsType.Resources);

			TechType techType = CraftData.GetTechType(obj);

			if (techType == TechType.Crash)
				return (objType == ObjectsType.All || objType == ObjectsType.Creatures);
			
			if (techType == TechType.CrashPowder)
				return false;

			int begin = (objType == ObjectsType.All)? 0: techIndex[(int)objType - 1];
			int end = (objType == ObjectsType.All)? Gravsphere.allowedTechTypes.Length: techIndex[(int)objType];

			return 0 <= Gravsphere.allowedTechTypes.findIndex(begin, end, t => t == techType);
		}
	}
}