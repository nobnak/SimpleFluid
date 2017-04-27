using UnityEngine;
using System.Collections;

namespace SimpleFluid {
    
    public class Solver : MonoBehaviour {
        public const int SOLVER_FLUID = 0;
        public const int SOLVER_ADVECT = 1;

        public const string PROP_FLUID_TEX = "_FluidTex";
        public const string PROP_BOUNDARY_TEX = "_BoundaryTex";
        public const string PROP_FORCE_TEX = "_ForceTex";
        public const string PROP_DT = "_Dt";
        public const string PROP_K_VIS = "_KVis";
        public const string PROP_S = "_S";
        public const string PROP_FORCE_POWER = "_ForcePower";

        public const float rho0 = 1f;
        public const float dx = 1f;

        public Material solverMat;
        public float forcePower = 0.002f;
        public float k = 0.12f;
        public float vis = 0.1f;
    	public float timeStep = 0.1f;
        public float timeScale = 10f;

        Texture2D _initTex;
        Texture2D _boundaryTex;
        Texture _forceTex;
        RenderTexture _fluidTex0;
        RenderTexture _fluidTex1;
        float _timeAccum = 0f;
        int _width = -1;
        int _height = -1;

        public Texture ForceTex { set { _forceTex = value; } get { return _forceTex; } }
		public Texture FluidTex { get { return _fluidTex0; } }
        public int Width { get { return _width; } }
        public int Height { get { return _height; } }
        public float DeltaTime { get { return Time.deltaTime * timeScale; } }

        public static void Swap<T>(ref T t0, ref T t1) { var tmp = t0; t0 = t1; t1 = tmp; }

        public void Solve(float dt) {
            var nIterations = CalculateIterations(dt);
            for (var i = 0; i < nIterations; i++) {
                UpdateSolver(timeStep);
            }
        }
        public void SetSize(int width, int height) {
            InitSolver (width, height);
        }
        public void SetProperties(Material mat, string fluidTex) {
            mat.SetTexture (fluidTex, _fluidTex0);
        }

        #region Unity
        void OnDestroy() {
            ReleaseSolver();
        }
        #endregion

        void ReleaseSolver() {
            Object.Destroy(_initTex);
            Object.Destroy(_boundaryTex);
            Object.Destroy(_fluidTex0);
            Object.Destroy(_fluidTex1);
        }
        void InitSolver(int width, int height) {
            if (width != _width || height != _height) {
                ReleaseSolver();
                _width = width;
                _height = height;

                _initTex = new Texture2D(_width, _height, TextureFormat.ARGB32, false);
                _boundaryTex = new Texture2D(_width, _height, TextureFormat.ARGB32, false);
                _fluidTex0 = new RenderTexture(_width, _height, 0, RenderTextureFormat.ARGBFloat);
                _fluidTex1 = new RenderTexture(_width, _height, 0, RenderTextureFormat.ARGBFloat);
        		_fluidTex0.wrapMode = _fluidTex1.wrapMode = _initTex.wrapMode = TextureWrapMode.Clamp;
        		_fluidTex0.filterMode = _fluidTex1.filterMode = _initTex.filterMode = FilterMode.Bilinear;

                var initData = _initTex.GetPixels();
                var boundaryData = _boundaryTex.GetPixels();
                for (var y = 0; y < _height; y++) {
                    for (var x = 0; x < _width; x++) {
                        var i = x + y * _width;
                        initData[i] = new Color(0f, 0f, 0f, rho0);
                        boundaryData[i] = new Color(
                            (x == 0 || x == (_width-1)) ? 0f : 1f,
                            (y == 0 || y == (_height-1)) ? 0f : 1f,
                            1f, 1f);
                    }
                }
                _initTex.SetPixels(initData);
                _boundaryTex.SetPixels(boundaryData);
                _initTex.Apply();
                _boundaryTex.Apply();
                Graphics.Blit(_initTex, _fluidTex0);
            }
        }
        void UpdateSolver(float dt) {
            var kvis = vis / rho0;
            var s = k * (dx * dx) / (dt * rho0);

            solverMat.SetTexture(PROP_FLUID_TEX, _fluidTex0);
            solverMat.SetTexture(PROP_BOUNDARY_TEX, _boundaryTex);
            solverMat.SetTexture(PROP_FORCE_TEX, ForceTex);
            solverMat.SetFloat(PROP_FORCE_POWER, forcePower);
            solverMat.SetFloat(PROP_DT, dt);
            solverMat.SetFloat(PROP_K_VIS, kvis);
            solverMat.SetFloat(PROP_S, s);
    		Graphics.Blit (null, _fluidTex1, solverMat, SOLVER_FLUID);
    		Swap (ref _fluidTex0, ref _fluidTex1);
        }

        int CalculateIterations(float dt) {
            _timeAccum += dt;
            var nIterations = Mathf.Max(0, (int)(_timeAccum / timeStep));
            _timeAccum = Mathf.Max(0f, _timeAccum - timeStep * nIterations);
            return nIterations;
        }
    }
}
