Shader "_OwnShader/Unlit/SimplePillarClaim"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		// Background Color
		//_InnerColor("Inner Color", Color) = (1,1,1,1)
		_TintColor("BackgroundColor", Color) = (1,1,1,1)
		_ClaimColor("ClaimColor", Color) = (1,1,1,1)
		_ClaimValue("ClaimValue", float) = 0
		_FogStrength("FogStrength", float) = 0
		
		_TextureFactor("TextureIntensity: multiply", float) = 0
		_TextureFactorOffset("TextureIntensity: additive", float) = 0

		_TextureFactorBackground("TextureIntensity: multiply", float) = 0
		_TextureFactorOffsetBackground("TextureIntensity: additive", float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Cull Back

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal: NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float2 meshUV : TEXCOORD2;
				float intensity : TEXCOORD3;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _TintColor;
			fixed4 _ClaimColor;
			half _ClaimValue;
			half _FogStrength;

			half _TextureFactor;
			half _TextureFactorOffset;
			
			float _TextureFactorBackground;
			float _TextureFactorOffsetBackground;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.meshUV = v.uv;
				
				float3 position_ws = mul(unity_ObjectToWorld, v.vertex);
				float3 viewDirection = normalize(_WorldSpaceCameraPos - position_ws);
				float3 normal_ws = UnityObjectToWorldNormal(v.normal);

				float viewDependentFade = abs(dot(normal_ws, viewDirection));

				o.intensity = viewDependentFade;
				
				
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 tex = tex2D(_MainTex, i.uv);
				fixed4 claimColor = (tex * _TextureFactor + _TextureFactorOffset) * _ClaimColor;
				fixed4 backgroundColor = (tex * _TextureFactorBackground + _TextureFactorOffsetBackground) * _TintColor;
				fixed4 col = lerp(backgroundColor, claimColor, (i.meshUV.y) < _ClaimValue) * i.intensity;
				fixed4 oCol = col * i.intensity;
				
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				
				return lerp(oCol, col, _FogStrength);
			}
			ENDCG
		}
	}
}
