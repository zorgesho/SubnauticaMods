//#define DBG_CPOINTS

using System;
using System.Collections.Generic;

using UnityEngine;
using Common;

namespace TrfHabitatBuilder
{
	class TrfBuilderControl: MonoBehaviour
	{
		class ConstructionPoint
		{
			float constructionTime = 0f;
			float constructionInterval = 0f;
			public Vector3 constructionPoint { get; private set; } = Vector3.zero;

#if DBG_CPOINTS
			readonly GameObject debugSphere = null;

			public ConstructionPoint()
			{
				debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				debugSphere.transform.localScale = new Vector3(2.0f, 1.0f, 1.0f);
				debugSphere.GetComponent<Renderer>().material.color = Color.red;
				debugSphere.destroyComponent<Collider>(false);
				debugSphere.SetActive(false);
			}

			~ConstructionPoint() => Destroy(debugSphere);
#endif
			public void update(float dt, Constructable constructable)
			{
				constructionTime += dt;
				if (constructionTime >= constructionInterval)
					reset(constructable);
			}

			public void reset(Constructable constructable)
			{
				if (constructable == null)
				{
#if DBG_CPOINTS
					debugSphere.SetActive(false);
#endif
					constructionTime = 0f;
					return;
				}

				constructionInterval = UnityEngine.Random.Range(pointSwitchTimeMin, pointSwitchTimeMax);
				constructionPoint = constructable.GetRandomConstructionPoint();
				constructionTime %= constructionInterval;
#if DBG_CPOINTS
				debugSphere.SetActive(true);
				debugSphere.transform.position = constructionPoint;
#endif
			}
		}

		class ConstructionBeam
		{
			readonly Transform beam, port;
#if DBG_CPOINTS
			GameObject debugSphere = null;

			public void initDebugSphere()
			{
				debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				debugSphere.transform.localScale = new Vector3(1.0f, 2.0f, 1.0f);
				debugSphere.transform.parent = port.parent;
				debugSphere.GetComponent<Renderer>().material.color = Color.green;
				debugSphere.destroyComponent<Collider>(false);
				debugSphere.SetActive(false);
			}

			~ConstructionBeam() => Destroy(debugSphere);
#endif
			public ConstructionBeam(Transform _beam, Transform _port)
			{
				beam = _beam;
				port = _port;
#if DBG_CPOINTS
				initDebugSphere();
#endif
			}

			public ConstructionBeam(GameObject rootToAdd, GameObject beamPrefab, string beamName, Vector3 position)
			{
				port = new GameObject("beamPort" + beamName).transform;
				port.parent = rootToAdd.transform;
				port.localPosition = position;

				beam = Instantiate(beamPrefab).transform;
				beam.gameObject.name = "beam" + beamName;
				beam.parent = port;
				beam.localPosition = Vector3.zero;
				beam.localEulerAngles = Vector3.zero;
#if DBG_CPOINTS
				initDebugSphere();
#endif
			}

			public void setActive(bool state)
			{
				beam.gameObject.SetActive(state);
#if DBG_CPOINTS
				debugSphere.SetActive(state);
#endif
			}

			public void update(float dt, ConstructionPoint cpoint)
			{
				Vector3 targetPoint = port.parent.InverseTransformPoint(cpoint.constructionPoint);
#if DBG_CPOINTS
				debugSphere.SetActive(true);
				debugSphere.transform.localPosition = targetPoint;
#endif
				Vector3 localScale = beam.localScale;
				localScale.z = targetPoint.magnitude;
				beam.localScale = localScale;

				port.localRotation = Quaternion.Slerp(port.localRotation, Quaternion.LookRotation(targetPoint, Vector3.up), portRotationSpeed * dt);
			}
		}

		static class AnimationHelper
		{
			struct Info
			{
				public int hash;
				public readonly float speed;

				public Info(float _speed) { hash = 0; speed = _speed; }
			}

			public enum Anim
			{
				terF_idle,
				terF_panels_up,
				terF_panels_down,
				terF_use_open_panel_start,
				terF_use_open_panel_end,
				terF_use_open_panel_loop,
			};

			static readonly Info[] animInfo = new Info[]
			{
				new Info(1f),	// terF_idle
				new Info(3f),	// terF_panels_up
				new Info(3f),	// terF_panels_down
				new Info(1f),	// terF_use_open_panel_start
				new Info(1f),	// terF_use_open_panel_end
				new Info(Main.config.slowLoopAnim? 0.5f: 1f) // terF_use_open_panel_loop
			};

