using System.Text;

using Harmony;

using Common;

namespace MiscPrototypes
{
	[HarmonyPatch(typeof(SubRoot), "Update")]
	static class SubRoot_Update_Patch
	{
		static void Prefix(SubRoot __instance)
		{
			"update".onScreen("sub root update");

			if (__instance is BaseRoot baseRoot)
				baseRoot.ConsumePower();
		}
	}


	[HarmonyPatch(typeof(BaseRoot), "ConsumePower")]
	static class BaseRoot_ConsumePower_Patch
	{
		static bool Prefix(BaseRoot __instance)
		{
			if (!__instance.powerRelay)
				return false;

			"----------------- consuming begin".log();

			StringBuilder sb = new StringBuilder();
			//return Base.CellPowerConsumption[(int)this.GetCell(cell)];

			float num = 0f;
			Int3.RangeEnumerator allCells = __instance.baseComp.AllCells;
			while (allCells.MoveNext())
			{
				Int3 cell = allCells.Current;
				//$"cell: {cell}".log();
				if (__instance.baseComp.GetCellMask(cell))
				{
					float cons = __instance.baseComp.GetCellPowerConsumption(cell);
					num += cons;
					if (cons > 0f)
						sb.AppendLine($"cell: {cell} ({__instance.baseComp.GetCell(cell)}) power: {cons}");
					//$"num added {num}".log();
				}
			}
			num *= 1f;
			DayNightCycle main = DayNightCycle.main;
			if (main)
			{
				num *= main.deltaTime;
			}
			float num2;
			__instance.powerRelay.ConsumeEnergy(num, out num2);

			$"consuming {num}".onScreen("base root power");
			sb.ToString().onScreen("cells power");

			$"consuming {num} {UnityEngine.Time.frameCount}".log();

			return false;
		}
	}

	[HarmonyPatch(typeof(BaseRoot), "Start")]
	static class BaseRoot_Start_Patch
	{
		static void Prefix(BaseRoot __instance)
		{
			"---------------- START!!!".log();
		}
	}
}