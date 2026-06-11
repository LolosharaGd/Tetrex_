Shader "Unlit/UnderUIBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _X_Resolution ("X Resolution", Float) = 1
        _Y_Resolution ("Y Resolution", Float) = 1
        _Step_Size ("Step Size", Float) = 0.003
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            int _X_Resolution;
            int _Y_Resolution;
            float _Step_Size;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = fixed4(0, 0, 0, 0);
                float total_weight = 0;

                for (int x = (_X_Resolution - 1) / -2; x <= (_X_Resolution - 1) / 2; ++x)
                {
                    for (int y = (_Y_Resolution - 1) / -2; y <= (_Y_Resolution - 1) / 2; ++y)
                    {
                        float weight = 1 / (abs(x / (_X_Resolution / 4 - 0.25)) + abs(y / (_Y_Resolution / 4 - 0.25)) + 1);
                        col += tex2D(_MainTex, i.uv + float2(x * _Step_Size, y * _Step_Size)) * weight;
                        total_weight += weight;
                    }
                }

                return col / total_weight;
            }
            ENDCG
        }
    }
}
