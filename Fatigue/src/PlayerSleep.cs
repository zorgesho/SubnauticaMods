using UnityEngine;
using UnityEngine.Events;

using UWE;

namespace Fatigue
{
	class PlayerSleep: MonoBehaviour
	{
		public bool isSleeping
		{
			get => _isSleeping;

			private set
			{
				if (_isSleeping != value)
					sleepChangeEvent.Invoke(value);

				_isSleeping = value;

				if (_isSleeping)
				{
					player.playerDeathEvent.AddHandler(gameObject, new Event<Player>.HandleFunction(this.onPlayerDeath));
					player.isUnderwater.changedEvent.AddHandler(gameObject, new Event<global::Utils.MonitoredValue<bool>>.HandleFunction(this.onUnderwater));
				}
				else
				{
					player.playerDeathEvent.RemoveHandler(gameObject, new Event<Player>.HandleFunction(this.onPlayerDeath));
					player.isUnderwater.changedEvent.RemoveHandler(gameObject, new Event<global::Utils.MonitoredValue<bool>>.HandleFunction(this.onUnderwater));
				}
			}
		}
		bool _isSleeping = false;

		public class SleepChangeEvent: UnityEvent<bool> {}
		public SleepChangeEvent sleepChangeEvent { get; private set; } = new SleepChangeEvent();

		Player player = null;

		const float secondsToSleepHour = 1;

		void Awake()
		{
			player = gameObject.GetComponent<Player>();
		}

		void Update()
		{
			if (isSleeping && !DayNightCycle.main.IsInSkipTimeMode())
				stopSleep();
		}

		public void startSleep(float hours)
		{
			DayNightCycle.main.SkipTime(hours * 3600f / DayNightCycle.gameSecondMultiplier, hours * secondsToSleepHour);

			isSleeping = true;
		}

		public void stopSleep()
		{
			if (DayNightCycle.main.IsInSkipTimeMode())
				DayNightCycle.main.StopSkipTimeMode();

			player.timeLastSleep = DayNightCycle.main.timePassedAsFloat;

			isSleeping = false;
		}

		void onPlayerDeath(Player _player)
		{
			if (player == _player)
				stopSleep();
		}

		void onUnderwater(global::Utils.MonitoredValue<bool> isUnderwater)
		{
			if (player && player.isUnderwater.value)
				stopSleep();
		}
	}
}


//sleepin

//				DayNightCycle.main.SkipTime(__instance.kSleepGameTimeDuration, __instance.kSleepRealTimeDuration);
//			//uGUI_PlayerSleep.main.StartSleepScreen();
//			SleepGUI.main.StartSleepScreen();

////			trigger.addListener(EventTriggerType.PointerEnter, (eventData) => { text.color = new Color(1, 0, 0, 1); });

//			if (sleepGUIEventAction != null)
//				ErrorMessage.AddDebug("---- SLEEPGUI NOTNULL!!!!!!!");

//			sleepGUIEventAction = new UnityAction<SleepGUI.State>((state) => { ErrorMessage.AddDebug("---- SLEEPGUI state " + state + " " + __instance.name); });
//			SleepGUI.main.stateChangeEvent.AddListener(sleepGUIEventAction);

//			return false;


	//sleepout

	//		if (__instance.inUseMode == Bed.InUseMode.Sleeping)
	//		{
	//			if (DayNightCycle.main.IsInSkipTimeMode())
	//			{
	//				DayNightCycle.main.StopSkipTimeMode();
	//			}
	//			player.timeLastSleep = DayNightCycle.main.timePassedAsFloat;
	//			//uGUI_PlayerSleep.main.StopSleepScreen();
	//			SleepGUI.main.StopSleepScreen();
	//		}
	//		if (player == __instance.currentPlayer)
	//		{
	//			if (!skipCinematics)
	//			{
	//				__instance.ResetAnimParams(player.playerAnimator);
	//				__instance.currentStandUpCinematicController.StartCinematicMode(player);
	//			}
	//			if (GameOptions.GetVrAnimationMode() || skipCinematics)
	//			{
	//				__instance.ForcedSetAnimParams(false, true, player.playerAnimator);
	//			}
	//		}
	//		MainCameraControl.main.lookAroundMode = false;
	//		__instance.inUseMode = Bed.InUseMode.None;

	//		SleepGUI.main.stateChangeEvent.RemoveListener(Bed_EnterInUseMode_Patch.sleepGUIEventAction);
	//		Bed_EnterInUseMode_Patch.sleepGUIEventAction = null;

	//		return false;
