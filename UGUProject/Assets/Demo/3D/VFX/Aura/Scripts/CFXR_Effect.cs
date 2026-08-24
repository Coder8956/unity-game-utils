using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CartoonFX
{
	[RequireComponent(typeof(ParticleSystem))]
	[DisallowMultipleComponent]
	public partial class CFXR_Effect : MonoBehaviour
	{
		const float GLOBAL_CAMERA_SHAKE_MULTIPLIER = 1.0f;
		static readonly int _GameObjectWorldPosition = Shader.PropertyToID("_GameObjectWorldPosition");

#if UNITY_EDITOR
		[InitializeOnLoadMethod]
		static void InitGlobalOptions()
		{
			AnimatedLight.editorPreview = EditorPrefs.GetBool("CFXR Light EditorPreview", true);
			CameraShake.editorPreview = EditorPrefs.GetBool("CFXR CameraShake EditorPreview", true);
		}
#endif

		public enum ClearBehavior { None, Disable, Destroy }

		[System.Serializable]
		public class AnimatedLight
		{
			static public bool editorPreview = true;
			public Light light;
			public bool loop;
			public bool animateIntensity;
			public float intensityStart = 8f;
			public float intensityEnd = 0f;
			public float intensityDuration = 0.5f;
			public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
			public bool perlinIntensity;
			public float perlinIntensitySpeed = 1f;
			public bool fadeIn;
			public float fadeInDuration = 0.5f;
			public bool fadeOut;
			public float fadeOutDuration = 0.5f;
			public bool animateRange;
			public float rangeStart = 8f;
			public float rangeEnd = 0f;
			public float rangeDuration = 0.5f;
			public AnimationCurve rangeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
			public bool perlinRange;
			public float perlinRangeSpeed = 1f;
			public bool animateColor;
			public Gradient colorGradient;
			public float colorDuration = 0.5f;
			public AnimationCurve colorCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
			public bool perlinColor;
			public float perlinColorSpeed = 1f;

			public void animate(float time)
			{
#if UNITY_EDITOR
				if (!editorPreview && !EditorApplication.isPlaying) return;
#endif
				if (light != null)
				{
					if (animateIntensity)
					{
						float delta = loop ? Mathf.Clamp01((time % intensityDuration)/intensityDuration) : Mathf.Clamp01(time/intensityDuration);
						delta = perlinIntensity ? Mathf.PerlinNoise(Time.time * perlinIntensitySpeed, 0f) : intensityCurve.Evaluate(delta);
						light.intensity = Mathf.LerpUnclamped(intensityEnd, intensityStart, delta);
						if (fadeIn && time < fadeInDuration) light.intensity *= Mathf.Clamp01(time / fadeInDuration);
					}
					if (animateRange)
					{
						float delta = loop ? Mathf.Clamp01((time % rangeDuration)/rangeDuration) : Mathf.Clamp01(time/rangeDuration);
						delta = perlinRange ? Mathf.PerlinNoise(Time.time * perlinRangeSpeed, 10f) : rangeCurve.Evaluate(delta);
						light.range = Mathf.LerpUnclamped(rangeEnd, rangeStart, delta);
					}
					if (animateColor)
					{
						float delta = loop ? Mathf.Clamp01((time % colorDuration)/colorDuration) : Mathf.Clamp01(time/colorDuration);
						delta = perlinColor ? Mathf.PerlinNoise(Time.time * perlinColorSpeed, 0f) : colorCurve.Evaluate(delta);
						light.color = colorGradient.Evaluate(delta);
					}
				}
			}

			public void animateFadeOut(float time)
			{
				if (fadeOut && light != null) light.intensity *= 1.0f - Mathf.Clamp01(time / fadeOutDuration);
			}

			public void reset()
			{
				if (light != null)
				{
					if (animateIntensity) light.intensity = (fadeIn || fadeOut) ? 0 : intensityEnd;
					if (animateRange) light.range = rangeEnd;
					if (animateColor) light.color = colorGradient.Evaluate(1f);
				}
			}
		}

		public static bool GlobalDisableCameraShake;
		public static bool GlobalDisableLights;

		[Tooltip("Defines an action to execute when the Particle System has completely finished playing and emitting particles.")]
		public ClearBehavior clearBehavior = ClearBehavior.Destroy;
		[Space] public CameraShake cameraShake;
		[Space] public AnimatedLight[] animatedLights;
		[Tooltip("Defines which Particle System to track to trigger light fading out.\nLeave empty if not using fading out.")]
		public ParticleSystem fadeOutReference;

		float time;
		ParticleSystem rootParticleSystem;
		[System.NonSerialized] MaterialPropertyBlock materialPropertyBlock;
		[System.NonSerialized] Renderer particleRenderer;

		public void ResetState()
		{
			time = 0f;
			fadingOutStartTime = 0f;
			isFadingOut = false;
			if (animatedLights != null) foreach (var animLight in animatedLights) animLight.reset();
			if (cameraShake != null && cameraShake.enabled) cameraShake.StopShake();
		}

		void Awake()
		{
			if (cameraShake != null && cameraShake.enabled) cameraShake.fetchCameras();
			startFrameOffset = GlobalStartFrameOffset++;
			particleRenderer = this.GetComponent<ParticleSystemRenderer>();
			if (particleRenderer.sharedMaterial != null && particleRenderer.sharedMaterial.IsKeywordEnabled("_CFXR_LIGHTING_WPOS_OFFSET"))
				materialPropertyBlock = new MaterialPropertyBlock();
			if (!GraphicsSettings.lightsUseLinearIntensity && animatedLights != null)
			{
				foreach (var animLight in animatedLights)
				{
					animLight.intensityStart = Mathf.LinearToGammaSpace(animLight.intensityStart);
					animLight.intensityEnd = Mathf.LinearToGammaSpace(animLight.intensityEnd);
				}
			}
		}

		void OnEnable()
		{
			if (animatedLights == null) return;
			foreach (var animLight in animatedLights)
			{
				if (animLight.light != null) animLight.light.enabled = !GlobalDisableLights;
			}
		}

		void OnDisable() { ResetState(); }

		const int CHECK_EVERY_N_FRAME = 20;
		static int GlobalStartFrameOffset;
		int startFrameOffset;

		void Update()
		{
			time += Time.deltaTime;
			Animate(time);
			if (fadeOutReference != null && !fadeOutReference.isEmitting && (fadeOutReference.isPlaying || isFadingOut)) FadeOut(time);
			if (clearBehavior != ClearBehavior.None)
			{
				if (rootParticleSystem == null) rootParticleSystem = this.GetComponent<ParticleSystem>();
				if ((Time.renderedFrameCount + startFrameOffset) % CHECK_EVERY_N_FRAME == 0)
				{
					if (!rootParticleSystem.IsAlive(true))
					{
						if (clearBehavior == ClearBehavior.Destroy) GameObject.Destroy(this.gameObject);
						else this.gameObject.SetActive(false);
					}
				}
			}
			if (materialPropertyBlock != null)
			{
				particleRenderer.GetPropertyBlock(materialPropertyBlock);
				materialPropertyBlock.SetVector(_GameObjectWorldPosition, this.transform.position);
				particleRenderer.SetPropertyBlock(materialPropertyBlock);
			}
		}

		public void Animate(float _time)
		{
			if (animatedLights != null && !GlobalDisableLights) foreach (var animLight in animatedLights) animLight.animate(_time);
			if (cameraShake != null && cameraShake.enabled && !GlobalDisableCameraShake) cameraShake.animate(_time);
		}

		bool isFadingOut;
		float fadingOutStartTime;
		public void FadeOut(float _time)
		{
			if (animatedLights == null) return;
			if (!isFadingOut) { isFadingOut = true; fadingOutStartTime = _time; }
			foreach (var animLight in animatedLights) animLight.animateFadeOut(_time - fadingOutStartTime);
		}
	}
}
