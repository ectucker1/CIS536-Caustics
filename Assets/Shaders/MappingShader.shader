Shader "Custom/MappingShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _Specular ("Specular", Range(0,1)) = 0.5
        _Diffuse ("Diffuse", Range(0,1)) = 0.5
        _IOR ("IOR", Range(0, 5.0)) = 1.0
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent"}
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        // Physically based StandardSpecular lighting model, and enable shadows on all light types
        #pragma surface surf StandardSpecular fullforwardshadows keepalpha

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Smoothness;
        half _Specular;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandardSpecular o)
        {
            o.Albedo = _Color.rgb;
            o.Specular = _Specular;
            o.Smoothness = _Smoothness;
            o.Alpha = _Color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
