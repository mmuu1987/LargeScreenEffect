using System;
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


    private Coroutine _coroutine;

    /// <summary>
    /// 当前状态
    /// </summary>
    private UIState _curState;

    public bool IsUseStandby = false;

    public float StandbyTime = 7f;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        DicUI = new Dictionary<UIState, UIStateFSM>();

        _Machine = new UIStateMachine(this);

        yield return StartCoroutine(TextureInstanced.Instance.Wait());

        DicUI.Add(UIState.InteractionState, new InteractionFSM(this.transform.Find("InteractionFSM")));
        DicUI.Add(UIState.PlayVideoState, new PlayVideoStateFSM(this.transform.Find("PlayVideoStateFSM")));

        KinectManager.Instance.AddingUserEvent += AddingUserEvent;
        KinectManager.Instance.RemoveUserEvent += RemoveUserEvent;

        _Machine.SetCurrentState(DicUI[UIState.PlayVideoState]);
        _curState = UIState.PlayVideoState;

        StartStandby();
    }

    private void RemoveUserEvent(long obj)
    {
       //  Debug.LogError("移除人");
        StartComputeStandby();
    }

    private void AddingUserEvent(long obj)
    {
       // Debug.LogError("增加人"); 
       CanceStandby(null);
    }

    public void ChangeState(UIState state)
    {
        if (state != _curState)
        {
            _curState = state;
            _Machine.ChangeState(DicUI[_curState]);
        }

    }

    private void StopComputeStandby()
    {
        //Debug.LogError("停止计算待机");
        if (_coroutine != null)
            StopCoroutine(_coroutine);
    }
     
    private void StartComputeStandby()
    {
        if (_coroutine != null)
            StopCoroutine(_coroutine);
        // Debug.LogError("开始计算待机");
        _coroutine =StartCoroutine(Common.WaitTime(this.StandbyTime, StartStandby));

    }

    private void StartStandby()
    {
        Debug.LogError("改变为待机状态");
        ChangeState(UIState.PlayVideoState);

        //KinectManager.Instance.playerCalibrationPose = KinectGestures.Gestures.None;
        //KinectManager.Instance.maxTrackedUsers=1;

    }
    private void CanceStandby(Action action)
    { 
        

        ChangeState(UIState.InteractionState);

        this.StopComputeStandby();
        if (action != null)
        {
            action();
        }
    }

    private void OnDestroy()
    {
        KinectManager.Instance.AddingUserEvent -= AddingUserEvent;
        KinectManager.Instance.RemoveUserEvent -= RemoveUserEvent;
    }
    private void CheckUserRange(float x, float y, float z)
    {
       
        if (KinectManager.Instance.GetAllUserIds().Count > 0)
        {
            List<long> ids = KinectManager.Instance.GetAllUserIds();
            foreach (long id in ids)
            {
                Vector3 pos = KinectManager.Instance.GetUserPosition(id);
                
                if (Mathf.Abs(pos.x) > x)
                {
                    KinectManager.Instance.ClearKinectUsers();
                }
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        this.CheckUserRange(0.65f, 0.45f, 3f);
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
    /// 交互状态
    /// </summary>
    InteractionState,
    /// <summary>
    /// 播放视频待机状态
    /// </summary>
    PlayVideoState

}