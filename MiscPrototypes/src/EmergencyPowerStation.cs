using System;

using Harmony;
using UnityEngine;
using SMLHelper.V2.Crafting;

using Common;
using Common.Crafting;

namespace MiscPrototypes
{
	[HarmonyPatch(typeof(Charger), "Update")]
	static class Charger_Update_Patch
	{
		static bool Prefix(Charger __instance)
		{
			if (!__instance.GetComponent<EmergencyPowerStation.Tag>())
				return true;

			if (PowerSource.FindRelay(__instance.transform) is PowerRelay powerRelay0)
			{
				$"{powerRelay0.GetPower()} {powerRelay0.GetMaxPower()}".onScreen("power relay");

				if (powerRelay0.GetPower() > Main.config.maxPowerOnBatteries + 10)
					return true;

				__instance.sequence.Update();

				if (Time.deltaTime == 0f)
					return false;

				if (powerRelay0.GetPower() > Main.config.maxPowerOnBatteries)
					return false;

				bool active = false;

				foreach (var slot in __instance.batteries)
				{
					var battery = slot.Value;
					if (battery == null)
						continue;

					if (battery.charge > 0f)
					{
						float getPower = Math.Min(battery.charge, DayNightCycle.main.deltaTime * __instance.chargeSpeed * battery.capacity);

						$"{getPower} {DayNightCycle.main.deltaTime} {__instance.chargeSpeed}".log();

						battery.charge -= getPower;
						powerRelay0.AddEnergy(getPower, out float num4);

						active = true;
					}

					if (__instance.slots.TryGetValue(slot.Key, out Charger.SlotDefinition definition))
						__instance.UpdateVisuals(definition, battery.charge / battery.capacity);
				}

				__instance.ToggleUIPowered(active);
				//__instance.ToggleChargeSound(charging);

				if (__instance.player != null && (__instance.player.transform.position - __instance.transform.position).sqrMagnitude >= 16f)
				{
					__instance.player = null;
					if (!__instance.HasChargables())
					{
						__instance.opened = false;
						__instance.OnClose();
					}
				}
			}
			return false;
#if VANILLA_METHOD
			bool charging = false;
			if (true)
			{
				int num = 0;
				bool flag = false;
				PowerRelay powerRelay = PowerSource.FindRelay(__instance.transform);
				if (powerRelay != null)
				{
					float num2 = 0f;
					foreach (KeyValuePair<string, IBattery> keyValuePair in __instance.batteries)
					{
						IBattery value = keyValuePair.Value;
						if (value != null)
						{
							float charge = value.charge;
							float capacity = value.capacity;
							if (charge < capacity)
							{
								num++;
								float num3 = DayNightCycle.main.deltaTime * __instance.chargeSpeed * capacity;
								if (charge + num3 > capacity)
								{
									num3 = capacity - charge;
								}
								num2 += num3;
							}
						}
					}
					float num4 = 0f;
					if (num2 > 0f && powerRelay.GetPower() > num2)
					{
						flag = true;
						powerRelay.ConsumeEnergy(num2, out num4);
						$"{num2}".log();
					}
					if (num4 > 0f)
					{
						charging = true;
						float num5 = num4 / (float)num;
						foreach (KeyValuePair<string, IBattery> keyValuePair2 in __instance.batteries)
						{
							string key = keyValuePair2.Key;
							IBattery value2 = keyValuePair2.Value;
							if (value2 != null)
							{
								float charge2 = value2.charge;
								float capacity2 = value2.capacity;
								if (charge2 < capacity2)
								{
									float num6 = num5;
									float num7 = capacity2 - charge2;
									if (num6 > num7)
									{
										num6 = num7;
									}
									value2.charge += num6;
									Charger.SlotDefinition definition;
									if (__instance.slots.TryGetValue(key, out definition))
									{
										__instance.UpdateVisuals(definition, value2.charge / value2.capacity);
									}
								}
							}
						}
					}
				}
				if (num == 0 || !flag)
				{
					__instance.nextChargeAttemptTimer = 5f;
				}
				__instance.ToggleUIPowered(num == 0 || flag);
			}
			if (__instance.nextChargeAttemptTimer >= 0f)
			{
				int num8 = Mathf.CeilToInt(__instance.nextChargeAttemptTimer);
				string text = null;
				if (!__instance.unpoweredNotifyStrings.TryGetValue(num8, out text))
				{
					text = Language.main.GetFormat<int>("ChargerInsufficientPower", num8);
					__instance.unpoweredNotifyStrings.Add(num8, text);
				}
				__instance.uiUnpoweredText.text = text;
			}
			__instance.ToggleChargeSound(charging);
			if (__instance.player != null && (__instance.player.transform.position - __instance.transform.position).sqrMagnitude >= 16f)
			{
				__instance.player = null;
				if (!__instance.HasChargables())
				{
					__instance.opened = false;
					__instance.OnClose();
				}
			}

			return false;
#endif
		}
	}

	class EmergencyPowerStation: CraftableObject
	{
		public class Tag: MonoBehaviour {}

		protected override TechData getTechData() => new TechData(new Ingredient(TechType.Titanium, 2));

		public override void patch()
		{
			register();

			addToGroup(TechGroup.InteriorModules, TechCategory.InteriorModule, TechType.PowerCellCharger);
			unlockOnStart();
		}

		public override GameObject getGameObject()
		{
			var prefab = CraftHelper.Utils.prefabCopy(TechType.PowerCellCharger);
			prefab.AddComponent<Tag>();

			var powerSource = prefab.AddComponent<PowerSource>();
			powerSource.maxPower = 200f;
			powerSource.power = 0f;

			prefab.GetComponent<Constructable>().techType = TechType;
			prefab.GetComponentsInChildren<SkinnedMeshRenderer>().forEach(rend => rend.material.color = Color.red);

			return prefab;
		}
	}
}