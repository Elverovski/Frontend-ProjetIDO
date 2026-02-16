Shader "WebRTC/I420ToRGB"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv.y = 1.0 - o.uv.y;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float y = tex2D(_MainTex, i.uv).r;
                float u = tex2D(_MainTex, float2(i.uv.x / 2.0, 0.75 + i.uv.y / 4.0)).r - 0.5;
                float v = tex2D(_MainTex, float2(i.uv.x / 2.0 + 0.5, 0.75 + i.uv.y / 4.0)).r - 0.5;
                
                float r = y + 1.402 * v;
                float g = y - 0.344 * u - 0.714 * v;
                float b = y + 1.772 * u;
                
                return fixed4(r, g, b, 1.0);
            }
            ENDCG
        }
    }
}
