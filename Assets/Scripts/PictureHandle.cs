using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

using System.IO;
using System.Linq;
using System.Text;

using Microsoft.Win32.SafeHandles;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Video;
using Debug = UnityEngine.Debug;
using Graphics = UnityEngine.Graphics;

/// <summary>
/// 处理图片，整理，分类
/// </summary>
public class PictureHandle : MonoBehaviour
{

    public static PictureHandle Instance;

    public MotionType MotionType;

  

    public Texture2DArray TexArr { get; set; }

    
    /// <summary>
    /// 所有的大事件集合
    /// </summary>
    private List<ClassInfo> _classInfos = new List<ClassInfo>();


    public List<Texture2D> Texs = new List<Texture2D>();
   
    private int _pictureCount = 0;
   

    public ComputeShader ScaleImageComputeShader;

   

    private void Awake()
    {
        if (Instance != null) throw new UnityException("单例错误");

        Instance = this;
        DontDestroyOnLoad(this.gameObject);



    }

    public IEnumerator HandlerPicture()
    {
        Debug.Log(1);

        List<string> paths = GetAllPath("flower");


        foreach (string path in paths)
        {

            List<ClassInfo> infos = LoadPicture(path);
            _classInfos.AddRange(infos);

        }

        _pictureCount = 0;
        yield return StartCoroutine(LoadTextureAssets());




        HandleTextureArry(Texs);

        DestroyTexture();//贴图加载到GPU那边后这边内存就清理掉

       // yield return null;
    }
    IEnumerator Wait()
    {
       // Debug.Log(1);
        List<string> paths = GetAllPath("flower");


        foreach (string path in paths)
        {
           
            List<ClassInfo> infos = LoadPicture(path);
            _classInfos.AddRange(infos);
            
        }

        _pictureCount = 0;
        yield return StartCoroutine(LoadTextureAssets());

       


        HandleTextureArry(Texs);



        DestroyTexture();//贴图加载到GPU那边后这边内存就清理掉

      
    }

   
    // Update is called once per frame
    void Update()
    {

    }
    /// <summary>
    /// 获取根目录下的所有子目录路径
    /// </summary>
    /// <returns></returns>
    List<string> GetAllPath(string root)
    {
        List<string> allPath = new List<string>();

        string path = Application.streamingAssetsPath + "/" + root;

        DirectoryInfo rootDir = new DirectoryInfo(path);

        foreach (DirectoryInfo directory in rootDir.GetDirectories())
        {
            if (directory.Parent.Name == rootDir.Name)
            {

                if (directory.GetDirectories().Length != 0)
                {
                    allPath.Add(directory.FullName);
                    
                }

            }
        }

        return allPath;
    }





    /// <summary>
    /// 根据数量获取该数量下的图片索引和尺寸,是否是重复获取，索引所在的类别位置
    /// </summary>
    /// <param name="number"></param>
    /// <param name="level"></param>
    /// <param name="isRest"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public Vector2 GetIndexSizeOfNumber(int number, out int index)
    {
        index = number % _pictureCount;

        foreach (ClassInfo classInfo in _classInfos)
        {
            foreach (ObjectInfo objectInfo in classInfo.ObjectInfos)
            {
                if (objectInfo.PictureSizeInfo.ContainsKey(index))
                {
                    return objectInfo.PictureSizeInfo[index];
                }
            }
        }
        return Vector2.zero;
    }
    /// <summary>
    /// 获取某个类型图片的索引和尺寸
    /// </summary>
    /// <param name="number"></param>
    /// <param name="level">哪个类型的图片</param>
    /// <param index="">返回该图片的索引</param>
    /// <returns></returns>
    public Vector2 GetIndexSizeOfNumber(int number,int level, out int index)
    {
        //index = number % _pictureCount;

        foreach (ClassInfo classInfo in _classInfos)
        {
            foreach (ObjectInfo objectInfo in classInfo.ObjectInfos)
            {
                if (objectInfo.IndexPos == level)
                {
                    int count = objectInfo.PictureIndes.Count;

                    int tempIndex = number % count;
                    int temp = 0;
                    foreach (KeyValuePair<int, Vector2> pair in objectInfo.PictureSizeInfo)
                    {
                        if (temp != tempIndex) temp++;
                        else
                        {
                            index = pair.Key;
                            return pair.Value;
                        }
                    }
                }
               
            }
        }

        index = 0;
        return Vector2.zero;
    }






