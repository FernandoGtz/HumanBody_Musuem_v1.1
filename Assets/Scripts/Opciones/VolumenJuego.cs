using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumenJuego : MonoBehaviour
{
    public Slider slider;
    public float sliderValue;
    public RawImage imagenMute;

    // Start is called before the first frame update
    void Start()
    {
        slider.value = PlayerPrefs.GetFloat("VolumenAudio", 0.9f);
        AudioListener.volume = slider.value;
        RevisarSiEstoyEnMute();
    }

    public void ChangeSlider(float valor)
    {
        sliderValue = valor;
        PlayerPrefs.SetFloat("volumenAudio", sliderValue);
        AudioListener.volume = slider.value;
        RevisarSiEstoyEnMute();
    }


    public void RevisarSiEstoyEnMute()
    {
        if(sliderValue == 0)
        {
            imagenMute.enabled = true;
        }
        else 
        {
            imagenMute.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
