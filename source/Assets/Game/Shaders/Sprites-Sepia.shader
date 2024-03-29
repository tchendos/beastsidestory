Shader "Custom/Sprites-Sepia"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Desat ("Desaturation", Range(0,1.0)) = 0.5
		_Tone ("Toning", Range(0,1.0)) = 0.5
		_LightColor ("Paper Tone", Color) = (1,0.9,0.5,1)
		_DarkColor ("Stain Tone", Color) = (0.2,0.05,0,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZTest Always
		ZWrite Off
		Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile DUMMY PIXELSNAP_ON
			#include "UnityCG.cginc"
			#include "Shared.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
			};
			
			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			half _Desat;
			half _Tone;
			fixed4 _LightColor;
			fixed4 _DarkColor;

			fixed4 frag(v2f IN) : COLOR
			{
				fixed4 tex = tex2D(_MainTex, IN.texcoord) * IN.color;
			    fixed3 result = applySepia(tex, _Desat, _Tone, _LightColor, _DarkColor);

				return  fixed4(result, tex.a);
			}
		ENDCG
		}
	}
}