    public List<ClassInfo> LoadPicture(string path)
    {
        List<ClassInfo> classInfos = new List<ClassInfo>();

        DirectoryInfo directoryInfo = new DirectoryInfo(path);

        FileInfo cfileInfo = new FileInfo(path);

        ClassInfo cinfo = new ClassInfo();
        cinfo.ClassName = cfileInfo.Name;
        classInfos.Add(cinfo);

        DirectoryInfo[] infos = directoryInfo.GetDirectories();//获取年份目录
        cinfo.ClassCount = infos.Length;
        int count = 0;
        foreach (DirectoryInfo info in infos)
        {
            ObjectInfo objectInfo = new ObjectInfo();
            objectInfo.BelongsClass = info.Name;
            objectInfo.IndexPos = count;
            count++;
            FileInfo[] fileInfos = info.GetFiles();

            foreach (FileInfo fileInfo in fileInfos)
            {
                if (fileInfo.Extension == ".txt")
                {

                    objectInfo.DescribePath = fileInfo.FullName;

                    byte[] bytes = File.ReadAllBytes(fileInfo.FullName);

                    string str = Encoding.UTF8.GetString(bytes);

                    objectInfo.Describe = str;
                }
                else if (fileInfo.Extension == ".jpg" || fileInfo.Extension == ".JPG" || fileInfo.Extension == ".jpeg")
                {

                    objectInfo.PicturesPath.Add(fileInfo.FullName);
                }
                else if (fileInfo.Extension == ".mp4")
                {
                    objectInfo.ObjectVideo = fileInfo.FullName;
                }
                else if (fileInfo.Extension == ".png" || fileInfo.Extension == ".PNG")
                {
                    objectInfo.PicturesPath.Add(fileInfo.FullName);
                    // Debug.Log(fileInfo.FullName);
                }
            }

            cinfo.ObjectInfos.Add(objectInfo);


        }
        return classInfos;

    }
    /// <summary>
    /// 加载图片资源
    /// </summary>
    public IEnumerator LoadTextureAssets()
    {


        //先默认为512*512的图片,原始图片的长宽我们在用另外的vector2保存
        //生成需要表现的图片
        foreach (ClassInfo classInfo in _classInfos)
        {
            foreach (ObjectInfo objectInfo in classInfo.ObjectInfos)
            {
                if (objectInfo.PicturesPath.Count <= 0)//如果没有图片，我们生成一个logo的先填充
                {
                    string s = Application.streamingAssetsPath + "/logo.png";

                    Vector2 vector2;

                    FileInfo fileInfo = new FileInfo(s);


                    Texture newTex = HandlePicture( fileInfo.DirectoryName, fileInfo.Name, out vector2);

                    Texture2D tex2D = ScaleImageUserRt(newTex, Common.PictureWidth, Common.PictureHeight);



                    Texs.Add(tex2D);

                    //yearsEvent.PictureIndes.Add(pictureIndex);

                    objectInfo.AddPictureInfo(_pictureCount, vector2);

                    _pictureCount++;
                }
                else
                    foreach (string s in objectInfo.PicturesPath)
                    {


                        if (File.Exists(s))
                        {

                            yield return null;

                            //count++;
                            //if (count >= 100) yield break;

                            Vector2 vector2;

                            FileInfo fileInfo = new FileInfo(s);



                            Texture newTex = HandlePicture(fileInfo.DirectoryName, fileInfo.Name, out vector2);

                            Texture2D tex2D = ScaleImageUserRt(newTex, Common.PictureWidth, Common.PictureHeight);



                            // GC.Collect();

                            Texs.Add(tex2D);

                            objectInfo.AddPictureInfo(_pictureCount, vector2);

                            _pictureCount++;
                        }

                    }
            }
        }


    }
    /// <summary>
    /// 缩放图片
    /// </summary>
    /// <param name="targeTexture2D"></param>
    /// <param name="dstWidth">目标宽</param>
    /// <param name="dstHeight">目标高</param>
    /// <returns></returns>
    public Texture2D ScaleImageUserRt(Texture targeTexture2D, int dstWidth, int dstHeight)
    {


        float widthScale = dstWidth * 1f / targeTexture2D.width;
        float heightScale = dstHeight * 1f / targeTexture2D.height;



        RenderTexture rtDes = new RenderTexture(dstWidth, dstHeight, 24);
        rtDes.enableRandomWrite = true;
        rtDes.Create();


        ////////////////////////////////////////
        //    Compute Shader
        ////////////////////////////////////////
        //1 找到compute shader中所要使用的KernelID
        int k = ScaleImageComputeShader.FindKernel("CSMain");
        //2 设置贴图    参数1=kid  参数2=shader中对应的buffer名 参数3=对应的texture, 如果要写入贴图，贴图必须是RenderTexture并enableRandomWrite
        ScaleImageComputeShader.SetTexture(k, "Source", targeTexture2D);

        ScaleImageComputeShader.SetTexture(k, "Dst", rtDes);
        ScaleImageComputeShader.SetFloat("widthScale", widthScale);
        ScaleImageComputeShader.SetFloat("heightScale", heightScale);




        //Debug.Log("tex info width is " + texWidth + "  Height is " + texHeight);
        //3 运行shader  参数1=kid  参数2=线程组在x维度的数量 参数3=线程组在y维度的数量 参数4=线程组在z维度的数量
        ScaleImageComputeShader.Dispatch(k, dstWidth, dstHeight, 1);


        Texture2D jpg = new Texture2D(rtDes.width, rtDes.height, TextureFormat.ARGB32, false);
        //RenderTexture.active = rtDes;
        RenderTexture.active = rtDes;

        jpg.ReadPixels(new Rect(0, 0, rtDes.width, rtDes.height), 0, 0);
        jpg.Apply();
        RenderTexture.active = null;



        // SrcRawImage.texture = targeTexture2D;
        // DstRawImage.texture = jpg;

        Destroy(targeTexture2D);
        Destroy(rtDes);
        return jpg;


    }


