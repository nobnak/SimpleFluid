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
float4 fluidable(float4 color, float4 source) {
#if defined(FLUIDABLE_OUTPUT_SOURCE)
    return fluidOutMultiplier(source);
#else
    return color;
#endif
}

void fluidOutMultiplier_float(float4 cin, out float4 cout) {
    cout = fluidOutMultiplier(cin, 1.0);
}

#endif