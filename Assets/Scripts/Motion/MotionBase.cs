using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 组成立方体的运动类型
/// </summary>
public class MotionBase : MonoBehaviour
{
    public string computeShaderName;

    public string InitName;
    protected int dispatchID = -1;

    protected int InitID = -1;
    [SerializeField]
    protected ComputeShader ComputeShader;

    protected const int _Thread = 64;
    protected const string _BufferKey = "positionBuffer";

    /// <summary>
    /// 进入该运动后，给初始化的时间
    /// </summary>
    public float InitTime = 0.2f;


    protected bool IsInitEnd = false;
    /// <summary>
    /// 屏幕宽
    /// </summary>
    protected int Width;
    /// <summary>
    /// 屏幕高
    /// </summary>
    protected int Height;

   

    /// <summary>
    /// 是否进入循环
    /// </summary>
    private bool _isEnterUpdate = false;

    private Coroutine _coroutine;
    protected virtual void Start() { }
    protected virtual void Update() { }

    protected ComputeBuffer ComputeBuffer;

    protected float TimeTmep = 0f;



    public void StartMotion(TextureInstanced system)
    {

        if (_coroutine == null)
        {


            Init();
            TimeTmep = 0f;

            _coroutine = StartCoroutine(WaitStart());
        }
        else
        {
            if (_isEnterUpdate)
            {
                Dispatch(ComputeBuffer);
            }
        }
    }

    /// <summary>
    /// 设置数据
    /// </summary>
    public void SetData()
    {

    }
    protected virtual void Init()
    {
        ComputeBuffer = TextureInstanced.Instance.positionBuffer;
       
        Width = TextureInstanced.Instance.Width;
        Height = TextureInstanced.Instance.Height;
       
    }
    public IEnumerator WaitStart()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            TimeTmep += Time.deltaTime;
            //Debug.Log("run");
            InitDisPatch(InitID);//运行初始化需要的操作

            if (TimeTmep >= InitTime)
            {
                _isEnterUpdate = true;

                yield break;

            }

        }



    }
    protected virtual void Dispatch(ComputeBuffer system)
    {
        Dispatch(dispatchID, system);
    }

    protected virtual void InitDisPatch(int id)
    {
        if (id < 0) return;
        ComputeShader.SetBuffer(id, _BufferKey, ComputeBuffer);
        ComputeShader.Dispatch(id, ComputeBuffer.count / _Thread + 1, 1, 1);
    }

    protected void Dispatch(int id, ComputeBuffer system)
    {
        if (id < 0) return;
        ComputeShader.SetBuffer(id, _BufferKey, ComputeBuffer);
        ComputeShader.Dispatch(id, ComputeBuffer.count / _Thread + 1, 1, 1);
    }

    public virtual void ExitMotion()
    {
        if (_coroutine != null) StopCoroutine(_coroutine);
        _coroutine = null;
        _isEnterUpdate = false;
      
    }

}



