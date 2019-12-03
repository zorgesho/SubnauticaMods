using Harmony;
using UnityEngine;

namespace MiscPatches
{
	// Stop dead creatures twitching animations (stop any animations, to be clear)
	class CreatureDeathWatcher: MonoBehaviour
	{
		const float timeToStopAnimator = 5f;
		
		void Start() => Invoke(nameof(stopAnimations), timeToStopAnimator);
		
		void stopAnimations()
		{
			Animator animator = gameObject.GetComponentInChildren<Animator>();
			
			if (animator != null)
				animator.enabled = false;

			Destroy(this);
		}
	}


	[HarmonyPatch(typeof(CreatureDeath), "OnKill")]
	static class CreatureDeath_OnKill_Patch
	{
		static void Postfix(CreatureDeath __instance) => __instance.gameObject.AddComponent<CreatureDeathWatcher>();
	}
}