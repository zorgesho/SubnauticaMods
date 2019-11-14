using System.Diagnostics;
using UnityEngine;

using Common;

namespace DayNightSpeed
{
	static class DayNightSpeedWatcher
	{
		static GameObject gameObject = null;
		
		class DayNightSpeedWatch: MonoBehaviour
		{
			void Update()
			{
				if (DayNightCycle.main == null)
					return;

				$"{DayNightCycle.main.dayNightSpeed}".onScreen("dayNightSpeed");
				$"{DayNightCycle.main.timePassed}".onScreen("time passed");
				$"{DayNightCycle.ToGameDateTime(DayNightCycle.main.timePassedAsFloat)}".onScreen("date/time");
			}
		}
		
		[Conditional("DEBUG")]
		public static void init()
		{
			if (gameObject == null)
				gameObject = UnityHelper.createPersistentGameObject<DayNightSpeedWatch>("DayNightSpeedWatcher");
		}
	}
}