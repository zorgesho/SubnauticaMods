using UnityEngine;

namespace PrawnSuitSonarUpgrade
{
	class PrawnSonarControl: MonoBehaviour
	{
		const float pingInterval = 5f;	// default is 5f
		const float energyCost = 1f;	// default is 1f

		Exosuit exosuit;

		int activeSlotID = -1;
		bool isActive = false;
		bool isPlayerInside = false;

		FMODAsset sonarSoundAsset;
		FMOD_CustomEmitter sonarSound;

		void Awake()
		{
			exosuit = gameObject.GetComponent<Exosuit>();

			sonarSoundAsset = ScriptableObject.CreateInstance<FMODAsset>();
			sonarSoundAsset.path = "event:/sub/seamoth/sonar_loop";
			sonarSound = gameObject.AddComponent<FMOD_CustomEmitter>();
			sonarSound.asset = sonarSoundAsset;
		}


		void Start()
		{
			setPlayerInside(Player.main.GetVehicle() == exosuit);
		}


		void Update()
		{
			if (!isActive || exosuit.GetQuickSlotCooldown(activeSlotID) < 1f)
				return;

			if (!exosuit.HasEnoughEnergy(energyCost))
			{
				setActive(false);
				return;
			}

			// ping sonar
			sonarSound.Stop();
			sonarSound.Play();
			SNCameraRoot.main.SonarPing();

			exosuit.ConsumeEnergy(energyCost);

			// update quick slot
			exosuit.quickSlotTimeUsed[activeSlotID] = Time.time;
			exosuit.quickSlotCooldown[activeSlotID] = pingInterval;
		}


		public void setPlayerInside(bool value)
		{
			isPlayerInside = value;

			if (!isPlayerInside)
				setActive(false);
		}


		public void onToggle(int slotID, bool state)
		{
			if (exosuit.GetSlotBinding(slotID) == PrawnSonarModule.TechType)
			{
				setActive(state);
				activeSlotID = state? slotID: -1;
			}
		}


		void setActive(bool active)
		{
			isActive = active && isPlayerInside;
		}
	}
}