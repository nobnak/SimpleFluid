using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ColorMatrix {
    public const string DEFAULT_PROPERTY_NAME = "_ColorMatrix";

    public enum ModeEnum { ARGB = 0, Alpha }

    public ModeEnum mode;

    public void SetMatrix(Material m, string prop) {
        m.SetMatrix (prop, GenerateMatrix ());
    }
    public void SetMatrix(Material m) {
        SetMatrix (m, DEFAULT_PROPERTY_NAME);
    }

    public Matrix4x4 GenerateMatrix() {
        var m = Matrix4x4.identity;
        switch (mode) {
        case ModeEnum.Alpha:
            var alpha = new Vector4 (0f, 0f, 0f, 1f);
            for (var i = 0; i < 4; i++)
                m.SetRow (i, alpha);
            break;
        }
        return m;
    }
}
