using UnityEngine;
using UWE;

using Common;

namespace RemoteTorpedoDetonator
{
	class TorpedoDetonatorControl: MonoBehaviour
	{
		Vehicle vehicle = null;

		FMODAsset soundAsset;
		FMOD_CustomEmitter sound;

		void Awake()
		{
			vehicle = gameObject.GetComponent<Vehicle>();

			soundAsset = ScriptableObject.CreateInstance<FMODAsset>();
			soundAsset.path = "event:/tools/flashlight/turn_on";
			sound = gameObject.AddComponent<FMOD_CustomEmitter>();
			sound.asset = soundAsset;

			Player.main.playerModeChanged.AddHandler(gameObject, new Event<Player.Mode>.HandleFunction(onPlayerModeChanged));
		}

		void OnDestroy() => Player.main.playerModeChanged.RemoveHandler(gameObject, onPlayerModeChanged);

		public void checkEnabled() =>
			enabled = vehicle && Player.main.getVehicle() == vehicle && vehicle.modules.GetCount(TorpedoDetonatorModule.TechType) > 0;

		void onPlayerModeChanged(Player.Mode playerMode)
		{
			if (playerMode == Player.Mode.LockedPiloting)
			{
				checkEnabled();

				if (enabled && Main.config.showHotkeyMessage)
					LanguageCache.GetButtonFormat(L10n.str("ids_enterVehicleMessage"), Main.config.hotkey).onScreen();
			}
			else if (playerMode == Player.Mode.Normal)
				enabled = false;
		}

		void Update()
		{																								"TorpedoDetonatorControl.Update: player not in vehicle".logDbgError(Player.main.GetVehicle() != vehicle);
			if (GameInput.GetButtonDown(Main.config.hotkey))
				detonateTorpedoes();
		}

		public void detonateTorpedoes()
		{
			sound.Stop();
			sound.Play();

			FindObjectsOfType<SeamothTorpedo>().ForEach(torpedo => torpedo.Explode());
		}
	}
}