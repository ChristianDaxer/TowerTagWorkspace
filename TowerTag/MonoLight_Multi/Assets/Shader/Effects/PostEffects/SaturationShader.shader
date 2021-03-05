Shader "_OwnShader/PostEffect/Saturation"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "" {}
		_BlendFactor("Saturation", Range(0.0, 1.0)) = 1.0
	}

	CGINCLUDE
	#include "UnityCG.cginc"

	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};

	float _BlendFactor;
	sampler2D _MainTex;
	float4 _MainTex_ST;

	v2f vert(appdata_img v) 
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = UnityStereoScreenSpaceUVAdjust(v.texcoord.xy, _MainTex_ST);
		return o;
	}


	float4 frag(v2f i) : SV_Target
	{
		float4 color = tex2D(_MainTex, i.uv);
		float luminance = dot(color.rgb, float3(0.2126729, 0.7151522, 0.072175));
		color.rgb = lerp(luminance.xxx, color.rgb, _BlendFactor.xxx);
		return color;
	}

	ENDCG

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}
}