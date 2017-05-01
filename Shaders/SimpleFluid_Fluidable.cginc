#ifndef __FLUIDABLE__
#define __FLUIDABLE__

#pragma multi_compile FLUIDABLE_OUTPUT_COLOR FLUIDABLE_OUTPUT_SOURCE

#ifdef FLUIDABLE_OUTPUT_SOURCE
float _Fluidity;
#endif

float4 fluidOutMultiplier(float4 picture, float fluidity) {
    #if defined(FLUIDABLE_OUTPUT_SOURCE)
    return picture * float4(1,1,1,_Fluidity * fluidity);
    #else
    return picture;
    #endif
}
float4 fluidOutMultiplier(float4 picture) {
    return fluidOutMultiplier(picture, 1.0);
}



#pragma multi_compile NO_NEED_GAMMA_CONVERSION NEED_GAMMA_CONVERSION

static const float GAMMA_CURVE = 2.2;
static const float GAMMA_CURVE_INV = 1.0 / GAMMA_CURVE;

float4 GammaToLinear(float4 pic) {
    #ifdef NEED_GAMMA_CONVERSION
    return pow(pic, GAMMA_CURVE_INV);
    #else
    return pic;
    #endif
}
float4 LinearToGamma(float4 pic) {
    #ifdef NEED_GAMMA_CONVERSION
    return pow(pic, GAMMA_CURVE);
    #else
    return pic;
    #endif
}

#endif