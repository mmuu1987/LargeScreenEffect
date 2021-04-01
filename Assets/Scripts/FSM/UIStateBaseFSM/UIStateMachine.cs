using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStateMachine : FsmStateMachine<MainManager>
{
    public UIStateMachine(MainManager owner)
        : base(owner)
    {

    }
}
