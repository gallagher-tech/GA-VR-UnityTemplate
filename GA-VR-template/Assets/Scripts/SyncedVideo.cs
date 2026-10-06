using UnityEngine;
using UnityEngine.Video;
using Normal.Realtime;
using System;

public class SyncedVideo : RealtimeComponent<SyncedVideoModel>
{
    [SerializeField] private VideoPlayer videoPlayer;

    private double myStartTime = 0;

    protected override void OnRealtimeModelReplaced(SyncedVideoModel previousModel, SyncedVideoModel currentModel)
    {
        if (previousModel != null)
        {
            previousModel.startTimeDidChange -= StartTimeDidChange;
            previousModel.isPlayingDidChange -= IsPlayingDidChange;
        }

        if (currentModel != null)
        {
            if (currentModel.isFreshModel)
            {
                Debug.Log("initialize with a fresh model");
                currentModel.startTime = 0;
                currentModel.isPlaying = false;
            }

            currentModel.startTimeDidChange += StartTimeDidChange;
            currentModel.isPlayingDidChange += IsPlayingDidChange;

            UpdateVideoPlayerState();
        }
    }

    private void StartTimeDidChange(SyncedVideoModel model, double value)
    {
        UpdateVideoPlayerState();
    }

    private void IsPlayingDidChange(SyncedVideoModel model, bool value)
    {
        UpdateVideoPlayerState();
    }
    private void UpdateVideoPlayerState()
    {
        Debug.Log("Sync time");
        double currentTime = videoPlayer.time;
        if (model != null)
        {
            currentTime = model.startTime;
        }
        else
        {
            Debug.Log("Model was null, overwrite startTime");
            model.startTime = videoPlayer.time;
        }
        currentTime = currentTime % videoPlayer.length;
        myStartTime = currentTime;
        videoPlayer.time = currentTime;

        if (model.isPlaying)
        {
            videoPlayer.Play();
        }
        else
        {
            videoPlayer.Pause();
        }
    }

    public void TogglePlayPause()
    {
        model.isPlaying = !model.isPlaying;
        if (!model.isPlaying)
        {
            Debug.Log("store time as " + videoPlayer.time);
            model.startTime = videoPlayer.time;
        }
    }

    public void InitialPlay()
    {
        Debug.Log("Changed video!");
        model.startTime = 0;
        model.isPlaying = false;
        Debug.Log("Set the model time to 0 and play to true");
        UpdateVideoPlayerState();
    }

    public void ForceSync()
    {
        model.startTime = videoPlayer.time;
    }

    public void Restart()
    {
        model.startTime = 0;
    }

    public void JumpForward()
    {
        model.startTime = Math.Min(videoPlayer.length, model.startTime + 5);
    }

    public void JumpBackward()
    {
        model.startTime = Math.Max(0, model.startTime - 5);
    }
}