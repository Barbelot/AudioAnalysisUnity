using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using B83.MathHelpers;
using UnityEngine.Playables;
using UnityEngine.VFX;

public class AudioClipSpectrum : MonoBehaviour
{
    [Header("AudioClip")]
    public AudioClip clip;

    public float time = 0;

    [Header("Timeline")]
    public bool useDirector = false;
    public PlayableDirector director;

    [Header("Spectrum")]
    public int spectrumSize = 1024;
    public float spectrumScale = 1;
    public float[] spectrum = new float[1024];
    public RenderTexture spectrumTexture;

    [Header("Compute Shader")]
    public ComputeShader spectrumTextureShader;

    [Header("Stuff to bind")]
    public List<VisualEffect> vfx;
    public string materialPropertyName = "_SpectrumTexture";
    public List<Material> materials;

    [Range(0, 1)] public float spectrumDisplayRange = 1;

    float[] _clipData = new float[2048];
    Complex[] _dataComplex = new Complex[2048];

    ComputeBuffer _spectrumBuffer;
    const int _numThreads = 8;

    bool _initialized = false;

	private void OnEnable() {

        if (!_initialized)
            Initialize();
    }

	private void Update() {

        if (!clip)
            return;

        if (useDirector && !director)
            return;

        if (spectrum.Length != spectrumSize || _clipData.Length != spectrumSize * 2 || _dataComplex.Length != spectrumSize * 2)
            Initialize();

		if (useDirector) {
            clip.GetData(_clipData, Mathf.Clamp((int)(director.time * clip.frequency) - spectrumSize, 0, clip.samples - spectrumSize*2));
        } else {
            clip.GetData(_clipData, Mathf.Clamp((int)(time * clip.frequency) - spectrumSize, 0, clip.samples - spectrumSize*2));
        }
        
        // copy the output data into the complex array
        for (int i = 0; i < _clipData.Length; i++) {
            _dataComplex[i] = new Complex(_clipData[i] * GetWindowCoefficient(i), 0);
        }

        // calculate the FFT
        FFT.CalculateFFT(_dataComplex, spectrum, false);

        //Update texture
        UpdateSpectrumTexture();
    }

	private void OnDisable() {

        if (_initialized)
            CleanUp();
	}

	void CreateSpectrumTexture() {
        spectrumTexture = new RenderTexture(spectrum.Length, 1, 0, RenderTextureFormat.ARGBFloat);
        spectrumTexture.enableRandomWrite = true;
        spectrumTexture.Create();
    }

    void CreateSpectrumBuffer() {

        if (_spectrumBuffer != null)
            _spectrumBuffer.Release();

        _spectrumBuffer = new ComputeBuffer(spectrum.Length, sizeof(float));
    }

    void UpdateSpectrumTexture() {

        _spectrumBuffer.SetData(spectrum);

        spectrumTextureShader.Dispatch(0, spectrumSize / _numThreads, 1, 1);
    }

	float GetWindowCoefficient(int position) {

        return 0.5f - 0.5f * Mathf.Cos(2 * Mathf.PI * position / (spectrumSize * 2.0f - 1));
	}

    void Initialize() {

        spectrumSize = Mathf.ClosestPowerOfTwo(spectrumSize);

        spectrum = new float[spectrumSize];
        _clipData = new float[spectrumSize * 2];
        _dataComplex = new Complex[spectrumSize * 2];

        CreateSpectrumTexture();
        CreateSpectrumBuffer();

        spectrumTextureShader.SetBuffer(0, "_SpectrumBuffer", _spectrumBuffer);
        spectrumTextureShader.SetTexture(0, "_SpectrumTexture", spectrumTexture);

        foreach (var v in vfx)
            v.SetTexture("Spectrum", spectrumTexture);

        foreach(var m in materials)
            m.SetTexture(materialPropertyName, spectrumTexture);

        _initialized = true;
    }

    void CleanUp() {

        if (spectrumTexture)
            spectrumTexture.Release();

        if (_spectrumBuffer != null)
            _spectrumBuffer.Release();

        _initialized = false;
    }
}
