using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// 播放视频状态
/// </summary>
public class PlayVideoStateFSM : UIStateFSM
{
    private VideoPlayer _videoPlayer;
    private string _videoPath;
    public PlayVideoStateFSM(Transform go) : base(go)
    {
        _videoPath = "test.mp4";
        _videoPlayer = go.transform.Find("videoPlay").GetComponent<VideoPlayer>();
        if(_videoPlayer==null)throw new UnityException("没有找到播放组件");

    }

    public override void Enter()
    {
        string url = "file:///"+Application.streamingAssetsPath + "/" + _videoPath;
        _videoPlayer.url = url;
        _videoPlayer.Play();
        Target.ShowTip(false,true,15f);
        base.Enter();

    }

    public override void Exit()
    {
        base.Exit();
        _videoPlayer.Stop();
    }
}
