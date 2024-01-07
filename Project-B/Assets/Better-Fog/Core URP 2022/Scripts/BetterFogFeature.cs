using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;
using System.Collections.Generic;
using INab.BetterFog.Core;

namespace INab.BetterFog.URP
{
	public class BetterFogFeature : ScriptableRendererFeature
	{
		[Serializable]
		public class BetterFogSettings
		{
			public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;

			public bool _UseCustomDepthTexture = false;
			public bool _UseFogOffsetTexture = false;
		}

		public BetterFogSettings settings = new BetterFogSettings();
		private BetterFogPass betterFogPass;

		private Material DepthBlit;
		private Material FogBlit;
		private Material FinalBlit;
		private Material SSMS;

		public override void Create()
		{
			// Create materials
			DepthBlit = CoreUtils.CreateEngineMaterial(Shader.Find("Shader Graphs/DepthBlit"));
			FogBlit = CoreUtils.CreateEngineMaterial(Shader.Find("Shader Graphs/FogBlit"));
			FinalBlit = CoreUtils.CreateEngineMaterial(Shader.Find("Shader Graphs/FinalBlit"));
			SSMS = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/INabStudio/SSMS_URP"));


			betterFogPass = new BetterFogPass(settings, DepthBlit, FogBlit, FinalBlit, SSMS);

		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			if (renderingData.cameraData.cameraType != CameraType.Game && renderingData.cameraData.cameraType != CameraType.SceneView && renderingData.cameraData.cameraType != CameraType.VR) return;
			
			//if (betterFogVolume)
			{
				renderer.EnqueuePass(betterFogPass);
			}
		}
		public override void SetupRenderPasses(ScriptableRenderer renderer,
										  in RenderingData renderingData)
		{
			// The target is used after allocation
			betterFogPass.SetCameraTarget(renderer.cameraColorTargetHandle);
		}


		protected override void Dispose(bool disposing)
		{
			CoreUtils.Destroy(DepthBlit);
			CoreUtils.Destroy(FogBlit);
			CoreUtils.Destroy(FinalBlit);
			CoreUtils.Destroy(SSMS);
		}

		public class BetterFogPass : ScriptableRenderPass
		{
			private RTHandle m_FogFactorRT;
			private RTHandle m_CustomDepthRT;
			private RTHandle m_FogOffsetRT;
			private RTHandle m_TemporaryRT;

			private RTHandle cameraColorTarget;

			private BetterFogSettings m_CurrentSettings;
			private BetterFogVolume m_BetterFogVolume;

			private Material m_DepthBlit;
			private Material m_FogBlit;
			private Material m_FinalBlit;
			private Material m_SSMS;

			const int kMaxIterations = 16;
			RTHandle[] _blurBuffer1 = new RTHandle[kMaxIterations];
			RTHandle[] _blurBuffer2 = new RTHandle[kMaxIterations];
			RTHandle prefilteredRT;
			RTHandle last;

			private List<CustomRenderer> m_DepthRenderers = new List<CustomRenderer>();
			private List<CustomRenderer> m_FogOffsetRenderers = new List<CustomRenderer>();

			public BetterFogPass(BetterFogSettings settings, Material depthBlit, Material fogBlit, Material finalBlit, Material SSMS)
			{
				for (int i = 0; i < kMaxIterations; i++)
				{
					_blurBuffer1[i] = RTHandles.Alloc("_MipDown" + i, name: "_MipDown" + i);
					_blurBuffer2[i] = RTHandles.Alloc("_MipUp" + i, name: "_MipUp" + i);
				}

				renderPassEvent = settings.Event;

				m_CurrentSettings = settings;

				m_FogBlit = fogBlit;
				m_DepthBlit = depthBlit;
				m_FinalBlit = finalBlit;
				m_SSMS = SSMS;

				m_CustomDepthRT = RTHandles.Alloc("_CustomDepth", name: "_CustomDepth");
				m_FogFactorRT = RTHandles.Alloc("_FogFactor_RT", name: "_FogFactor_RT");
				m_FogOffsetRT = RTHandles.Alloc("_FogOffset", name: "_FogOffset");
				m_TemporaryRT = RTHandles.Alloc("_TemporaryRT", name: "_TemporaryRT");
			}

