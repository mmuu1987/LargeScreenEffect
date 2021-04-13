using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 项目的总控脚本
/// </summary>
public class MainManager : MonoBehaviour
{

    public UIStateMachine _Machine;

    public Dictionary<UIState, UIStateFSM> DicUI;

    // Start is called before the first frame update
    void Start()
    {
        DicUI = new Dictionary<UIState, UIStateFSM>();

        _Machine = new UIStateMachine(this);

        DicUI.Add(UIState.StandBy, new StandbyFSM(this.transform.Find("StandbyFSM")));
        DicUI.Add(UIState.PlayVideoState, new PlayVideoStateFSM(this.transform.Find("PlayVideoStateFSM")));

        _Machine.SetCurrentState(DicUI[UIState.StandBy]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
#if UNITY_EDITOR
    private void OnGUI()
    {
    //    if (GUI.Button(new Rect(0f, 0f, 100f, 100f), "video"))
    //    {
    //        _Machine.ChangeState(DicUI[UIState.PlayVideoState]);
    //    }
    //    if (GUI.Button(new Rect(100f, 0f, 100f, 100f), "StandBy"))
    //    {
    //        _Machine.ChangeState(DicUI[UIState.StandBy]);
    //    }
    }
#endif

}
public enum UIState
{

    None,
    /// <summary>
    /// 待机
    /// </summary>
    StandBy,
    /// <summary>
    /// 播放视频状态
    /// </summary>
    PlayVideoState
   
}