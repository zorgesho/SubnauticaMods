using System.Collections.Generic;

using UnityEngine;

using Common;
using Common.GameSerialization;

namespace FloatingCargoCrate
{
	class FloatingCargoCrateControl: MonoBehaviour, IConstructable, IProtoEventListener
	{
		class SaveData { public string beaconID; }

		// deploy animation for pillow also contains translate and we don't need it
		// so when animation will play we'll keep pillow on the same position relative to parent
		// and we disable animator after animation, so it won't play on deconstruct
		class AnimFixer
		{
			readonly Animator animator;
			readonly Transform pillowTransform;

			readonly int animHash = Animator.StringToHash("open/close.deploy");

			public AnimFixer(GameObject model)
			{
				animator = model.GetComponent<Animator>();
				animator.enabled = true;

				pillowTransform = model.transform.Find("main/attach");

				animator.Rebind();
				animator.Play("deploy");
			}

			public bool update()
			{
				pillowTransform.localPosition = new Vector3(0f, -0.1875f, 0f);

				bool animPlaying = animator.GetCurrentAnimatorStateInfo(0).fullPathHash == animHash;

				if (!animPlaying)
					animator.enabled = false;

				return animPlaying;
			}
		}
		AnimFixer animFixer;


		const float maxGravityChange	 = 0.1f;

		const float distanceHide		 = 200f;
		const float distancePhysicsOff	 = 40f;
		const float distanceBeaconAttach = 4f;

		const float distanceHideSqr = distanceHide * distanceHide;
		const float distancePhysicsOffSqr = distancePhysicsOff * distancePhysicsOff;
		const float distanceBeaconAttachSqr = distanceBeaconAttach * distanceBeaconAttach;

		const float dtPhysicsChangeMin = 10f;
		const float dtPhysicsChangeMax = 20f;

		float gravitySign = 1.0f;
		float timeNextPhysicsChange = 0f;

		bool lastVisible = true;
		bool lastPhysicsEnabled = false;

		public bool needShowBeaconText { get; private set; }

		int containerSize = 0;

		Rigidbody rigidbody;
		StorageContainer storageContainer => _cachedSC ??= gameObject.GetComponentInChildren<StorageContainer>();
		StorageContainer _cachedSC;

		string id;

		Beacon beaconAttached = null;
		string _beaconID; // used after loading
		static readonly Dictionary<Beacon, FloatingCargoCrateControl> allBeaconsAttached = new Dictionary<Beacon, FloatingCargoCrateControl>();

		void Awake()
		{
			if (LargeWorldStreamer.main?.globalRoot == null) // that's probably prefab that loaded early
			{
				gameObject.SetActive(false);
				return;
			}

			rigidbody = gameObject.GetComponent<Rigidbody>();
			rigidbody.isKinematic = true; // switch physics off by default

			containerSize = storageContainer.width * storageContainer.height;

			id = GetComponent<PrefabIdentifier>().Id;
			OnProtoDeserialize(null);

			if (_beaconID != "")
				Invoke(nameof(reattachBeaconAfterLoad), 0.2f);
		}

		void Start()
		{
			if (!Builder.prefab || CraftData.GetTechType(Builder.prefab) != FloatingCargoCrate.TechType) // don't play animation for ghost model
				animFixer = new AnimFixer(gameObject.getChild("3rd_person_model/floating_storage_cube_tp"));
		}

		void Update()
		{
			if (gameObject.transform.position.y > -4500) // if this is prefab in SMLHelper then do not change visibility
				updateDistanceFromCam();

			if (Main.config.experimentalFeaturesOn && !rigidbody.isKinematic && Time.time > timeNextPhysicsChange)
			{
				timeNextPhysicsChange = Time.time + Random.Range(dtPhysicsChangeMin, dtPhysicsChangeMax);

				updateGravityChange();
				updateMass();
			}
		}

		void LateUpdate()
		{
			if (animFixer != null && !animFixer.update())
				animFixer = null;
		}

		public bool CanDeconstruct(out string reason)
		{
			if (animFixer != null) // don't deconstruct while deploy animation is played
			{
				reason = null;
				return false;
			}

			if (!beaconAttached)
				return storageContainer.CanDeconstruct(out reason);

			reason = L10n.str(L10n.ids_removeBeaconFirst);
			return false;
		}

		public void OnConstructedChanged(bool constructed)
		{
			storageContainer.enabled = constructed;
		}

		void updateMass()
		{
			int itemCount = 0;

			if (storageContainer.container.count > 0)
			{
				for (int i = 0; i < storageContainer.width; i++)
					for (int j = 0; j < storageContainer.height; j++)
						if (storageContainer.container.itemsMap[i, j] != null)
							itemCount++;
			}

			if (itemCount == 0) // if we didn't open storage, itemsMap will be empty
				itemCount = storageContainer.container.count;

			rigidbody.mass = (Main.config.crateMassFull - Main.config.crateMassEmpty) * ((float)itemCount / containerSize) + Main.config.crateMassEmpty;
		}

