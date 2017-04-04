using UnityEngine;
using System;
using System.Collections;

namespace OceanWhiteCaps
{
	/// <summary>
	/// 
	/// Generates a wave spectrum using the formula in the follow research paper.
	/// Can the found with a bit of googling.
	/// 
	/// WAVES SPECTRUM
	/// using "A unified directional spectrum for long and short wind-driven waves"
	/// T. Elfouhaily, B. Chapron, K. Katsaros, D. Vandemark
	/// Journal of Geophysical Research vol 102, p781-796, 1997
	/// 
	/// </summary>
	public class WaveSpectrumGPU
	{
		
		//CONST DONT CHANGE
		const float WAVE_CM = 0.23f;	// Eq 59
		const float WAVE_KM = 370.0f;	// Eq 59
		
		/// <summary>
		/// The current buffer index.
		/// Flips between 0 and 1 each frame. 
		/// </summary>
		int m_idx = 0;
		
		/// <summary>
		/// This is the fourier transform size, must pow2 number. 
		/// </summary>
		int m_size = 128;
		float m_fsize;
		
		/// <summary>
		/// Size of the variance texture.
		/// </summary>
		int m_varianceSize = 16;
		
		/// <summary>
		/// A higher wind speed gives greater swell to the waves.
		/// </summary>
		float m_windSpeed = 8.0f;
		
		/// <summary>
		/// Scales the height of the waves.
		/// </summary>
		float m_waveAmp = 1.0f;
		
		/// <summary>
		/// A lower number means the waves last longer and 
		/// will build up larger waves.
		/// </summary>
		float m_omega = 0.84f;

		/// <summary>
		/// The amount of foam.
		/// </summary>
		public float whiteCapStr { get { return m_whiteCapStr; } }
		float m_whiteCapStr = 0.5f;
		
		/// <summary>
		/// Used for uv offset. 
		/// </summary>
		Vector4 m_offset;
		
		/// <summary>
		/// The waves are made up of 4 layers of heights
		/// at different wave lengths. These grid sizes 
		/// are basically the wave length for each layer.
		/// </summary>
		public Vector4 gridSizes { get { return m_gridSizes; } }
		Vector4 m_gridSizes = new Vector4(893, 101, 21, 11);
		Vector4 m_inverseGridSizes;

		/// <summary>
		/// Amount of displacment for each grid on the xz axis.
		/// </summary>
		public Vector4 choppyness { get { return m_choppyness; } }
		Vector4 m_choppyness = new Vector4(2.3f, 2.1f, 1.3f, 0.9f);
		
		/// <summary>
		/// Number of mipmap levels in the height maps.
		/// </summary>
		public float mipMapLevels { get { return m_mipMapLevels; } }
		float m_mipMapLevels;
		
		/// <summary>
		/// Material to initilize the spectrum before fourier transform.
		/// </summary>
		Material m_initSpectrumMat, m_initDisplacementMat, m_initJacobiansMat, m_whiteCapsPrecomputeMat;
		
		/// <summary>
		/// The fourier buffers and spectrum data.
		/// </summary>
		RenderTexture m_spectrum01, m_spectrum23;
		RenderTexture[] m_fourierBuffer0, m_fourierBuffer1, m_fourierBuffer2, m_fourierBuffer3, m_fourierBuffer4, m_fourierBuffer5, m_fourierBuffer6, m_fourierBuffer7;
		RenderTexture m_WTable;
		
		/// <summary>
		/// The maps holding the height and slope data.
		/// </summary>
		public RenderTexture map0 { get { return m_map0; } }
		public RenderTexture map1 { get { return m_map1; } }
		public RenderTexture map2 { get { return m_map2; } }
		public RenderTexture map3 { get { return m_map3; } }
		public RenderTexture map4 { get { return m_map4; } }
		RenderTexture m_map0, m_map1, m_map2, m_map3, m_map4, m_map5, m_map6;

