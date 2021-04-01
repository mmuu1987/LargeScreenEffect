using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandbyFSM : UIStateFSM
{

    private FluidMotion _fluidMotion;
    public StandbyFSM(Transform go) : base(go)
    {
        _fluidMotion = go.GetComponent<FluidMotion>();


    }

    public override void Enter()
    {
        TextureInstanced.Instance.Type = MotionType.Fluid;
        KinectManager.Instance.ClearKinectUsers();
        KinectManager.Instance.maxTrackedUsers = 1;
        base.Enter();
        
    }

    public override void Exit()
    {
        base.Exit();
        TextureInstanced.Instance.Type = MotionType.None;
        KinectManager.Instance.ClearKinectUsers();
        KinectManager.Instance.maxTrackedUsers = 0;

    }
}
