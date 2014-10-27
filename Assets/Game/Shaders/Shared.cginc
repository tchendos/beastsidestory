fixed4 applySepia(fixed4 tex, half desat, half tone, fixed4 lightColor, fixed4 darkColor)
{
    fixed3 grayXfer = float3(0.3,0.59,0.11);
    fixed  gray = dot(grayXfer,tex.xyz);
    fixed3 muted = lerp(tex.xyz,gray.xxx,desat);
    fixed3 sepia = lerp(darkColor,lightColor,gray);
    fixed3 result = lerp(muted,sepia,tone);
	return fixed4(result, tex.a);
}
