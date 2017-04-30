using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleFluid {

    [System.Serializable]
    public class ColorMatrix {
        public enum FilmModeEnum { ARGB = 0, Alpha }
        public enum BlendModeEnum { One = 0, SrcAlpha }
        public const string DEFAULT_MATRIX_NAME = "_ColorMatrix";
        public const string DEFAULT_COLOR_NAME = "_Color";

        public FilmModeEnum filmMode;
        public BlendModeEnum blendMode;
        public Color backgroundColor;

        public void SetMatrix(Material m, string prop) {
            m.SetMatrix (prop, GenerateMatrix ());
        }
        public void SetMatrix(Material m) {
            SetMatrix (m, DEFAULT_MATRIX_NAME);
        }
        public void SetKeyword(Material m) {
            m.shaderKeywords = null;
            m.EnableKeyword (blendMode.ToString ());
        }
        public void SetColor(Material m, string prop) {
            m.SetColor(DEFAULT_COLOR_NAME, backgroundColor);
        }
        public void SetColor(Material m) {
            SetColor (m, DEFAULT_COLOR_NAME);
        }

        public void Setup(Material m) {
            SetKeyword (m);
            SetMatrix (m);
            SetColor (m);
        }

        public Matrix4x4 GenerateMatrix() {
            var m = Matrix4x4.identity;
            switch (filmMode) {
            case FilmModeEnum.Alpha:
                var alpha = new Vector4 (0f, 0f, 0f, 1f);
                for (var i = 0; i < 4; i++)
                    m.SetRow (i, alpha);
                break;
            }
            return m;
        }
    }
}
