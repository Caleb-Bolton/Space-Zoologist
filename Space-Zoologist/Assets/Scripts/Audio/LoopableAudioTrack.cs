using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is a track that can be looped, but because there is reverb or something at the end of the loop, the "beginning" of the loop
/// sounds different than when the beginning plays again. In essence, assuming no pausing, the 'startupClip' plays first, and then
/// the 'mainClip' plays right afterwards, and then loops forever.
/// </summary>
[System.Serializable]
public class LoopableAudioTrack {
    public AudioClip mainClip;      // the main loop of the track, which is what loops
    public AudioClip startupClip;   // the start of the track, which only plays when starting from the beginning

    AudioSource startupSource;      // a reference to the source playing the 'startupClip'
    AudioSource mainSource;         // a reference to the source playing the 'mainClip'

    bool isPastStartup;             // tracks (lazily) whether we are currently playing the 'startupClip'
    bool isInitialized;             // whether we have set up this track (and can pause/play/stop)
    bool isPaused;                  // whether this track is currently paused

    public const double BUFFER = 0.05;     // to align start time

    public bool haveStarted => isInitialized;

    // initialize this track to play in 'baseSource', and start it from the beginning
    public void StartTrack(AudioSource baseSource) {
        if (isInitialized) StopTrack();

        mainSource = baseSource;
        mainSource.clip = mainClip;
        mainSource.loop = true;

        startupSource = CreateCopyOf(mainSource);
        startupSource.clip = startupClip;
        startupSource.loop = false;

        isPastStartup = false;
        isPaused = false;
        isInitialized = true;

        startupSource.PlayScheduled(AudioSettings.dspTime + BUFFER);
        mainSource.PlayScheduled(AudioSettings.dspTime + SamplesToTime(startupClip, startupClip.samples) + BUFFER); // start this clip right after the first one finishes
    }

    // pause this track at the current timestep
    public void PauseTrack() {
        if (!isInitialized) {
            Debug.LogWarning("Tried to pause a track that was never set up: " + mainClip.name);
            return;
        }

        if (isPaused) {
            Debug.LogWarning("Tried to pause a track that was already paused: " + mainClip.name);
            return;
        }

        if (!startupSource.isPlaying) {
            mainSource.Pause();
            isPastStartup = true;
        } else { // if we haven't gotten past the startup, we will have to re-initialize
            startupSource.Pause();
            mainSource.Stop();
        }

        isPaused = true;
    }

    // resume this track from when it was last paused
    public void ResumeTrack() {
        if (!isInitialized) {
            Debug.LogWarning("Tried to pause a track that was never set up: " + mainClip.name);
            return;
        }

        if (!isPaused) {
            Debug.LogWarning("Tried to resume a track that was not paused: " + mainClip.name);
            return;
        }

        if (isPastStartup) {
            mainSource.UnPause();
        } else { // re-initialize if we are not yet in the 'mainClip'
            startupSource.UnPause();
            double timeLeft = SamplesToTime(startupClip, startupClip.samples - startupSource.timeSamples); // how much time is left in the current track
            mainSource.PlayScheduled(AudioSettings.dspTime + timeLeft);
        }

        isPaused = false;
    }

    // stop the track and detach it from its current source
    public void StopTrack() {
        if (!isInitialized) {
            Debug.LogWarning("Tried to stop a track that was never set up: " + mainClip?.name);
            return;
        }

        isInitialized = false;

        if (mainSource != null) mainSource.Stop();
        Object.Destroy(startupSource);
    }

    // returns the translated realtime position of the current track, where 0 is the start of the 'startupClip'
    public float GetCurrentTime() {
        if (!startupSource.isPlaying) {
            return startupClip.length + mainSource.time; // offset by the startup time
        } else {
            return startupSource.time;
        }
    }

    // changes the volume of this track
    public void SetVolume(float volume) {
        mainSource.volume = volume;
        startupSource.volume = volume;
    }

    // converts number of samples to realtime
    double SamplesToTime(AudioClip clip, int samples) {
        return samples / (double)clip.frequency;
    }

    // creates a duplicate of 'source' on the GameObject it's attached to - HACK lol i suck
    AudioSource CreateCopyOf(AudioSource source) {
        AudioSource copy = source.gameObject.AddComponent<AudioSource>();

        // essentials
        copy.pitch = source.pitch;
        copy.volume = source.volume;
        copy.loop = source.loop;
        copy.spatialBlend = source.spatialBlend;
        copy.panStereo = source.panStereo;

        // distance and space stuff
        copy.maxDistance = source.maxDistance;
        copy.minDistance = source.minDistance;
        copy.rolloffMode = source.rolloffMode;
        copy.dopplerLevel = source.dopplerLevel;
        copy.spatialize = source.spatialize;
        copy.spatializePostEffects = source.spatializePostEffects;
        copy.spread = source.spread;

        // bypassing stuff idk
        copy.bypassEffects = source.bypassEffects;
        copy.bypassListenerEffects = source.bypassListenerEffects;
        copy.bypassReverbZones = source.bypassReverbZones;
        copy.ignoreListenerPause = source.ignoreListenerPause;
        copy.ignoreListenerVolume = source.ignoreListenerVolume;
        copy.reverbZoneMix = source.reverbZoneMix;

        // i have no clue
        copy.outputAudioMixerGroup = source.outputAudioMixerGroup;
        copy.priority = source.priority;
        copy.velocityUpdateMode = source.velocityUpdateMode;

        return copy;
    }

    public static bool IsEmpty(LoopableAudioTrack track) {
        if (track is null || track.mainClip == null && track.startupClip == null) return true;
        return false;
    }
}
