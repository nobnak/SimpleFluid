using Gist2.Extensions.ComponentExt;
using Unity.Mathematics;
using UnityEngine;

namespace SimpleFluid {

	public class SimpleAndFastFluids : System.IDisposable {
		public enum S_SOLVER {
			Init = 0,
			Fluid,
		};

		public const string PATH = "SimpleFluid_Solver";

		public static readonly int P_ForceTex = Shader.PropertyToID("_ForceTex");
		public static readonly int P_BoundaryTex = Shader.PropertyToID("_BoundaryTex");
		
		public static readonly int P_Dt = Shader.PropertyToID("_Dt");
        public static readonly int P_KVis = Shader.PropertyToID("_KVis");
        public static readonly int P_S = Shader.PropertyToID("_S");
        public static readonly int P_ForcePower = Shader.PropertyToID("_ForcePower");

		protected Material mat;
		protected float t_residue = 0;

		public SimpleAndFastFluids() {
			mat = new Material(Resources.Load<Shader>(PATH));
		}

		#region properties
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

		public void Init() {
			t_residue = 0f;
		}
		public void Clear(RenderTexture fluid0) {
			Graphics.Blit(null, fluid0, mat, (int)S_SOLVER.Init);
		}
        public void Solve(RenderTexture fluid0, RenderTexture fluid1,
			Tuner tuner, float dt,
			Texture force = null,
			Texture	boundary = null
			) {

			t_residue += dt * tuner.timeScale;
			var n = math.max(0, (int)math.floor(t_residue / tuner.timeStep));
			t_residue = math.max(0, t_residue - n * tuner.timeStep);

			for (var i = 0; i < n; i++) {
				var kvis = tuner.vis;
				var s = tuner.k / dt;

				mat.SetTexture(P_ForceTex, force);
				mat.SetTexture(P_BoundaryTex, boundary);
				mat.SetFloat(P_ForcePower, tuner.forcePower);
				mat.SetFloat(P_Dt, tuner.timeStep);
				mat.SetFloat(P_KVis, kvis);
				mat.SetFloat(P_S, s);
				Graphics.Blit(fluid0, fluid1, mat, (int)S_SOLVER.Fluid);
				Swap(ref fluid0, ref fluid1);
			}
        }

		#region declarations
		[System.Serializable]
		public class Tuner {
			public float forcePower = 1f;
			public float k = 0.12f;
			public float vis = 0.1f;
			public float timeStep = 0.01f;
			public float timeScale = 1f;
		}
		#endregion
	}
}
