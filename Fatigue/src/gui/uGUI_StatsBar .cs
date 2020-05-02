using UnityEngine;
using UnityEngine.UI;

using UWE;

#pragma warning disable

namespace Fatigue
{
	class uGUI_StatsBar: MonoBehaviour
	{
		public virtual float getUpdatedValue()
		{
			Survival survival = Player.main?.GetComponent<Survival>();
			
			return (survival != null)? survival.water: 0f;
		}
		
		
		public virtual bool subscribe(bool state)
		{
			Survival survival = Player.main?.GetComponent<Survival>();
			if (survival != null)
			{
				if (state)
					survival.onDrink.AddHandler(gameObject, new Event<float>.HandleFunction(OnDrink));
				else
					survival.onDrink.RemoveHandler(gameObject, new Event<float>.HandleFunction(OnDrink));

				return true;
			}

			return false;
		}

		void OnDrink(float drinkAmount)
		{
			ErrorMessage.AddDebug("DRINK!!!!!");
			float maxScale = 1f + Mathf.Clamp01(drinkAmount / 100f);
			Punch(2.5f, maxScale);
		}

		void Awake()
		{
			punchTween = new CoroutineTween(this)
			{
				mode = CoroutineTween.Mode.Once,
				onStart = new CoroutineTween.OnStart(OnPunchStart),
				onUpdate = new CoroutineTween.OnUpdate(OnPunchUpdate),
				onStop = new CoroutineTween.OnStop(OnPunchStop)
			};

			pulseTween = new CoroutineTween(this)
			{
				mode = CoroutineTween.Mode.Loop,
				duration = 0f,
				onUpdate = new CoroutineTween.OnUpdate(OnPulse)
			};

			pulseAnimation.wrapMode = WrapMode.Loop;
			pulseAnimation.Stop();
			pulseAnimationState = pulseAnimation.GetState(0);

			if (pulseAnimationState != null)
			{
				pulseAnimationState.blendMode = AnimationBlendMode.Blend;
				pulseAnimationState.weight = 1f;
				pulseAnimationState.layer = 0;
				pulseAnimationState.speed = 0f;
			}
		}

		void OnEnable()
		{
			lastFixedUpdateTime = Time.time;
			
			if (pulseAnimationState != null)
				pulseAnimationState.enabled = true;
			
			pulseTween.Start();
		}

		
		void LateUpdate()
		{
			bool flag = showNumbers;
			showNumbers = false;

			if (!subscribed && subscribe(true))
				subscribed = true;

			float val = getUpdatedValue();

			float capacity = 100f;
			SetValue(val, capacity);
			
			float num = Mathf.Clamp01(val / pulseReferenceCapacity);
			float time = 1f - num;

			pulseDelay = System.Math.Max(0f, pulseDelayCurve.Evaluate(time));
			pulseTime = System.Math.Max(0f, pulseTimeCurve.Evaluate(time));
			
			float num2 = pulseDelay + pulseTime;
			if (pulseTween.duration > 0f && num2 <= 0f)
				pulseAnimationState.normalizedTime = 0f;
			
			pulseTween.duration = num2;
				
			PDA pda = Player.main?.GetPDA();
			if (pda != null && pda.isInUse)
				showNumbers = true;

			icon.localScale = (pulseAnimationState != null && pulseAnimationState.enabled)? icon.localScale + punchScale: punchScale;

			if (flag != showNumbers)
				rotationVelocity += UnityEngine.Random.Range(-rotationRandomVelocity, rotationRandomVelocity);

			float time2 = Time.time;
			float num3 = 0.02f;
			float num4 = time2 - lastFixedUpdateTime;
			int num5 = Mathf.FloorToInt(num4);
			if (num5 > 20)
			{
				num5 = 1;
				num3 = num4;
			}

			lastFixedUpdateTime += (float)num5 * num3;

			for (int i = 0; i < num5; i++)
			{
				float num6 = rotationCurrent;
				float num7 = (!showNumbers) ? 0f : 180f;
				MathExtensions.Spring(ref rotationVelocity, ref rotationCurrent, num7, rotationSpringCoef, num3, rotationVelocityDamp, rotationVelocityMax);
				
				if (Mathf.Abs(num7 - rotationCurrent) < 1f && Mathf.Abs(rotationVelocity) < 1f)
				{
					rotationVelocity = 0f;
					rotationCurrent = num7;
				}

				if (num6 != rotationCurrent)
					icon.localRotation = Quaternion.Euler(0f, rotationCurrent, 0f);
			}
		}

		
		void OnDisable()
		{
			punchTween.Stop();
			pulseTween.Stop();
			
			if (subscribed && subscribe(false))
				subscribed = false;
		}


		void SetValue(float has, float capacity)
		{
			float target = has / capacity;
			curr = Mathf.SmoothDamp(curr, target, ref vel, dampSpeed);
			bar.value = curr;
			int num = Mathf.CeilToInt(curr * capacity);
			if (cachedValue != num)
			{
				cachedValue = num;
				text.text = IntStringCache.GetStringForInt(cachedValue);
			}
		}


		void Punch(float duration, float maxScale)
		{
			punchTween.duration = duration;
			punchMaxScale = maxScale;
			punchTween.Start();
		}


		void OnPunchStart()
		{
			punchInitialScale = icon.localScale;
			punchSeed = UnityEngine.Random.value;
		}


		void OnPunchUpdate(float t)
		{
			float num = 0f;
			float num2;
			MathExtensions.Oscillation(100f, 5f, punchSeed, t, out num2, out num);
			punchScale = new Vector3(num2 * punchMaxScale, num * punchMaxScale, 0f);
		}


		void OnPunchStop()
		{
			punchScale = new Vector3(0f, 0f, 0f);
			if (icon != null)
				icon.localScale = punchInitialScale;
		}


		void OnPulse(float scalar)
		{
			if (pulseAnimationState != null)
				pulseAnimationState.normalizedTime = Mathf.Clamp01((pulseTween.duration * scalar - pulseDelay) / pulseTime);
		}

		const float punchDamp = 100f;

		const float puchFrequency = 5f;

		[AssertNotNull]
		public uGUI_CircularBar bar;

		[AssertNotNull]
		public RectTransform icon;

		[AssertNotNull]
		public Text text;

		public float dampSpeed = 0.1f;

		[Space]
		public float pulseReferenceCapacity = 100f;

		public AnimationCurve pulseDelayCurve = new AnimationCurve();

		public AnimationCurve pulseTimeCurve = new AnimationCurve();

		[AssertNotNull]
		public Animation pulseAnimation;

		public float rotationSpringCoef = 100f;

		public float rotationVelocityDamp = 0.9f;

		public float rotationVelocityMax = -1f;

		public float rotationRandomVelocity = 1000f;

		private float curr;

		private float vel;

		private float punchSeed;

		private float punchMaxScale = 2f;

		private Vector3 punchInitialScale;

		private Vector3 punchScale = new Vector3(0f, 0f, 0f);

		private CoroutineTween punchTween;

		private bool subscribed;

		private CoroutineTween pulseTween;

		private float pulseDelay = -1f;

		private float pulseTime = -1f;

		private AnimationState pulseAnimationState;

		private int cachedValue = int.MinValue;

		private float rotationCurrent;

		private float rotationVelocity;

		private bool showNumbers;

		private float lastFixedUpdateTime;
	}
}