﻿Shader "Custom/BlendingShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _Specular ("Specular", Range(0,1)) = 0.0
        _Diffuse ("Diffuse", Range(0,1)) = 0.0
        _PhotonMap ("Radiance Map", 2D) = "white" {}
        _RadianceAmount ("Radiance Coefficent", Range(0,20)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based StandardSpecular lighting model, and enable shadows on all light types
        #pragma surface surf StandardSpecular fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _PhotonMap;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv2_PhotonMap;
        };

        half _Smoothness;
        half _Specular;
        half _Diffuse;
        half _RadianceAmount;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf(Input IN, inout SurfaceOutputStandardSpecular o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb * _Diffuse;
            o.Alpha = c.a;
            // Radiance from photon map
            fixed4 rad = tex2D(_PhotonMap, IN.uv2_PhotonMap) * _RadianceAmount;
            o.Emission = rad.rgb;
            // Metallic and smoothness come from slider variables
            o.Specular = _Specular;
            o.Smoothness = _Smoothness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
