using UnityEngine;
using System.Collections;
using nobnak.Gist;
using nobnak.Gist.Events;
using Gist2.Extensions.ComponentExt;
using UnityEngine.Events;
using Gist2.Wrappers;
using Gist2.Extensions.SizeExt;

namespace SimpleFluid {

    public class ForceFieldTouch : MonoBehaviour {
        public const string PROP_DIR_AND_CENTER = "_DirAndCenter";
        public const string PROP_INV_RADIUS = "_InvRadius";

		[System.Serializable]
		public class Events {
			public TextureEvent OnCreate;

			[System.Serializable]
			public class TextureEvent : UnityEvent<Texture> { }
		}

		public Events events = new Events();
        public Material forceFieldMat;
        public float forceRadius = 0.05f;

		Vector3 _mousePos;
        RenderTextureWrapper force;

		#region unity
		private void OnEnable() {
			var c = Camera.main;
			force = new RenderTextureWrapper(size => {
				var tex = new RenderTexture(size.x, size.y, 0, RenderTextureFormat.RGFloat);
				tex.hideFlags = HideFlags.DontSave;
				tex.wrapMode = TextureWrapMode.Clamp;
				tex.filterMode = FilterMode.Bilinear;
				return tex;
			});
			force.Changed += v => {
				events.OnCreate?.Invoke(v);
			};

			UpdateMousePos(Input.mousePosition);
		}
    	void Update () {
            UpdateForceField();
        }
		private void OnDisable() {
			if (force != null) {
				events.OnCreate?.Invoke(null);
				force.Dispose();
				force = null;
			}          
        }
		#endregion

		#region methods
		void UpdateForceField() {
            var mousePos = Input.mousePosition;
			var dx = UpdateMousePos(mousePos) / Time.deltaTime;
            var forceVector = Vector2.zero;
            var uv = Vector2.zero;

            if (Input.GetMouseButton (0)) {
                uv = Camera.main.ScreenToViewportPoint (mousePos);
                forceVector = Vector2.ClampMagnitude ((Vector2)dx, 1f);
            }

			var c = Camera.main;
			force.Size = c.Size();

            forceFieldMat.SetVector(PROP_DIR_AND_CENTER, 
                new Vector4(forceVector.x, forceVector.y, uv.x, uv.y));
            forceFieldMat.SetFloat(PROP_INV_RADIUS, 1f / forceRadius);
            Graphics.Blit(null, force, forceFieldMat);
        }
        Vector3 UpdateMousePos (Vector3 mousePos) {
            var dx = mousePos - _mousePos;
            _mousePos = mousePos;
            return dx;
        }
		#endregion
	}

}