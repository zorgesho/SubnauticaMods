using System;
using UnityEngine;

#pragma warning disable

namespace Fatigue
{
	public class EnergySurvival: MonoBehaviour
	{
		public float energy = 100;

		const float energySeconds = 200f;
		const float updateInterval = 5f;

		void Start()
		{
			InvokeRepeating("updateFatigue", 0f, updateInterval);
		}
		
		void updateFatigue()
		{
			if (GameModeUtils.RequiresSurvival())
			{
				updateEnergy(updateInterval);
				//if (this.liveMixin && num > 1.401298E-45f)
				//{
				//	this.liveMixin.TakeDamage(num, this.player.transform.position, DamageType.Starve, null);
				//}
				//if (this.food + this.water >= 150f && this.liveMixin)
				//{
				//	float num2 = 0.0416666679f;
				//	this.liveMixin.AddHealth(num2 * this.kUpdateHungerInterval);
				//}
			}
		}

		float updateEnergy(float dt)
		{
			if (dt > float.Epsilon)
			{
				float prevEnergy = energy;
				float deltaEnergy = dt / energySeconds * 100f;

				energy = Math.Max(0f, energy - deltaEnergy);
			}

			return energy;
		}
	}
}