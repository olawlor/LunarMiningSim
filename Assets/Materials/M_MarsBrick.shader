﻿/*
Multi-scale detail shader.

Written by Orion Lawlor, lawlor@alaska.edu, 2020-07 for Nexus Aurora (public domain)

*/


Shader "Custom/Mars Brick Shader"
{
    Properties
    {
        _Color ("Multiply Color", Color) = (1,1,1,1)
        
        _DetailATex ("Detail Texture A (RGB)",2D) = "white" {}
        _DetailARepeats ("Detail A Repeat Size (meters)", Range(0.1,10000)) = 32.0
        _DetailAContrast ("Detail A Contrast", Range(0,10)) = 1.0
        
        _DetailBTex ("Detail Texture B (RGB)",2D) = "white" {}
        _DetailBRepeats ("Detail B Repeat Size (meters)", Range(0.1,10000)) = 512.0
        _DetailBContrast ("Detail B Contrast", Range(0,10)) = 0.1
        
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _AerialTex;
        
        // The detail textures are repeating (tiling) to provide detail
        //   They use mipmaps to fade out from far away.
        sampler2D _DetailATex;
        float _DetailARepeats;
        half _DetailAContrast;
        
        sampler2D _DetailBTex;
        float _DetailBRepeats;
        half _DetailBContrast;
        
        struct Input
        {
            float2 uv_AerialTex;
            float3 worldPos;  // magically get world position (so detail shaders line up at object edges)
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv=IN.uv_AerialTex; 
            
            // Albedo comes from a texture tinted by color
            float2 detailUV=IN.worldPos.xz; // meters
            
            fixed4 detailA = tex2D (_DetailATex, detailUV*(1.0/_DetailARepeats));
            fixed4 detailB = tex2D (_DetailBTex, detailUV*(1.0/_DetailBRepeats));
            
            fixed4 c=_Color*
                ((detailA*2.0-1.0)*_DetailAContrast+1)*
                ((detailB*2.0-1.0)*_DetailBContrast+1); 
            
            o.Albedo = c.rgb;
            
            
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