		/// <summary>
		/// The render textures holding the foam data.
		/// </summary>
		public RenderTexture foam0 { get { return m_foam0; } }
		public RenderTexture foam1 { get { return m_foam1; } }
		RenderTexture m_foam0, m_foam1;
		
		/// <summary>
		/// The variance for the BRDF.
		/// </summary>
		public RenderTexture variance { get { return m_variance; } }
		RenderTexture m_variance;
		
		/// <summary>
		/// Fourier object to do the fourier transform.
		/// </summary>
		FourierGPU m_fourier;
		
		/// <summary>
		/// Compute the variance for the BRDF.
		/// </summary>
		ComputeShader m_varianceShader;
		
		/// <summary>
		/// Used to write data to the GPU.
		/// </summary>
		ComputeShader m_writeData;

		public WaveSpectrumGPU(int size, float windSpeed, float waveAmp, float omega, int ansio, int foamAnsio, Vector4 gridSzes, float whiteCapStr, Vector4 choppynesss)
		{
			
			//Fourier size can not be greater than 256 because the
			//FourierGPU object uses a 8 bit texture for the 
			//butterfly look up table. This limitation could be
			//removed in theory if a look up table with more bits is used.
			if (size > 256)
			{
				Debug.Log("Fourier grid size must not be greater than 256, changing to 256");
				size = 256;
			}
			
			if (!Mathf.IsPowerOfTwo(size))
			{
				Debug.Log("Fourier grid size must be pow2 number, changing to nearest pow2 number");
				size = Mathf.NextPowerOfTwo(size);
			}
			
			//Load the init spectrum shader. Must be in the resources folder.
			Shader initSpectrumShader = Shader.Find("Ocean/InitSpectrum");
			if (initSpectrumShader == null)
				throw new InvalidOperationException("Could not find shader Ocean/InitSpectrum in the resources folder");
			m_initSpectrumMat = new Material(initSpectrumShader);

			Shader initDisplacementShader = Shader.Find("Ocean/InitDisplacement");
			if(initDisplacementShader == null) 
				throw new InvalidOperationException("Could not find shader Ocean/InitDisplacement in the resources folder");
			m_initDisplacementMat = new Material(initDisplacementShader);
			
			Shader initJacobiansShader = Shader.Find("Ocean/InitJacobians");
			if(initJacobiansShader == null) 
				throw new InvalidOperationException("Could not find shader Ocean/InitJacobians in the resources folder");
			m_initJacobiansMat = new Material(initJacobiansShader);
			
			Shader whiteCapsPrecomputeShader = Shader.Find("Ocean/WhiteCapsPrecompute");
			if(whiteCapsPrecomputeShader == null) 
				throw new InvalidOperationException("Could not find shader Ocean/WhiteCapsPrecompute in the resources folder");
			m_whiteCapsPrecomputeMat = new Material(whiteCapsPrecomputeShader);
			
			m_varianceShader = Resources.Load<ComputeShader>("Variance");
			if(m_varianceShader == null)
				throw new InvalidOperationException("Could not find Variance compute shader in the resources folder");
			
			m_writeData = Resources.Load<ComputeShader>("WriteData");
			if(m_writeData == null)
				throw new InvalidOperationException("Could not find Write compute shader in the resources folder");
			
			m_size = size;
			m_waveAmp = waveAmp;
			m_windSpeed = windSpeed;
			m_omega = omega;
			m_gridSizes = gridSzes;
			m_choppyness = choppyness;
			m_whiteCapStr = whiteCapStr;
			
			m_fsize = (float)m_size;
			m_mipMapLevels = Mathf.Log(m_fsize) / Mathf.Log(2.0f);
			m_offset = new Vector4(1.0f + 0.5f / m_fsize, 1.0f + 0.5f / m_fsize, 0, 0);

			float factor = 2.0f * Mathf.PI * m_fsize;
			m_inverseGridSizes = new Vector4(factor / m_gridSizes.x, factor / m_gridSizes.y, factor / m_gridSizes.z, factor / m_gridSizes.w);
			
			m_fourier = new FourierGPU(m_size);
			
			CreateRenderTextures(ansio, foamAnsio);
			GenerateWavesSpectrum();
			CreateWTable();
			
			m_initSpectrumMat.SetTexture("_Spectrum01", m_spectrum01);
			m_initSpectrumMat.SetTexture("_Spectrum23", m_spectrum23);
			m_initSpectrumMat.SetTexture("_WTable", m_WTable);
			m_initSpectrumMat.SetVector("_Offset", m_offset);
			m_initSpectrumMat.SetVector("_InverseGridSizes", m_inverseGridSizes);
			
			m_initDisplacementMat.SetVector("_InverseGridSizes", m_inverseGridSizes);
			
			m_initJacobiansMat.SetTexture("_Spectrum01", m_spectrum01);
			m_initJacobiansMat.SetTexture("_Spectrum23", m_spectrum23);
			m_initJacobiansMat.SetTexture("_WTable", m_WTable);
			m_initJacobiansMat.SetVector("_Offset", m_offset);
			m_initJacobiansMat.SetVector("_InverseGridSizes", m_inverseGridSizes);
			
		}

