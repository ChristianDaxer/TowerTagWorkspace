Shader "Unlit/UnlitFresnel"
{
	Properties
	{
		_Color ("Color", color) = (1,1,1)
		[Toggle(INVERSE_FRESNEL)] _INVERSE_FRESNEL("INVERSE_FRESNEL", Int) = 0
		_FresnelExponent( "_FresnelExponent", Range(0.0001, 6) ) = 1
		_MinAlpha("_MinAlpha", Range(0,1)) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#pragma multi_compile _ INVERSE_FRESNEL
			#include "UnityCG.cginc"

		
			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float3 normal : NORMAL;
				float3 viewDir : NORMAL1;
				UNITY_FOG_COORDS(0)
				float4 vertex : SV_POSITION;
			};

			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.viewDir = UnityWorldSpaceViewDir(mul(unity_ObjectToWorld, v.vertex));
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			half4 _Color;
			half _FresnelExponent, _MinAlpha;
			fixed4 frag (v2f i) : SV_Target
			{
				
				half fresnel = saturate(dot(normalize(i.normal), normalize(i.viewDir)));
#ifdef INVERSE_FRESNEL
			fresnel = 1 - fresnel;
#endif
				fresnel = pow(fresnel, _FresnelExponent);
				half4 col = _Color;
				col.a = max(_MinAlpha, fresnel * _Color.a);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
