using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public LoopableAudioTrack curMusic;
    public LoopableAudioTrack nextMusic;
    public LoopableAudioTrack queuedMusic;

    public AudioSource curMusicSource;
    public AudioSource nextMusicSource;

    bool isInTrasition;
    bool frequentTransition;

    const int TEMPO = 112;                                      // tempo of track, in beats per minute
    const int BEATS_PER_BAR = 2;                                // the tracks have 4 beats per bar, but I'm allowing half-bar transitions
    const float SECONDS_PER_BAR = BEATS_PER_BAR * 60f / TEMPO;  // (beats / bars) * (60 seconds / 1 minute) * (minutes / beats)

    float volume = 1f;

    Coroutine transition = null;

    private void Awake()
    {
        if (curMusicSource == null) {
            curMusicSource = gameObject.AddComponent<AudioSource>();
        }
        if (nextMusicSource == null) {
            nextMusicSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Start()
    {
        isInTrasition = false;
        frequentTransition = false;
        if (!LoopableAudioTrack.IsEmpty(curMusic) && !curMusic.haveStarted) {
            curMusic.StartTrack(curMusicSource);
        }
    }

    public void SetNextTrack(LoopableAudioTrack nextTrack)
    {
        if (nextTrack == curMusic) return;

        if(LoopableAudioTrack.IsEmpty(nextMusic))
            nextMusic = nextTrack;
        else
            queuedMusic = nextTrack; // if there is queued music that isn't played, just overwrite it
    }

    // !! CALL THIS TO SWITCH FROM PROLOGUE MUSIC TO MAIN MENU MUSIC !!
    public void StartTransition()
    {
        if (isInTrasition)
        {
            if (LoopableAudioTrack.IsEmpty(queuedMusic)) return; // cancel if the transition isn't queued up

            nextMusic.StopTrack();
            StopCoroutine(transition);
            nextMusic = queuedMusic;
            queuedMusic = null;
            transition = null;
        }
        if (LoopableAudioTrack.IsEmpty(nextMusic))
        {
            return;
        }
        else if (LoopableAudioTrack.IsEmpty(curMusic))
        {
            curMusic = nextMusic;
            nextMusic = null;
            curMusic.StartTrack(curMusicSource);
            return;
        }
        else if (nextMusic == curMusic) {
            nextMusic = null;
            return;
        }

        isInTrasition = true;

        float curPlayheadTime = curMusic.GetCurrentTime();
        int nextBar = Mathf.CeilToInt(curPlayheadTime / SECONDS_PER_BAR);
        float nextBarTime = nextBar * SECONDS_PER_BAR;
        transition = StartCoroutine(MusicTransition(nextBarTime - curPlayheadTime));
    }

    // timed sequence of events to make the transition smooth
    IEnumerator MusicTransition(float delay)
    {
        float buffer = (float)LoopableAudioTrack.BUFFER + Time.deltaTime; //HACK to deal with start buffer

        delay -= buffer;
        if (delay < 0) delay += SECONDS_PER_BAR;

        yield return new WaitForSeconds(delay); // wait for the beat

        nextMusic.StartTrack(nextMusicSource); // start the main menu music

        float waitTime = SECONDS_PER_BAR * 1f; // wait for a full bar
        yield return new WaitForSeconds(waitTime);
        float fadeTime = SECONDS_PER_BAR * 1.25f; // fade for a bit longer

        // fade out the prologue
        float p = 0f;
        while (p < 1f)
        { // we only need to fade most of the way out
            curMusic.SetVolume(volume * (1 - p));
            p += Time.deltaTime / fadeTime;
            yield return null;
        }

        curMusic.StopTrack(); //wait for main menu to start before stopping prologue
        curMusicSource.volume = volume;

        curMusic = nextMusic;
        nextMusic = queuedMusic;
        queuedMusic = null;

        AudioSource temp = curMusicSource;
        curMusicSource = nextMusicSource;
        nextMusicSource = temp;

        isInTrasition = false;
        frequentTransition = false;
        transition = null;
    }

    public void SetVolume(float vol)
    {
        volume = vol;
        curMusicSource.volume = volume;
        nextMusicSource.volume = volume;
    }
}