		/// <summary>
		/// Must call release before object is destroyed else the
		/// data in the render textures will be leaked.
		/// </summary>
		public void Release()
		{
			m_map0.Release();
			m_map1.Release();
			m_map2.Release();
			m_map3.Release();
			m_map4.Release();
			
			m_foam0.Release();
			m_foam1.Release();
			
			m_spectrum01.Release();
			m_spectrum23.Release();
			
			m_WTable.Release();
			m_variance.Release();
			
			for(int i = 0; i < 2; i++)
			{
				m_fourierBuffer0[i].Release();
				m_fourierBuffer1[i].Release();
				m_fourierBuffer2[i].Release();
				m_fourierBuffer3[i].Release();
				m_fourierBuffer4[i].Release();
				m_fourierBuffer5[i].Release();
				m_fourierBuffer6[i].Release();
				m_fourierBuffer7[i].Release();
				
			}
		}

		/// <summary>
		/// Create all the textures need to hold the data.
		/// </summary>
		void CreateRenderTextures(int ansio, int foamAniso)
		{
			RenderTextureFormat mapFormat = RenderTextureFormat.ARGBFloat;
			RenderTextureFormat format = RenderTextureFormat.ARGBFloat;
			
			//These texture hold the actual data use in the ocean renderer
			CreateMap(ref m_map0, mapFormat, ansio); 
			CreateMap(ref m_map1, mapFormat, ansio); 
			CreateMap(ref m_map2, mapFormat, ansio); 
			CreateMap(ref m_map3, mapFormat, ansio); 
			CreateMap(ref m_map4, mapFormat, ansio); 

			CreateMap(ref m_foam0, format, foamAniso);
			CreateMap(ref m_foam1, format, foamAniso);
			
			//These textures are used to perform the fourier transform
			CreateBuffer(ref m_fourierBuffer0, format);//heights
			CreateBuffer(ref m_fourierBuffer1, format);// slopes X
			CreateBuffer(ref m_fourierBuffer2, format);// slopes Y
			CreateBuffer(ref m_fourierBuffer3, format);// displacement X
			CreateBuffer(ref m_fourierBuffer4, format);// displacement Y
			CreateBuffer(ref m_fourierBuffer5, format);// Jacobians XX
			CreateBuffer(ref m_fourierBuffer6, format);// Jacobians YY
			CreateBuffer(ref m_fourierBuffer7, format);// Jacobians XY
			
			//These textures hold the specturm the fourier transform is performed on
			m_spectrum01 = new RenderTexture(m_size, m_size, 0, format);
			m_spectrum01.filterMode = FilterMode.Point;
			m_spectrum01.wrapMode = TextureWrapMode.Repeat;
			m_spectrum01.enableRandomWrite = true;
			m_spectrum01.Create();
			
			m_spectrum23 = new RenderTexture(m_size, m_size, 0, format);
			m_spectrum23.filterMode = FilterMode.Point;
			m_spectrum23.wrapMode = TextureWrapMode.Repeat;	
			m_spectrum23.enableRandomWrite = true;
			m_spectrum23.Create();
			
			m_WTable = new RenderTexture(m_size, m_size, 0, format);
			m_WTable.filterMode = FilterMode.Point;
			m_WTable.wrapMode = TextureWrapMode.Clamp;
			m_WTable.enableRandomWrite = true;
			m_WTable.Create();
			
			m_variance = new RenderTexture(m_varianceSize, m_varianceSize, 0, RenderTextureFormat.RGFloat);
			m_variance.volumeDepth = m_varianceSize;
			m_variance.wrapMode = TextureWrapMode.Clamp;
			m_variance.filterMode = FilterMode.Bilinear;
			m_variance.isVolume = true;
			m_variance.enableRandomWrite = true;
			m_variance.useMipMap = true;
			m_variance.Create();

		}
		
