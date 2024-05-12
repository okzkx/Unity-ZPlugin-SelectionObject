Shader "ZPlugin/SelectionObject/DrawInstanceID"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            int _InstanceID;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // return _InstanceID;
                
                return fixed4(
                    ((_InstanceID >> 24) & 0xF) / 255.0,
                    ((_InstanceID >> 16) & 0xF) / 255.0,
                    ((_InstanceID >> 8) & 0xF) / 255.0,
                    (_InstanceID & 0xF) / 255.0
                );
            }
            ENDCG
        }
    }
}
