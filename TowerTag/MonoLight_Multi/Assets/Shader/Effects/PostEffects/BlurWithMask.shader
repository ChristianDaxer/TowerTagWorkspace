// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "PostEffects/BlurWithMask" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "" {}
		_InnerRadius("Inner Radius", Range(0.0, 1.0)) = 1.0
		_OuterRadius("Outer Radius", Range(0.0, 1.0)) = 1.0
	}

	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	struct v2f 
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float2 uv_original : TEXCOORD4;
		float4 uv01 : TEXCOORD1;
		float4 uv23 : TEXCOORD2;
		float4 uv45 : TEXCOORD3;
	};
	
	float4 offsets;
	
	sampler2D _MainTex;
	float4 _MainTex_ST;
	sampler2D _BlurredTex;
	half4 _blurTintColor;
	half _distortionOffset;
	fixed _InnerRadius;
	fixed _OuterRadius;
		
	v2f vert (appdata_img v) {
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);

		/*o.uv.xy = v.texcoord.xy;
		o.uv01 =  v.texcoord.xyxy + offsets.xyxy * float4(1,1, -1,-1);
		o.uv23 =  v.texcoord.xyxy + offsets.xyxy * float4(1,1, -1,-1) * 2.0;
		o.uv45 =  v.texcoord.xyxy + offsets.xyxy * float4(1,1, -1,-1) * 3.0;*/

		float2 uv = UnityStereoScreenSpaceUVAdjust(v.texcoord.xy, _MainTex_ST);
		o.uv_original = v.texcoord.xy;
		o.uv.xy = uv;
		o.uv01 = uv.xyxy + offsets.xyxy * float4(1, 1, -1, -1);
		o.uv23 = uv.xyxy + offsets.xyxy * float4(1, 1, -1, -1) * 2.0;
		o.uv45 = uv.xyxy + offsets.xyxy * float4(1, 1, -1, -1) * 3.0;

		return o;  
	}
		
	half4 fragBlur (v2f i) : SV_Target 
	{
		half4 color = float4 (0,0,0,0);
		color += 0.40 * tex2D (_MainTex, i.uv);
		color += 0.15 * tex2D (_MainTex, i.uv01.xy);
		color += 0.15 * tex2D (_MainTex, i.uv01.zw);
		color += 0.10 * tex2D (_MainTex, i.uv23.xy);
		color += 0.10 * tex2D (_MainTex, i.uv23.zw);
		color += 0.05 * tex2D (_MainTex, i.uv45.xy);
		color += 0.05 * tex2D (_MainTex, i.uv45.zw);	

		return color;
	} 

	half4 fragInterpolate (v2f i) : SV_Target 
	{
		/*fixed x = i.uv.x - 0.5f;
		fixed y = i.uv.y - 0.5f;*/
		fixed x = i.uv_original.x - 0.5f;
		fixed y = i.uv_original.y - 0.5f;
		fixed radius = sqrt(x*x + y*y);
		fixed t = clamp(((radius - _InnerRadius) / (_OuterRadius - _InnerRadius)), 0, 1);
		half sinT = sin(_Time.z);
		fixed t2 = (_blurTintColor.a);//(0.5f * sinT + 0.5f) * (_blurTintColor.a);

		fixed2 o = fixed2(x, y) * _distortionOffset  * sin(_Time.w);
		half blurred = tex2D (_BlurredTex, i.uv + o);

		return ((1-t) * tex2D (_MainTex, i.uv)) + (t * ((blurred * _blurTintColor * (t2)) + ((1 - t2) * tex2D (_BlurredTex, i.uv + o))));
	} 

	ENDCG
	
Subshader {
 Pass {
	  ZTest Always Cull Off ZWrite Off

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment fragBlur
      ENDCG
  }
  Pass {
	  ZTest Always Cull Off ZWrite Off

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment fragInterpolate
      ENDCG
  }
}

Fallback off

	
} // shader
