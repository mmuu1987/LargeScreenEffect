using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MotionInputMoveBase : MotionBase, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler,IBeginDragHandler
    
{

    protected MotionType MotionType;
    protected override void Start()
    {
        dispatchID = ComputeShader.FindKernel(computeShaderName);
        if (!string.IsNullOrEmpty(InitName))
         InitID = ComputeShader.FindKernel(InitName);
        base.Start();
    }


    protected override void Init()
    {
        base.Init();
      

        Camera.main.fieldOfView = 60f;

    }

   

    
    

    protected virtual void  CheckZ()
    {
        

       
    }
    protected override void Dispatch(ComputeBuffer system)
    {
       // Debug.Log("run");
        Dispatch(dispatchID, system);

    }

    public override void ExitMotion()
    {
        base.ExitMotion();
      
       
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
       // Debug.Log("OnPointerUp");
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
       // Debug.Log("OnPointerClick");
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
      //  Debug.Log("OnPointerDown");
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
       
      //  Debug.Log("OnDrag");
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
       
      //  Debug.Log("OnEndDrag");
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
       // Debug.Log("OnBeginDrag");
    }
}
