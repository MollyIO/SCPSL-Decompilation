Shader "Outline Shaders/Outline 3.1"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _Outline ("Outline width", Range(0, 5)) = 0.005
        _IntensityBase ("Alpha Base", Float) = 1
        _IntensityMax ("Alpha Max", Float) = 0
        _MaxIntensityDist ("Max Alpha Distance", Float) = 30
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        // First pass: draw original object, write depth
        Pass
        {
            Name "BASE"
            Cull Back
            // You can use fixed-function or surface shader here

            CGPROGRAM
            #pragma vertex vertBase
            #pragma fragment fragBase
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; };

            v2f vertBase(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 fragBase(v2f i) : SV_Target
            {
                return fixed4(0,0,0,0); // depth only pass
            }
            ENDCG
        }

        // Outline pass: inverted hull
        Pass
        {
            Name "OUTLINE"
            Cull Front

            CGPROGRAM
            #pragma vertex vertOutline
            #pragma fragment fragOutline
            #include "UnityCG.cginc"

            uniform float _Outline;
            uniform float4 _OutlineColor;
            uniform float _IntensityBase;
            uniform float _IntensityMax;
            uniform float _MaxIntensityDist;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            v2f vertOutline(appdata v)
            {
                v2f o;
                // compute world position
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldPos = worldPos;
                // push along normal
                float3 offset = normalize(mul((float3x3)unity_ObjectToWorld, v.normal)) * _Outline;
                float4 pos = v.vertex + float4(offset, 0);
                o.pos = UnityObjectToClipPos(pos);
                return o;
            }

            fixed4 fragOutline(v2f i) : SV_Target
            {
                // fade alpha based on camera distance
                float dist = distance(i.worldPos, _WorldSpaceCameraPos);
                float alpha = _IntensityBase;
                if (_IntensityMax > _IntensityBase && _MaxIntensityDist > 0)
                {
                    alpha = lerp(_IntensityBase, _IntensityMax, saturate(dist / _MaxIntensityDist));
                }
                return fixed4(_OutlineColor.rgb, _OutlineColor.a * alpha);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}