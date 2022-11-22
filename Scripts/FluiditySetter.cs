using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleFluid {

	[ExecuteAlways]
	public class FluiditySetter : MonoBehaviour {

		public readonly static int P_Fluidity = Shader.PropertyToID("_Fluidity");

		[Range(0f, 1f)]
		public float fluidity;

		protected bool changed;

		#region unity
		private void OnEnable() {
			changed = true;
		}
		private void OnValidate() {
			changed = true;
		}
		private void Update() {
			if (changed) {
				changed = false;
				SetFluidity(fluidity);
			}
		}
		#endregion

		public void SetFluidity(float v) {
			foreach (var r in GetComponentsInChildren<Renderer>())
				r.sharedMaterial.SetFloat(P_Fluidity, v);
		}
	}
}
