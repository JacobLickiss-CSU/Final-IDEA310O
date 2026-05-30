using UnityEngine;
using UnityEngine.Audio;

public class AudioSlider : MonoBehaviour
{
    public static AudioSlider Instance;

    public float MaxValue;

    public AudioMixer mixer;

    public string effectsName;

    public string musicName;

    // Update is called once per frame
    void Update()
    {
        
    }

    // See https://discussions.unity.com/t/how-do-i-make-a-change-a-volume-with-a-slider/154975/4
    void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        float savedMusicVol = PlayerPrefs.GetFloat(musicName, MaxValue);
        SetMusicVolume(savedMusicVol); //Manually set value & volume before subscribing to ensure it is set even if slider.value happens to start at the same value as is saved
        //slider.value = savedVol;
        //slider.onValueChanged.AddListener((float _) => SetVolume(_)); //UI classes use unity events, requiring delegates (delegate(float _) { SetVolume(_); }) or lambda expressions ((float _) => SetVolume(_))

        float savedEffectsVol = PlayerPrefs.GetFloat(effectsName, MaxValue);
        SetEffectsVolume(savedEffectsVol);
    }

    // See https://discussions.unity.com/t/how-do-i-make-a-change-a-volume-with-a-slider/154975/4
    public void SetMusicVolume(float _value)
    {
        Debug.Log("Set music volume to " + _value);
        mixer.SetFloat(musicName, ConvertToDecibel(_value / MaxValue)); //Dividing by max allows arbitrary positive slider maxValue
        PlayerPrefs.SetFloat(musicName, _value);
    }

    // See https://discussions.unity.com/t/how-do-i-make-a-change-a-volume-with-a-slider/154975/4
    public void SetEffectsVolume(float _value)
    {
        Debug.Log("Set effects volume to " + _value);
        mixer.SetFloat(effectsName, ConvertToDecibel(_value / MaxValue)); //Dividing by max allows arbitrary positive slider maxValue
        PlayerPrefs.SetFloat(effectsName, _value);
    }

    public float GetVolume(string param)
    {
        return PlayerPrefs.GetFloat(param, MaxValue);
    }

    public void SetVolume(string param, float _value)
    {
        mixer.SetFloat(param, ConvertToDecibel(_value / MaxValue)); //Dividing by max allows arbitrary positive slider maxValue
        PlayerPrefs.SetFloat(param, _value);
    }

    // See https://discussions.unity.com/t/how-do-i-make-a-change-a-volume-with-a-slider/154975/4
    public float ConvertToDecibel(float _value)
    {
        return Mathf.Log10(Mathf.Max(_value, 0.0001f)) * 20f;
    }
}
