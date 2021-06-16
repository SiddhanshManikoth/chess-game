using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundEffects : MonoBehaviour
{
    public AudioSource Btnfx;
    public AudioClip Hover;
    public AudioClip Selected;

    public void selectedBtn()
    {
        Btnfx.PlayOneShot(Selected);


    }

    public void HoverBtn()
    {
        Btnfx.PlayOneShot(Hover);

    }


}
