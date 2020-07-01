using System.Collections;

using UnityEngine;

using Common;
using Common.Crafting;

namespace HabitatPlatform
{
	class PlatformInitializer: MonoBehaviour
	{
		Base platformBase = null;

		IEnumerator Start()
		{
			if (gameObject.GetComponentInChildren<VFXConstructing>() is var vfx)
			{
				yield return null; // wait for one frame while VFXConstructing is initialized

				if (vfx.constructed < 1)
					_addAllFoundations();
			}

			Destroy(this);
		}

		void _addAllFoundations()
		{
			_addFirstFoundation();

			for (int x = 0; x < 4; x++)
			{
				for (int z = 0; z < 3; z++)
				{
					if ((x == 0 && z == 0) || (x == 3 && z == 1))
						continue;

					_addNextFoundation(-10f * x, -10f * z);
				}
			}
		}

		void _addFirstFoundation()
		{
			GameObject newFoundation = CraftHelper.Utils.prefabCopy(TechType.BaseFoundation);

			var csBase = newFoundation.GetComponent<ConstructableBase>();
			var baseGhost = csBase.model.GetComponent<BaseGhost>();

			baseGhost.ghostBase.SetSize(Base.CellSize[2]);
			baseGhost.ghostBase.SetCell(Int3.zero, Base.CellType.Foundation);

			if (baseGhost.targetBase != null)
			{
				csBase.transform.parent = baseGhost.targetBase.transform;
			}

			// !!!!!!!!!
			//if (baseGhost.TargetBase != null)
			//	constructableBase.transform.SetParent(baseGhost.TargetBase.transform, true);

			if (baseGhost.targetBase == null)
			{
				var baseObject = CraftHelper.Utils.prefabCopy("WorldEntities/Structures/Base", csBase.transform.position, csBase.transform.rotation);
				LargeWorld.main?.streamer.cellManager.RegisterEntity(baseObject);
				baseGhost.targetBase = baseObject.GetComponent<Base>();

				platformBase = baseGhost.targetBase; //!!!!!!!!!!!!
			}

			baseGhost.targetOffset = baseGhost.targetBase.WorldToGrid(baseGhost.transform.position);
			baseGhost.targetBase.CopyFrom(baseGhost.ghostBase, baseGhost.ghostBase.Bounds, baseGhost.targetOffset);

			// parenting to platform
			baseGhost.targetBase.transform.parent = gameObject.transform;

			//baseGo.transform.localPosition = new Vector3(11.7f, 1.4f, 7.5f);
			//baseGhost.targetBase.transform.localPosition = new Vector3(11.7f, 1.4f, 7.5f);
			baseGhost.targetBase.transform.localPosition = new Vector3(11.7f, 1.4f + Random.value * 3, 7.5f);
			baseGhost.targetBase.transform.localEulerAngles = Vector3.zero;

			Destroy(newFoundation);
		}

		void _addNextFoundation(float deltaX, float deltaZ)
		{
			if (platformBase == null)
			{
				"NULL BASE".onScreen();
				return;
			}

			GameObject newFoundation = CraftHelper.Utils.prefabCopy(TechType.BaseFoundation);
			var constructableBase = newFoundation.GetComponent<ConstructableBase>();

			BaseGhost baseGhost = constructableBase.model.GetComponent<BaseGhost>();

			baseGhost.ghostBase.SetSize(Base.CellSize[2]);
			baseGhost.ghostBase.SetCell(Int3.zero, Base.CellType.Foundation);

			baseGhost.targetBase = platformBase;

			constructableBase.transform.parent = baseGhost.targetBase.transform;
			constructableBase.transform.localPosition = new Vector3(deltaX, 0, deltaZ);

			// !!!!!!!!!
			//if (baseGhost.TargetBase != null)
			//	constructableBase.transform.SetParent(baseGhost.TargetBase.transform, true);

			baseGhost.targetOffset = baseGhost.targetBase.WorldToGrid(baseGhost.transform.position);
			baseGhost.targetBase.CopyFrom(baseGhost.ghostBase, baseGhost.ghostBase.Bounds, baseGhost.targetOffset);

			//// parenting to platform
			//baseGhost.targetBase.transform.parent = gameObject.transform;

			////baseGo.transform.localPosition = new Vector3(11.7f, 1.4f, 7.5f);
			//baseGhost.targetBase.transform.localPosition = new Vector3(11.7f, 1.7f, 7.5f);
			//baseGhost.targetBase.transform.localEulerAngles = Vector3.zero;

			Destroy(newFoundation);
		}
	}
}