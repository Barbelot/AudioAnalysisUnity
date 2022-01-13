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

    [Header("Analysis")]
    public int windowSize = 1024;
    public float timeStep = 0.1f;

    [Header("Audio Amplitude")]
    public float amplitude;

    [Header("Audio Spectrum")]
    public float spectrumScale = 1;
    public float[] spectrum = new float[1024];

    [Header("Visual Effects")]
    public string vfxPropertyName = "SpectrumTexture";
    public List<VisualEffect> vfx;

    [Range(0, 1)] public float spectrumDisplayRange = 1;

    float[] _amplitude = new float[1];
    float[] _clipData = new float[2048];
    Complex[] _dataComplex = new Complex[2048];

    GraphicsBuffer _spectrumGraphicsBuffer;

    bool _initialized = false;

    private float _timeStepTimer = 0;

	private void OnEnable() {

        if (!_initialized)
            Initialize();
    }

	private void Update() {

        if (!clip)
            return;

        if (useDirector && !director)
            return;

        _timeStepTimer += Time.deltaTime;
        if (_timeStepTimer < timeStep)
            return;

        _timeStepTimer = 0;

        if (spectrum.Length != windowSize || _clipData.Length != windowSize * 2 || _dataComplex.Length != windowSize * 2)
            Initialize();

		if (useDirector) {
            clip.GetData(_clipData, Mathf.Clamp((int)(director.time * clip.frequency) - windowSize, 0, clip.samples - windowSize * 2));
        } else {
            clip.GetData(_clipData, Mathf.Clamp((int)(time * clip.frequency) - windowSize, 0, clip.samples - windowSize * 2));
        }

        //Compute amplitude
        amplitude = 0;
        for(int i=0; i<_clipData.Length; i++)
            amplitude += Mathf.Abs(_clipData[i]);
        amplitude /= _clipData.Length;

        // copy the output data into the complex array
        for (int i = 0; i < _clipData.Length; i++) {
            _dataComplex[i] = new Complex(_clipData[i] * GetWindowCoefficient(i), 0);
        }

        // calculate the FFT
        FFT.CalculateFFT(_dataComplex, spectrum, false);

        //Update Graphics buffer
        _spectrumGraphicsBuffer.SetData(spectrum);
    }

	private void OnDisable() {

        if (_initialized)
            CleanUp();
	}

    void CreateSpectrumBuffer() {

        if (_spectrumGraphicsBuffer != null)
            _spectrumGraphicsBuffer.Release();

        _spectrumGraphicsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, spectrum.Length, sizeof(float));
        
    }

	float GetWindowCoefficient(int position) {

        return 0.5f - 0.5f * Mathf.Cos(2 * Mathf.PI * position / (windowSize * 2.0f - 1));
	}

    void Initialize() {

        windowSize = Mathf.ClosestPowerOfTwo(windowSize);

        spectrum = new float[windowSize];
        _clipData = new float[windowSize * 2];
        _dataComplex = new Complex[windowSize * 2];

        CreateSpectrumBuffer();

        foreach (var v in vfx) {
            v.SetGraphicsBuffer("SpectrumBuffer", _spectrumGraphicsBuffer);
        }

        _initialized = true;
    }

    void CleanUp() {

        if (_spectrumGraphicsBuffer != null)
            _spectrumGraphicsBuffer.Release();

        _initialized = false;
    }
}
