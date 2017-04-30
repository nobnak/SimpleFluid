#ifndef __FLUIDABLE__
#define __FLUIDABLE__

#pragma multi_compile FLUIDABLE_OUTPUT_COLOR FLUIDABLE_OUTPUT_SOURCE

float _Fluidity;

float4 fluidOutMultiplier(float4 picture, float4 fluidSource) {
	#if defined(FLUIDABLE_OUTPUT_SOURCE)
	return fluidSource;
	#else
	return picture;
	#endif
}
float4 fluidOutMultiplier(float4 picture, float fluidity) {
    return fluidOutMultiplier(picture, picture * float4(1,1,1, fluidity));
}
float4 fluidOutMultiplier(float4 picture) {
    return fluidOutMultiplier(picture, _Fluidity);
}



#endif