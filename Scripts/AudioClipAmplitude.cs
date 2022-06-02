using UnityEngine;
using B83.MathHelpers;
using UnityEngine.Playables;

public class AudioClipAmplitude : MonoBehaviour
{
    [Header("AudioClip")]
    public AudioClip clip;

    public float time = 0;

    [Header("Timeline")]
    public bool useDirector = false;
    public PlayableDirector director;
    public float timeLength;

    [Header("Analysis")]
    public int windowSize = 8192;
    public int waveformResolution = 20;
    public float amplitudeScale = 5;
    public bool useWindowCoefficient = false;
    public bool computeAmplitudeAtRuntime = false;

    [Header("Debug")]
    [Tooltip("Show current timeline time but costly. Recommended for debugging purposes only.")] public bool forceInspectorRepaint = false;
    public float currentAmplitude;
    public float currentWaveformValue;
    public float[] waveform;

    float[] _clipData;

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

        if (_clipData.Length != windowSize * 2)
            Initialize();

        if (computeAmplitudeAtRuntime) {
            UpdateAmplitude();
        }

		if (waveform != null && director) {
            if(waveform.Length > 0) {
                currentWaveformValue = GetWaveformHeight(0);
			}
		}
    }

    private void OnDisable() {

        if (_initialized)
            CleanUp();
    }

    public void ComputeCompleteWaveform() {

        Initialize();

        waveform = new float[waveformResolution * clip.samples / clip.frequency];

        for (int i=0; i<waveform.Length; i++) {
            clip.GetData(_clipData, Mathf.Clamp((int)((float)i * clip.frequency / waveformResolution - windowSize), 0, clip.samples - windowSize * 2));

            waveform[i] = 0;
            for (int j = 0; j < _clipData.Length; j++)
                waveform[i] += Mathf.Abs(useWindowCoefficient ? _clipData[j] * GetWindowCoefficient(j) : _clipData[j]);

            waveform[i] /= _clipData.Length;
            waveform[i] *= amplitudeScale;
        }
    }

    void UpdateAmplitude() {

        if (useDirector) {
            clip.GetData(_clipData, Mathf.Clamp((int)(director.time * clip.frequency) - windowSize, 0, clip.samples - windowSize * 2));
        } else {
            clip.GetData(_clipData, Mathf.Clamp((int)(time * clip.frequency) - windowSize, 0, clip.samples - windowSize * 2));
        }

        //Compute amplitude
        currentAmplitude = 0;
        float sum = 0;

        for (int i = 0; i < _clipData.Length; i++) {
            currentAmplitude += Mathf.Abs(useWindowCoefficient ? _clipData[i] * GetWindowCoefficient(i) : _clipData[i]);
            sum += useWindowCoefficient ? GetWindowCoefficient(i) : 1;
        }

        currentAmplitude /= sum;
        currentAmplitude *= amplitudeScale;
    }

    float GetWindowCoefficient(int position) {

        return 0.5f - 0.5f * Mathf.Cos(2 * Mathf.PI * position / (windowSize * 2.0f - 1));
    }

    void Initialize() {

        windowSize = Mathf.ClosestPowerOfTwo(windowSize);

        _clipData = new float[windowSize * 2];

        _initialized = true;
    }

    void CleanUp() {

        _initialized = false;
    }

    public float GetWaveformHeight(float timeOffset) {

        float time = (float)director.time + timeOffset;
		float waveformTimeStep = timeLength / waveform.Length;
		int previousIndex = Mathf.Min(waveform.Length - 2, Mathf.Max(Mathf.FloorToInt(time / waveformTimeStep), 0));
		int nextIndex = Mathf.Max(1, Mathf.Min(Mathf.CeilToInt(time / waveformTimeStep), waveform.Length - 1));

		return Mathf.Lerp(waveform[previousIndex],
						  waveform[nextIndex],
						  Mathf.InverseLerp(previousIndex * waveformTimeStep, nextIndex * waveformTimeStep, time));
	}
}
