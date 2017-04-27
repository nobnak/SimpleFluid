using UnityEngine;
using System.Collections;

namespace SimpleFluid {

    public class SampleObject : MonoBehaviour {
        public const string PROP_FLUID_TEX = "_FluidTex";
		public const string PROP_REFERENCE_TEX = "_RefTex";
        public const string PROP_DT = "_Dt";

		public Solver solver;

        public Material advectMat;
        public Texture2D imageTex;

		public int lod = 1;
        public float timeScale = 1f;

        public Material[] outputMats;

    	RenderTexture _imageTex0;
		RenderTexture _imageTex1;

    	void Update () {
			var dt = Time.deltaTime * timeScale;
            solver.SetSize(imageTex.width >> lod, imageTex.height >> lod);

            solver.Solve(dt);
			UpdateImage (dt);
            #if true
    		NotifyResult(_imageTex0);
            #else
    		NotifyResult(_fluidTex0);
            #endif
		}
        void OnDestroy() {
            Release();
        }

		void Release() {
			ReleaseImage ();
        }

		void ReleaseImage () {
			Destroy (_imageTex0);
			Destroy (_imageTex1);
		}
    	void NotifyResult(Texture outputTex) {
            foreach (var mat in outputMats)
    			mat.mainTexture = outputTex;
        }
		void UpdateImage (float dt) {
			InitImage ();
			solver.SetProperties (advectMat, PROP_FLUID_TEX);
			advectMat.SetTexture (PROP_REFERENCE_TEX, imageTex);
    		advectMat.SetFloat (PROP_DT, dt);
            Graphics.Blit (_imageTex0, _imageTex1, advectMat);
    		Solver.Swap (ref _imageTex0, ref _imageTex1);
    	}
		void InitImage() {
			if (_imageTex0 == null || _imageTex0.width != imageTex.width || _imageTex0.height != imageTex.height) {
				ReleaseImage();
				_imageTex0 = new RenderTexture (imageTex.width, imageTex.height, 0, RenderTextureFormat.ARGBFloat);
				_imageTex1 = new RenderTexture (imageTex.width, imageTex.height, 0, RenderTextureFormat.ARGBFloat);
				_imageTex0.filterMode = _imageTex1.filterMode = FilterMode.Bilinear;
				_imageTex0.wrapMode = _imageTex1.wrapMode = TextureWrapMode.Clamp;
				Graphics.Blit (imageTex, _imageTex0);
			}
		}
    }
}