			public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
			{
				RenderTextureDescriptor textureDescriptor = renderingData.cameraData.cameraTargetDescriptor;
				textureDescriptor.msaaSamples = 1;
				//textureDescriptor.depthBufferBits = 32;
				textureDescriptor.depthBufferBits = 0; // 32 bits breaks everytinhg

				RenderingUtils.ReAllocateIfNeeded(ref m_TemporaryRT, textureDescriptor, FilterMode.Point, name: "_TemporaryRT");

				textureDescriptor.depthBufferBits = 0; // 32 bits breaks everytinhg
				textureDescriptor.colorFormat = RenderTextureFormat.RHalf;
				RenderingUtils.ReAllocateIfNeeded(ref m_FogFactorRT, textureDescriptor, FilterMode.Point,name: "_FogFactor_RT");

				if (m_CurrentSettings._UseCustomDepthTexture)
                {
					//textureDescriptor.depthBufferBits = 32;
					textureDescriptor.colorFormat = RenderTextureFormat.RFloat;
					RenderingUtils.ReAllocateIfNeeded(ref m_CustomDepthRT, textureDescriptor, FilterMode.Point, name: "_CustomDepth");
				}

				if (m_CurrentSettings._UseFogOffsetTexture)
				{
					//textureDescriptor.depthBufferBits = 32;
					textureDescriptor.colorFormat = RenderTextureFormat.RGHalf;
					RenderingUtils.ReAllocateIfNeeded(ref m_FogOffsetRT, textureDescriptor, FilterMode.Point, name: "_FogOffset");
				}
			}

			public void SetCameraTarget(RTHandle cameraColorTarget)
			{
				this.cameraColorTarget = cameraColorTarget;
			}

