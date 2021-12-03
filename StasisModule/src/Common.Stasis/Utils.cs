using System.Collections;

using FMOD.Studio;
using UnityEngine;

namespace Common.Stasis
{
	static class Utils
	{
		public static EventDescription getDescription(this EventInstance eventInstance)
		{
			eventInstance.getDescription(out EventDescription desc);
			return desc;
		}

		static int getInstanceCount(this EventDescription desc)
		{
			desc.getInstanceCount(out int count);
			return count;
		}

		public static IEnumerator releaseAllEventInstances(string eventPath, int waitFramesMax = 10)
		{																									$"Utils.releaseAllEventInstances: {eventPath}".logDbg();
			FMODUWE.GetEventInstance(eventPath, out EventInstance eventInstance);
			var desc = eventInstance.getDescription();
			eventInstance.release();

			int count = desc.getInstanceCount();															$"Utils.releaseAllEventInstances: instances count = {count}".logDbg();

			if (count == 0)
				yield break;

			desc.releaseAllInstances();
			yield return new WaitWhile(() => desc.getInstanceCount() > 0 && waitFramesMax-- > 0);
		}
	}
}