#ifndef __FLUIDABLE__
#define __FLUIDABLE__

float _Fluidity;

float4 fluidOutMultiplier(float4 picture, float4 fluidSource) {
	#if defined(FLUIDABLE_OUTPUT_SOURCE)
	return fluidSource;
	#else
	return picture;
	#endif
}
float4 fluidOutMultiplierTransparent(float4 picture) {
    #if defined(FLUIDABLE_OUTPUT_SOURCE)
    return picture * float4(picture.www, _Fluidity);
    #else
    return picture;
    #endif
}
float4 fluidOutMultiplier(float4 picture) {
    return fluidOutMultiplier(picture, picture * float4(1,1,1, _Fluidity));
}



#endif