			private void FogBlitProperties()
            {
				if (m_BetterFogVolume._UseDistanceFog.value)
				{
					m_FogBlit.EnableKeyword("_USEDISTANCEFOG_ON");
				}
				else
				{
					m_FogBlit.DisableKeyword("_USEDISTANCEFOG_ON");
				}

				if (m_BetterFogVolume._UseSkyboxHeightFog.value)
				{
					m_FogBlit.EnableKeyword("_USESKYBOXHEIGHTFOG_ON");
				}
				else
				{
					m_FogBlit.DisableKeyword("_USESKYBOXHEIGHTFOG_ON");
				}

				if (m_BetterFogVolume._UseHeightFog.value)
				{
					m_FogBlit.EnableKeyword("_USEHEIGHTFOG_ON");
				}
				else
				{
					m_FogBlit.DisableKeyword("_USEHEIGHTFOG_ON");
				}

				if (m_BetterFogVolume._UseNoise.value)
				{
					m_FogBlit.EnableKeyword("_USENOISE_ON");
				}
				else
				{
					m_FogBlit.DisableKeyword("_USENOISE_ON");
				}

				m_FogBlit.SetFloat("_FogIntensity", m_BetterFogVolume._FogIntensity.value);
				m_FogBlit.SetInt("_UseRadialDistance", m_BetterFogVolume._UseRadialDistance.value ? 1 : 0);

				switch (m_BetterFogVolume._FogType.value)
				{
					case FogMode.Linear:
						m_FogBlit.EnableKeyword("_FOGTYPE_LINEAR");
						m_FogBlit.DisableKeyword("_FOGTYPE_EXP");
						m_FogBlit.DisableKeyword("_FOGTYPE_EXP2");

						break;
					case FogMode.Exponential:
						m_FogBlit.DisableKeyword("_FOGTYPE_LINEAR");
						m_FogBlit.EnableKeyword("_FOGTYPE_EXP");
						m_FogBlit.DisableKeyword("_FOGTYPE_EXP2");
						break;
					case FogMode.ExponentialSquared:
						m_FogBlit.DisableKeyword("_FOGTYPE_LINEAR");
						m_FogBlit.DisableKeyword("_FOGTYPE_EXP");
						m_FogBlit.EnableKeyword("_FOGTYPE_EXP2");
						break;
				}

				m_FogBlit.SetFloat("_DistanceFogOffset", m_BetterFogVolume._DistanceFogOffset.value);

				m_FogBlit.SetFloat("_SkyboxFogIntensity", m_BetterFogVolume._SkyboxFogIntensity.value);
				m_FogBlit.SetFloat("_SkyboxFogHardness", m_BetterFogVolume._SkyboxFogHardness.value);
				m_FogBlit.SetFloat("_SkyboxFogOffset", m_BetterFogVolume._SkyboxFogOffset.value);
				m_FogBlit.SetFloat("_SkyboxFill", m_BetterFogVolume._SkyboxFill.value);

				m_FogBlit.SetFloat("_HeightDensity", Mathf.Pow(m_BetterFogVolume._HeightDensity.value, 4));
				m_FogBlit.SetFloat("_Height", m_BetterFogVolume._Height.value);
				m_FogBlit.SetInt("_HeightFogTypeExp", ((int)m_BetterFogVolume._HeightFogType.value == 2 ? 1 : 0));

				m_FogBlit.SetFloat("_Scale1", m_BetterFogVolume._Scale1.value);
				m_FogBlit.SetFloat("_NoiseTimeScale1", m_BetterFogVolume._NoiseTimeScale1.value);
				m_FogBlit.SetFloat("_Lerp1", m_BetterFogVolume._Lerp1.value);
				m_FogBlit.SetFloat("_NoiseDistanceEnd", m_BetterFogVolume._NoiseDistanceEnd.value);
				m_FogBlit.SetFloat("_NoiseIntensity", m_BetterFogVolume._NoiseIntensity.value);
				m_FogBlit.SetFloat("_NoiseEndHardness", m_BetterFogVolume._NoiseEndHardness.value);

				m_FogBlit.SetVector("_NoiseSpeed1", m_BetterFogVolume._NoiseSpeed1.value);

				int useNoiseDistance = 0;
				int useNoiseHeight = 0;

				switch (m_BetterFogVolume._NoiseAffect.value)
				{
					case NoiseAffect.DistanceOnly:
						useNoiseDistance = 1;
						useNoiseHeight = 0;

						break;
					case NoiseAffect.HeightOnly:
						useNoiseDistance = 0;
						useNoiseHeight = 1;
						break;
					case NoiseAffect.Both:
						useNoiseDistance = 1;
						useNoiseHeight = 1;
						break;
				}

				if (m_BetterFogVolume._UseDistanceFog.value == false)
					useNoiseDistance = 0;

				if (m_BetterFogVolume._UseHeightFog.value == false)
					useNoiseHeight = 0;

				m_FogBlit.SetInt("_UseNoiseDistance", useNoiseDistance);
				m_FogBlit.SetInt("_UseNoiseHeight", useNoiseHeight);


				// Distance Fog Values
				Vector4 sceneParams;
				float diff = m_BetterFogVolume._SceneEnd.value - m_BetterFogVolume._SceneStart.value;
				float invDiff = Mathf.Abs(diff) > 0.0001f ? 1.0f / diff : 0.0f;
				sceneParams.x = m_BetterFogVolume._FogDensity.value * 1.2011224087f; // density / sqrt(ln(2)), used by Exp2 fog mode
				sceneParams.y = m_BetterFogVolume._FogDensity.value * 1.4426950408f; // density / ln(2), used by Exp fog mode
				sceneParams.z = -invDiff;
				sceneParams.w = m_BetterFogVolume._SceneEnd.value * invDiff;
				m_FogBlit.SetVector("_SceneFogParams", sceneParams);
			}

