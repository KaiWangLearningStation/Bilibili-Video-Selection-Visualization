Shader "NodeShader"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _EdgeColor ("Edge Color", Color) = (0,0,0,1)
        _EdgeThickness ("Edge Thickness", Range(0.1, 10)) = 1.0
        _Transparency ("Transparency", Range(0.0, 1.0)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            Lighting Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
            };

            fixed4 _Color;
            fixed4 _EdgeColor;
            float _EdgeThickness;
            float _Transparency;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = normalize(v.normal);
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate the facing ratio
                float facing = dot(i.normal, i.viewDir);
                
                // Calculate edge intensity
                float edge = pow(1.0 - abs(facing), _EdgeThickness);

                // Mix the edge color with the main color
                fixed4 color = lerp(_Color, _EdgeColor, edge);

                // Apply transparency
                color.a *= _Transparency;

                return color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
