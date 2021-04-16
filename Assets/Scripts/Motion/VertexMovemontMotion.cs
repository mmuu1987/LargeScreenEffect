using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class VertexMovemontMotion : MotionInputMoveBase
{
    // Start is called before the first frame update
    private PosAndDir[] _posDirs;
    private int TexWidth = Screen.width;
    private int TexHeight = Screen.height;
    public Material CurMaterial;
    /// <summary>
    /// 粒子初始化的时候在世界空间生成的范围
    /// </summary>
    public Vector3 RanomPos;

    private MeshData _meshData;
    /// <summary>
    /// 与相机的距离
    /// </summary>
    private float Zdepth = 0;

    private ComputeBuffer _meshDataBuffer;

    public Transform TargetPosRight;

    
    private WaitForEndOfFrame wfef;
    private Coroutine _coroutine;
    /// <summary>
    /// 点击作用的半径
    /// </summary>
    public float ForceRadius = 2f;

    public Image GUICursor;

    public Transform logoLable;
    public Material LogoMaterial;
    protected override void Init()
    {
        //float temp = 3;
        //float value = (temp /= 5) * temp * temp;
        //Debug.Log("value is " +value+ "  tmep is" + temp);

        if (IsInitEnd) return;

        base.Init();
        Common.Init();
        MotionType = MotionType.VertexMovement;
       
        wfef = new WaitForEndOfFrame();
        LoadAnimationData();

        Debug.Log("screen width is " + Screen.width + "  screen height is " + Screen.height);
        TexWidth = Screen.width;
        TexHeight = Screen.height;

        int stride = Marshal.SizeOf(typeof(PosAndDir));
        Debug.Log("stride byte size is " + stride);
        ComputeBuffer = new ComputeBuffer(ComputeBuffer.count, stride);

        ComputeBuffer colorBuffer = new ComputeBuffer(ComputeBuffer.count, 16);

        Vector4[] colors = new Vector4[ComputeBuffer.count];
        _posDirs = new PosAndDir[ComputeBuffer.count];

        TextureInstanced.Instance.ChangeInstanceMat(CurMaterial);
        CurMaterial.enableInstancing = true;
        // RanomPos = new Vector3(20, 12, 1);
        SetData(_posDirs);

      




        colorBuffer.SetData(colors);
        ComputeBuffer.SetData(_posDirs);


        CurMaterial.SetVector("_WHScale", new Vector4(1f, 1f, 1f, 1f));
        CurMaterial.SetBuffer("positionBuffer", ComputeBuffer);
        CurMaterial.SetBuffer("colorBuffer", colorBuffer);
        CurMaterial.SetTexture("_TexArr", TextureInstanced.Instance.TexArr);
       
        CurMaterial.SetInt("_VertexSize", _meshData.VertexSize);
        CurMaterial.SetInt("_VertexCount", _meshData.VertexCount);
        CurMaterial.SetBuffer("VertexBuffer", _meshDataBuffer);



        ComputeShader.SetBuffer(dispatchID, "positionBuffer", ComputeBuffer);

        //因不能从unityCG.cginc里拿到屏幕参数，所以从这里传入进去
        ComputeShader.SetInt("ScreenWidth", Screen.width);
        ComputeShader.SetInt("ScreenHeight", Screen.height);
        ComputeShader.SetInt("StateCode", 0);

        ComputeShader.SetFloat("dt", Time.deltaTime);
      
        ComputeShader.SetVector("MoveRange", RanomPos);

        PositionConvert.Instance.HandEvent += HandEvent;
        InteractionInputModule.Instance.HandDetectionEvent += HandDetectionEvent;


       // logoLable.gameObject.SetActive(false);
    }

    private void HandDetectionEvent(bool isRight, bool isGrip)
    {
        if (Common.Category != 0)
        {
            GUICursor.enabled = true;
            return;//如果不是第1种运动种类则不执行

        }

        
        if (isRight)
        {
            Color col = LogoMaterial.color;
            if (isGrip)
            {
                GUICursor.enabled = false;
                Common.ChangeStateCode(1);
                logoLable.DOKill();
                logoLable.gameObject.SetActive(true);
                LogoMaterial.DOColor(new Color(col.r, col.g, col.b, 1f), 3f).SetEase(Ease.InOutCubic);


            }
            else
            {
                GUICursor.enabled = true;
                Common.ChangeStateCode(2);
                if (_coroutine != null) StopCoroutine(_coroutine);
                logoLable.DOKill();
                _coroutine = StartCoroutine(WaitChangeCategory(500));
                LogoMaterial.color = new Color(col.r,col.g,col.b,0);

            }

        }
       
    }

    public void ExternalInit()
    {
        Init();
        IsInitEnd = true;
    }

    private void HandEvent(bool obj)
    {

        if (Common.Category != 1) return;//如果不是第二种运动种类则不执行
        if (obj)
        {
            
           
            Common.ChangeStateCode(2);
            if (_coroutine != null) StopCoroutine(_coroutine);
            _coroutine = StartCoroutine(WaitChangeCategory(500));

        }
        else
        {
           
            Common.ChangeStateCode(1);

        }
    }
    /// <summary>
    /// 等待一定的帧数变运动的种类
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitChangeCategory(int count)
    {
        int tempCount = 0;
        while (true)
        {
            yield return wfef;
            //Debug.Log(" stateCode IS " + Common.StateCode);
            if (Common.StateCode != 2) yield break;//如果状态改变，则停止计算

            tempCount++;
            if (tempCount >= count)
            {
                Common.ChangeCategory(0);
                yield break;
            }
        }


    }
    private void SetData(PosAndDir[] data)
    {
        float temp = 0f;
       

        
        //粒子位置的取值范围

        List<Vector2> randomPos2 = Common.Sample2D(RanomPos.x, RanomPos.y, 1f, 30);
        float heightTest = 0f;

       

        for (int i = 0; i < data.Length; i++)
        {
            int picIndex = 0;

            //_posDirs[i].picIndex = i%TextureInstanced.Instance.TexArr.depth;

            _posDirs[i].bigIndex = 0;//大类ID
            _posDirs[i].initialVelocity = new Vector3(1, 1, 0f);//填充真实宽高
            Vector2 rangeVector2 = randomPos2[Random.Range(0, randomPos2.Count)];
            Vector4 value = new Vector4(rangeVector2.x - RanomPos.x / 2, rangeVector2.y - RanomPos.y / 2 + heightTest, Zdepth + Random.Range(0, RanomPos.z), 3f);
            //Vector4 value = new Vector4(Random.Range(-ranomPos.x, ranomPos.x)+ camPos.x, Random.Range(-ranomPos.y, ranomPos.y)+ camPos.y, Random.Range(0, ranomPos.z)+ camPos.z, 1f);
            _posDirs[i].position = value;
            
            _posDirs[i].moveDir = Vector3.zero;
            _posDirs[i].originalPos = value;
            _posDirs[i].stateCode = 0;//状态码
            
            //第一状态的前向移动速度要用到 第一个参数是移动速度
            _posDirs[i].initialVelocity = new Vector4(Random.Range(1f,5f),0f,0f,0f);
           
            //第一个参数为第一状态的布尔值，用来存储布尔逻辑,第二个为顶点旋转的概率，只有旋转，跟不旋转，同样是布尔值，第三个是旋转的速度,第四个参数同样为布尔值，用在指示是否旋转
            //在computeshader里赋值
            _posDirs[i].uv2Offset = new Vector4(1, GetProbability(0.15f), Random.Range(0,2f),1);
            
            //存储两根骨骼数据的数组索引
            List<int> indexs = Common.BonePos[Random.Range(0, Common.BonePos.Count)];
            _posDirs[i].indexRC = new Vector2(indexs[0], indexs[1]);

            _posDirs[i].velocity = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0f);

            //第一个参数是时间间隔，第二个参数在computeshader那边保存时间缓存,第三个参数为速度,第四个参数为第二状态的时间间隔，第三状态的随机值
            _posDirs[i].uvOffset = new Vector4(Random.Range(1f, 3f), 0f, Random.Range(0.05f, 0.1f), Random.Range(0f, 1f));


        }
    }

    /// <summary>
    /// 计算出多少概率获得true
    /// </summary>
    /// <param name="arg">大于0小于1的参数</param>
    /// <returns>0为false,1为true</returns>
    private int GetProbability(float arg)
    {
        if (arg < 1f && arg > 0f)
        {
            float v = Random.Range(0f, 1f);

            if (v <= arg) return 1;
            return 0;
        }
        if (arg > 1) return 1; 
        return 0;
        
    }
    /// <summary>
    /// 导入动画数据
    /// </summary>
    private void LoadAnimationData()
    {
        string path = Application.streamingAssetsPath + "/data.txt";

        using (FileStream fileStream = File.OpenRead(path))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            _meshData = (MeshData)binaryFormatter.Deserialize(fileStream);
        }

        int stride = Marshal.SizeOf(typeof(Vector3));
        Debug.Log("stride byte size is " + stride);

        Vector3[] temps =new Vector3[_meshData.VertexPosData.Length];

        for (int i = 0; i < _meshData.VertexPosData.Length; i++)
        {
            temps[i] = _meshData.VertexPosData[i].ConevrtVector3();
        }
        _meshDataBuffer = new ComputeBuffer(_meshData.VertexPosData.Length, stride);
        _meshDataBuffer.SetData(temps);

    }
    protected override void Dispatch(ComputeBuffer system)
    {
        // UpdateRt();


        //屏幕坐标转为投影坐标矩阵
        Matrix4x4 p = Camera.main.projectionMatrix;
        //世界坐标到相机坐标矩阵
        Matrix4x4 v = Camera.main.worldToCameraMatrix;
        Matrix4x4 ip = Matrix4x4.Inverse(p);
        Matrix4x4 iv = Matrix4x4.Inverse(v);

        ComputeShader.SetMatrix("p", p);
        ComputeShader.SetMatrix("v", v);
        ComputeShader.SetMatrix("ip", ip);
        ComputeShader.SetMatrix("iv", iv);
       
        //根据不同的交互类型过滤不


        Common.Filter((b =>
        {
            if (b)
            {
                ComputeShader.SetInt("StateCode", Common.StateCode);
            }
        }));



        ComputeShader.SetFloat("ForceRadius", ForceRadius);
        ComputeShader.SetFloat("Seed", Random.Range(0f, 1f));
        ComputeShader.SetFloat("dt", Time.deltaTime);
        if(Common.Category==1)
         ComputeShader.SetVectorArray("bonePos", PositionConvert.Instance.GetPosArray());


       
        if(Common.Category==0)//自由向前运动是需要的是屏幕的位置
        {
           // Debug.LogError("输入坐标");

            Vector2 screenPos = GUICursor.rectTransform.anchoredPosition;
            ComputeShader.SetVector("TargetPosRight", screenPos);
            Vector3 screenToWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 30.5f));
            //Vector3 screenToWorldPos = new Vector4(0f, 0f, 30.5f, 0f);
            ComputeShader.SetVector("TargetWorldPos", screenToWorldPos);//new Vector4(0f,0f,30.5f,0f)固定集中在中间
            logoLable.position = screenToWorldPos + new Vector3(0f, -10f, 0f);

           


        }
       
        base.Dispatch(dispatchID, system);
    }

   

    private Vector3 GetWorldPos()
    {
        Vector3 screenPos = PositionConvert.Instance.GetScreenPos("HandRight");

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 30.5f));

        return worldPos;


    }

    public override void ExitMotion()
    {
        base.ExitMotion();
        PositionConvert.Instance.HandEvent -= HandEvent;
        InteractionInputModule.Instance.HandDetectionEvent -= HandDetectionEvent;
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (GUI.Button(new Rect(300f, 0f, 100f, 100f), "gatherState"))
        {
            HandDetectionEvent(true, true);
        }
        if (GUI.Button(new Rect(400f, 0f, 100f, 100f), "diffuseState"))
        {
            HandDetectionEvent(true, false);
        }
    }
#endif
}
[Serializable]
public class MeshData
{
    public int VertexCount;

    public int VertexSize;

    public Vector3Data[] VertexPosData;

}
[Serializable]
public class Vector3Data
{
    public float X;
    public float Y;
    public float Z;

    public Vector3Data(Vector3 vector3)
    {
        X = vector3.x;
        Y = vector3.y;
        Z = vector3.z;
    }

    public Vector3 ConevrtVector3()
    {
        Vector3 v = new Vector3(X,Y,Z);
        return v;
    }
}