		void CreateBuffer(ref RenderTexture[] tex, RenderTextureFormat format)
		{
			tex = new RenderTexture[2];
			
			for(int i = 0; i < 2; i++)
			{
				tex[i] = new RenderTexture(m_size, m_size, 0, format);
				tex[i].filterMode = FilterMode.Point;
				tex[i].wrapMode = TextureWrapMode.Clamp;
				tex[i].Create();
			}
		}
		
		void CreateMap(ref RenderTexture map, RenderTextureFormat format, int ansio)
		{
			map = new RenderTexture(m_size, m_size, 0, format);
			map.filterMode = FilterMode.Trilinear;
			map.wrapMode = TextureWrapMode.Repeat;
			map.anisoLevel = ansio;
			map.useMipMap = true;
			map.Create();
		}
		
		float sqr(float x) { return x*x; }

		float omega(float k) { return Mathf.Sqrt(9.81f * k * (1.0f + sqr(k / WAVE_KM))); } // Eq 24

		/// <summary>
		/// I know this is a big chunk of ugly math but dont worry to much about what it all means
		/// It recreates a statistcally representative model of a wave spectrum in the frequency domain.
		/// </summary>
		float Spectrum(float kx, float ky, bool omnispectrum = false)
		{

			float U10 = m_windSpeed;

			// phase speed
			float k = Mathf.Sqrt(kx * kx + ky * ky);
			float c = omega(k) / k;
			
			// spectral peak
			float kp = 9.81f * sqr(m_omega / U10); // after Eq 3
			float cp = omega(kp) / kp;
		
			// friction velocity
			float z0 = 3.7e-5f * sqr(U10) / 9.81f * Mathf.Pow(U10 / cp, 0.9f); // Eq 66
			float u_star = 0.41f * U10 / Mathf.Log(10.0f / z0); // Eq 60
		
			float Lpm = Mathf.Exp(- 5.0f / 4.0f * sqr(kp / k)); // after Eq 3
			float gamma = (m_omega < 1.0f) ? 1.7f : 1.7f + 6.0f * Mathf.Log(m_omega); // after Eq 3 // log10 or log?
			float sigma = 0.08f * (1.0f + 4.0f / Mathf.Pow(m_omega, 3.0f)); // after Eq 3
			float Gamma = Mathf.Exp(-1.0f / (2.0f * sqr(sigma)) * sqr(Mathf.Sqrt(k / kp) - 1.0f));
			float Jp = Mathf.Pow(gamma, Gamma); // Eq 3
			float Fp = Lpm * Jp * Mathf.Exp(-m_omega / Mathf.Sqrt(10.0f) * (Mathf.Sqrt(k / kp) - 1.0f)); // Eq 32
			float alphap = 0.006f * Mathf.Sqrt(m_omega); // Eq 34
			float Bl = 0.5f * alphap * cp / c * Fp; // Eq 31
		
			float alpham = 0.01f * (u_star < WAVE_CM ? 1.0f + Mathf.Log(u_star / WAVE_CM) : 1.0f + 3.0f * Mathf.Log(u_star / WAVE_CM)); // Eq 44
			float Fm = Mathf.Exp(-0.25f * sqr(k / WAVE_KM - 1.0f)); // Eq 41
			float Bh = 0.5f * alpham * WAVE_CM / c * Fm * Lpm; // Eq 40 (fixed)
			
			if(omnispectrum) return m_waveAmp * (Bl + Bh) / (k * sqr(k)); // Eq 30
		
			float a0 = Mathf.Log(2.0f) / 4.0f; 
			float ap = 4.0f; 
			float am = 0.13f * u_star / WAVE_CM; // Eq 59
			float Delta = (float)System.Math.Tanh(a0 + ap * Mathf.Pow(c / cp, 2.5f) + am * Mathf.Pow(WAVE_CM / c, 2.5f)); // Eq 57
		
			float phi = Mathf.Atan2(ky, kx);
		
			if (kx < 0.0f) return 0.0f;
		
			Bl *= 2.0f;
			Bh *= 2.0f;
		
			// remove waves perpendicular to wind dir
			float tweak = Mathf.Sqrt(Mathf.Max(kx/Mathf.Sqrt(kx*kx+ky*ky),0.0f));
		
			return m_waveAmp * (Bl + Bh) * (1.0f + Delta * Mathf.Cos(2.0f * phi)) / (2.0f * Mathf.PI * sqr(sqr(k))) * tweak; // Eq 67
		}
		
