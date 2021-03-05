Shader "Unlit/RopeTess"
{
	Properties
	{
		_PlasmaMap1 ("Texture", 2D) = "white" {}
		_PlasmaMap2 ("Texture", 2D) = "white" {}
		_ropeWidth( "ropeWidth" , Float ) = 1 
		_ropeHullWidth("hullWidth" , Float ) = 1 
		_Clamps ( "clamps" , Vector ) = ( 0,1,0,1 ) 
		_TintColor("tint color" , Color ) = ( 1,1,1,1 )
		_BlendTexFactor("blend tex factor" , Range(0,1) ) = 0
		_sheenColor ( "sheen color ( outer hull, ignores alpha))" , Color ) = ( 0.2 , 0.2 , 0.2 , 0.5 ) 
		_fadeInSlope ( "hull fade in slope" , Range(0,2 ) )  = 0 
		
		// ------------------------
		
		_plasmaMapVelocities ( "_plasmaMapVelocities" , Vector) = (0,0,0,0) 
		
		// -----------------------
		_ts      ("ramp style interval values for color lookup" , Vector ) = ( 0 , 0.2 , 0.5 , 1 ) 
		_S1      ("Spline1", Vector) = (1,1,1,1)
		_S2      ("Spline2", Vector) = (1,1,1,1)
		_S3      ("Spline3", Vector) = (1,1,1,1)
		_S4      ("Spline4", Vector) = (1,1,1,1)
		_S5      ("Spline5", Vector) = (1,1,1,1)
		
		// --------------------------
		
		_ts_plasma_ramp ("plasma ramp keyframes" , Vector ) = (0,0,0,0)
		_S1_pl      ("Spline1_pl", Vector) = (1,1,1,1)
		_S2_pl      ("Spline2_pl", Vector) = (1,1,1,1)
		_S3_pl      ("Spline3_pl", Vector) = (1,1,1,1)
		_S4_pl      ("Spline4_pl", Vector) = (1,1,1,1)
		_S5_pl      ("Spline5_pl", Vector) = (1,1,1,1)
		
		// ----------------------------
		
		_ts_time_ramp ( "_ts_time_ramp" , Vector ) = (0,0,0,0 )
		
		_S1_time ( "_S1_time" , Vector ) = ( 0,0,0,0)
		_S2_time ( "_S2_time" , Vector ) = ( 0,0,0,0)
		_S3_time ( "_S3_time" , Vector ) = ( 0,0,0,0)
		_S4_time ( "_S4_time" , Vector ) = ( 0,0,0,0)
		_S5_time ( "_S5_time" , Vector ) = ( 0,0,0,0)
		
		

	}
		SubShader
		{
			
			// additive blend 
			Tags { "RenderType" = "Transparent"  "Queue" = "Transparent" }
			//Blend SrcAlpha One
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZWrite Off
			// ----
			
			LOD 100
			
			

		Pass
		{
			
			CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma hull hull_F 
			#pragma domain domain_F_simplistic
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"
			
			#pragma shader_feature _QUAT_DEBUG
			
			sampler2D _PlasmaMap1;
			sampler2D _PlasmaMap2;
		// Micha: add Tiling/Offset for both plasma maps	
			float4 _PlasmaMap1_ST;
			float4 _PlasmaMap2_ST;
		//

			float _ropeWidth; 
			float4 _Clamps;
			float4 _TintColor;
			float  _BlendTexFactor;
			
			
			float4 _ts_plasma_ramp;
			float4 _S1_pl;
			float4 _S2_pl;
			float4 _S3_pl;
			float4 _S4_pl;
			float4 _S5_pl;
			
			float4 _plasmaMapVelocities;
			
			float4 _ts_time_ramp;
			float4 _S1_time;
			float4 _S2_time;
			float4 _S3_time;
			float4 _S4_time;
			float4 _S5_time;
			

			struct appdata
			{
				float4 vertex : POSITION    ;
				float4 tan    : TANGENT ; 
			};
			
			
			
			// fallback method
			// significantly slower , but since unity's SetVector4Array() semantics are kind of ... excentric
			float4 select_spline_IndividualVars_time ( float t ) {
				float3 in_lower_bound = t.xxx >= _ts_time_ramp.xyz; 
				float3 in_upper_bound = t.xxx <  _ts_time_ramp.yzw; 
				float3 in_interval = in_lower_bound * in_upper_bound;
				return 
				_S1_time * (t<_ts_time_ramp.x) + 
				_S2_time * in_interval.x + 
				_S3_time * in_interval.y + 
				_S4_time * in_interval.z + 
				_S5_time * (t >= _ts_time_ramp.w) ; 
				
			}

			struct v2hull
			{

				float4 P : SV_POSITION ;
				float4 VPOS : TEXCOORD0;
				float4 tan : TANGENT;
				float  slurp_time : TEXCOORD1;
			};

			
		
			v2hull vert (appdata v)
			{
				float t = _BlendTexFactor;  // <- [0,1] claimValue comes in here 
				float t2 = t*t;
				float4 t_arg = float4( t2 * t , t2 , t , 1 ) ;
				float4 spline = select_spline_IndividualVars_time ( t ) ; 
				float slurp_time = dot ( spline , t_arg ) ; 
				
				v2hull o = (v2hull) 0;
				o.P = v.vertex;
				o.tan = v.tan;
				o.VPOS = UnityWorldToClipPos( v.vertex.xyz ) ; 
				o.slurp_time = slurp_time;
				
				//UNITY_TRANSFER_FOG(o,o.vertex)           ;
				return o;
			}
			
			float3 Bezier( 
				float3 CP0, float3 CP1 , float3 CP2 , float3 CP3,
				float t ) 
			{
				float nt = 1-t;
				float t2 = t*t;
				float t3 = t2*t;
				float nt2 = nt * nt;
				float nt3 = nt2 * nt;

				CP1 *= 3;
				CP2 *= 3;

				return  CP0 * nt3 + CP1 * nt2 *t + CP2 * nt * t2 + CP3 * t3;
			}
			
			
			
			struct hsconst { 
				float edges [4] : SV_TessFactor;
				float inside[2] : SV_InsideTessFactor;
			};

			hsconst patchconst_F ( 
				InputPatch<v2hull,4> patch ,
				uint PatchID : SV_PrimitiveID
				)
			{
				hsconst O;
				
//					controlPoints    come in as Quad like this : 
//				    A1------B1           P1 - B1 
//				    |       |                  |
//				 	P0      P1           P0 - A1 
//
//				 	edges ?            
//				 	                  e3
//				 	             e0       e2
//				 	                  e1 
//
//				 	                  ( nach rumexperimentieren )
//
//					i guess - documentation would be nice 
//				
				// tessX goes along the spline 
				// tessY is along the circular part 
				float ty_min = 6 ;
				float ty_max = 20 ; 
				float tx_min = 8;
				float tx_max = 30;
				float z_min  = 1 ; 
				float z_max  = 3 ; 
				
				float viewZ , zlf;
				
				viewZ   = patch[0].VPOS.w ; // projection matrix application transports z dist from cam to .w 
				zlf     = saturate (  (abs(viewZ) - z_min) / ( z_max - z_min ) ) ; // z lerp factor 
				
				float tY0 = lerp ( ty_max , ty_min, zlf ) ; 
				float tX  = lerp ( tx_max , tx_min, zlf ) ;
				
				viewZ   = patch[3].VPOS.w ; 
				zlf     = saturate (  (abs(viewZ) - z_min) / ( z_max - z_min ) ) ; // z lerp factor 
				
				float tY1 = lerp ( ty_max , ty_min , zlf ) ; 
				
				O.edges[0]  = tY0;
				O.edges[1]  = tX;
				O.edges[2]  = tY1;
				O.edges[3]  = tX;
				O.inside[0] = tX ;
				O.inside[1] = max( tY0 , tY1 )  ;
				return O;
			}
			
			struct hull2domain {
				float4 P:SV_POSITION;
				float4 VPOS:TEXCOORD0;
				float4 tan   : TANGENT;
				float slurp_time : TEXCOORD1;
			};
			
			[domain("quad")]            
			[partitioning("fractional_even")]
			[outputtopology("triangle_ccw")]
			[outputcontrolpoints(4)]
			[patchconstantfunc("patchconst_F" ) ]
			
			hull2domain hull_F( 
				InputPatch<v2hull,4> patch,
				uint                 i        : SV_OutputControlPointID,
				uint                 PatchID  : SV_PrimitiveID
				)
			{
				hull2domain O  = (hull2domain) 0;
				O.P    = patch[i].P;
				O.tan  = patch[i].tan; 
				O.VPOS = patch[i].VPOS;
				O.slurp_time = patch[i].slurp_time;

				return  O;
			}
			
			
			
			
			void quad2base( float4 q , out float3 e_x , out float3 e_y ) {
				float  s = q.w; // from observation: Quaternion.identity.w == 1 
				float3 v = q.xyz;
				
				float3 e , m ;
				
				e = float3(1 ,0,0 ) ; 
				m = (s * e) - cross( e, v )  ; // dummy for $( was auch immer zwischenergenis im englischen ist ) 
				
				e_x = cross ( v , m ) + s * m +  v * dot ( e , v ) ; 
				
				e = float3(0,1,0 ) ; 
				m = (s * e) - cross( e, v )  ; // dummy for $( was auch immer zwischenergenis im englischen ist ) 
				
				e_y = cross ( v , m ) + s * m +  v * dot ( e , v ) ; 
			}
			
			
			// u -> ( i1 , i2 , k ) // index of quaternion1 quaternion2 and interpolation factor between them 
			float3 patch_selection ( float u ) {
				float u3 = u * 3 ; 
				float i1 = floor ( u3 ) ; 
				float k  = u3 % 1 ; 
				if ( i1 >= 3 ) { i1 = 2 ; k = 1 ; } 
				return float3 ( i1 , i1 +1 , k ) ;
			}
			
			struct domain2frag { 
				float4 P       : SV_POSITION;
				float2 UV      : TEXCOORD0;
				float  patch_i : TEXCOORD1;
				float  slurp_time : TEXCOORD2;
			};
			
			float4 x_lerp ( float4 q1 , float4 q2 , float t ) {
				float4 q_res = ( 1-t ) * q1 + t * q2 ; 
				 
				return normalize ( q_res ) ; 
			}

			[domain("quad")]
			domain2frag domain_F_simplistic ( 
				hsconst const_in,
				float2 UV : SV_DomainLocation,
				const OutputPatch<hull2domain,4> patch
				)
			{
				domain2frag O = (domain2frag) 0;
				
				float3 p_spline = Bezier( patch[0].P , patch[1].P , patch[2].P , patch[3].P , UV.x );
				float3 sel = patch_selection ( UV.x ) ; 

				//#define _QUAT_DEBUG 1 
	
				float3 ex , ey ; 
				
				#if _QUAT_DEBUG
				// --------------------------------------------
				// dummy shit to visualize individual quaternions passed in 
				
				int i = floor ( UV.x * 3.99 )  ;
				
				
				float4 q = patch[i].tan;
				quad2base ( q , ex , ey ); 
				
				float3 p_offs = ex * UV.y  ;

				#else 

				// --------------------------------------------

				float4 q1 = patch[sel.x].tan ; 
				float4 q2 = patch[sel.y].tan ;
				
				float4 q = x_lerp ( q1 , q2 , sel.z ) ;
				
				quad2base( q , ex , ey ) ; 
				float ang = UV.y * 2 * UNITY_PI;
				float3 p_offs = (ex * cos ( ang ) +  ey * sin( ang ) ) * _ropeWidth   ; 
				
				#endif 
				
				
				O.P = float4(p_spline,1);
				O.P += float4(p_offs , 0 ) ; 
		
				O.P = UnityWorldToClipPos(O.P);
				O.UV = UV;
				O.patch_i = sel.x ; 
				O.slurp_time = patch[0].slurp_time;
				return O; 
			}
			/*
			fixed4 simplistic_frag ( domain2frag i ) {
				fixed4 col = tex2D(_MainTex, i.UV);
				UNITY_APPLY_FOG(i.fogCoord, col);
				// return fixed4( i.patch_i % 0.9 , (i.patch_i +1 ) % 0.9 ,i.UV.y,0.6); // dummy visualization for quaternion bases per control point 
				return fixed4 ( col.rgb , 0.5 ) ; 
			}
			*/
			
			//  float4 _Time; // (t/20, t, t*2, t*3)
			//  float4 _SinTime; // sin(t/8), sin(t/4), sin(t/2), sin(t)

			float2 uv_warp ( float2 uv_in , float2 offset  ) {
				float2 r =   uv_in + _Time.y * offset ;
				//r.y += 0.1 * _SinTime.z ; 
				return r ; 
			}
			float extract ( float x , float a , float b ) {
				return (x - a ) / ( b -a ) ; 
			}
			
			// fallback method
			// significantly slower , but since unity's SetVector4Array() semantics are kind of ... excentric
			float4 select_spline_IndividualVars_pl ( float t ) {
				float3 in_lower_bound = t.xxx >= _ts_plasma_ramp.xyz; 
				float3 in_upper_bound = t.xxx <  _ts_plasma_ramp.yzw; 
				float3 in_interval = in_lower_bound * in_upper_bound;
				return 
				_S1_pl * (t<_ts_plasma_ramp.x) + 
				_S2_pl * in_interval.x + 
				_S3_pl * in_interval.y + 
				_S4_pl * in_interval.z + 
				_S5_pl * (t >= _ts_plasma_ramp.w) ; 
				
			}
			
			// i-wie noch primitive index durchziehen  fuer scaling entlang UV.x ohne tiling artefakte - sind sichtbar, aber sehr schwach 
			fixed4 frag (domain2frag i) : SV_Target
			{
				/* original Till
				float grey = tex2D(_PlasmaMap1, uv_warp ( i.UV , _plasmaMapVelocities.xy ) ).a ;
				grey += tex2D( _PlasmaMap2 , uv_warp ( i.UV , _plasmaMapVelocities.zw ) ).x ; */

			// Micha: add Tiling/Offset for both plasma maps
				float2 uv_PM1 = TRANSFORM_TEX(i.UV, _PlasmaMap1);
				float2 uv_PM2 = TRANSFORM_TEX(i.UV, _PlasmaMap2);

				float grey = tex2D(_PlasmaMap1, uv_warp(uv_PM1, _plasmaMapVelocities.xy)).a;
				grey += tex2D(_PlasmaMap2, uv_warp(uv_PM2, _plasmaMapVelocities.zw)).x;
			//

				float e1  = extract ( grey , _Clamps.x , _Clamps.y ) ; 
				float e2  = extract ( grey , _Clamps.z , _Clamps.w ) ; 
				
				float alpha = lerp ( e1 , e2 , i.slurp_time );
				float al2 = alpha * alpha ;
				
				float4 arg = float4 ( al2 * alpha , al2 , alpha , 1 );
				
				float4 spline = select_spline_IndividualVars_pl ( alpha ); 
				float spline_res = dot ( arg , spline ) ; 
				

				return fixed4( _TintColor.rgb , spline_res ) ; 
				
			}
			ENDCG
		}
		
		
				Pass
		{
			
			// pass fuer hull 
			// eigentlich muss diese Berechnung gar nicht verdoppelt werden
			// man koennte das "extruden" 2er faces auch ueber einen geometry shader erledigen 
			// fragment muss dann allerdings einen bedingten sprung haben - das koennte unterm strich sogar langsamer sein ... 
			CGPROGRAM
			
			#pragma target 5.0
			
			#pragma vertex vert
			#pragma fragment frag_outer_hull
			#pragma hull hull_F 
			#pragma domain domain_F_simplistic
			// make fog work
			
			
			
			#include "UnityCG.cginc"
			
			#pragma shader_feature _QUAT_DEBUG
			
			
			
			float _ropeWidth; 
			float4 _Clamps;
			float4 _TintColor;
			float  _BlendTexFactor;
			float _ropeHullWidth;
			float4 _sheenColor;
			float _fadeInSlope;
			

			struct appdata
			{
				float4 vertex : POSITION  ;
				float4 tan    : TANGENT ; 
				float2 uv_dummy : TEXCOORD0; 
				
			};

			struct v2hull
			{
				UNITY_FOG_COORDS(1)
				float4 P : SV_POSITION ;
				float4 VPOS : TEXCOORD0;
				float4 tan : TANGENT;
				float2 uv_dummy : TEXCOORD1;
				
			};

			
		
			v2hull vert (appdata v)
			{
				v2hull o;
				o.P = v.vertex;
				o.tan = v.tan;
				o.VPOS = UnityWorldToClipPos( v.vertex.xyz ) ; 
				o.uv_dummy = v.uv_dummy;
				
				return o;
			}
			
			float3 Bezier( 
				float3 CP0, float3 CP1 , float3 CP2 , float3 CP3,
				float t ) 
			{
				float nt = 1-t;
				float t2 = t*t;
				float t3 = t2*t;
				float nt2 = nt * nt;
				float nt3 = nt2 * nt;

				CP1 *= 3;
				CP2 *= 3;

				return  CP0 * nt3 + CP1 * nt2 *t + CP2 * nt * t2 + CP3 * t3;
			}
			
			
			
			struct hsconst { 
				float edges [4] : SV_TessFactor;
				float inside[2] : SV_InsideTessFactor;
			};

			hsconst patchconst_F ( 
				InputPatch<v2hull,4> patch ,
				uint PatchID : SV_PrimitiveID
				)
			{
				hsconst O;
				
//					controlPoints    come in as Quad like this : 
//				    A1------B1           P1 - B1 
//				    |       |                  |
//				 	P0      P1           P0 - A1 
//
//				 	edges ?            
//				 	                  e3
//				 	             e0       e2
//				 	                  e1 
//
//				 	                  ( nach rumexperimentieren )
//
//					i guess - documentation would be nice 
//				
				// tessX goes along the spline 
				// tessY is along the circular part 
				float ty_min = 6 ;
				float ty_max = 20 ; 
				float tx_min = 8;
				float tx_max = 30;
				float z_min  = 1 ; 
				float z_max  = 3 ; 
				
				float viewZ , zlf;
				
				viewZ   = patch[0].VPOS.w ; // projection matrix application transports z dist from cam to .w 
				zlf     = saturate (  (abs(viewZ) - z_min) / ( z_max - z_min ) ) ; // z lerp factor 
				
				float tY0 = lerp ( ty_max , ty_min, zlf ) ; 
				float tX  = lerp ( tx_max , tx_min, zlf ) ;
				
				viewZ   = patch[3].VPOS.w ; 
				zlf     = saturate (  (abs(viewZ) - z_min) / ( z_max - z_min ) ) ; // z lerp factor 
				
				float tY1 = lerp ( ty_max , ty_min , zlf ) ; 
				
				O.edges[0]  = tY0;
				O.edges[1]  = tX;
				O.edges[2]  = tY1;
				O.edges[3]  = tX;
				O.inside[0] = tX ;
				O.inside[1] = max( tY0 , tY1 )  ;
				return O;
			}
			
			struct hull2domain {
				float4 P:SV_POSITION;
				float4 VPOS:TEXCOORD0;
				float4 tan   : TANGENT;
				float2 uv_dummy: TEXCOORD1;
			};
			
			[domain("quad")]            
			[partitioning("fractional_even")]
			[outputtopology("triangle_ccw")]
			[outputcontrolpoints(4)]
			[patchconstantfunc("patchconst_F" ) ]
			
			hull2domain hull_F( 
				InputPatch<v2hull,4> patch,
				uint                 i        : SV_OutputControlPointID,
				uint                 PatchID  : SV_PrimitiveID
				)
			{
				hull2domain O  = (hull2domain) 0;
				O.P    = patch[i].P;
				O.tan  = patch[i].tan; 
				O.VPOS = patch[i].VPOS;
				O.uv_dummy = patch[i].uv_dummy;

				return  O;
			}
			
			
			
			
			void quad2base( float4 q , out float3 e_x , out float3 e_y ) {
				float  s = q.w; // from observation: Quaternion.identity.w == 1 
				float3 v = q.xyz;
				
				float3 e , m ;
				
				e = float3(1 ,0,0 ) ; 
				m = (s * e) - cross( e, v )  ; // dummy for $( was auch immer zwischenergenis im englischen ist ) 
				
				e_x = cross ( v , m ) + s * m +  v * dot ( e , v ) ; 
				
				e = float3(0,1,0 ) ; 
				m = (s * e) - cross( e, v )  ; // dummy for $( was auch immer zwischenergenis im englischen ist ) 
				
				e_y = cross ( v , m ) + s * m +  v * dot ( e , v ) ; 
			}
			
			
			// u -> ( i1 , i2 , k ) // index of quaternion1 quaternion2 and interpolation factor between them 
			float3 patch_selection ( float u ) {
				float u3 = u * 3 ; 
				float i1 = floor ( u3 ) ; 
				float k  = u3 % 1 ; 
				if ( i1 >= 3 ) { i1 = 2 ; k = 1 ; } 
				return float3 ( i1 , i1 +1 , k ) ;
			}
			
			struct domain2frag { 
				float4 P       : SV_POSITION;
				float3 UV      : TEXCOORD0;
				float3 norm    : NORMAL;
				float3 eyeVec  : TEXCOORD1;
				
			};
			
			float4 x_lerp ( float4 q1 , float4 q2 , float t ) {
				float4 q_res = ( 1-t ) * q1 + t * q2 ; 
				 
				return normalize ( q_res ) ; 
			}

			[domain("quad")]
			domain2frag domain_F_simplistic ( 
				hsconst const_in,
				float2 UV : SV_DomainLocation,
				const OutputPatch<hull2domain,4> patch
				)
			{
				domain2frag O = (domain2frag) 0;
				
				float3 p_spline = Bezier( patch[0].P , patch[1].P , patch[2].P , patch[3].P , UV.x );
				float3 sel = patch_selection ( UV.x ) ; 

				//#define _QUAT_DEBUG 1 
	
				float3 ex , ey ; 
				float3 extrude_normal = (float3) 0; 
				float3 WorldP = (float3)0;
				
				#if _QUAT_DEBUG
				// --------------------------------------------
				// dummy shit to visualize individual quaternions passed in 
				
				int i = floor ( UV.x * 3.99 )  ;
				
				
				float4 q = patch[i].tan;
				quad2base ( q , ex , ey ); 
				
				float3 p_offs = ex * UV.y  ;

				#else 

				// --------------------------------------------

				float4 q1 = patch[sel.x].tan ; 
				float4 q2 = patch[sel.y].tan ;
				
				float4 q = x_lerp ( q1 , q2 , sel.z ) ;
				
				quad2base( q , ex , ey ) ; 
				float ang = UV.y * 2 * UNITY_PI;
				extrude_normal = ex * cos ( ang ) +  ey * sin( ang ); 
				float3 p_offs = extrude_normal * _ropeHullWidth ; 
				WorldP = p_spline + p_offs;
				#endif 
				
				O.norm = extrude_normal;
				O.P = UnityWorldToClipPos(float4(WorldP,1));
				
				O.eyeVec = normalize ( _WorldSpaceCameraPos -  WorldP) ;
				
				O.UV = float3 ( UV , 0 ) ;
				O.UV.z = patch[0].uv_dummy;
				
				
				return O; 
			}
			
			//  float4 _Time; // (t/20, t, t*2, t*3)
			//  float4 _SinTime; // sin(t/8), sin(t/4), sin(t/2), sin(t)

			float2 uv_warp ( float2 uv_in , float2 offset  ) {
				float2 r =   uv_in + _Time.y * offset ;
				//r.y += 0.1 * _SinTime.z ; 
				return r ; 
			}
			
			float4 _Splines[5];
			float4 _ts;
			
			float4 _S1 , _S2 , _S3 , _S4 , _S5 ; 
			
			float4 select_spline_Arr( float t ) {
				int i = dot ( t.xxxx >= _ts , (1).xxxx ) ;
				return _Splines[i];
			}
			
			// fallback method
			// significantly slower , but since unity's SetVector4Array() semantics are kind of ... excentric
			float4 select_spline_IndividualVars ( float t ) {
				float3 in_lower_bound = t.xxx >= _ts.xyz; 
				float3 in_upper_bound = t.xxx <  _ts.yzw; 
				float3 in_interval = in_lower_bound * in_upper_bound;
				return 
				_S1 * (t<_ts.x) + 
				_S2 * in_interval.x + 
				_S3 * in_interval.y + 
				_S4 * in_interval.z + 
				_S5 * (t >= _ts.w) ; 
				
			}
			
			
			float4 frag_outer_hull (domain2frag i) : SV_Target
			{
				// cos of eye-ray incidence ( if the tesselation is high enough, normalization could be left out ) 
				float c = dot ( normalize( i.norm ) , normalize( i.eyeVec ) ); 
				c = abs(c); // have the backfaces mirror the behaviour of front facing ones 
				
				float4 spline ;
				spline = select_spline_IndividualVars(c);
				
				float c2 = c*c;
				float4 T = float4( c*c2 , c2 , c , 1 ); 
				float val = dot( spline , T); 
				
				
				float fade_in_fac = saturate ( (1 - i.UV.x) + i.UV.z);
				float sloped_fac = fade_in_fac * fade_in_fac * ( 1 - _fadeInSlope ) + fade_in_fac * _fadeInSlope ; // <- breddy expensive , maybe do this in domain_F 
				
				// original :
				return float4 ( _sheenColor.rgb , val * sloped_fac  ) ; 
				//return float4 ( _sheenColor.rgb , min ( val ,  sloped_fac )   ) ; // <- micha's variante 
				
				//return float4( 1 ,0,0,fade_in_fac); 
				
				// debug view normal and eyeVec
				// !!!! make sure to use a PERSPECTIVE camera -- (CamPpos - WorldPos) = eyeRay is not true for orthographic projection 
				/*
				return  lerp ( 
					float4(1,0,0,1), // red for grasing angles - cos = 0 
					float4(0,1,0,1), // green for surface is normal to viewer 
					c ); 
					*/
					
				/*	
				return  lerp ( 
					float4(1,0,0,1), // red for grasing angles - cos = 0 
					float4(0,1,0,1), // green for surface is normal to viewer 
					val );           // see the spline output directly 
					*/
					
					

			}
			ENDCG
		}
		
		
	}
}
