using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Current implementation is to play the music of the current scene and then self-destruct, probably changed later to coordinate with fade in/out
/// </summary>
public class MusicQueuer : MonoBehaviour
{
    [SerializeField] LoopableAudioTrack nextTrack = default;
    
    private void Start()
    {
        QueueAndTrigger();
        Destroy(gameObject);
    }

    // Start is called before the first frame update
    public void QueueMusic()
    {
        AudioManager.instance?.MusicManager.SetNextTrack(nextTrack);
    }

    public void QueueAndTrigger()
    {
        QueueMusic();
        AudioManager.instance?.MusicManager.StartTransition();
    }
}
