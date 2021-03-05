// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "PostEffects/HitRadar"
{
	Properties
	{
		_MainTex ( "Screen", 2D ) = "black" {}
		_Tint("Tint", Color) = (1,0,0,0)
		_InnerRadius("Inner Radius", Range( 0 , 1.5)) = 0
		_OuterRadius("Outer Radius", Range( 0 , 1.5)) = 1
		_Right("Right", Range( 0 , 1)) = 0
		_Top("Top", Range( 0 , 1)) = 0
		_Left("Left", Range( 0 , 1)) = 0
		_Bottom("Bottom", Range( 0 , 1)) = 0
		_DistortionStrength("Distortion Strength", Range( 0 , 10)) = 0
		_TintStrength("Tint Strength", Range( 0 , 1)) = 0
		_EmptyForUVCoords("Empty (For UV Coords)", 2D) = "white" {}
		_DirectionMaskBegin("Direction Mask Begin", Float) = 0.5
		_DirectionMaskEnd("Direction Mask End", Float) = 1
		_HitRadarGlass("HitRadarGlass", 2D) = "bump" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	}

	SubShader
	{
		
		
		ZTest Always
		Cull Off
		ZWrite Off
		

		Pass
		{ 
			CGPROGRAM 

			#pragma vertex vert_img_custom 
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"
			

			struct appdata_img_custom
			{
				float4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
				
			};

			struct v2f_img_custom
			{
				float4 pos : SV_POSITION;
				half2 uv   : TEXCOORD0;
				half2 stereoUV : TEXCOORD2;
		#if UNITY_UV_STARTS_AT_TOP
				half4 uv2 : TEXCOORD1;
				half4 stereoUV2 : TEXCOORD3;
		#endif
				
			};

			uniform sampler2D _MainTex;
			uniform half4 _MainTex_TexelSize;
			uniform half4 _MainTex_ST;
			
			uniform sampler2D _HitRadarGlass;
			uniform float4 _HitRadarGlass_ST;
			uniform float _DistortionStrength;
			uniform sampler2D _EmptyForUVCoords;
			uniform float4 _EmptyForUVCoords_ST;
			uniform float _InnerRadius;
			uniform float _OuterRadius;
			uniform float _DirectionMaskBegin;
			uniform float _DirectionMaskEnd;
			uniform float _Right;
			uniform float _Left;
			uniform float _Top;
			uniform float _Bottom;
			uniform float _TintStrength;
			uniform float4 _Tint;

			v2f_img_custom vert_img_custom ( appdata_img_custom v  )
			{
				v2f_img_custom o;
				
				o.pos = UnityObjectToClipPos ( v.vertex );
				o.uv = float4( v.texcoord.xy, 1, 1 );

				#if UNITY_UV_STARTS_AT_TOP
					o.uv2 = float4( v.texcoord.xy, 1, 1 );
					o.stereoUV2 = UnityStereoScreenSpaceUVAdjust ( o.uv2, _MainTex_ST );

					if ( _MainTex_TexelSize.y < 0.0 )
						o.uv.y = 1.0 - o.uv.y;
				#endif
				o.stereoUV = UnityStereoScreenSpaceUVAdjust ( o.uv, _MainTex_ST );
				return o;
			}

			half4 frag ( v2f_img_custom i ) : SV_Target
			{
				#ifdef UNITY_UV_STARTS_AT_TOP
					half2 uv = i.uv2;
					half2 stereoUV = i.stereoUV2;
				#else
					half2 uv = i.uv;
					half2 stereoUV = i.stereoUV;
				#endif	
				
				half4 finalColor;

				// ase common template code
				float2 uv_MainTex = i.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 uv_HitRadarGlass = i.uv.xy * _HitRadarGlass_ST.xy + _HitRadarGlass_ST.zw;
				float2 uv_EmptyForUVCoords = i.uv.xy * _EmptyForUVCoords_ST.xy + _EmptyForUVCoords_ST.zw;
				float2 break173 = uv_EmptyForUVCoords;
				float temp_output_5_0 = ( break173.x + -0.5 );
				float temp_output_4_0 = ( break173.y + -0.5 );
				float clampResult20 = clamp( sqrt( ( ( temp_output_5_0 * temp_output_5_0 ) + ( temp_output_4_0 * temp_output_4_0 ) ) ) , _InnerRadius , _OuterRadius );
				float2 break174 = uv_EmptyForUVCoords;
				float temp_output_163_0 = ( _DirectionMaskBegin + 0 );
				float temp_output_164_0 = ( _DirectionMaskEnd + 0 );
				float clampResult75 = clamp( ( break174.x + 0 ) , temp_output_163_0 , temp_output_164_0 );
				float temp_output_165_0 = ( _DirectionMaskBegin + 0 );
				float temp_output_166_0 = ( _DirectionMaskEnd + 0 );
				float clampResult130 = clamp( ( 1.0 - break174.x ) , temp_output_165_0 , temp_output_166_0 );
				float temp_output_167_0 = ( _DirectionMaskBegin + 0 );
				float temp_output_168_0 = ( _DirectionMaskEnd + 0 );
				float clampResult149 = clamp( ( break174.y + 0 ) , temp_output_167_0 , temp_output_168_0 );
				float temp_output_169_0 = ( _DirectionMaskBegin + 0 );
				float temp_output_170_0 = ( _DirectionMaskEnd + 0 );
				float clampResult148 = clamp( ( 1.0 - break174.y ) , temp_output_169_0 , temp_output_170_0 );
				float clampResult181 = clamp( ( ( (0 + (clampResult75 - temp_output_163_0) * (1 - 0) / (temp_output_164_0 - temp_output_163_0)) * _Right ) + ( (0 + (clampResult130 - temp_output_165_0) * (1 - 0) / (temp_output_166_0 - temp_output_165_0)) * _Left ) + ( (0 + (clampResult149 - temp_output_167_0) * (1 - 0) / (temp_output_168_0 - temp_output_167_0)) * _Top ) + ( (0 + (clampResult148 - temp_output_169_0) * (1 - 0) / (temp_output_170_0 - temp_output_169_0)) * _Bottom ) ) , 0 , 1.0 );
				float temp_output_111_0 = ( ( ( clampResult20 + -_InnerRadius ) / ( -_InnerRadius + _OuterRadius ) ) * clampResult181 );
				float temp_output_44_0 = ( temp_output_111_0 * _TintStrength );
				

				finalColor = ( ( tex2D( _MainTex, ( float3( uv_MainTex ,  0.0 ) + ( UnpackNormal( tex2D( _HitRadarGlass, uv_HitRadarGlass ) ) * _DistortionStrength * temp_output_111_0 ) ).xy ) * ( 1.0 - temp_output_44_0 ) ) + ( _Tint * temp_output_44_0 ) );

				return finalColor;
			} 
			ENDCG 
		}
	}
	CustomEditor "HitRadar"
}
/*ASEBEGIN
Version=15201
1927;29;1906;993;2698.777;1012.885;1.445647;True;True
Node;AmplifyShaderEditor.CommentaryNode;178;-4038.323,-1011.912;Float;False;2034.775;1047.447;Radius Mask;2;10;1;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TexturePropertyNode;60;-4925.518,40.50956;Float;True;Property;_EmptyForUVCoords;Empty (For UV Coords);9;0;Create;True;0;0;False;0;None;None;False;white;Auto;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.CommentaryNode;1;-3837.075,-877.8495;Float;False;1216.238;389.8168;Get Radius (Distance from screen center);8;173;7;4;13;9;6;5;3;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;48;-4640.56,41.06769;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;176;-4044.065,127.3022;Float;False;2164.271;1438.974;Direction Mask;11;111;181;182;110;162;77;141;140;161;174;129;;1,1,1,1;0;0
Node;AmplifyShaderEditor.BreakToComponentsNode;173;-3555.456,-618.104;Float;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RangedFloatNode;3;-3484.862,-747.1533;Float;False;Constant;_Float0;Float 0;0;0;Create;True;0;0;False;0;-0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;140;-3514.391,1227.613;Float;False;1088.171;314.7532;Bottom Mask;7;151;148;169;170;143;154;152;;1,1,1,1;0;0
Node;AmplifyShaderEditor.BreakToComponentsNode;174;-4007.409,800.1411;Float;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.CommentaryNode;141;-3508.856,883.1932;Float;False;1085.914;328.1376;Top Mask;7;153;149;168;167;145;155;150;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;161;-3985.125,708.743;Float;False;Property;_DirectionMaskEnd;Direction Mask End;11;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;5;-3226.97,-762.3701;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;162;-3997.585,631.9091;Float;False;Property;_DirectionMaskBegin;Direction Mask Begin;10;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;129;-3503.085,519.0985;Float;False;1080.483;333.825;Left Mask;7;131;130;134;165;166;133;132;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;77;-3504.613,186.5661;Float;False;1083.55;325.2699;Right Mask;7;76;75;164;139;163;95;94;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;4;-3222.97,-654.3702;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;166;-3477.986,740.7563;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;145;-3488.212,934.0519;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;134;-3478.222,577.4921;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;139;-3483.969,237.4248;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;164;-3480.279,415.7314;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;169;-3486.671,1354.952;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;143;-3489.528,1286.006;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;165;-3477.988,651.7887;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;170;-3486.67,1443.919;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;167;-3489.111,1026.125;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;168;-3489.11,1115.092;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-3088.97,-672.3702;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-3086.97,-784.3701;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;163;-3480.28,326.7637;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;148;-3295.791,1280.05;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;75;-3286.013,239.0031;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;130;-3284.485,571.5358;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;9;-2919.661,-729.8046;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;10;-2976.846,-466.1756;Float;False;927.7859;450.5469;Radius Mask;7;30;24;28;22;20;11;14;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ClampOpNode;149;-3290.256,935.6303;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;94;-2937.023,396.217;Float;False;Property;_Right;Right;3;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;152;-2946.801,1437.264;Float;False;Property;_Bottom;Bottom;6;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;132;-2935.495,728.7499;Float;False;Property;_Left;Left;5;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;150;-2941.266,1092.845;Float;False;Property;_Top;Top;4;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;151;-3125.727,1351.631;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;131;-3114.421,643.1166;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-2926.479,-343.3477;Float;False;Property;_InnerRadius;Inner Radius;1;0;Create;True;0;0;False;0;0;0.35;0;1.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;76;-3115.949,310.5838;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SqrtOpNode;13;-2772.387,-723.1523;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-2929.521,-237.2507;Float;False;Property;_OuterRadius;Outer Radius;2;0;Create;True;0;0;False;0;1;1.028168;0;1.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;153;-3120.192,1007.211;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;20;-2565.833,-365.6013;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;155;-2641.479,988.4119;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;133;-2635.708,624.3171;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;22;-2557.314,-241.2231;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;95;-2637.236,291.7843;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;154;-2647.014,1332.831;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;28;-2346.326,-151.0027;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;24;-2353.237,-301.0762;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;110;-2348.314,764.2916;Float;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;182;-2365.52,921.062;Float;False;Constant;_One;One;13;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;29;-1877.514,-560.4952;Float;False;765.9012;530.716;Distortion;6;35;41;36;38;32;187;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;30;-2201.359,-230.0755;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;181;-2208.548,821.7531;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;32;-1783.24,-504.5444;Float;True;0;0;_MainTex;Shader;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;187;-1842.954,-311.746;Float;True;Property;_HitRadarGlass;HitRadarGlass;12;0;Create;True;0;0;False;0;b80a8076d71d0ca4283831ac8bddada3;b80a8076d71d0ca4283831ac8bddada3;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;39;-874.7963,247.3015;Float;False;967.4266;514.1914;Tint;7;46;160;159;158;44;40;43;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;111;-2046.139,824.0884;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;35;-1824.485,-118.1464;Float;False;Property;_DistortionStrength;Distortion Strength;7;0;Create;True;0;0;False;0;0;0;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;37;-1050.848,-556.6316;Float;False;659.5901;495.488;Previous Image;2;45;42;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;40;-864.3949,674.8831;Float;False;Property;_TintStrength;Tint Strength;8;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;-1452.742,-199.1938;Float;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;36;-1528.06,-430.46;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;44;-630.239,479.0331;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;41;-1234.564,-215.3744;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;42;-1029.773,-509.7443;Float;True;0;0;_MainTex;Shader;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;45;-701.9449,-366.4076;Float;True;Property;_TextureSample1;Texture Sample 1;0;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;158;-465.3703,326.7486;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;43;-847.3395,294.0302;Float;False;Property;_Tint;Tint;0;0;Create;True;0;0;False;0;1,0,0,0;1,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;160;-297.853,295.3077;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;159;-450.4077,411.3818;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;46;-135.8149,382.842;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;156.5954,140.4618;Float;False;True;2;Float;HitRadar;0;1;PostEffects/HitRadar;c71b220b631b6344493ea3cf87110c93;0;0;SubShader 0 Pass 0;1;False;False;True;Off;False;False;True;2;True;7;False;True;0;False;0;0;0;False;False;False;False;False;False;False;False;False;True;2;0;0;0;1;0;FLOAT4;0,0,0,0;False;0
WireConnection;48;2;60;0
WireConnection;173;0;48;0
WireConnection;174;0;48;0
WireConnection;5;0;173;0
WireConnection;5;1;3;0
WireConnection;4;0;173;1
WireConnection;4;1;3;0
WireConnection;166;0;161;0
WireConnection;145;0;174;1
WireConnection;134;0;174;0
WireConnection;139;0;174;0
WireConnection;164;0;161;0
WireConnection;169;0;162;0
WireConnection;143;0;174;1
WireConnection;165;0;162;0
WireConnection;170;0;161;0
WireConnection;167;0;162;0
WireConnection;168;0;161;0
WireConnection;7;0;4;0
WireConnection;7;1;4;0
WireConnection;6;0;5;0
WireConnection;6;1;5;0
WireConnection;163;0;162;0
WireConnection;148;0;143;0
WireConnection;148;1;169;0
WireConnection;148;2;170;0
WireConnection;75;0;139;0
WireConnection;75;1;163;0
WireConnection;75;2;164;0
WireConnection;130;0;134;0
WireConnection;130;1;165;0
WireConnection;130;2;166;0
WireConnection;9;0;6;0
WireConnection;9;1;7;0
WireConnection;149;0;145;0
WireConnection;149;1;167;0
WireConnection;149;2;168;0
WireConnection;151;0;148;0
WireConnection;151;1;169;0
WireConnection;151;2;170;0
WireConnection;131;0;130;0
WireConnection;131;1;165;0
WireConnection;131;2;166;0
WireConnection;76;0;75;0
WireConnection;76;1;163;0
WireConnection;76;2;164;0
WireConnection;13;0;9;0
WireConnection;153;0;149;0
WireConnection;153;1;167;0
WireConnection;153;2;168;0
WireConnection;20;0;13;0
WireConnection;20;1;11;0
WireConnection;20;2;14;0
WireConnection;155;0;153;0
WireConnection;155;1;150;0
WireConnection;133;0;131;0
WireConnection;133;1;132;0
WireConnection;22;0;11;0
WireConnection;95;0;76;0
WireConnection;95;1;94;0
WireConnection;154;0;151;0
WireConnection;154;1;152;0
WireConnection;28;0;22;0
WireConnection;28;1;14;0
WireConnection;24;0;20;0
WireConnection;24;1;22;0
WireConnection;110;0;95;0
WireConnection;110;1;133;0
WireConnection;110;2;155;0
WireConnection;110;3;154;0
WireConnection;30;0;24;0
WireConnection;30;1;28;0
WireConnection;181;0;110;0
WireConnection;181;2;182;0
WireConnection;111;0;30;0
WireConnection;111;1;181;0
WireConnection;38;0;187;0
WireConnection;38;1;35;0
WireConnection;38;2;111;0
WireConnection;36;2;32;0
WireConnection;44;0;111;0
WireConnection;44;1;40;0
WireConnection;41;0;36;0
WireConnection;41;1;38;0
WireConnection;45;0;42;0
WireConnection;45;1;41;0
WireConnection;158;0;44;0
WireConnection;160;0;45;0
WireConnection;160;1;158;0
WireConnection;159;0;43;0
WireConnection;159;1;44;0
WireConnection;46;0;160;0
WireConnection;46;1;159;0
WireConnection;0;0;46;0
ASEEND*/
//CHKSM=511DFF64F44BB8D5FBB250C9C7B241DC7EBB79F2