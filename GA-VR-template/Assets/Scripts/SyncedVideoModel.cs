using Normal.Realtime;
using Normal.Realtime.Serialization;

[RealtimeModel]
public partial class SyncedVideoModel
{
    [RealtimeProperty(1, true, true)]
    private double _startTime;

    [RealtimeProperty(2, true, true)]
    private bool _isPlaying;
}