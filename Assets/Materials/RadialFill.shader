Shader "UI/RadialFill"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _FillAmount ("Fill Amount", Range(0, 1)) = 1.0
        _FillOrigin ("Fill Origin", Range(0, 1)) = 0.0
        
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 texcoord  : TEXCOORD0;
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _FillAmount;
            float _FillOrigin;
            fixed4 _TextureSampleAdd;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                
                // Calcular ángulo desde el centro
                float2 coord = IN.texcoord - 0.5;
                float angle = atan2(coord.y, coord.x);
                
                // Convertir de [-PI, PI] a [0, 2*PI]
                angle = fmod(angle + 6.28318530718, 6.28318530718);
                
                // Ajustar para empezar desde TOP (12 o'clock)
                angle = fmod(angle - 1.57079632679 + 6.28318530718, 6.28318530718);
                
                // Para sentido HORARIO, invertimos el ángulo
                angle = 6.28318530718 - angle;
                
                // Aplicar fill amount - CORREGIDO: mostrar donde angle < targetAngle
                float targetAngle = _FillAmount * 6.28318530718;
                if (angle > targetAngle) // MOSTRAR donde el ángulo es menor al objetivo
                {
                    color.a = 0;
                }
                
                return color;
            }
            ENDCG
        }
    }
}