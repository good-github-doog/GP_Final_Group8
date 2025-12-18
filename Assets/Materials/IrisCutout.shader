Shader "UI/IrisCutout"
{
    Properties
    {
        _Color ("Color", Color) = (0,0,0,1)
        _Center ("Center (UV)", Vector) = (0.5,0.5,0,0)
        _Radius ("Radius (UV)", Range(0, 2)) = 0.5
        _Softness ("Soft Edge", Range(0, 0.2)) = 0.02
        _Aspect ("Aspect", Float) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; };

            fixed4 _Color;
            float4 _Center;
            float _Radius;
            float _Softness;
            float _Aspect;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 做「等比例的圓」：把 x 乘上 aspect
                float2 p = i.uv;
                float2 c = _Center.xy;
                p.x = (p.x - c.x) * _Aspect + c.x;

                float d = distance(p, c);

                // 洞內 alpha=0（透明），洞外 alpha=1（黑）
                float a = smoothstep(_Radius - _Softness, _Radius, d);

                fixed4 col = _Color;
                col.a *= a;
                return col;
            }
            ENDCG
        }
    }
}
