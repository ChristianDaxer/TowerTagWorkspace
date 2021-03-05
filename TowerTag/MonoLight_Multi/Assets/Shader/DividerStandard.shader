// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "DividerStandard"
{
	Properties
	{
		[Gamma]_MainTex("Albedo", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,0)
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
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _PERVERTEXNORMAL_ON
		#include "Assets/Shader/CombinedMeshUtils.hlsl"
		#include "Assets/Shader/CombinedMeshUtils/CombinedMeshEmission.hlsl"
		#pragma only_renderers d3d11_9x d3d11 glcore gles3 
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows exclude_path:deferred 
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
			o.Albedo = ( _Color * tex2D( _MainTex, uv_MainTex ) ).rgb;
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
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
65;105;1920;865;1370;227.5;1;True;True
Node;AmplifyShaderEditor.FunctionNode;4;-543.5,242.5;Inherit;False;MetallicOutput;10;;4;addb0945cb0e34341aeb85fb9329cb89;0;0;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;40;-729.5,159.5;Inherit;False;SampleDistributedEmission;-1;;6;281770b971fc41c4c8f878522d04abf4;0;0;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;41;-595.5,18.5;Inherit;False;EmissionOutput;7;;10;88bf9b62f88b024438d3171d6047091d;0;0;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;1;-343.5,-192.5;Inherit;False;AlbedoOutput;0;;11;b1066fad48096344682d0d945d196f51;0;0;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;6;-409.5,432.5;Inherit;False;OcclusionOutput;12;;14;8fb9ca1365f89494e8b0e38698fd46a6;0;0;1;COLOR;0
Node;AmplifyShaderEditor.BreakToComponentsNode;5;-314.5,251.5;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleAddOpNode;42;-301.5,61.5;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;2;-333.5,-92.5;Inherit;False;BumpOutput;3;;15;4e78a794c49a70d4b84fa746d324b530;0;0;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;DividerStandard;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;4;d3d11_9x;d3d11;glcore;gles3;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;2;Include;./CombinedMeshUtils.hlsl;True;d99a4602d54a13a40b2b35c6ac3c99a8;Custom;Include;;True;5409e421bb6b3184587bc410d564ec1a;Custom;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;5;0;4;0
WireConnection;42;0;41;0
WireConnection;42;1;40;0
WireConnection;0;0;1;0
WireConnection;0;1;2;0
WireConnection;0;2;42;0
WireConnection;0;3;5;0
WireConnection;0;4;5;3
WireConnection;0;5;6;0
ASEEND*/
//CHKSM=D3AD869A7C16951CB60FDE24329CF0AEB4A34063