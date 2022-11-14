using nobnak.Gist;
using nobnak.Gist.Events;
using nobnak.Gist.Resizable;
using SimpleFluid;
using UnityEngine;
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
		public TextureEvent OnUpdateAdvectedImageTexture;

		public OutputModeEnum outputMode;

		[SerializeField]
		protected Material lerpMat;

        [Header("Visualizer")]
        public ColorMatrix colorMatrix;
        public Material colorVisualizerMat;

        [Header("Texture Format")]
        public UnityEngine.RenderTextureFormat textureFormatAdvected = UnityEngine.RenderTextureFormat.ARGBFloat;
        public UnityEngine.RenderTextureFormat textureFormatSource = UnityEngine.RenderTextureFormat.ARGB32;

        Camera _attachedCamera;
		ManuallyRenderCamera manualCam;
		ResizableRenderTexture _imageTex0;
		ResizableRenderTexture _imageTex1;
		ResizableRenderTexture _sourceTex;

		#region Unity
        protected virtual void OnEnable() {
            _attachedCamera = GetComponent<Camera> ();
            _attachedCamera.depthTextureMode = DepthTextureMode.Depth;

            manualCam = new ManuallyRenderCamera (_attachedCamera);
#if TEMPORAL
			_imageTex0 = new TemporalResizableRenderTexture(new nobnak.Gist.Resizable.FormatRT() {
				depth = 0,
				textureFormat = textureFormatAdvected,
				readWrite = RenderTextureReadWrite.Linear
			});
			_imageTex1 = new TemporalResizableRenderTexture(new nobnak.Gist.Resizable.FormatRT() {
				depth = 0,
				textureFormat = textureFormatAdvected,
				readWrite = RenderTextureReadWrite.Linear
			});
			_sourceTex = new TemporalResizableRenderTexture(new nobnak.Gist.Resizable.FormatRT() {
				depth = 24,
				textureFormat = textureFormatSource,
				readWrite = RenderTextureReadWrite.Linear,
				antiAliasing = QualitySettings.antiAliasing
			});
#else
			_imageTex0 = new ResizableRenderTexture(new nobnak.Gist.Resizable.FormatRT() {
				depth = 0,
				textureFormat = textureFormatAdvected,
				readWrite = RenderTextureReadWrite.Linear,
				filterMode = FilterMode.Point
			});
			_imageTex1 = new ResizableRenderTexture(new nobnak.Gist.Resizable.FormatRT() {
				depth = 0,
				textureFormat = textureFormatAdvected,
				readWrite = RenderTextureReadWrite.Linear,
				filterMode = FilterMode.Point
			});
			_sourceTex = new ResizableRenderTexture(new nobnak.Gist.Resizable.FormatRT() {
				depth = 24,
				textureFormat = textureFormatSource,
				readWrite = RenderTextureReadWrite.Linear,
				antiAliasing = QualitySettings.antiAliasing
			});
#endif

			Prepare();
        }
		protected virtual void Update() {
            Prepare ();
			CaptureAdvectionSource ();
			InjectSourceColorToImage ();
        }
		protected virtual void OnRenderImage(RenderTexture src, RenderTexture dst) {
            colorMatrix.Setup (colorVisualizerMat);

			switch (outputMode) {
			case OutputModeEnum.AdvectionSource:
                Graphics.Blit (_sourceTex.Texture, dst, colorVisualizerMat);
				break;
			case OutputModeEnum.AdvectedImage:
                Graphics.Blit (_imageTex0.Texture, dst, colorVisualizerMat);
				break;
			default:
                Graphics.Blit(src, dst);
				break;
			}
		}
		protected virtual void OnDisable() {
			//NotifyTextureOnChange(null);
            manualCam.Dispose ();
            if (_imageTex0 != null) {
                _imageTex0.Dispose ();
                _imageTex0 = null;
            }
            if (_imageTex1 != null) {
                _imageTex1.Dispose ();
                _imageTex1 = null;
            }
            if (_sourceTex != null) {
                _sourceTex.Dispose ();
                _sourceTex = null;
            }
        }
#endregion

		protected void Prepare () {
			var size = new Vector2Int(_attachedCamera.pixelWidth, _attachedCamera.pixelHeight);
			_imageTex0.Size = _imageTex1.Size = _sourceTex.Size = size;
			//_imageTex0.Lod = _imageTex1.Lod = _sourceTex.Lod = lod;
        }

		protected void SwapImageTexture() {
			SimpleAndFastFluids.Swap(ref _imageTex0, ref _imageTex1);
		}

		protected void CaptureAdvectionSource () {
			Shader.EnableKeyword (FLUIDABLE_KW_SOURCE);
            manualCam.Render (_sourceTex.Texture, tuner.basics.cullingMask);
			Shader.DisableKeyword (FLUIDABLE_KW_SOURCE);
		}

		protected void InjectSourceColorToImage () {
			lerpMat.SetFloat(PROP_LERP_EMISSION, tuner.basics.lerpEmission);
			lerpMat.SetFloat(PROP_LERP_DISSIPATION, tuner.basics.lerpDissipation);
			lerpMat.SetTexture(PROP_PREV_TEX, _imageTex0.Texture);
			Graphics.Blit(_sourceTex.Texture, _imageTex1.Texture, lerpMat);
			
			NotifyTextureOnChange(_imageTex1.Texture);
			SwapImageTexture();
		}

		private void NotifyTextureOnChange(Texture tex) {
			OnUpdateAdvectedImageTexture.Invoke(tex);
		}

		#region declarations
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

			public LayerMask cullingMask = -1;
		}
		[System.Serializable]
		public class Tuner {
			public BasicTuner basics = new BasicTuner();
		}
		#endregion
	}
}
