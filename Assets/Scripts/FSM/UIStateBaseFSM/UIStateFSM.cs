using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;


public class UIStateFSM : FsmState<MainManager>
{
    /// <summary>
    /// UI的根物体
    /// </summary>
    public Transform Parent;

    public UIStateFSM(Transform go)
    {

        
        if (go != null)
        {
            Parent = go;
        }


       
       
    }

    public override void Enter()
    {
        base.Enter();
       
        if (Parent != null)
        {
            Parent.gameObject.SetActive(true);
           
        }

       
    }
    /// <summary>
    /// 检测是否是视频贴图，如果是，则播放视频
    /// </summary>
    protected  void CheckVideoTex(Texture2D tex,GameObject showGo)
    {

    }
    public void AddVideoTex(List<Texture2D> texs, List<string> videoPaths)
    {
    }
    public override void Exit()
    {
        base.Exit();

        if (Parent != null)
            Parent.gameObject.SetActive(false);
    }
}
