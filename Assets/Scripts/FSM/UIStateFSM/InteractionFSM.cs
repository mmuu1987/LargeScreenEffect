using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionFSM : UIStateFSM
{

    
    public InteractionFSM(Transform go) : base(go)
    {
        TextureInstanced.Instance.VertexMovemontMotion.ExternalInit();
        
    }

    public override void Enter()
    {
        TextureInstanced.Instance.Type = MotionType.VertexMovement;
        Common.ChangeCategory(0);
        base.Enter();
        
    }

    public override void Exit()
    {
        base.Exit();
        TextureInstanced.Instance.Type = MotionType.None;
        KinectManager.Instance.ClearKinectUsers();
       

    }
}
