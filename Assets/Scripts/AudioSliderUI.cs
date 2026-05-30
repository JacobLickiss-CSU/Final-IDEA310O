using UnityEngine;
using UnityEngine.UI;

public class AudioSliderUI : MonoBehaviour
{
    public string param;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<Slider>().value = AudioSlider.Instance.GetVolume(param);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetValue(float value)
    {
        AudioSlider.Instance.SetVolume(param, value);
    }
}
