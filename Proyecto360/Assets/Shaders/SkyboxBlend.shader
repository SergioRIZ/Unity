Shader "Skybox/SkyboxBlend"
{
    Properties
    {
        _MainTex ("Texture 1", 2D) = "white" {}     // Primera textura (imagen panorámica inicial)
        _MainTex2 ("Texture 2", 2D) = "black" {}     // Segunda textura (imagen panorámica destino)
        _Blend ("Blend", Range(0, 1)) = 0            // Controla la mezcla entre ambas imágenes
        _Exposure ("Exposure", Float) = 1.0          // Factor multiplicador de brillo
        _Rotation ("Rotation", Float) = 0            // Rotación horizontal (grados)
    }

    SubShader
    {
        // Esto define cómo se dibuja el shader: de fondo, sin ZWrite ni culling
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM                         // Inicio del código en lenguaje Cg/HLSL
            #pragma vertex vert               // Función de vértices personalizada
            #pragma fragment frag             // Función de fragmentos (color final por píxel)
            #include "UnityCG.cginc"          // Librería con funciones útiles de Unity

             // Declaración de las propiedades como variables accesibles en el shader
            sampler2D _MainTex;
            sampler2D _MainTex2;
            float _Blend;
            float _Exposure;
            float _Rotation;

            // Función para rotar un vector 3D alrededor del eje Y
            float3 RotateAroundY(float3 dir, float degrees)
            {
                float rad = radians(degrees);        // Convertimos grados a radianes
                float sina, cosa;
                sincos(rad, sina, cosa);             // Calculamos seno y coseno
                // Multiplicamos por la matriz de rotación en Y
                float3 res;
                res.x = cosa * dir.x - sina * dir.z;
                res.z = sina * dir.x + cosa * dir.z;
                res.y = dir.y;                       // Y se mantiene igual
                return res;
            }

            // Convierte una dirección 3D en coordenadas esféricas 2D (longitud y latitud)
            float2 ToRadialCoords(float3 dir)
            {
                float3 normDir = normalize(dir);     // Normalizamos dirección
                float latitude = acos(normDir.y);    // Calculamos latitud
                float longitude = atan2(normDir.z, normDir.x); // Y longitud
                return float2(0.5 - longitude / (2 * UNITY_PI), 1.0 - latitude / UNITY_PI);
                // Convertimos a coordenadas UV en el rango [0,1]
            }
            // Estructura de entrada del vértice (posición original)
            struct appdata
            {
                float4 vertex : POSITION;
            };
            // Estructura de salida del vértice hacia el fragmento
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 dir : TEXCOORD0;
            };
            // Función de vértice: convierte coordenadas del vértice a dirección y posición
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);// Proyectamos a pantalla
                o.dir = v.vertex.xyz;                  // Guardamos dirección 3D
                return o;
            }

            // Función de fragmento: calcula el color final del píxel
            fixed4 frag(v2f i) : SV_Target
            {
                // Rotamos la dirección con el ángulo deseado
                float3 rotatedDir1 = RotateAroundY(i.dir, _Rotation);
                // Convertimos la dirección rotada en coordenadas UV esféricas
                float2 uv = ToRadialCoords(rotatedDir1);
                // Tomamos el color de cada textura usando las UV calculadas y aplicamos exposición
                fixed4 col1 = tex2D(_MainTex, uv) * _Exposure;
                fixed4 col2 = tex2D(_MainTex2, uv) * _Exposure;
                 // Mezclamos los colores según el valor de _Blend (0 = solo tex1, 1 = solo tex2)
                return lerp(col1, col2, _Blend);
            }
            ENDCG  // Fin del bloque Cg/HLSL
        }
    }

    FallBack "RenderFX/Skybox"  // En caso de que el shader falle, se usará este otro
}
