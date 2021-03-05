Shader "_OwnShader/Unlit/SimplePillarClaim_InvUVMapping"
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
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float2 meshUV : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _TintColor;
			fixed4 _ClaimColor;
			half _ClaimValue;
			half _FogStrength;
			half _TextureFactor;
			half _TextureFactorOffset;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.meshUV = v.uv;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				//fixed4 col = (1 - tex2D(_MainTex, i.uv) * _TextureFactor) * lerp(_TintColor, _ClaimColor, (1 - i.meshUV.y) < _ClaimValue);
				fixed4 col = (1 - tex2D(_MainTex, i.uv) * _TextureFactor + _TextureFactorOffset) * lerp(_TintColor, _ClaimColor, (i.meshUV.x) < _ClaimValue);

				fixed4 oCol = col;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return lerp(oCol, col, _FogStrength);
			}
			ENDCG
		}
	}
}
