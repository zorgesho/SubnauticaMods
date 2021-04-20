using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace Common.UnityDebug
{
	class DrawColliders: MonoBehaviour
	{
		interface ISetCollider { void setCollider(Collider collider); }

		abstract class DrawCollider<CL, WR>: MonoBehaviour, ISetCollider where CL: Collider where WR: DrawWire
		{
			protected CL collider;
			Bounds lastBounds;

			public void setCollider(Collider collider)
			{
				this.collider = collider as CL;
				Debug.assert(this.collider != null);

				lastBounds = collider.bounds;
			}

			protected WR drawWire;
			protected abstract void updateProps();

			void Awake() => drawWire = gameObject.AddComponent<WR>();
			void Start() => updateProps();
			void OnDestroy() => Destroy(drawWire);

			void Update()
			{
				if (lastBounds == collider.bounds)
					return;

				lastBounds = collider.bounds;
				updateProps();
			}
		}

		class DrawColliderBox: DrawCollider<BoxCollider, DrawBox>
		{
			protected override void updateProps() => drawWire.setProps(collider.center, collider.size);
		}

		class DrawColliderSphere: DrawCollider<SphereCollider, DrawSphere>
		{
			protected override void updateProps() => drawWire.setProps(collider.center, collider.radius);
		}

		class DrawColliderCapsule: DrawCollider<CapsuleCollider, DrawCapsule>
		{
			protected override void updateProps() => drawWire.setProps(collider.center, collider.radius, collider.height, collider.direction);
		}


		readonly List<Component> wires = new();

		static readonly (Type colliderType, Type componentType)[] types =
		{
			(typeof(BoxCollider), typeof(DrawColliderBox)),
			(typeof(SphereCollider), typeof(DrawColliderSphere)),
			(typeof(CapsuleCollider), typeof(DrawColliderCapsule))
		};

		void Start()
		{
			foreach (var collider in gameObject.GetAllComponentsInChildren<Collider>())
			{
				if (types.FirstOrDefault(pair => collider.GetType() == pair.colliderType).componentType is not Type componentType)
					continue;

				var wire = collider.gameObject.AddComponent(componentType);
				wires.Add(wire);

				(wire as ISetCollider)?.setCollider(collider);
			}
		}

		void OnDestroy() => wires.ForEach(Destroy);
	}
}