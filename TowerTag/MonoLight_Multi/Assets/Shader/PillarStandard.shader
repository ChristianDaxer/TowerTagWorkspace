// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "PillarStandard"
{
	Properties
	{
		[Gamma]_MainTex("Albedo", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,0)
		_EmissionMap("Emission Map", 2D) = "white" {}
		_LightIntensity("LightIntensity", Range( 0 , 10)) = 1
		_LightRange("LightRange", Range( 0 , 10)) = 1
		_LightOffset("LightOffset", Vector) = (0,6.5,0,0)
		_EmissionColor("Emission Color", Color) = (0.8627451,0.8627451,1,0)
		_ClaimColor("Claim Color", Color) = (0.8627451,0.8627451,1,0)
		_ClaimValue("Claim Value", Float) = 0
		_Smoothness("Smoothness", Range( 0 , 1)) = 0
		_EmissionColor("Emission Color", Color) = (1,1,1,0)
		_ThresholdTower("ThresholdTower", Float) = 2.75
		_ThresholdTowerTop("ThresholdTowerTop", Float) = 2.75
		_ClaimValue("Claim Percentage", Range( 0 , 1)) = 1
		_ClaimColor("Claim Color", Color) = (1,1,1,0)
		[Normal]_BumpMap("Normal Map", 2D) = "bump" {}
		_BumpScale("BumpScale", Float) = 1
		[Toggle(_PERVERTEXNORMAL_ON)] _PerVertexNormal("PerVertexNormal", Float) = 0
		_Metalness("Metalness", Range( 0 , 1)) = 0
		_OcclusionMap("AO Map", 2D) = "white" {}
		_MetallicGlossMap("Metallic and Smoothness", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityStandardUtils.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma shader_feature_local _PERVERTEXNORMAL_ON
		#include "Assets/Shader/PillarUtils.hlsl"
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform sampler2D _MainTex;
		uniform sampler2D _BumpMap;
		uniform float4 _BumpMap_ST;
		uniform float _BumpScale;
		uniform float4 _Color;
		uniform float4 _MainTex_ST;
		uniform sampler2D _EmissionMap;
		uniform float4 _EmissionMap_ST;
		uniform float _ThresholdTower;
		uniform float _ThresholdTowerTop;
		uniform float _ClaimValue;
		uniform float4 _EmissionColor;
		uniform float4 _ClaimColor;
		uniform sampler2D _OcclusionMap;
		uniform float4 _OcclusionMap_ST;
		uniform float3 _LightOffset;
		uniform float _LightRange;
		uniform float _LightIntensity;
		uniform sampler2D _MetallicGlossMap;
		uniform float4 _MetallicGlossMap_ST;
		uniform float _Metalness;
		uniform float _Smoothness;


		float4x4 GetClaimLocalToWorld123(  )
		{
			return _ClaimLocalToWorld;
		}


		float4x4 GetClaimLocalToWorld33_g86(  )
		{
			return _ClaimLocalToWorld;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_BumpMap = i.uv_texcoord * _BumpMap_ST.xy + _BumpMap_ST.zw;
			#ifdef _PERVERTEXNORMAL_ON
				float3 staticSwitch35_g84 = float3(0,0,1);
			#else
				float3 staticSwitch35_g84 = UnpackScaleNormal( tex2D( _BumpMap, uv_BumpMap ), _BumpScale );
			#endif
			float3 temp_output_108_0 = staticSwitch35_g84;
			o.Normal = temp_output_108_0;
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float2 uv_EmissionMap = i.uv_texcoord * _EmissionMap_ST.xy + _EmissionMap_ST.zw;
			float3 temp_output_10_0_g82 = (tex2D( _EmissionMap, uv_EmissionMap )).rgb;
			float3 clampResult16_g82 = clamp( temp_output_10_0_g82 , float3( 0,0,0 ) , float3( 1,0,0 ) );
			float ifLocalVar23_g82 = 0;
			if( ( (clampResult16_g82).x + (clampResult16_g82).y + (clampResult16_g82).z ) <= 0.0 )
				ifLocalVar23_g82 = 0.0;
			else
				ifLocalVar23_g82 = 1.0;
			float temp_output_152_13 = ( 1.0 - ifLocalVar23_g82 );
			o.Albedo = ( ( _Color * tex2D( _MainTex, uv_MainTex ) ) * temp_output_152_13 ).rgb;
			float4x4 localGetClaimLocalToWorld123 = GetClaimLocalToWorld123();
			float4 appendResult125 = (float4(0.0 , _ThresholdTower , 0.0 , 1.0));
			float4 appendResult126 = (float4(0.0 , _ThresholdTowerTop , 0.0 , 1.0));
			float lerpResult77 = lerp( mul( localGetClaimLocalToWorld123, appendResult125 ).y , mul( localGetClaimLocalToWorld123, appendResult126 ).y , _ClaimValue);
			float3 ase_worldPos = i.worldPos;
			float4 color72 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
			float4 ifLocalVar90 = 0;
			if( mul( localGetClaimLocalToWorld123, appendResult125 ).y <= ase_worldPos.y )
				ifLocalVar90 = _ClaimColor;
			else
				ifLocalVar90 = color72;
			float4 ifLocalVar86 = 0;
			if( _ClaimValue >= 1.0 )
				ifLocalVar86 = _EmissionColor;
			else
				ifLocalVar86 = ifLocalVar90;
			float3 temp_output_152_0 = ( temp_output_10_0_g82 * ifLocalVar86.rgb );
			float4 color91 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
			float4 ifLocalVar71 = 0;
			if( lerpResult77 >= ase_worldPos.y )
				ifLocalVar71 = float4( temp_output_152_0 , 0.0 );
			else
				ifLocalVar71 = color91;
			float2 uv_OcclusionMap = i.uv_texcoord * _OcclusionMap_ST.xy + _OcclusionMap_ST.zw;
			float temp_output_109_0 = ( (tex2D( _OcclusionMap, uv_OcclusionMap )).r * temp_output_152_13 );
			float4x4 localGetClaimLocalToWorld33_g86 = GetClaimLocalToWorld33_g86();
			float4 appendResult35_g86 = (float4(_LightOffset , 1.0));
			float3 temp_output_6_0_g86 = ( (mul( localGetClaimLocalToWorld33_g86, appendResult35_g86 )).xyz - ase_worldPos );
			float3 normalizeResult11_g86 = normalize( temp_output_6_0_g86 );
			float dotResult13_g86 = dot( (WorldNormalVector( i , temp_output_108_0 )) , normalizeResult11_g86 );
			float clampResult14_g86 = clamp( ( 0.1 + dotResult13_g86 ) , 0.0 , 1.0 );
			float clampResult10_g86 = clamp( ( _LightRange - distance( temp_output_6_0_g86 , float3( 0,0,0 ) ) ) , 0.0 , _LightRange );
			float clampResult19_g86 = clamp( ( clampResult14_g86 * pow( ( clampResult10_g86 / _LightRange ) , 3.0 ) * _LightIntensity ) , 0.0 , 1.0 );
			float4 ifLocalVar41_g86 = 0;
			if( _ClaimValue >= 1.0 )
				ifLocalVar41_g86 = _EmissionColor;
			else
				ifLocalVar41_g86 = _ClaimColor;
			o.Emission = ( (ifLocalVar71.rgb).xyz + ( temp_output_109_0 * ( clampResult19_g86 * (ifLocalVar41_g86).rgb ) ) );
			float2 uv_MetallicGlossMap = i.uv_texcoord * _MetallicGlossMap_ST.xy + _MetallicGlossMap_ST.zw;
			float4 break58 = tex2D( _MetallicGlossMap, uv_MetallicGlossMap );
			o.Metallic = ( break58.r * _Metalness );
			o.Smoothness = ( break58.a * _Smoothness );
			o.Occlusion = temp_output_109_0;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma only_renderers d3d11_9x d3d11 glcore gles3 
		#pragma surface surf Standard keepalpha fullforwardshadows exclude_path:deferred 

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
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
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
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
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
65;105;1920;865;1345.962;347.0171;1.3191;True;True
Node;AmplifyShaderEditor.RangedFloatNode;87;-2730.136,4.841839;Inherit;False;Property;_ThresholdTower;ThresholdTower;14;0;Create;True;0;0;0;False;0;False;2.75;2.75;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;123;-2734.44,-168.4427;Inherit;False;return _ClaimLocalToWorld@;6;False;0;GetClaimLocalToWorld;True;False;0;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.DynamicAppendNode;125;-2320.782,-57.00478;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;88;-2773.244,131.4986;Inherit;False;Property;_ThresholdTowerTop;ThresholdTowerTop;15;0;Create;True;0;0;0;False;0;False;2.75;5.9;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;124;-2145.782,-166.0048;Inherit;False;2;2;0;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.BreakToComponentsNode;143;-1965.981,-133.9149;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;126;-2336.782,110.9953;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ColorNode;72;-2060.82,511.1409;Inherit;False;Constant;_Black;Black;9;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;80;-2032.903,707.2841;Inherit;False;Property;_ClaimColor;Claim Color;17;0;Create;False;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldPosInputsNode;142;-2076.964,305.291;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;127;-2126.782,0.9953126;Inherit;False;2;2;0;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;64;-1665.782,231.9648;Inherit;False;Property;_ClaimValue;Claim Percentage;16;0;Create;False;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;90;-1656.339,630.6938;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;81;-1624.034,412.1126;Inherit;False;Property;_EmissionColor;Emission Color;13;0;Create;False;0;0;0;False;0;False;1,1,1,0;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ConditionalIfNode;86;-1362.202,405.8237;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;1;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.BreakToComponentsNode;144;-1959.544,40.30066;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.FunctionNode;20;-1250.638,-186.6861;Inherit;False;OcclusionOutput;23;;28;8fb9ca1365f89494e8b0e38698fd46a6;0;0;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;77;-1293.342,98.0708;Inherit;False;3;0;FLOAT;2.75;False;1;FLOAT;5.9;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;152;-1136.622,420.037;Inherit;False;InputColorEmissionOutput;3;;82;666f5628d6590f84eadf917c5f16238b;0;1;12;FLOAT3;0,0,0;False;3;FLOAT;13;FLOAT;26;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;112;-956.6428,-68.84149;Inherit;False;True;False;False;False;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;91;-848.7786,558.6287;Inherit;False;Constant;_Color0;Color 0;9;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;21;-341.2737,646.5325;Inherit;False;MetallicOutput;25;;81;addb0945cb0e34341aeb85fb9329cb89;0;0;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;108;-977.7771,19.25925;Inherit;False;BumpOutput;18;;84;4e78a794c49a70d4b84fa746d324b530;0;0;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;59;-255.2117,892.7288;Inherit;False;Property;_Smoothness;Smoothness;12;0;Create;False;0;0;0;False;0;False;0;0.75;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;109;-222.3171,-135.6227;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;18;-794.0103,-248.8438;Inherit;False;AlbedoOutput;0;;83;b1066fad48096344682d0d945d196f51;0;0;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;62;-282.2116,779.7289;Inherit;False;Property;_Metalness;Metalness;22;0;Create;False;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;58;-88.02562,629.638;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.ConditionalIfNode;71;-352.1752,335.3017;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;117;-222.8897,-238.8957;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StickyNoteNode;150;-3150.003,-402.0073;Inherit;False;617.9895;321.2739;New Note;;1,1,1,1;Since the pillar is static, the pillar is being combined into a single mesh within the scene. However, this means that the object position of vertices relative to the transform of the pillar is different. Therefore if we want to calculate the range at which _ClaimValue is calculated across the pillar's space, we need to transform the min/max range from object space into world space using the pillar's localToWorldMatrix, then we can calculate the claim percentage in the fragment's world space. If you rotate the pillar's on the X or Z axis, you will need to change this calculation as well. See PillarUtils.hlsl for the float4x4 _ClaimLocalToWorld declaration that's included into this shader.;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;99.78853,759.7289;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;151;-101.8378,226.3461;Inherit;False;PillarTopLightEmission;5;;86;7df78c3a334f6e7459c5d679a3618a57;0;3;26;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;61;94.78855,630.7291;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StickyNoteNode;153;-1293.524,228.2074;Inherit;False;321;144;New Note;;1,1,1,1;Interpolate between min and max to determine how high the emission creeps on the pillar.;0;0
Node;AmplifyShaderEditor.StickyNoteNode;154;-436.8585,-343.546;Inherit;False;350.3445;310.1516;New Note;;1,1,1,1;Mask out the albedo/AO if there is any emission.;0;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;602.5522,-5.862426;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;PillarStandard;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;4;d3d11_9x;d3d11;glcore;gles3;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;1;Include;;True;8ecc4fac21c8cde49acb656b5d8cd5d0;Custom;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;125;1;87;0
WireConnection;124;0;123;0
WireConnection;124;1;125;0
WireConnection;143;0;124;0
WireConnection;126;1;88;0
WireConnection;127;0;123;0
WireConnection;127;1;126;0
WireConnection;90;0;143;1
WireConnection;90;1;142;2
WireConnection;90;2;72;0
WireConnection;90;3;80;0
WireConnection;90;4;80;0
WireConnection;86;0;64;0
WireConnection;86;2;81;0
WireConnection;86;3;81;0
WireConnection;86;4;90;0
WireConnection;144;0;127;0
WireConnection;77;0;143;1
WireConnection;77;1;144;1
WireConnection;77;2;64;0
WireConnection;152;12;86;0
WireConnection;112;0;20;0
WireConnection;109;0;112;0
WireConnection;109;1;152;13
WireConnection;58;0;21;0
WireConnection;71;0;77;0
WireConnection;71;1;142;2
WireConnection;71;2;152;0
WireConnection;71;3;152;0
WireConnection;71;4;91;0
WireConnection;117;0;18;0
WireConnection;117;1;152;13
WireConnection;60;0;58;3
WireConnection;60;1;59;0
WireConnection;151;26;109;0
WireConnection;151;1;108;0
WireConnection;151;2;71;0
WireConnection;61;0;58;0
WireConnection;61;1;62;0
WireConnection;0;0;117;0
WireConnection;0;1;108;0
WireConnection;0;2;151;0
WireConnection;0;3;61;0
WireConnection;0;4;60;0
WireConnection;0;5;109;0
ASEEND*/
//CHKSM=3D10539624BC7336F6DE3B91B2C49F22B672327A