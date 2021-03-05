// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "DividerStandardTransparent"
{
	Properties
	{
		[Gamma]_MainTex("Albedo", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,0)
		Opacity("_Opacity", Range( 0 , 1)) = 0.5
		[Normal]_BumpMap("Normal Map", 2D) = "bump" {}
		_BumpScale("BumpScale", Float) = 1
		[Toggle(_PERVERTEXNORMAL_ON)] _PerVertexNormal("PerVertexNormal", Float) = 0
		_EmissionMap("Emission Map", 2D) = "white" {}
		[HDR]_EmissionColor("EmissionColor", Color) = (0,0,0,0)
		_MetallicGlossMap("Metallic and Smoothness", 2D) = "white" {}
		_OcclusionMap("AO Map", 2D) = "white" {}
		[HideInInspector] _texcoord3( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityStandardUtils.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _PERVERTEXNORMAL_ON
		#include "Assets/Shader/CombinedMeshUtils.hlsl"
		#include "Assets/Shader/CombinedMeshUtils/CombinedMeshEmission.hlsl"
		struct Input
		{
			float2 uv_texcoord;
			float2 uv3_texcoord3;
		};

		uniform float4 _EmissionColor;
		uniform sampler2D _MainTex;
		uniform sampler2D _BumpMap;
		uniform float4 _BumpMap_ST;
		uniform float _BumpScale;
		uniform float4 _Color;
		uniform float4 _MainTex_ST;
		uniform sampler2D _EmissionMap;
		uniform float4 _EmissionMap_ST;
		uniform sampler2D _MetallicGlossMap;
		uniform float4 _MetallicGlossMap_ST;
		uniform sampler2D _OcclusionMap;
		uniform float4 _OcclusionMap_ST;
		uniform float Opacity;


		float3 ConvertUVToInstanceInCombinedMesh1_g6( float2 InUV )
		{
			return _IndexedEmissionColors[UVToMeshInstanceInsideCombinedMesh(InUV)];
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_BumpMap = i.uv_texcoord * _BumpMap_ST.xy + _BumpMap_ST.zw;
			#ifdef _PERVERTEXNORMAL_ON
				float3 staticSwitch35_g15 = float3(0,0,1);
			#else
				float3 staticSwitch35_g15 = UnpackScaleNormal( tex2D( _BumpMap, uv_BumpMap ), _BumpScale );
			#endif
			o.Normal = staticSwitch35_g15;
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float4 temp_output_44_0 = ( _Color * tex2D( _MainTex, uv_MainTex ) );
			o.Albedo = temp_output_44_0.rgb;
			float2 uv_EmissionMap = i.uv_texcoord * _EmissionMap_ST.xy + _EmissionMap_ST.zw;
			float2 InUV1_g6 = i.uv3_texcoord3;
			float3 localConvertUVToInstanceInCombinedMesh1_g6 = ConvertUVToInstanceInCombinedMesh1_g6( InUV1_g6 );
			o.Emission = ( ( (tex2D( _EmissionMap, uv_EmissionMap )).rgb * (_EmissionColor).rgb ) + ( localConvertUVToInstanceInCombinedMesh1_g6 + float3(0,0,0) ) );
			float2 uv_MetallicGlossMap = i.uv_texcoord * _MetallicGlossMap_ST.xy + _MetallicGlossMap_ST.zw;
			float4 break5 = tex2D( _MetallicGlossMap, uv_MetallicGlossMap );
			o.Metallic = break5;
			o.Smoothness = break5.a;
			float2 uv_OcclusionMap = i.uv_texcoord * _OcclusionMap_ST.xy + _OcclusionMap_ST.zw;
			o.Occlusion = tex2D( _OcclusionMap, uv_OcclusionMap ).r;
			float clampResult8_g18 = clamp( ( temp_output_44_0.w + Opacity ) , 0.0 , 1.0 );
			o.Alpha = clampResult8_g18;
		}

		ENDCG
		CGPROGRAM
		#pragma only_renderers d3d11_9x d3d11 glcore gles3 
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows exclude_path:deferred 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float4 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.customPack1.zw = customInputData.uv3_texcoord3;
				o.customPack1.zw = v.texcoord2;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				surfIN.uv3_texcoord3 = IN.customPack1.zw;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
65;105;1920;865;1658;252.5;1;True;True
Node;AmplifyShaderEditor.FunctionNode;4;-543.5,242.5;Inherit;False;MetallicOutput;12;;4;addb0945cb0e34341aeb85fb9329cb89;0;0;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;40;-729.5,159.5;Inherit;False;SampleDistributedEmission;-1;;6;281770b971fc41c4c8f878522d04abf4;0;0;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;41;-595.5,18.5;Inherit;False;EmissionOutput;9;;10;88bf9b62f88b024438d3171d6047091d;0;0;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;44;-677.5,-267.5;Inherit;False;AlbedoOutput;0;;14;b1066fad48096344682d0d945d196f51;0;0;1;COLOR;0
Node;AmplifyShaderEditor.BreakToComponentsNode;5;-314.5,251.5;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleAddOpNode;42;-301.5,61.5;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;2;-450.5,-62.5;Inherit;False;BumpOutput;5;;15;4e78a794c49a70d4b84fa746d324b530;0;0;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;6;-402.5,577.5;Inherit;False;OcclusionOutput;14;;17;8fb9ca1365f89494e8b0e38698fd46a6;0;0;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;46;-480.5,423.5;Inherit;False;OpacityOutput;3;;18;ce259353473d87549a08a488e1eb3722;0;1;5;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;DividerStandardTransparent;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;ForwardOnly;4;d3d11_9x;d3d11;glcore;gles3;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;2;Include;./CombinedMeshUtils.hlsl;True;d99a4602d54a13a40b2b35c6ac3c99a8;Custom;Include;;True;5409e421bb6b3184587bc410d564ec1a;Custom;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;5;0;4;0
WireConnection;42;0;41;0
WireConnection;42;1;40;0
WireConnection;46;5;44;0
WireConnection;0;0;44;0
WireConnection;0;1;2;0
WireConnection;0;2;42;0
WireConnection;0;3;5;0
WireConnection;0;4;5;3
WireConnection;0;5;6;0
WireConnection;0;9;46;0
ASEEND*/
//CHKSM=248873D1E3BBC5DD2FA612B8DE27C7F6D2DC31DE