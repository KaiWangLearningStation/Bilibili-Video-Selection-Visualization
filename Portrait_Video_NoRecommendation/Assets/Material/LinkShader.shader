Shader "Custom/LinkShader"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 _Color;
            float _Transparency;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;  // 直接传递 UV 坐标
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 获取颜色和透明度
                fixed4 color = _Color;

                // 应用透明度
                color.a *= _Transparency;

                return color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
