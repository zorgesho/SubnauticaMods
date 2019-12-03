using System;
using SMLHelper.V2.Assets;
using UnityEngine;

namespace OxygenRefill
{
	// Token: 0x02000006 RID: 6
	internal class TankRefillPreFab : ModPrefab
	{
		// Token: 0x06000022 RID: 34 RVA: 0x00002575 File Offset: 0x00000775
		internal TankRefillPreFab(string classId, TechType techType) : base(classId, string.Format("{0}PreFab", classId), techType)
		{
		}

		// Token: 0x06000023 RID: 35 RVA: 0x0000258A File Offset: 0x0000078A
		public override GameObject GetGameObject()
		{
			return GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("WorldEntities/Tools/Tank"));
		}
	}
}
