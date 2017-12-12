using UnityEngine;
using System.Collections;
using nobnak.Gist;
using nobnak.Gist.Events;

namespace SimpleFluid {

    public class ForceFieldTouch : MonoBehaviour {
        public const string PROP_DIR_AND_CENTER = "_DirAndCenter";
        public const string PROP_INV_RADIUS = "_InvRadius";

        public TextureEvent OnUpdateForceField;

        public Solver solver;
        public Material forceFieldMat;
        public float forceRadius = 0.05f;

        Vector3 _mousePos;
        RenderTexture _forceFieldTex;

    	void Start () {
    	
    	}
    	void Update () {
            InitOrResizeForceField (solver.Width, solver.Height);

            UpdateForceField();
        }
        void OnDestroy() {
            ReleaseForceField ();            
        }

        void InitOrResizeForceField (int width, int height) {
            if (_forceFieldTex == null || _forceFieldTex.width != width || _forceFieldTex.height != height) {
                ReleaseForceField ();
                _forceFieldTex = new RenderTexture (width, height, 0, RenderTextureFormat.RGFloat);
            }
        }
        void UpdateForceField() {
            var mousePos = Input.mousePosition;
            var dx = UpdateMousePos(mousePos);
            var forceVector = Vector2.zero;
            var uv = Vector2.zero;

            if (Input.GetMouseButton (0)) {
                uv = Camera.main.ScreenToViewportPoint (mousePos);
                forceVector = Vector2.ClampMagnitude ((Vector2)dx, 1f);
            }

            forceFieldMat.SetVector(PROP_DIR_AND_CENTER, 
                new Vector4(forceVector.x, forceVector.y, uv.x, uv.y));
            forceFieldMat.SetFloat(PROP_INV_RADIUS, 1f / forceRadius);
            Graphics.Blit(null, _forceFieldTex, forceFieldMat);

            NotifyForceFieldUpdate ();
        }
        void NotifyForceFieldUpdate() {
            OnUpdateForceField.Invoke (_forceFieldTex);            
        }
        Vector3 UpdateMousePos (Vector3 mousePos) {
            var dx = mousePos - _mousePos;
            _mousePos = mousePos;
            return dx;
        }
        void ReleaseForceField () {
            Destroy (_forceFieldTex);
        }
    }

}