		Vector2 GetSpectrumSample(float i, float j, float lengthScale, float kMin)
		{
			float dk = 2.0f * Mathf.PI / lengthScale;
			float kx = i * dk;
			float ky = j * dk;
			Vector2 result = new Vector2(0.0f,0.0f);
			
			float rnd = UnityEngine.Random.value;
			
			if(Mathf.Abs(kx) >= kMin || Mathf.Abs(ky) >= kMin)
			{
				float S = Spectrum(kx, ky);
				float h = Mathf.Sqrt(S / 2.0f) * dk;
							
				float phi = rnd * 2.0f * Mathf.PI;
				result.x = h * Mathf.Cos(phi);
				result.y = h * Mathf.Sin(phi);
			}
			
			return result;
		}
		
		float GetSlopeVariance(float kx, float ky, Vector2 spectrumSample)
		{
			float kSquare = kx * kx + ky * ky;
			float real = spectrumSample.x;
			float img = spectrumSample.y;
			float hSquare = real * real + img * img;
			return kSquare * hSquare * 2.0f;
		}

		/// <summary>
		/// Generates the wave spectrum based on the 
		/// settings wind speed, wave amp and wave age.
		/// If these values change this function must be called again.
		/// </summary>
		void GenerateWavesSpectrum()
		{
			
			// Slope variance due to all waves, by integrating over the full spectrum.
			float theoreticSlopeVariance = 0.0f;
			float k = 5e-3f;
			while (k < 1e3f) 
			{
				float nextK = k * 1.001f;
				theoreticSlopeVariance += k * k * Spectrum(k, 0, true) * (nextK - k);
				k = nextK;
			}

			float[] spectrum01 = new float[m_size*m_size*4];
			float[] spectrum23 = new float[m_size*m_size*4];

			int idx;
			float i;
			float j;
			float totalSlopeVariance = 0.0f;
			Vector2 sample12XY;
			Vector2 sample12ZW;
			Vector2 sample34XY;
			Vector2 sample34ZW;
			
			UnityEngine.Random.seed = 0;
			
			for (int x = 0; x < m_size; x++) 
			{
				for (int y = 0; y < m_size; y++) 
				{
					idx = x+y*m_size;
					i = (x >= m_size / 2) ? (float)(x - m_size) : (float)x;
					j = (y >= m_size / 2) ? (float)(y - m_size) : (float)y;
					
					sample12XY = GetSpectrumSample(i, j, m_gridSizes.x, Mathf.PI / m_gridSizes.x);
					sample12ZW = GetSpectrumSample(i, j, m_gridSizes.y, Mathf.PI * m_fsize / m_gridSizes.x);
					sample34XY = GetSpectrumSample(i, j, m_gridSizes.z, Mathf.PI * m_fsize / m_gridSizes.y);
					sample34ZW = GetSpectrumSample(i, j, m_gridSizes.w, Mathf.PI * m_fsize / m_gridSizes.z);
					
					spectrum01[idx*4+0] = sample12XY.x;
					spectrum01[idx*4+1] = sample12XY.y;
					spectrum01[idx*4+2] = sample12ZW.x;
					spectrum01[idx*4+3] = sample12ZW.y;
					
					spectrum23[idx*4+0] = sample34XY.x;
					spectrum23[idx*4+1] = sample34XY.y;
					spectrum23[idx*4+2] = sample34ZW.x;
					spectrum23[idx*4+3] = sample34ZW.y;
					
					i *= 2.0f * Mathf.PI;
					j *= 2.0f * Mathf.PI;
					
					totalSlopeVariance += GetSlopeVariance(i / m_gridSizes.x, j / m_gridSizes.x, sample12XY);
					totalSlopeVariance += GetSlopeVariance(i / m_gridSizes.y, j / m_gridSizes.y, sample12ZW);
					totalSlopeVariance += GetSlopeVariance(i / m_gridSizes.z, j / m_gridSizes.z, sample34XY);
					totalSlopeVariance += GetSlopeVariance(i / m_gridSizes.w, j / m_gridSizes.w, sample34ZW);
				}
			}
			
			//Write floating point data into render texture
			ComputeBuffer buffer = new ComputeBuffer(m_size*m_size, sizeof(float)*4);
			
			buffer.SetData(spectrum01);
			CBUtility.WriteIntoRenderTexture(m_spectrum01, 4, buffer, m_writeData);
			
			buffer.SetData(spectrum23);
			CBUtility.WriteIntoRenderTexture(m_spectrum23, 4, buffer, m_writeData);
			
			buffer.Release();
			
			m_varianceShader.SetFloat("_SlopeVarianceDelta", 0.5f * (theoreticSlopeVariance - totalSlopeVariance));
			m_varianceShader.SetFloat("_VarianceSize", (float)m_varianceSize);
			m_varianceShader.SetFloat("_Size", m_fsize);
			m_varianceShader.SetVector("_GridSizes", m_gridSizes);
			m_varianceShader.SetTexture(0, "_Spectrum01", m_spectrum01);
			m_varianceShader.SetTexture(0, "_Spectrum23", m_spectrum23);
			m_varianceShader.SetTexture(0, "des", m_variance);
			
			m_varianceShader.Dispatch(0,m_varianceSize/4,m_varianceSize/4,m_varianceSize/4);

		}

