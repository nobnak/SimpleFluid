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

#endif