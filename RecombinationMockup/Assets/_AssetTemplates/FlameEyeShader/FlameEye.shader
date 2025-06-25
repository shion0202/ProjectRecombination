Shader "Visualab/FlameEye"
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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

			#define SIZE 2.8
			#define RADIUS 0.07
			#define INNER_FADE .8
			#define OUTER_FADE 0.02
			#define SPEED .1
			#define BORDER 0.21

			float random(in float2 p) {
				float3 p3 = frac(float3(p.xyx) * .1031);
				p3 += dot(p3, p3.yzx + 33.33);
				return frac((p3.x + p3.y) * p3.z);
			}

			float noise(in float2 _st) {
				float2 i = floor(_st);
				float2 f = frac(_st);

				// Four corners in 2D of a tile
				float a = random(i);
				float b = random(i + float2(1.0, 0.0));
				float c = random(i + float2(0.0, 1.0));
				float d = random(i + float2(1.0, 1.0));

				float2 u = f * f * (3. - 2.0 * f);

				return lerp(a, b, u.x) +
					(c - a) * u.y * (1. - u.x) +
					(d - b) * u.x * u.y;
			}

			float light(in float2 pos, in float size, in float radius, in float inner_fade, in float outer_fade) {
				float len = length(pos / size);
				return pow(clamp((1.0 - pow(clamp(len - radius, 0.0, 1.0), 1.0 / inner_fade)), 0.0, 1.0), 1.0 / outer_fade);
			}


			float flare(in float angle, in float alpha, in float time) {
				float t = time;
				float n = noise(float2(t + 0.5 + abs(angle) + pow(alpha, 0.6), t - abs(angle) + pow(alpha + 0.1, 0.6)) * 7.0);
				//	n = 1.0;
				float split = (15.0 + sin(t * 2.0 + n * 4.0 + angle * 20.0 + alpha * 1.0 * n) * (.3 + .5 + alpha * .6 * n));

				float rotate = sin(angle * 20.0 + sin(angle * 15.0 + alpha * 4.0 + t * 30.0 + n * 5.0 + alpha * 4.0)) * (.5 + alpha * 1.5);

				float g = pow((2.0 + sin(split + n * 1.5 * alpha + rotate) * 1.4) * n * 4.0, n * (1.5 - 0.8 * alpha));

				g *= alpha * alpha * alpha * .5;
				g += alpha * .7 + g * g * g;
				return g;
			}


			fixed4 frag(v2f i) : SV_Target
			{
				i.vertex  -= 5000;
				//float2 uv = (fragCoord.xy - iResolution.xy * 0.5) / iResolution.y;
				i.uv = i.uv / 1.2;
				i.uv.x = i.uv.x * 1.2;
				float f = .0;
				float f2 = .0;
				float t = _Time.y * SPEED;
				float alpha = light(i.uv,SIZE,RADIUS,INNER_FADE,OUTER_FADE);
				float angle = atan2(i.uv.x,i.uv.y);
				float n = noise(float2(i.uv.x * 20. + _Time.y,i.uv.y * 20. + _Time.y));

				float l = length(i.uv);
				if (l < BORDER) {
					t *= .8;
					alpha = (1. - pow(((BORDER - l) / BORDER),0.22) * 0.7);
					alpha = clamp(alpha - light(i.uv,0.2,0.0,1.3,.7) * .55,.0,1.);
					f = flare(angle * 1.0,alpha,-t * .5 + alpha);
					f2 = flare(angle * 1.0,alpha * 1.2,((-t + alpha * .5 + 0.38134)));

				}
			else if (alpha < 0.001) {
			   f = alpha;
			}
			else {
			   f = flare(angle,alpha,t) * 1.3;
			}
			float4 fragColor = float4(float3(f * (1.0 + sin(angle - t * 4.) * .3) + f2 * f2 * f2,f * alpha + f2 * f2 * 2.0,f * alpha * 0.5 + f2 * (1.0 + sin(angle + t * 4.) * .3)),1.0);
           
			return fragColor;
			}
            ENDCG
        }
    }
}