		/// <summary>
		/// Some of the values needed in the InitWaveSpectrum function can be precomputed.
		/// If the grid sizes change this function must called again.
		/// </summary>
		void CreateWTable()
		{
			//Some values need for the InitWaveSpectrum function can be precomputed
			Vector2 uv, st;
			float k1, k2, k3, k4, w1, w2, w3, w4;
			
			float[] table = new float[m_size*m_size*4];

			for (int x = 0; x < m_size; x++) 
			{
				for (int y = 0; y < m_size; y++) 
				{
					uv = new Vector2(x,y) / m_fsize;

		    		st.x = uv.x > 0.5f ? uv.x - 1.0f : uv.x;
		    		st.y = uv.y > 0.5f ? uv.y - 1.0f : uv.y;
		
			    	k1 = (st * m_inverseGridSizes.x).magnitude;
			    	k2 = (st * m_inverseGridSizes.y).magnitude;
			    	k3 = (st * m_inverseGridSizes.z).magnitude;
			    	k4 = (st * m_inverseGridSizes.w).magnitude;
					
					w1 = Mathf.Sqrt(9.81f * k1 * (1.0f + k1 * k1 / (WAVE_KM*WAVE_KM)));
					w2 = Mathf.Sqrt(9.81f * k2 * (1.0f + k2 * k2 / (WAVE_KM*WAVE_KM)));
					w3 = Mathf.Sqrt(9.81f * k3 * (1.0f + k3 * k3 / (WAVE_KM*WAVE_KM)));
					w4 = Mathf.Sqrt(9.81f * k4 * (1.0f + k4 * k4 / (WAVE_KM*WAVE_KM)));
					
					table[(x+y*m_size)*4+0] = w1;
					table[(x+y*m_size)*4+1] = w2;
					table[(x+y*m_size)*4+2] = w3;
					table[(x+y*m_size)*4+3] = w4;
				
				}
			}
					
			//Write floating point data into render texture
			ComputeBuffer buffer = new ComputeBuffer(m_size*m_size, sizeof(float)*4);
			
			buffer.SetData(table);
			CBUtility.WriteIntoRenderTexture(m_WTable, 4, buffer, m_writeData);
			
			buffer.Release();
		
		}
			
