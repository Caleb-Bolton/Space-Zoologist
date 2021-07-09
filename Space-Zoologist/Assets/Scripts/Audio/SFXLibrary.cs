using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SFXType { None, Unpause, Pause, FastForward, MenuOpen, MenuClose, Notification, TabSwitch, Dirt, NumTypes }

[CreateAssetMenu]
public class SFXLibrary : ScriptableObject
{

    /// <summary>
    /// A contianer for Audioclips that keeps track of the index of the next clip that should be played.
    /// Used for SFX
    /// </summary>
    [System.Serializable]
    public class AudioObject
    {
        [System.NonSerialized] public int index = 0;
        public SFXType type;
        public AudioClip[] clips;
    }


    public AudioObject[] SoundEffects => soundEffects;
    [SerializeField] AudioObject[] soundEffects;


    private void OnValidate()
    {
        AudioObject[] temp = new AudioObject[(int)SFXType.NumTypes];
        foreach (AudioObject audio in soundEffects)
        {
            if (audio.type == SFXType.NumTypes) continue;
            temp[(int)audio.type] = audio;
        }

        for (int i = 0; i < temp.Length; i++)
        {
            if (temp[i] == null)
            {
                temp[i] = new AudioObject();
                temp[i].type = (SFXType)i;
            }
        }
        soundEffects = temp;
    }
}
