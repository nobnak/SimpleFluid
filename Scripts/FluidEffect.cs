using nobnak.Gist;
using nobnak.Gist.Events;
using nobnak.Gist.Resizable;
using UnityEngine;

namespace SimpleFluid {
	[RequireComponent(typeof(Camera))]
    public class FluidEffect : BaseFluidEffect {
		public enum SimulationModeEnum { Fluid = 0, Scraped }
		public enum OutputModeEnum { Normal = 0, Force, Fluid, AdvectionSource, AdvectedImage }

        public const string FLUIDABLE_KW_SOURCE = "FLUIDABLE_OUTPUT_SOURCE";

        public const string PROP_FLUID_TEX = "_FluidTex";
        public const string PROP_IMAGE_TEX = "_ImageTex";
		public const string PROP_PREV_TEX = "_PrevTex";
        public const string PROP_DT = "_Dt";

        public const string PROP_LERP_EMISSION = "_Emission";
        public const string PROP_LERP_DISSIPATION = "_Dissipation";

		public TextureEvent OnUpdateAdvectedImageTexture;

		public SimulationModeEnum simulationMode;
		public OutputModeEnum outputMode;
        public int lod = 1;

		[SerializeField]
		protected Solver solver;
		[SerializeField]
		protected Material advectMat;
		[SerializeField]
		protected Material lerpMat;

        [Header("Visualizer")]
        public ColorMatrix colorMatrix;
        public Material colorVisualizerMat;

        [Header("Texture Format")]
        public RenderTextureFormat textureFormatAdvected = RenderTextureFormat.ARGBFloat;
        public RenderTextureFormat textureFormatSource = RenderTextureFormat.ARGB32;

        Camera _attachedCamera;
    	LODRenderTexture _imageTex0;
        LODRenderTexture _imageTex1;
		ManuallyRenderCamera manualCam;
		LODRenderTexture _sourceTex;

		#region Unity
        void Start() {
            _attachedCamera = GetComponent<Camera> ();
            _attachedCamera.depthTextureMode = DepthTextureMode.Depth;

			System.Func<Vector2Int> fsize = () => 
				new Vector2Int(_attachedCamera.pixelWidth, _attachedCamera.pixelHeight);
            manualCam = new ManuallyRenderCamera (_attachedCamera);
            _imageTex0 = new LODRenderTexture (
				fsize, 0, textureFormatAdvected, RenderTextureReadWrite.Linear);
            _imageTex1 = new LODRenderTexture (
				fsize, 0, textureFormatAdvected, RenderTextureReadWrite.Linear);
            _sourceTex = new LODRenderTexture (
				fsize, 24, textureFormatSource, RenderTextureReadWrite.Linear, 
				QualitySettings.antiAliasing);

            _imageTex0.AfterCreateTexture += UpdateAfterCreateTexture;
            _imageTex1.AfterCreateTexture += UpdateAfterCreateTexture;

            Prepare ();
        }
        void Update() {
            var dt = solver.DeltaTime;
            Prepare ();

			if (simulationMode == SimulationModeEnum.Fluid) {
				solver.Solve(dt);
				UpdateImage(dt);
			}

			CaptureAdvectionSource ();
			InjectSourceColorToImage ();
        }
		void OnRenderImage(RenderTexture src, RenderTexture dst) {
            colorMatrix.Setup (colorVisualizerMat);

			switch (outputMode) {
			case OutputModeEnum.Fluid:
                Graphics.Blit (solver.FluidTex, dst, colorVisualizerMat);
				break;
			case OutputModeEnum.Force:
                Graphics.Blit (solver.ForceTex, dst, colorVisualizerMat);
				break;
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
        void OnDestroy() {
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
		
		protected void UpdateAfterCreateTexture(LODRenderTexture obj) {
			obj.Texture.wrapMode = TextureWrapMode.Clamp;
			obj.Texture.filterMode = FilterMode.Bilinear;
			obj.Clear(Color.clear);
		}
		protected void Prepare () {
            var width = _attachedCamera.pixelWidth;
            var height = _attachedCamera.pixelHeight;
            solver.SetSize (width >> lod, height >> lod);
			_imageTex0.Lod = lod;
			_imageTex1.Lod = lod;
			_sourceTex.Lod = lod;
            _imageTex0.Update ();
            _imageTex1.Update ();
            _sourceTex.Update ();
        }
		protected void UpdateImage (float dt) {
			solver.SetProperties(advectMat, PROP_FLUID_TEX);
			advectMat.SetFloat(PROP_DT, dt);
			Graphics.Blit(_imageTex0.Texture, _imageTex1.Texture, advectMat);
			SwapImageTexture();
		}

		protected void SwapImageTexture() {
			Solver.Swap(ref _imageTex0, ref _imageTex1);
		}

		protected void CaptureAdvectionSource () {
			Shader.EnableKeyword (FLUIDABLE_KW_SOURCE);
            manualCam.Render (_sourceTex.Texture);
			Shader.DisableKeyword (FLUIDABLE_KW_SOURCE);
		}

		protected void InjectSourceColorToImage () {
            lerpMat.SetFloat (PROP_LERP_EMISSION, lerpEmission);
            lerpMat.SetFloat (PROP_LERP_DISSIPATION, lerpDissipation);
            lerpMat.SetTexture (PROP_PREV_TEX, _imageTex0.Texture);
            Graphics.Blit (_sourceTex.Texture, _imageTex1.Texture, lerpMat);
            OnUpdateAdvectedImageTexture.Invoke (_imageTex1.Texture);
			SwapImageTexture();
		}
    }
}