		/// <summary>
		/// Initilize the spectrum for a time period.
		/// </summary>
		void InitWaveSpectrum(float t)
		{
			// init heights (0) and slopes (1,2)
			RenderTexture[] buffers012 = new RenderTexture[] { m_fourierBuffer0[1], m_fourierBuffer1[1], m_fourierBuffer2[1] };

			m_initSpectrumMat.SetFloat("_T", t);
			
			RTUtility.MultiTargetBlit(buffers012, m_initSpectrumMat);
			
			// Init displacement (3,4)
			RenderTexture[] buffers34 = new RenderTexture[] { m_fourierBuffer3[1], m_fourierBuffer4[1] };
			
			m_initDisplacementMat.SetTexture("_Buffer1", m_fourierBuffer1[1]);
			m_initDisplacementMat.SetTexture("_Buffer2", m_fourierBuffer2[1]);
			
			RTUtility.MultiTargetBlit(buffers34, m_initDisplacementMat);
			
			// Init jacobians (5,6,7)
			RenderTexture[] buffers567 = new RenderTexture[] { m_fourierBuffer5[1], m_fourierBuffer6[1], m_fourierBuffer7[1] };

			m_initJacobiansMat.SetFloat("_T", t);
			
			RTUtility.MultiTargetBlit(buffers567, m_initJacobiansMat);
		}

		/// <summary>
		/// Simulates the waves for time period.
		/// </summary>
		public void SimulateWaves(float t)
		{
			
			InitWaveSpectrum(t);

			m_idx = m_fourier.PeformFFT(m_fourierBuffer0, m_fourierBuffer1, m_fourierBuffer2);
			
			m_fourier.PeformFFT(m_fourierBuffer3, m_fourierBuffer4);
			
			m_fourier.PeformFFT(m_fourierBuffer5, m_fourierBuffer6, m_fourierBuffer7);

			Graphics.Blit(m_fourierBuffer0[m_idx], m_map0);
			Graphics.Blit(m_fourierBuffer1[m_idx], m_map1);
			Graphics.Blit(m_fourierBuffer2[m_idx], m_map2);
			Graphics.Blit(m_fourierBuffer3[m_idx], m_map3);
			Graphics.Blit(m_fourierBuffer4[m_idx], m_map4);
			
			m_whiteCapsPrecomputeMat.SetTexture("_Map5", m_fourierBuffer5[m_idx]);
			m_whiteCapsPrecomputeMat.SetTexture("_Map6", m_fourierBuffer6[m_idx]);
			m_whiteCapsPrecomputeMat.SetTexture("_Map7", m_fourierBuffer7[m_idx]);
			m_whiteCapsPrecomputeMat.SetVector("_Choppyness", m_choppyness);

			Graphics.Blit(null, m_foam0, m_whiteCapsPrecomputeMat, 0); 
			Graphics.Blit(null, m_foam1, m_whiteCapsPrecomputeMat, 1);
				
		}
		
	}

}















