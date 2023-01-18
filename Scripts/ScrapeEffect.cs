using Gist2.Extensions.LODExt;
using Gist2.Extensions.SizeExt;
using Gist2.Wrappers;
using NvAPIWrapper.Native.Display.Structures;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace SimpleFluid {

	[RequireComponent(typeof(Camera))]
    public class ScrapeEffect : BaseFluidEffect {
		public enum OutputModeEnum { Normal = 0, Force, Fluid, AdvectionSource, AdvectedImage }

        public const string FLUIDABLE_KW_SOURCE = "FLUIDABLE_OUTPUT_SOURCE";

        public const string PROP_FLUID_TEX = "_FluidTex";
        public const string PROP_IMAGE_TEX = "_ImageTex";
		public const string PROP_PREV_TEX = "_PrevTex";
        public const string PROP_DT = "_Dt";

        public const string PROP_LERP_EMISSION = "_Emission";
        public const string PROP_LERP_DISSIPATION = "_Dissipation";

		public Tuner tuner = new Tuner();
		public Events events = new Events();
		public Preset preset = new Preset();

		[SerializeField]
		protected Material lerpMat;

        [Header("Texture Format")]
        public UnityEngine.RenderTextureFormat textureFormatAdvected = UnityEngine.RenderTextureFormat.ARGBFloat;
        public UnityEngine.RenderTextureFormat textureFormatSource = UnityEngine.RenderTextureFormat.ARGB32;

        protected Camera _attachedCamera;
		protected CameraWrapper captureCam;
		protected RenderTextureWrapper image0, image1, source;

		#region Unity
        protected virtual void OnEnable() {
            _attachedCamera = GetComponent<Camera> ();
            _attachedCamera.depthTextureMode |= DepthTextureMode.Depth;

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
				c.cullingMask = (c.cullingMask & preset.cullingMask);
				c.targetTexture = source;
				return c;
			});

			image0 = new RenderTextureWrapper(GenImageTex);
			image1 = new RenderTextureWrapper(GenImageTex);
			source = new RenderTextureWrapper(GenSourceTex);

			image0.Changed += v => {
				if (v.Value != null) Clear(v);
			};
			image1.Changed += v => {
				if (v.Value != null) Clear(v);
			};
        }
		protected virtual void Update() {
            Prepare ();
			CaptureAdvectionSource ();
			InjectSourceColorToImage ();
			Notify();
        }
		protected virtual void OnDisable() {
            captureCam.Dispose ();
            if (image0 != null) {
				image0.Dispose ();
				image0 = null;
            }
            if (image1 != null) {
				image1.Dispose ();
				image1 = null;
            }
            if (source != null) {
				source.Dispose ();
				source = null;
            }
        }
		#endregion

		#region methods
		void Notify() {
			Texture target = null;
			switch (tuner.debug.outputMode) {
				case OutputModeEnum.AdvectionSource:
				target = source;
				break;
				default:
				target = image0;
				break;
			}
			events.OnUpdateAdvectedImageTexture?.Invoke(target);
		}
		protected void Prepare () {
			var size = _attachedCamera.Size();
			var size_image = size.LOD(tuner.basics.lod_image);
			var prev_image = image0.Size;

			image0.Size = image1.Size = source.Size = size_image;
			if (math.any(size_image != prev_image))
				Debug.Log($"Bombing size changed: image={size_image}");
		}
		protected void CaptureAdvectionSource () {
			Shader.EnableKeyword (FLUIDABLE_KW_SOURCE);
			captureCam.Value.Render();
			Shader.DisableKeyword (FLUIDABLE_KW_SOURCE);
		}
		protected void InjectSourceColorToImage () {
			lerpMat.SetFloat(PROP_LERP_EMISSION, tuner.basics.lerpEmission);
			lerpMat.SetFloat(PROP_LERP_DISSIPATION, tuner.basics.lerpDissipation);
			lerpMat.SetTexture(PROP_PREV_TEX, image0);
			Graphics.Blit(source, image1, lerpMat);

			SimpleAndFastFluids.Swap(ref image0, ref image1);

			events.OnUpdateAdvectedImageTexture?.Invoke(image1);
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
			public int lod_image = 1;

			public LayerMask cullingMask = -1;
		}
		[System.Serializable]
		public class DebugTuner {
			public OutputModeEnum outputMode;
		}
		[System.Serializable]
		public class Tuner {
			public BasicTuner basics = new BasicTuner();
			public DebugTuner debug = new DebugTuner();
		}
		#endregion
	}
}
