using nobnak.Gist;
using nobnak.Gist.Events;
using nobnak.Gist.Resizable;
using UnityEngine;

namespace SimpleFluid {

    public class BaseFluidEffect : MonoBehaviour {
        [Header("Lerp Material")]
        public float lerpEmission = 0.1f;
        public float lerpDissipation = 0.1f;
    }
}
