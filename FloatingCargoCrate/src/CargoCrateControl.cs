using System.Collections.Generic;
using UnityEngine;

using Common;
using Common.GameSerialization;

namespace FloatingCargoCrate
{
	class FloatingCargoCrateControl: MonoBehaviour, IConstructable, IProtoEventListener
	{
		class SaveData { public string beaconID; }

		const float maxGravityChange	 = 0.1f;
		const float distanceBeaconAttach = 4f;
		const float distancePhysicsOff	 = 40f;
		const float distanceHide		 = 200f;
		
		const float distanceBeaconAttachSqr = distanceBeaconAttach * distanceBeaconAttach;
		const float distanceHideSqr = distanceHide * distanceHide;
		const float distancePhysicsOffSqr = distancePhysicsOff * distancePhysicsOff;

		const float dtPhysicsChangeMin = 10f;
		const float dtPhysicsChangeMax = 20f;
		
		float timeNextPhysicsChange = 0f;
		float gravitySign = 1.0f;

		bool lastPhysicsEnabled = false;
		bool lastVisible = true;
		
		public bool needShowBeaconText { get; private set; }

		int containerSize = 0;

		StorageContainer storageContainer;
		Rigidbody rigidbody;

		string id;

		Beacon beaconAttached = null;
		string _beaconID; // used after loading
		static readonly Dictionary<Beacon, FloatingCargoCrateControl> allBeaconsAttached = new Dictionary<Beacon, FloatingCargoCrateControl>();
		
		void Awake()
		{
			rigidbody = gameObject.GetComponent<Rigidbody>();
			rigidbody.isKinematic = true; // switch physics off by default

			storageContainer = gameObject.GetComponentInChildren<StorageContainer>();
			containerSize = storageContainer.width * storageContainer.height;

			id = GetComponent<PrefabIdentifier>().Id;
			OnProtoDeserialize(null);

			if (_beaconID != "")
				Invoke(nameof(reattachBeaconAfterLoad), 0.2f);
		}

		void Start()
		{
			if ((!Builder.prefab || CraftData.GetTechType(Builder.prefab) != FloatingCargoCrate.TechType))
				Invoke(nameof(playDeployAnimation), 0.5f);
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

		public bool CanDeconstruct(out string reason)
		{
			if (beaconAttached)
			{
				reason = "You need to remove beacon first.";
				return false;
			}

			return storageContainer.CanDeconstruct(out reason);
		}

		public void OnConstructedChanged(bool constructed) {}

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
			if (val != lastPhysicsEnabled)
			{
				lastPhysicsEnabled = val;
				rigidbody.isKinematic = !val;
			}
		}
		
		void setVisible(bool val)
		{
			if (lastVisible != val)
			{
				lastVisible = val;

				foreach (var r in gameObject.GetComponent<SkyApplier>().renderers)
					r.enabled = val;
			}
		}

		void updateBeaconText(bool distVal)
		{
			needShowBeaconText = distVal && !beaconAttached && (Inventory.main.GetHeldTool() && Inventory.main.GetHeldTool().pickupable.GetTechType() == TechType.Beacon);
		}

		void updateDistanceFromCam()
		{
			LargeWorldStreamer lwsMain = LargeWorldStreamer.main;
			if (lwsMain == null)
				return;

			float distanceFromCamSqr = (gameObject.transform.position - lwsMain.cachedCameraPosition).sqrMagnitude;

			setRigidBodyPhysicsEnabled(distanceFromCamSqr < distancePhysicsOffSqr);
			setVisible(distanceFromCamSqr < distanceHideSqr || gameObject.transform.position.y > -3);
			updateBeaconText(distanceFromCamSqr < distanceBeaconAttachSqr);
		}

		void playDeployAnimation() // TODO: is all this lines necessary ?
		{
			GameObject model = gameObject.getChild("3rd_person_model");
			model.SetActive(true);

			Animator animator = model.GetComponentInChildren<Animator>();
			animator.enabled = true;
			animator.StartPlayback();
			animator.Rebind();
			animator.Play("deploy");
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