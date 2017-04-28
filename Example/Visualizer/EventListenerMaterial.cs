using UnityEngine;
using System.Collections;

namespace SimpleFluid {

    public class EventListenerMaterial : MonoBehaviour {
        public string propertyName = "_MainTex";
        public Material[] materials;

        public void Listen(Texture tex) {
            foreach (var mat in materials)
                mat.SetTexture (propertyName, tex);
        }
    }
}