			private void FinalBlitProperties()
            {
				if (m_BetterFogVolume._UseSunLight.value)
				{
					m_FinalBlit.EnableKeyword("_USESUNLIGHT_ON");
				}
				else
				{
					m_FinalBlit.DisableKeyword("_USESUNLIGHT_ON");
				}

				if (m_BetterFogVolume._UseGradient.value)
				{
					m_FinalBlit.EnableKeyword("_USEGRADIENT_ON");
				}
				else
				{
					m_FinalBlit.DisableKeyword("_USEGRADIENT_ON");
				}


				m_FinalBlit.SetColor("_SunColor", m_BetterFogVolume._SunColor.value);
				m_FinalBlit.SetFloat("_SunPower", m_BetterFogVolume._SunPower.value);
				m_FinalBlit.SetFloat("_SunIntensity", m_BetterFogVolume._SunIntensity.value);

				m_FinalBlit.SetColor("_FogColor", m_BetterFogVolume._FogColor.value);

				m_FinalBlit.SetFloat("_GradientStart", m_BetterFogVolume._GradientStart.value);
				m_FinalBlit.SetFloat("_GradientEnd", m_BetterFogVolume._GradientEnd.value);

				// Custom texture lerping
				// TODO change to standard TextureParemeter after Unity adds Texture Interp (in 2023 not available yet)
				if (m_BetterFogVolume._GradientTexture.value != null)
				{
					m_FinalBlit.SetFloat("_GradientLerp", m_BetterFogVolume._GradientTexture.LerpValue);
					m_FinalBlit.SetTexture("_GradientTextureFrom", m_BetterFogVolume._GradientTexture.FromTexture);
					m_FinalBlit.SetTexture("_GradientTexture", m_BetterFogVolume._GradientTexture.value);
				}

				m_FinalBlit.SetFloat("_EnergyLoss", m_BetterFogVolume._EnergyLoss.value);
			}

			private int SSMSProperties(float tw,float th)
			{
				// determine the iteration count
				var logh = Mathf.Log(th, 2) + m_BetterFogVolume._Radius.value - 8;
				var logh_i = (int)logh;
				var iterations = Mathf.Clamp(logh_i, 1, kMaxIterations);

				// update the shader properties
				var lthresh = m_BetterFogVolume._Threshold.value;
				m_SSMS.SetFloat("_Threshold", lthresh);

				var knee = lthresh * m_BetterFogVolume._SoftKnee.value + 1e-5f;
				var curve = new Vector3(lthresh - knee, knee * 2, 0.25f / knee);
				m_SSMS.SetVector("_Curve", curve);

				var pfo = !m_BetterFogVolume._HighQuality.value && m_BetterFogVolume._AntiFlicker.value;
				m_SSMS.SetFloat("_PrefilterOffs", pfo ? -0.5f : 0.0f);

				m_SSMS.SetFloat("_SampleScale", 0.5f + logh - logh_i);
				m_SSMS.SetFloat("_Intensity", m_BetterFogVolume._Intensity.value);

				var fadeRampTexture = m_BetterFogVolume._FadeRamp.value;
				if(fadeRampTexture != null) m_SSMS.SetTexture("_FadeTex", fadeRampTexture);
				m_SSMS.SetFloat("_BlurWeight", m_BetterFogVolume._BlurWeight.value);
				m_SSMS.SetFloat("_Radius", m_BetterFogVolume._Radius.value);

				if (m_BetterFogVolume._AntiFlicker.value)
				{
					m_SSMS.EnableKeyword("ANTI_FLICKER_ON");
				}
				else
                {
					m_SSMS.DisableKeyword("ANTI_FLICKER_ON");
				}

				if (m_BetterFogVolume._HighQuality.value)
				{
					m_SSMS.EnableKeyword("_HIGH_QUALITY_ON");
				}
				else
				{
					m_SSMS.DisableKeyword("_HIGH_QUALITY_ON");
				}

				return iterations;
			}

			private void DrawRenderers(List<CustomRenderer> list, CommandBuffer cmd)
			{
				foreach (var customRenderer in list)
				{
					if (customRenderer.render == false) continue;

					var material = customRenderer.material;

					var renderer = customRenderer.renderer;

					if (renderer == false || material == false) continue;

					if(!customRenderer.alwaysRender)
                    {
						if (renderer.enabled == false || renderer.gameObject.activeInHierarchy == false) continue;
					}

					if (customRenderer.drawAllSubmeshes && renderer is not ParticleSystemRenderer)
					{
						Mesh mesh = null;
						if (renderer is SkinnedMeshRenderer)
							mesh = (renderer as SkinnedMeshRenderer).sharedMesh;
						else if (renderer is MeshRenderer)
							mesh = renderer.GetComponent<MeshFilter>().sharedMesh;

						for (int i = 0; i < mesh.subMeshCount; i++)
						{
							cmd.DrawRenderer(renderer, material, i, 0);
						}
					}
					else
					{
						cmd.DrawRenderer(renderer, material, 0, 0);
					}
				}
			}

