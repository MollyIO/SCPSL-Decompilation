// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Vashchuk/RedDot(Standard)" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGBA)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		[Space]
		_RedDotColor ("Red Dot Color(RGB) Brightness(A)", Color) = (1,1,1,1)
		_RedDotTex ("Red Dot Texture (A)", 2D) = "white" {}
		_RedDotSize ("Red Dot size", Range(0,10)) = 0.0
		[Toggle(FIXED_SIZE)] _FixedSize ("Use Fixed Size", Float) = 0
		_RedDotDist ("Red Dot offset distance", Range(0,50)) = 2.0
		_OffsetX ("Side Offset", Float) = 0.0
		_OffsetY ("Height Offset", Float) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="Transparent"}
		LOD 200
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert alpha:blend

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		#pragma shader_feature FIXED_SIZE

		sampler2D _MainTex;
		sampler2D _RedDotTex;

		struct Input {
			float2 uv_MainTex;
			float2 RedDotUV;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		fixed4 _RedDotColor;
		fixed _RedDotSize;
		fixed _RedDotDist;
		fixed _OffsetX;
		fixed _OffsetY;

		void vert( inout appdata_full v, out Input o ) {
			UNITY_INITIALIZE_OUTPUT(Input,o);
			fixed3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			fixed3 viewDir = _WorldSpaceCameraPos - worldPos;
			
			#if defined(FIXED_SIZE)
				fixed3 objectCenter = mul(unity_ObjectToWorld, fixed4(0,0,0,1));
				fixed dist = length(objectCenter - _WorldSpaceCameraPos);
				_RedDotSize *= dist;
			#endif

			o.RedDotUV = v.vertex.xy - fixed2(_OffsetX, _OffsetY); //+ fixed2(0.5, 0.5);
			o.RedDotUV -= mul(unity_WorldToObject, viewDir).xy * _RedDotDist;
			o.RedDotUV /= _RedDotSize;
			o.RedDotUV += fixed2(0.5, 0.5);
		}

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			fixed redDot = tex2D (_RedDotTex, IN.RedDotUV).a;
			o.Emission = redDot * _RedDotColor.rgb * _RedDotColor.a;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a + redDot * _RedDotColor.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}