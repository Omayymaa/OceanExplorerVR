using UnityEngine;

public class SoundFX : MonoBehaviour
{
    public static SoundFX Instance;

    public AudioClip binHit;
    public AudioClip correct;
    public AudioClip wrong;
    [Range(0f, 1f)] public float volume = 0.8f;

    AudioSource src;

    void Awake()
    {
        Instance = this;

        src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = 0f;

        if (binHit == null) binHit = Thud(95f, 0.22f);
        if (correct == null) correct = Tone(new float[] { 523f, 659f, 784f }, 0.13f);
        if (wrong == null) wrong = Tone(new float[] { 196f, 147f }, 0.2f);
    }

    AudioClip Tone(float[] freqs, float noteLength)
    {
        int rate = 44100;
        int perNote = (int)(rate * noteLength);
        float[] data = new float[perNote * freqs.Length];

        for (int n = 0; n < freqs.Length; n++)
            for (int i = 0; i < perNote; i++)
            {
                float t = (float)i / rate;
                float env = Mathf.Clamp01(1f - (float)i / perNote);
                data[n * perNote + i] = Mathf.Sin(2f * Mathf.PI * freqs[n] * t) * env * 0.5f;
            }

        return Make(data, rate);
    }

    AudioClip Thud(float freq, float length)
    {
        int rate = 44100;
        int count = (int)(rate * length);
        float[] data = new float[count];
        float phase = 0f;

        for (int i = 0; i < count; i++)
        {
            float k = (float)i / count;
            float f = Mathf.Lerp(freq, freq * 0.5f, k);
            phase += 2f * Mathf.PI * f / rate;
            data[i] = Mathf.Sin(phase) * Mathf.Pow(1f - k, 2f) * 0.7f;
        }

        return Make(data, rate);
    }

    AudioClip Make(float[] data, int rate)
    {
        AudioClip clip = AudioClip.Create("fx", data.Length, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    void PlayClip(AudioClip c)
    {
        if (c != null && src != null) src.PlayOneShot(c, volume);
    }

    public static void BinHit() { if (Instance != null) Instance.PlayClip(Instance.binHit); }
    public static void Correct() { if (Instance != null) Instance.PlayClip(Instance.correct); }
    public static void Wrong() { if (Instance != null) Instance.PlayClip(Instance.wrong); }
}