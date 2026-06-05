// Assets/Shaders/RadialGradientTimer.shader
Shader "UI/RadialGradientTimer"
{
    Properties
    {
        _MorningColor ("Morning Color", Color) = (0.4, 0.7, 1.0, 1)
        _NoonColor    ("Noon Color",    Color) = (1.0, 0.5, 0.1, 1)
        _NightColor   ("Night Color",   Color) = (0.1, 0.0, 0.2, 1)
        _FillAmount   ("Fill Amount",   Range(0,1)) = 1.0
        _RingWidth    ("Ring Width",    Range(0,0.5)) = 0.15
        _MainTex      ("Main Tex",      2D) = "white" {}
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f    { float4 pos:SV_POSITION;  float2 uv:TEXCOORD0; };

            fixed4 _MorningColor, _NoonColor, _NightColor;
            float  _FillAmount, _RingWidth;
            sampler2D _MainTex;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // UV を -0.5?0.5 に正規化
                float2 uv = i.uv - 0.5;
                float dist = length(uv);

                // リング範囲外は透明
                float inner = 0.5 - _RingWidth;
                if (dist > 0.5 || dist < inner) return fixed4(0,0,0,0);

                // 角度を計算（12時=0、時計回りで0?1）
                // atan2(x, y) で真上=0 になる
                float angle = atan2(uv.x, uv.y);         // -π ? π
                float t = (angle / (2.0 * UNITY_PI)) + 0.5; // 0 ? 1

                // FillAmount でマスク（tがFillAmountを超えたら透明）
                if (t > _FillAmount) return fixed4(0,0,0,0);

                // 色グラデーション（朝→昼→夜）
                fixed4 col;
                if (t < 0.5)
                    col = lerp(_MorningColor, _NoonColor, t * 2.0);
                else
                    col = lerp(_NoonColor, _NightColor, (t - 0.5) * 2.0);

                return col;
            }
            ENDCG
        }
    }
}