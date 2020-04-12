using System.Collections.Generic;
using UnityEngine;
using Common.GameSerialization;

namespace GravTrapImproved
{
	class GravTrapObjectsType: MonoBehaviour, IProtoEventListener
	{
		static class Types
		{
			public static int listCount { get; private set; }
			static List<TypesConfig.TechTypeList> typeLists;

			public static string getListName(int index) => typeLists[index].name;
			public static bool contains(int index, TechType techType) => typeLists[index].contains(techType);
			public static void init(TypesConfig typesConfig)
			{
				listCount = typesConfig.techTypeLists.Count;

				typeLists = typesConfig.techTypeLists;
				typeLists.Insert(0, new TypesConfig.TechTypeList("ids_All"));

				for (int i = 1; i <= listCount; i++)
				{
					if (!typesConfig.noJoin.Contains(typeLists[i].name))
						typeLists[0].add(typeLists[i]);

					L10n.add(typeLists[i].name, typeLists[i].name);
				}
			}
		}

		public static void init(TypesConfig typesConfig) => Types.init(typesConfig);

		class SaveData { public int trapObjType; }
		string id;

		public int techTypeListIndex
		{
			get => _techTypeListIndex;

			set => _techTypeListIndex = value < 0? Types.listCount: value % (Types.listCount + 1);
		}
		int _techTypeListIndex = 0;

		public string techTypeListName // for GUI
		{
			get
			{
				if (_cachedIndex != techTypeListIndex)
				{
					_cachedGUIString = L10n.str("ids_objectsType") + L10n.str(Types.getListName(techTypeListIndex));
					_cachedIndex = techTypeListIndex;
				}

				return _cachedGUIString;
			}
		}
		int _cachedIndex = -1;
		string _cachedGUIString = null;

		public void OnProtoDeserialize(ProtobufSerializer serializer) =>
			techTypeListIndex = Mathf.Min(Types.listCount, SaveLoad.load<SaveData>(id)?.trapObjType ?? 0);

		public void OnProtoSerialize(ProtobufSerializer serializer) =>
			SaveLoad.save(id, new SaveData { trapObjType = techTypeListIndex });

		// we may add this component while gameobject is inactive (while in inventory) and Awake for it is not called
		// so we need initialize it that way
		public static GravTrapObjectsType getFrom(GameObject go) =>
			go.GetComponent<GravTrapObjectsType>() ?? go.AddComponent<GravTrapObjectsType>().init();

		void Awake() => init();

		bool inited = false;
		GravTrapObjectsType init()
		{
			if (!inited && (inited = true))
			{
				id = GetComponent<PrefabIdentifier>().Id;
				OnProtoDeserialize(null);
			}

			return this;
		}

		public void handleAttracted(GameObject obj, bool added)
		{
			if (added && obj.GetComponent<Crash>() is Crash crash)
			{
				crash.AttackLastTarget(); // if target object is CrashFish we want to pull it out
			}
			else
			if (added && obj.GetComponent<SinkingGroundChunk>() is SinkingGroundChunk sgc)
			{
				Destroy(sgc);

				BoxCollider c = obj.AddComponent<BoxCollider>();
				c.size = new Vector3(0.736f,0.51f,0.564f);
				c.center = new Vector3(0.076f,0.224f,0.012f);

				obj.FindChild("models").transform.localPosition = Vector3.zero;
			}

			if (GetComponent<GravTrapMK2.Tag>() && obj.GetComponent<GasPod>() is GasPod gasPod)
			{
				gasPod.grabbedByPropCannon = added;
				if (!added)
					gasPod.PrepareDetonationTime();
			}
		}

		TechType getObjectTechType(GameObject obj)
		{
			if (obj.GetComponentInParent<SinkingGroundChunk>() || obj.name.Contains("TreaderShale"))
				return TechType.ShaleChunk;

			if (obj.GetComponent<GasPod>() is GasPod gasPod)
				return gasPod.detonated? TechType.None: TechType.GasPod;

			return CraftData.GetTechType(obj);
		}

		public bool isValidTarget(GameObject obj) // ! called on each frame for each attracted object
		{
			if (obj.GetComponent<Pickupable>()?.attached == true)
				return false;

			return Types.contains(techTypeListIndex, getObjectTechType(obj));
		}
	}
}