			private void GetFogVolume()
			{
				// Renderer feature stuff
				if (m_BetterFogVolume == null)
				{
					// Get better fog volume
					var stack = VolumeManager.instance.stack;
					m_BetterFogVolume = stack.GetComponent<BetterFogVolume>();
				}
				if (m_BetterFogVolume == null)
				{
					Debug.LogError("There is no BetterFogVolume in Volume Stack.");
				}
			}

			public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
			{
				if (!renderingData.cameraData.isSceneViewCamera && !renderingData.cameraData.isDefaultViewport)
					return;

				GetFogVolume();

				//Only process if the effect is active
				if (!m_BetterFogVolume.IsActive())
					return;

				var cmd = CommandBufferPool.Get("Better Fog");

				RenderTextureDescriptor cameraDesc = renderingData.cameraData.cameraTargetDescriptor;
				var renderer = renderingData.cameraData.renderer;


				if (renderingData.cameraData.cameraType == CameraType.Game)
                {
					var sceneCamera = FindObjectOfType<Camera>();
					var set = sceneCamera.GetComponent<BetterFogRenderers>();

					if (set != null)
					{
						m_DepthRenderers = set.depthRenderers;
						m_FogOffsetRenderers = set.fogOffsetRenderers;
					}
				}


				if (m_CurrentSettings._UseCustomDepthTexture)
				{
					m_FogBlit.EnableKeyword("_USECUSTOMDEPTH_ON");
					m_FinalBlit.EnableKeyword("_USECUSTOMDEPTH_ON");

					//Blit(cmd, renderer.cameraColorTargetHandle, m_CustomDepthRT, m_DepthBlit, 0);
					Blitter.BlitCameraTexture(cmd, renderer.cameraColorTargetHandle, m_CustomDepthRT, m_DepthBlit, 0);

					cmd.SetRenderTarget(m_CustomDepthRT);

					// we need this to avoid depth testing glitches
					cmd.ClearRenderTarget(true, false, Color.black);

					DrawRenderers(m_DepthRenderers, cmd);

					cmd.SetGlobalTexture("_CustomDepth", m_CustomDepthRT);
				}

				if (m_CurrentSettings._UseFogOffsetTexture)
				{
					m_FogBlit.SetInt("_UseFogOffset", 1);

					cmd.SetRenderTarget(m_FogOffsetRT);

					// we need this to avoid depth testing glitches
					cmd.ClearRenderTarget(true, true, Color.black);

					DrawRenderers(m_FogOffsetRenderers, cmd);

					cmd.SetGlobalTexture("_FogOffset", m_FogOffsetRT);
				}
				else
                {
					m_FogBlit.SetInt("_UseFogOffset", 0);
				}

				FogBlitProperties();
				FinalBlitProperties();

				m_FogBlit.SetMatrix("_InverseView", renderingData.cameraData.camera.cameraToWorldMatrix);
				m_FinalBlit.SetMatrix("_InverseView", renderingData.cameraData.camera.cameraToWorldMatrix);

				// Fog post process blit
				if (!m_BetterFogVolume._UseSSMS.value)
				{
					// Fog blit to FogFactorRT
					Blitter.BlitCameraTexture(cmd, cameraColorTarget, m_FogFactorRT, m_FogBlit, 0);
					cmd.SetGlobalTexture("_FogFactor_RT", m_FogFactorRT);

					// Camera fog blit
					cmd.SetGlobalTexture("_MainTex", cameraColorTarget);
					Blitter.BlitCameraTexture(cmd, cameraColorTarget, m_TemporaryRT, m_FinalBlit, 0);
					Blitter.BlitCameraTexture(cmd, m_TemporaryRT, cameraColorTarget);

				}
				else
				{
					// Fog blit to FogFactorRT
					Blitter.BlitCameraTexture(cmd, cameraColorTarget, m_FogFactorRT, m_FogBlit, 0);
					cmd.SetGlobalTexture("_FogFactor_RT", m_FogFactorRT);

					// Camera fog blit
					cmd.SetGlobalTexture("_MainTex", cameraColorTarget);
					Blitter.BlitCameraTexture(cmd, cameraColorTarget, m_TemporaryRT, m_FinalBlit, 0);

					// source texture size
					var tw = cameraDesc.width;
					var th = cameraDesc.height;

					// Do fog on a half resolution, full resolution doesn't bring much
					tw /= 2;
					th /= 2;

					var iterations = SSMSProperties(tw,th);

					var rtFormat = RenderTextureFormat.ARGBHalf;
					var rtFilterMode = FilterMode.Bilinear;

					// prefilter pass
					RenderTextureDescriptor prefilteredDesc = new RenderTextureDescriptor(cameraDesc.width, cameraDesc.height,rtFormat,0,0);
					RenderingUtils.ReAllocateIfNeeded(ref prefilteredRT, prefilteredDesc, rtFilterMode);

					var pass = 0;
					cmd.SetGlobalTexture("_MainTex", m_TemporaryRT);
					//Blit(cmd, m_TemporaryRT, prefilteredRT, m_SSMS, pass);
					Blitter.BlitCameraTexture(cmd, m_TemporaryRT, prefilteredRT, m_SSMS, pass);


					// construct a mip pyramid
					last = prefilteredRT;

					for (var level = 0; level < iterations; level++)
					{
						RenderTextureDescriptor blurBufferDesc = new RenderTextureDescriptor(tw, th, rtFormat, 0, 0);
						RenderingUtils.ReAllocateIfNeeded(ref _blurBuffer1[level], blurBufferDesc, rtFilterMode);
						RenderingUtils.ReAllocateIfNeeded(ref _blurBuffer2[level], blurBufferDesc, rtFilterMode);

						tw = Mathf.Max(tw / 2, 1);
						th = Mathf.Max(th / 2, 1);

						pass = (level == 0) ?  1 : 2;
						cmd.SetGlobalTexture("_MainTex", last);
						//Blit(cmd, last, _blurBuffer1[level], m_SSMS, pass);
						Blitter.BlitCameraTexture(cmd, last, _blurBuffer1[level], m_SSMS, pass);
						
						last = _blurBuffer1[level];
					}

					// upsample and combine loop
					for (var level = iterations - 2; level >= 0; level--)
					{
						var basetex = _blurBuffer1[level];
						cmd.SetGlobalTexture("_BaseTex", basetex);

						pass = 3;
						cmd.SetGlobalTexture("_MainTex", last);
						//Blit(cmd, last, _blurBuffer2[level], m_SSMS, pass);
						Blitter.BlitCameraTexture(cmd, last, _blurBuffer2[level], m_SSMS, pass);

						last = _blurBuffer2[level];
					}


					// finish process
					cmd.SetGlobalTexture("_BaseTex", m_TemporaryRT);
					pass = 4;
					//Blit(cmd, last, renderer.cameraColorTarget, m_SSMS, pass);
					Blitter.BlitCameraTexture(cmd, last, renderer.cameraColorTargetHandle, m_SSMS, pass);


					// release the temporary buffers
					for (var i = 0; i < kMaxIterations; i++)
					{
						//cmd.ReleaseTemporaryRT(_blurBuffer1[i]);
						//cmd.ReleaseTemporaryRT(_blurBuffer2[i]);

						cmd.ReleaseTemporaryRT(Shader.PropertyToID(_blurBuffer1[i].name));
						cmd.ReleaseTemporaryRT(Shader.PropertyToID(_blurBuffer2[i].name));

					}

					//cmd.ReleaseTemporaryRT(prefilteredRT);
					cmd.ReleaseTemporaryRT(Shader.PropertyToID(prefilteredRT.name));
				}

				context.ExecuteCommandBuffer(cmd);
				CommandBufferPool.Release(cmd);
			}

			public override void OnCameraCleanup(CommandBuffer cmd)
			{
				// When no alloc on the BetterFogPass constructor then it throws null

				cmd.ReleaseTemporaryRT(Shader.PropertyToID(m_FogFactorRT.name));
				cmd.ReleaseTemporaryRT(Shader.PropertyToID(m_CustomDepthRT.name));
				cmd.ReleaseTemporaryRT(Shader.PropertyToID(m_FogOffsetRT.name));
				cmd.ReleaseTemporaryRT(Shader.PropertyToID(m_TemporaryRT.name));
			}

		}
	}
}