    /// <summary>
    /// 给图片加边框和标题
    /// </summary>
    /// <param name="yearTex">附着在左上角的提示贴图</param>
    /// <param name="contents"></param>
    /// <param name="fileName"></param>
    /// <param name="size">返回图片原始尺寸</param>
    /// <returns></returns>
    Texture HandlePicture( string contents, string fileName, out Vector2 size)
    {
        byte[] bytes;

        bytes = File.ReadAllBytes(contents + "/" + fileName);

        //512,512参数只是临时，下面的apply应用后，会自动把图片变为原图尺寸
        Texture2D sourceTex = new Texture2D(512, 512);

        sourceTex.LoadImage(bytes);

        sourceTex.Apply();

        size.x = sourceTex.width;
        size.y = sourceTex.height;

       

        return sourceTex;


    }



    public void DestroyTexture()
    {
        foreach (Texture2D texture2D in Texs)
        {
            Destroy(texture2D);
        }
        Texs.Clear();
        Texs = null;
        Resources.UnloadUnusedAssets();
    }


    private void HandleTextureArry(List<Texture2D> texs)
    {

        if (texs == null || texs.Count == 0)
        {
            enabled = false;
            return;
        }

        if (SystemInfo.copyTextureSupport == CopyTextureSupport.None ||
            !SystemInfo.supports2DArrayTextures)
        {
            enabled = false;
            return;
        }
        TexArr = new Texture2DArray(texs[0].width, texs[0].width, texs.Count, TextureFormat.RGBA32, false, false);

        for (int i = 0; i < texs.Count; i++)
        {
            //Debug.Log(" index is" + i);
            try
            {
                Graphics.CopyTexture(texs[i], 0, 0, TexArr, i, 0);
            }
            catch (Exception e)
            {
                Debug.Log("index is" + i);
                throw e;
            }


        }

        TexArr.wrapMode = TextureWrapMode.Clamp;
        TexArr.filterMode = FilterMode.Bilinear;

        Debug.Log("HandleTextureArry End ===============>>>>>>>>>>>   TexArr Length is " + TexArr.depth);
    }

}