		void updateGravityChange()
		{
			gravitySign = -gravitySign;
			gameObject.GetComponent<WorldForces>().underwaterGravity = gravitySign * maxGravityChange * Random.value;
		}

		void setRigidBodyPhysicsEnabled(bool val)
		{
			if (val == lastPhysicsEnabled)
				return;

			lastPhysicsEnabled = val;
			rigidbody.isKinematic = !val;
		}

		void setVisible(bool val)
		{
			if (lastVisible == val)
				return;

			lastVisible = val;

			foreach (var r in gameObject.GetComponent<SkyApplier>().renderers)
				r.enabled = val;
		}

		void updateBeaconText(bool distVal)
		{
			needShowBeaconText = distVal && !beaconAttached && GameUtils.getHeldToolType() == TechType.Beacon;
		}

		void updateDistanceFromCam()
		{
			if (LargeWorldStreamer.main == null)
				return;

			float distanceFromCamSqr = (gameObject.transform.position - LargeWorldStreamer.main.cachedCameraPosition).sqrMagnitude;

			setRigidBodyPhysicsEnabled(distanceFromCamSqr < distancePhysicsOffSqr);
			setVisible(distanceFromCamSqr < distanceHideSqr || gameObject.transform.position.y > -3);
			updateBeaconText(distanceFromCamSqr < distanceBeaconAttachSqr);
		}

		public bool setBeaconAttached(Beacon beacon, bool attaching)
		{
			if (!beacon || (attaching && beaconAttached))
				return false;

			GameObject beaconObject = beacon.gameObject;
			if (attaching)
			{
				beaconObject.transform.parent = gameObject.transform;
				beaconObject.transform.localPosition = new Vector3(-0.36f, 1.66f, -1.3f);
				beaconObject.transform.localEulerAngles = new Vector3(0f, 180f, 15f);
			}

			beaconObject.GetComponent<WorldForces>().enabled = !attaching;
			beaconObject.GetComponent<Stabilizer>().enabled = !attaching;
			beaconObject.GetComponentInChildren<Animator>().enabled = !attaching;

			beaconObject.GetComponent<Rigidbody>().isKinematic = attaching;

			GameObject beaconCollider = beaconObject.getChild("buildcheck");
			beaconCollider.GetComponent<BoxCollider>().center = new Vector3(0f, 0f, attaching? 0.1f: 0.0f);
			beaconCollider.GetComponent<BoxCollider>().size = attaching? new Vector3(0.5f, 0.8f, 0.15f): new Vector3(0.2f, 0.5f, 0.15f);
			beaconCollider.layer = attaching? LayerID.Player: LayerID.Default; // little hack for excluding beacon from propulsion cannon targets, don't find a better way

			GameObject labelCollider = beaconObject.getChild("label");
			labelCollider.GetComponent<BoxCollider>().center = new Vector3(0f, 0.065f, attaching? 0.18f: 0.08f);
			labelCollider.layer = attaching? LayerID.Player: LayerID.Default; // same hack ^^^

			if (attaching)
				allBeaconsAttached.Add(beacon, this);
			else
				allBeaconsAttached.Remove(beacon);

			beaconAttached = attaching? beacon: null;

			return true;
		}

		public bool tryAttachBeacon(Beacon beacon)
		{
			if (GetComponent<Constructable>()?.constructed == false)
				return false;

			if (beacon && !beaconAttached && (gameObject.transform.position - beacon.gameObject.transform.position).sqrMagnitude < distanceBeaconAttachSqr)
				return setBeaconAttached(beacon, true);

			return false;
		}

		public static void tryDetachBeacon(Beacon beacon)
		{
			if (allBeaconsAttached.TryGetValue(beacon, out FloatingCargoCrateControl c))
				c.setBeaconAttached(beacon, false);
		}

		void reattachBeaconAfterLoad()
		{
			if (UniqueIdentifier.TryGetIdentifier(_beaconID, out UniqueIdentifier beacon))
				setBeaconAttached(beacon.GetComponent<Beacon>(), true);
		}

		public void OnProtoDeserialize(ProtobufSerializer serializer) =>
			_beaconID = SaveLoad.load<SaveData>(id)?.beaconID ?? "";

		public void OnProtoSerialize(ProtobufSerializer serializer) =>
			SaveLoad.save(id, new SaveData { beaconID = beaconAttached?.GetComponent<UniqueIdentifier>().Id ?? "" });
	}
}