			static Dictionary<int, float> animSpeeds = null;

			public static int   getAnimHash(Anim anim) => animInfo[(int)anim].hash;
			public static float getAnimSpeed(int hash) => animSpeeds.TryGetValue(hash, out float speed)? speed: 1.0f;

			public static void init()
			{
				if (animSpeeds != null)
					return;

				animSpeeds =  new Dictionary<int, float>();

				foreach (Anim anim in Enum.GetValues(typeof(Anim)))
				{
					int hash = Animator.StringToHash("Base Layer." + anim.ToString());

					animSpeeds[hash] = animInfo[(int)anim].speed;
					animInfo[(int)anim].hash = hash;
				}
			}
		}

		const int beamsCount = 3;
		const int pointsCount = 3;

		const float pointSwitchTimeMin = 0.5f;
		const float pointSwitchTimeMax = 1.5f;
		const float portRotationSpeed  = 10f;

		readonly ConstructionPoint[] cpoints = new ConstructionPoint[pointsCount].init();
		readonly ConstructionBeam[] cbeams = new ConstructionBeam[beamsCount];

		Animator animator;
		BuilderTool builderTool;

		bool inited = false;

		public void init()
		{
			if (inited || !(inited = true))
				return;

			AnimationHelper.init();

			//cbeams[0] = new ConstructionBeam(builderTool.beamLeft, builderTool.nozzleLeft);
			//cbeams[1] = new ConstructionBeam(builderTool.beamRight, builderTool.nozzleRight);

			GameObject beamsRoot = gameObject.getChild("terraformer_anim/Terraformer_Export_Skele/root_jnt/body_jnt/head_jnt");
			GameObject beamPrefab = CraftData.GetPrefabForTechType(TechType.Builder).getChild("builder/builder_FP/Root/r_nozzle_root/r_nozzle/R_laser/beamRight");

			cbeams[0] = new ConstructionBeam(beamsRoot, beamPrefab, "Left", new Vector3(-0.1813f, -0.007f, 0.06f));
			cbeams[1] = new ConstructionBeam(beamsRoot, beamPrefab, "Right", new Vector3(0.1813f, -0.007f, 0.06f));
			cbeams[2] = new ConstructionBeam(beamsRoot, beamPrefab, "Center1", new Vector3(0, -0.007f, 0.06f));
			//cbeams[3] = new ConstructionBeam(beamsRoot, beamPrefab, "Center2", new Vector3(0, -0.007f, 0.06f));

			setBeamsActive(false);
		}

		void Awake()
		{
			builderTool = gameObject.GetComponent<BuilderTool>();
			animator = builderTool.animator;

			init();
		}

		void Start()
		{
			builderTool.UpdateText();
		}

		void setBeamsActive(bool state)
		{
			cbeams.ForEach(cbeam => cbeam.setActive(state));
		}

		void setAnimationEnabled(bool state)
		{
			if (animator && animator.isActiveAndEnabled)
			{
				SafeAnimator.SetBool(animator, "use_loop", state);
				SafeAnimator.SetBool(animator, "terraformer_mode_on", state);
			}
		}

		public void updateBeams()
		{
			int animHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
			animator.speed = AnimationHelper.getAnimSpeed(animHash);								$"anim: {animator.GetCurrentAnimatorClipInfo(0)[0].clip.name}\tspeed: {animator.speed}".onScreen("anim info");

			bool isConstructing = builderTool.constructable != null;
			if (builderTool.isConstructing != isConstructing)
			{
				builderTool.isConstructing = isConstructing;

				foreach (var cpoint in cpoints)
					cpoint.reset(builderTool.constructable);
			}

			setBeamsActive(builderTool.isConstructing);
			setAnimationEnabled(builderTool.isConstructing);

			if (builderTool.isConstructing)
			{
				foreach (var cpoint in cpoints)
					cpoint.update(Time.deltaTime, builderTool.constructable);

				bool isRotating = (animHash == AnimationHelper.getAnimHash(AnimationHelper.Anim.terF_use_open_panel_loop));
				cbeams[0].setActive(isRotating);
				cbeams[1].setActive(isRotating);

				for (int i = 0; i < beamsCount; i++)
					cbeams[i].update(Time.deltaTime, cpoints[i]);
			}

			if (builderTool.isConstructing)
				builderTool.buildSound.Play();
			else
				builderTool.buildSound.Stop();

			builderTool.constructable = null;
		}
	}
}