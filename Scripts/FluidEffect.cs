using Gist2.Extensions.LODExt;
using Gist2.Extensions.SizeExt;
using Gist2.Wrappers;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace SimpleFluid {
	[RequireComponent(typeof(Camera))]
    public class FluidEffect : BaseFluidEffect {
		public enum OutputModeEnum { Normal = 0, Force, Fluid, AdvectionSource, AdvectedImage }

        public const string FLUIDABLE_KW_SOURCE = "FLUIDABLE_OUTPUT_SOURCE";

        public const string PROP_FLUID_TEX = "_FluidTex";
        public const string PROP_IMAGE_TEX = "_ImageTex";
		public const string PROP_PREV_TEX = "_PrevTex";
        public const string PROP_DT = "_Dt";

        public const string PROP_LERP_EMISSION = "_Emission";
        public const string PROP_LERP_DISSIPATION = "_Dissipation";

		public Preset preset = new Preset();
		public Tuner tuner = new Tuner();
		public Events events = new Events();

		[SerializeField]
		protected Material advectMat;
		[SerializeField]
		protected Material lerpMat;

        [Header("Texture Format")]
        public RenderTextureFormat textureFormatAdvected = RenderTextureFormat.ARGBFloat;
        public RenderTextureFormat textureFormatSource = RenderTextureFormat.ARGB32;

		protected Camera _attachedCamera;

		protected SimpleAndFastFluids solver;
		protected RenderTextureWrapper fluid0, fluid1, image0, image1, source;
		protected CameraWrapper captureCam;

		#region properties
		public Texture Force { get; set; }
		#endregion

		#region Unity
		void OnEnable() {
			solver = new SimpleAndFastFluids();

            _attachedCamera = GetComponent<Camera> ();
            _attachedCamera.depthTextureMode = DepthTextureMode.Depth;

            captureCam = new CameraWrapper(c => {
				if (c == null) {
					var go = new GameObject("Capture");
					go.hideFlags = HideFlags.DontSave;
					c = go.AddComponent<Camera>();
				}
				c.CopyFrom(_attachedCamera);
				c.enabled = false;
				c.clearFlags = CameraClearFlags.Color;
				c.backgroundColor = Color.clear;
				c.cullingMask = (c.cullingMask & preset.cullingMask) + preset.additionalMask;
				c.targetTexture = source;
				return c;
			});

			fluid0 = new RenderTextureWrapper(GenFluidTex);
			fluid1 = new RenderTextureWrapper(GenFluidTex);
			image0 = new RenderTextureWrapper(GenImageTex);
			image1 = new RenderTextureWrapper(GenImageTex);
			source = new RenderTextureWrapper(GenSourceTex);

			fluid0.Changed += v => {
				if (solver != null) {
					solver.Init();
					if (v.Value != null) solver.Clear(v);
				}
			};
			fluid1.Changed += v => {
				if (v.Value != null) solver.Clear(v);
			};
			image0.Changed += v => {
				if (v.Value != null) Clear(v);
			};
			image1.Changed += v => {
				if (v.Value != null) Clear(v);
			};
		}
		void OnDisable() {
			captureCam.Dispose();
			if (fluid0 != null) {
				fluid0.Dispose();
				fluid0 = null;
			}
			if (fluid1 != null) {
				fluid1.Dispose();
				fluid1 = null;
			}
			if (image0 != null) {
				image0.Dispose();
				image0 = null;
			}
			if (image1 != null) {
				image1.Dispose();
				image1 = null;
			}
			if (source != null) {
				source.Dispose();
				source = null;
			}
			if (solver != null) {
				solver.Dispose();
				solver = null;
			}
		}
		void Update() {
			var dt = Time.deltaTime;
			Prepare();
			Solve(dt);
			UpdateImage(dt);

			CaptureAdvectionSource();
			InjectSourceColorToImage();
			Notify();
		}
		void Notify() {
			Texture target = null;
			switch (tuner.debug.outputMode) {
			case OutputModeEnum.Fluid:
                target = fluid0;
				break;
			case OutputModeEnum.Force:
                target = Force;
				break;
			case OutputModeEnum.AdvectionSource:
                target = source;
				break;
			case OutputModeEnum.AdvectedImage:
			default:
				target = image0;
				break;
			}
			events.OnUpdateAdvectedImageTexture?.Invoke(target);
		}
		#endregion

		#region interfaces
		public void Reset() {
			fluid0?.Release();
			fluid1?.Release();
			image0?.Release();
			image1?.Release();
			source?.Release();
		}
		#endregion

		#region methods
		protected void Prepare () {
			var size = _attachedCamera.Size();
			var size_solver = size.LOD(tuner.basics.lod_solver + tuner.basics.lod_image);
			var size_image = size.LOD(tuner.basics.lod_image);
			var prev_solver = fluid0.Size;
			var prev_image = image0.Size;
			fluid0.Size = fluid1.Size = size_solver;
			image0.Size = image1.Size = source.Size = size_image;
			if (math.any(size_solver != prev_solver) || math.any(size_image != prev_image))
				Debug.Log($"Fluid size changed: fluid={size_solver} image={size_image}");
		}

		private void Solve(float dt) {
			solver.Solve(fluid0, fluid1, Force, tuner.fluid, dt);
			SimpleAndFastFluids.Swap(ref fluid0, ref fluid1);
		}
		protected void UpdateImage (float dt) {
			advectMat.SetTexture(PROP_FLUID_TEX, fluid0);
			advectMat.SetFloat(PROP_DT, dt);
			Graphics.Blit(image0, image1, advectMat);
			SimpleAndFastFluids.Swap(ref image0, ref image1);
		}
		protected void CaptureAdvectionSource () {
			Shader.EnableKeyword (FLUIDABLE_KW_SOURCE);
            captureCam.Value.Render();
			Shader.DisableKeyword (FLUIDABLE_KW_SOURCE);
		}
		protected void InjectSourceColorToImage () {
            lerpMat.SetFloat (PROP_LERP_EMISSION, tuner.basics.lerpEmission);
            lerpMat.SetFloat (PROP_LERP_DISSIPATION, tuner.basics.lerpDissipation);
            lerpMat.SetTexture (PROP_PREV_TEX, image0);
            Graphics.Blit (source, image1, lerpMat);
			SimpleAndFastFluids.Swap(ref image0, ref image1);
		}
		protected RenderTexture GenFluidTex(int2 size) {
			var tex = new RenderTexture(size.x, size.y, 0, textureFormatAdvected);
			tex.hideFlags = HideFlags.DontSave;
			tex.wrapMode = TextureWrapMode.Clamp;
			return tex;
		}
		protected RenderTexture GenImageTex(int2 size) {
			var tex = new RenderTexture(size.x, size.y, 0, textureFormatAdvected);
			tex.hideFlags = HideFlags.DontSave;
			tex.filterMode = FilterMode.Bilinear;
			tex.wrapMode = TextureWrapMode.Clamp;
			return tex;
		}
		protected RenderTexture GenSourceTex(int2 size) {
			var tex = new RenderTexture(size.x, size.y, 0, textureFormatSource, RenderTextureReadWrite.Linear); ;
			tex.hideFlags = HideFlags.DontSave;
			tex.antiAliasing = math.max(1, QualitySettings.antiAliasing);
			return tex;
		}
		protected void Clear(RenderTexture rt) {
			var active = RenderTexture.active;
			RenderTexture.active = rt;
			GL.Clear(true, true, Color.clear);
			RenderTexture.active = active;
		}
		#endregion

		#region declarations
		[System.Serializable]
		public class Preset {
			public LayerMask cullingMask = -1;
			public LayerMask additionalMask = 0;
		}
		[System.Serializable]
		public class Events {
			public TextureEvent OnUpdateAdvectedImageTexture = new TextureEvent();

			[System.Serializable]
			public class TextureEvent : UnityEvent<Texture> { }
		}
		[System.Serializable]
		public class BasicTuner {
			[Header("Lerp Material")]
			public float lerpEmission = 0.1f;
			public float lerpDissipation = 0.1f;

			[Header("Quality")]
			[Range(0, 4)]
			[FormerlySerializedAs("lod")]
			public int lod_solver = 1;
			public int lod_image = 1;
		}
		[System.Serializable]
		public class DebugTuner {
			public OutputModeEnum outputMode;
		}
		[System.Serializable]
		public class Tuner {
			public BasicTuner basics = new BasicTuner();
			public SimpleAndFastFluids.Tuner fluid = new SimpleAndFastFluids.Tuner();
			public DebugTuner debug = new DebugTuner();
		}
		#endregion
	}
}
