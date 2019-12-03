using UnityEngine;

namespace OxygenRefill
{
	class OxygenAdvanced: MonoBehaviour, ICraftTarget
	{
		// Token: 0x0600001E RID: 30 RVA: 0x00002467 File Offset: 0x00000667
		void Awake()
		{
			Init();
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00002470 File Offset: 0x00000670
		void Init()
		{
			this.oxygen = base.GetComponent<Oxygen>();
			if (this.oxygen)
			{
				TechType techType = CraftData.GetTechType(base.gameObject);
				float multiplier = 1;								/////////////////////////Controller.Settings.GetMultiplier();
				if (techType <= TechType.DoubleTank)
				{
					if (techType != TechType.Tank)
					{
						if (techType == TechType.DoubleTank)
						{
							this.oxygen.oxygenCapacity = 400f * multiplier;
						}
					}
					else
					{
						this.oxygen.oxygenCapacity = 200f * multiplier;
					}
				}
				else if (techType != TechType.PlasteelTank)
				{
					if (techType == TechType.HighCapacityTank)
					{
						this.oxygen.oxygenCapacity = 800f * multiplier;
					}
				}
				else
				{
					this.oxygen.oxygenCapacity = 400f * multiplier;
				}
				this.oxygen.oxygenAvailable = Mathf.Min(this.oxygen.oxygenAvailable, this.oxygen.oxygenCapacity);
			}
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00002550 File Offset: 0x00000750
		public void OnCraftEnd(TechType techType)
		{
			ErrorMessage.AddDebug("craft end " + techType);
		
			if (this.oxygen)
			{
				if (techType == TechType.Tank || techType == TechType.DoubleTank)
					this.oxygen.oxygenAvailable = 0f;
				else
					this.oxygen.oxygenAvailable = this.oxygen.oxygenCapacity;
			}
		}

		// Token: 0x04000008 RID: 8
		Oxygen oxygen;
	}
}
