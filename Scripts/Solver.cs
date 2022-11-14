using Gist2.Extensions.ComponentExt;
using UnityEngine;

namespace SimpleFluid {

	public class SimpleAndFastFluids : System.IDisposable {
		public enum S_SOLVER {
			Init = 0,
			Fluid,
		};

		public const string PATH = "SimpleFluid_Solver";

		public const string PROP_FORCE_TEX = "_ForceTex";
        public const string PROP_DT = "_Dt";
        public const string PROP_K_VIS = "_KVis";
        public const string PROP_S = "_S";
        public const string PROP_FORCE_POWER = "_ForcePower";

		protected Material mat;

		public SimpleAndFastFluids() {
			mat = new Material(Resources.Load<Shader>(PATH));
		}

		#region properties
		public float DeltaTime => Time.deltaTime;
		#endregion

		#region IDisposable
		public void Dispose() {
			if (mat != null) {
				mat.Destroy();
				mat = null;
			}
		}
		#endregion

		public static void Swap<T>(ref T t0, ref T t1) { var tmp = t0; t0 = t1; t1 = tmp; }

		public void Init(RenderTexture fluid0) {
			Graphics.Blit(null, fluid0, mat, (int)S_SOLVER.Init);
		}
        public void Solve(RenderTexture fluid0, RenderTexture fluid1, Texture force, 
			Tuner tuner, float dt) {

			var kvis = tuner.vis;
            var s = tuner.k / dt;

            mat.SetTexture(PROP_FORCE_TEX, force);
            mat.SetFloat(PROP_FORCE_POWER, tuner.forcePower);
            mat.SetFloat(PROP_DT, dt);
            mat.SetFloat(PROP_K_VIS, kvis);
            mat.SetFloat(PROP_S, s);
    		Graphics.Blit (fluid0, fluid1, mat, (int)S_SOLVER.Fluid);
    		Swap (ref fluid0, ref fluid1);
        }

		#region declarations
		[System.Serializable]
		public class Tuner {
			public float forcePower = 0.002f;
			public float k = 0.12f;
			public float vis = 0.1f;
			public float timeStep = 0.1f;
			public float timeScale = 5f;
		}
		#endregion
	}
}