/// <summary>
/// 物品所属的大类
/// </summary>

public class ClassInfo
{
    /// <summary>
    /// 该类物品的名字
    /// </summary>
    public string ClassName;
    /// <summary>
    /// 该类物体细分下去的个数
    /// </summary>
    public int ClassCount;
    /// <summary>
    /// 细分下去物体的详细信息
    /// </summary>
    public List<ObjectInfo> ObjectInfos;


    public ClassInfo()
    {
        ObjectInfos = new List<ObjectInfo>();
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append("\r\n");
        sb.Append("\r\n");
        sb.Append("Years is  " + ClassName + "  \r\n");
        sb.Append("EventCount is  " + ClassCount + " \r\n");
        foreach (ObjectInfo yearsEvent in ObjectInfos)
        {
            sb.Append(yearsEvent.ToString());
        }
        sb.Append("\r\n");
        sb.Append("\r\n");
        return sb.ToString();
    }
}

/// <summary>
/// 物品信息
/// </summary>
public class ObjectInfo
{
    /// <summary>
    /// 所属的类
    /// </summary>
    public string BelongsClass;

    public string ObjectName;

    /// <summary>
    /// 所属的类别索引
    /// </summary>
    public int IndexPos;

    /// <summary>
    /// 物品索引集合，如果有多个表示一个物品有多张图片展示
    /// </summary>
    public List<int> PictureIndes;

    /// <summary>
    /// 物品的描述
    /// </summary>
    public string Describe;

    /// <summary>
    /// 物品的描述的文件路径
    /// </summary>
    public string DescribePath;


    /// <summary>
    /// 该物品下的图片描述集合，存的是路径
    /// </summary>
    public List<string> PicturesPath;

    /// <summary>
    /// 描述该物品的的视频
    /// </summary>
    public string ObjectVideo;
    /// <summary>
    /// 每个key对应着每个物品图片的源长和源宽
    /// </summary>
    public Dictionary<int, Vector2> PictureSizeInfo;

    public ObjectInfo()
    {
        PicturesPath = new List<string>();
        PictureIndes = new List<int>();
        PictureSizeInfo = new Dictionary<int, Vector2>();
    }

    public void AddPictureInfo(int index, Vector2 size)
    {
        if (!PictureIndes.Contains(index))
            PictureIndes.Add(index);
        PictureSizeInfo.Add(index, size);
    }
    public override string ToString()
    {


        StringBuilder sb = new StringBuilder();

        sb.Append("\r\n");
        sb.Append("\r\n");
        sb.Append("Years is  " + BelongsClass + " \r\n");
        sb.Append("IndexPos is  " + IndexPos + " \r\n");
        sb.Append("DescribePath is  " + DescribePath + " \r\n");
        foreach (string s in PicturesPath)
        {
            sb.Append("PicturesPath is " + s + "\r\n");
        }
        sb.Append("YearEventVideo is  " + ObjectVideo + "\r\n");
        sb.Append("\r\n");
        sb.Append("\r\n");

        return sb.ToString();
    }
}

