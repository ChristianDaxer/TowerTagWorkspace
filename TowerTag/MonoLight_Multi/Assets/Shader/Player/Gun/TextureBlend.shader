Shader "_OwnShader/Unlit/TextureBlend"
{
	Properties
	{
		_MainTex ("BackgroundTexture", 2D) = "white" {}
		_MainTex_2 ("ForegroundTexture", 2D) = "white" {}
		_TintColor("TintColor", Color) = (1,1,1,1)
		_BlendValue("BlendValue", float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
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
			sampler2D _MainTex_2;
			float4 _MainTex_ST;
			fixed4 _TintColor;
			half _BlendValue;

			
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
				fixed4 col = lerp( tex2D(_MainTex, i.uv) , tex2D(_MainTex_2, i.uv), (i.meshUV.y) < _BlendValue);

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
