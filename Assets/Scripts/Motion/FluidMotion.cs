using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class FluidMotion : MotionInputMoveBase
{

    private PosAndDir[] _posDirs;

    public Material CurMaterial;

    public float OpenSpeed = 0.35f;

    /// <summary>
    /// 点击作用的半径
    /// </summary>
    public float ForceRadius = 2f;

    public Vector3 ForcePos;

    /// <summary>
    /// 粒子运动状态码
    /// </summary>
    public int StateCode = 0;

    public Transform TargetPosLeft;
    public Transform TargetPosRight;

    public RawImage Tip;

    public Material DivergenceMat;
    public Material PressureMat;
    public Material SubtractMat;
    public Material AdvectionDyeMat;
    public Material AdvectionVelocityMat;
    public Material InitDyeMat;
    public Material BlockMat;
    public Material DisplayMat;
    public Material DisplayRainbowMat;
    public Material ViscosityMat;
    private int TexWidth = Screen.width;
    private int TexHeight = Screen.height;

    public RenderTexture DivergenceRT;
    public RenderTexture DyeRT;
    public RenderTexture DyeRT2;
    public RenderTexture VelocityRT;
    public RenderTexture VelocityRT2;
    public RenderTexture PressureRT;
    public RenderTexture PressureRT2;
    public RenderTexture InitDyeRT;
    public RenderTexture BlockRT;

    public RenderTexture VelocityRT2Temp;

    public float dt = 0.1f;

    public Vector3 RanomPos;

    public ComputeShader ScaleImageComputeShader;

    /// <summary>
    /// 与相机的距离
    /// </summary>
    private float Zdepth = 30;

    public Vector4 Vector4;

    private RenderTexture rtDes;

    /// <summary>
    /// 粒子生成后，所占的比例，int为ID，float为比例
    /// </summary>
    public Dictionary<int, float> GenerateRatios = new Dictionary<int, float>();

    public PositionConvert PositionConvert;

    protected override void Init()
    {
        base.Init();
        Common.Init();

        GenerateRatios.Add(0, 0.3f);
        GenerateRatios.Add(1, 0.7f);




        MotionType = MotionType.Fluid;

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

        MaterialPropertyBlock mpb = new MaterialPropertyBlock();

        


        colorBuffer.SetData(colors);
        ComputeBuffer.SetData(_posDirs);


        CurMaterial.SetVector("_WHScale", new Vector4(1f, 1f, 1f, 1f));
        CurMaterial.SetBuffer("positionBuffer", ComputeBuffer);
        CurMaterial.SetBuffer("colorBuffer", colorBuffer);
        CurMaterial.SetTexture("_TexArr", TextureInstanced.Instance.TexArr);

      

        ComputeShader.SetBuffer(dispatchID, "positionBuffer", ComputeBuffer);

        //因不能从unityCG.cginc里拿到屏幕参数，所以从这里传入进去
        ComputeShader.SetInt("ScreenWidth", Screen.width);
        ComputeShader.SetInt("ScreenHeight", Screen.height);
        ComputeShader.SetInt("StateCode", StateCode);
        ComputeShader.SetFloat("OpenSpeed", OpenSpeed);
        ComputeShader.SetFloat("ForceRadius", ForceRadius);
        ComputeShader.SetFloat("dt", Time.deltaTime);
        ComputeShader.SetVector("TargetPosLeft", TargetPosLeft.position);
        ComputeShader.SetVector("TargetPosRight", TargetPosRight.position);
        ComputeShader.SetVector("Range", new Vector4(RanomPos.x / 2, RanomPos.y / 2, RanomPos.z / 2, 1));

        DivergenceRT = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.RHalf); DivergenceRT.Create();
        DyeRT = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.ARGBHalf); DyeRT.Create();
        DyeRT2 = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.ARGBHalf); DyeRT2.Create();
        InitDyeRT = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.ARGBHalf); InitDyeRT.Create();
        VelocityRT = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.RGHalf); VelocityRT.Create();
        VelocityRT2 = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.RGHalf);
        VelocityRT2.enableRandomWrite = true;
        VelocityRT2.Create();

        VelocityRT2Temp = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.RGHalf);
        VelocityRT2Temp.enableRandomWrite = true;
        VelocityRT2Temp.Create();


        PressureRT = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.RHalf); PressureRT.Create();
        PressureRT2 = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.RHalf); PressureRT2.Create();
        BlockRT = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.ARGB32);
        BlockRT.enableRandomWrite = true;
        BlockRT.Create();
        int k = ScaleImageComputeShader.FindKernel("CSMain");
        ScaleImageComputeShader.SetTexture(k, "Dst", BlockRT);
        // Vector4 = new Vector4(TexWidth / 2, TexHeight / 2, 50f, 0f);
        Vector4 = new Vector4(640, 360, 50f, 0f);

        ComputeShader.SetTexture(dispatchID, "VelocityTemp", VelocityRT2Temp);

        Graphics.Blit(null, InitDyeRT, InitDyeMat);
        Graphics.Blit(null, BlockRT, BlockMat);

       
    }

    

    private void OnDestroy()
    {
        
    }
    private void SetData(PosAndDir[] data)
    {
        float temp = 0f;
        foreach (KeyValuePair<int, float> pair in GenerateRatios)
        {
            temp += pair.Value;
        }
        if (Math.Abs(temp - 1f) > 0.0000001f) throw new UnityException("粒子比例相加不为1");

        int index = 0;
        float beginPosX = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Zdepth)).x;
        //粒子位置的取值范围

        List<Vector2> randomPos2 = Common.Sample2D(RanomPos.x, RanomPos.y, 0.1f, 30);
        float heightTest = 0f;

        foreach (KeyValuePair<int, float> pair in GenerateRatios)
        {
            int count = (int)(data.Length * pair.Value);
            int key = pair.Key;

            Debug.Log("id " + pair.Key + " 生成个数是：" + count + "   随机生成个数是： " + count);

            if (pair.Key == 0) heightTest = 0f;
            else heightTest = 0f;

            for (int i = index; i <= count; i++)
            {
                int picIndex = 0;

                //Vector2 size = PictureHandle.Instance.GetIndexSizeOfNumber(i, out picIndex);//得到缩放尺寸
                Vector2 size = PictureHandle.Instance.GetIndexSizeOfNumber(i, key, out picIndex);//得到缩放尺寸

                float xScale = size.x / 512f;
                float yScale = size.y / 512f;
                float proportion = size.x / size.y;
                if (xScale >= 2 || yScale >= 2)
                {
                    //如果超过2倍大小，则强制缩放到一倍大小以内，并以宽度为准，等比例减少  
                    int a = (int)xScale;
                    xScale = xScale - (a) + 2f;

                    yScale = xScale / proportion;
                }

                _posDirs[i].picIndex = picIndex;

                _posDirs[i].bigIndex = key;//大类ID
                _posDirs[i].initialVelocity = new Vector3(xScale, yScale, 0f);//填充真实宽高
                Vector2 rangeVector2 = randomPos2[Random.Range(0, randomPos2.Count)];
                Vector4 value = new Vector4(rangeVector2.x - RanomPos.x / 2, rangeVector2.y - RanomPos.y / 2 + heightTest, Zdepth + Random.Range(0, RanomPos.z), 0f);
                //Vector4 value = new Vector4(Random.Range(-ranomPos.x, ranomPos.x)+ camPos.x, Random.Range(-ranomPos.y, ranomPos.y)+ camPos.y, Random.Range(0, ranomPos.z)+ camPos.z, 1f);
                _posDirs[i].position = value;
                _posDirs[i].moveDir = Vector3.zero;
                _posDirs[i].originalPos = value;
                _posDirs[i].stateCode = 0;//状态码
                //存储两根骨骼数据的数组索引
                List<int> indexs = Common.BonePos[Random.Range(0, Common.BonePos.Count)];
                
                _posDirs[i].indexRC = new Vector2(indexs[0], indexs[1]);
                _posDirs[i].velocity = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0f);
                //第一个参数是时间间隔，第二个参数在computeshader那边保存时间缓存,第三个参数为速度,第四个参数为第二状态的时间间隔，第三状态的随机值
                _posDirs[i].uvOffset = new Vector4(Random.Range(3f, 10f), 0f, Random.Range(0.05f, 0.1f), Random.Range(0f, 1f));


            }

            index = count;
        }
    }
    void UpdateRt()
    {
        ScaleImageUserRt();

        //第一步：平流速度
        AdvectionVelocityMat.SetTexture("VelocityTex", VelocityRT2);
        AdvectionVelocityMat.SetTexture("BlockTex", BlockRT);
        AdvectionVelocityMat.SetFloat("dt", dt);
        Graphics.Blit(VelocityRT2, VelocityRT, AdvectionVelocityMat);
        Graphics.Blit(VelocityRT, VelocityRT2);

        //第二步，加大流体粘性可抑制边界层分离现象
        for (int i = 0; i < 0; i++)
        {
            ViscosityMat.SetTexture("_VelocityTex", VelocityRT2);
            Graphics.Blit(VelocityRT2, VelocityRT, ViscosityMat);
            Graphics.Blit(VelocityRT, VelocityRT2);
        }

        //第三步：计算散度
        DivergenceMat.SetTexture("VelocityTex", VelocityRT2);
        Graphics.Blit(VelocityRT2, DivergenceRT, DivergenceMat);

        //第四步：计算压力
        PressureMat.SetTexture("DivergenceTex", DivergenceRT);
        for (int i = 0; i < 100; i++)
        {
            PressureMat.SetTexture("PressureTex", PressureRT2);
            Graphics.Blit(PressureRT2, PressureRT, PressureMat);
            Graphics.Blit(PressureRT, PressureRT2);
        }
        //第五步：速度场减去压力梯度，得到无散度的速度场
        SubtractMat.SetTexture("PressureTex", PressureRT2);
        SubtractMat.SetTexture("VelocityTex", VelocityRT2);
        Graphics.Blit(VelocityRT2, VelocityRT, SubtractMat);
        Graphics.Blit(VelocityRT, VelocityRT2);

        //第六步：用最终速度去平流密度
        Graphics.Blit(DyeRT, DyeRT2);
        AdvectionDyeMat.SetTexture("VelocityTex", VelocityRT2);
        AdvectionDyeMat.SetTexture("DensityTex", DyeRT2);
        AdvectionDyeMat.SetTexture("BlockTex", BlockRT);
        AdvectionDyeMat.SetTexture("InitDyeTex", InitDyeRT);
        AdvectionDyeMat.SetFloat("dt", dt);
        Graphics.Blit(DyeRT2, DyeRT, AdvectionDyeMat);

        // MoveObject(VelocityRT2);
        //第七步：显示
        DisplayMat.SetTexture("BlockTex", BlockRT);
        //DisplayRainbowMat.SetTexture("BlockTex", BlockRT);
        //Graphics.Blit(DyeRT, destination, DisplayMat);
        //Graphics.Blit(VelocityRT2, destination, DisplayRainbowMat);
        //Graphics.Blit(PressureRT2, destination, DisplayRainbowMat);
        // Graphics.Blit(source, destination);
        Tip.texture = VelocityRT2;
    }


    private void Test()
    {
        Mesh mesh;

       
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


        ComputeShader.SetTexture(dispatchID, "Velocity", VelocityRT2);


        
        ComputeShader.SetVectorArray("bonePos", PositionConvert.GetPosArray());
        //根据不同的交互类型过滤不


        Common.Filter((b =>
        {
            if(b) ComputeShader.SetInt("StateCode", Common.StateCode);
        } ));
            



        ComputeShader.SetFloat("Seed", Random.Range(0f, 1f));
        ComputeShader.SetFloat("dt", Time.deltaTime);
        
        if (Common.Category == 0)
            ComputeShader.SetVector("TargetPosLeft", GetWorldPos());
        ComputeShader.SetVector("TargetPosRight", TargetPosRight.position);
        //ComputeShader.SetVectorArray();
        ////因不能从unityCG.cginc里拿到屏幕参数，所以从这里传入进去
        //ComputeShader.SetInt("ScreenWidth", Screen.width);
        //ComputeShader.SetInt("ScreenHeight", Screen.height);
        base.Dispatch(dispatchID, system);


        //FluidStruct[] data = new FluidStruct[ComputeBuffer.count];
        //ComputeBuffer.GetData(data);
        //Debug.Log(data[0].position + "  " + data[1].position + "  " + data[2].position );
    }

    private Vector3 GetWorldPos()
    {
        Vector3 screenPos = PositionConvert.Instance.GetScreenPos("HandRight");

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 30.5f));

        return worldPos;


    }

    public void ScaleImageUserRt()
    {
        //    Compute Shader

        //1 找到compute shader中所要使用的KernelID
        int k = ScaleImageComputeShader.FindKernel("CSMain");



        ScaleImageComputeShader.SetVector("pos", Vector4);

        //3 运行shader  参数1=kid  参数2=线程组在x维度的数量 参数3=线程组在y维度的数量 参数4=线程组在z维度的数量
        ScaleImageComputeShader.Dispatch(k, (int)TexWidth, (int)TexHeight, 1);

        //cumputeShader gpu那边已经计算完毕。rtDes是gpu计算后的结果




        //后续操作，把reDes转为Texture2D  
        //删掉rtDes,SourceTexture2D，我们就得到了所要的目标，并且不产生内存垃圾
    }




}
