using System;
using System.Collections;
using System.Collections.Generic;

using System.IO;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = System.Random;

public static class Common
{



    /// <summary>
    /// 交互的类别
    /// </summary>
    public static int Category = 0;

    /// <summary>
    /// 状态码
    /// </summary>
    public static int StateCode;
    /// <summary>
    /// 种类的状态码
    /// </summary>
    public static Dictionary<int, List<int>> CategoryDic = new Dictionary<int, List<int>>();
    /// <summary>
    /// 图片的宽
    /// </summary>
    public static int PictureWidth = 256;
    /// <summary>
    /// 图片的高
    /// </summary>
    public static int PictureHeight = 256;

    /// <summary>
    /// 每个层次的图片个数
    /// </summary>
    public static int PictureCount = 200;

    public static float HeightTemp = 0f;

    /// <summary>
    /// 向屏幕的移动速度
    /// </summary>
    public static float MoveSpeed = 10f;
    /// <summary>
    /// 透明度的速度
    /// </summary>
    public static float Alpha = 20f;
    /// <summary>
    /// 荣誉墙头像索引
    /// </summary>
    public static Int64 PictureIndex;

    /// <summary>
    /// 骨骼连接点
    /// </summary>
    public static Dictionary<int,List<int>>  BonePos = new Dictionary<int, List<int>>();

    public static void Init()
    {
        Debug.Log("init");
        CategoryDic.Add(0, new List<int> { 0,1,2 });
       // CategoryDic.Add(1, new List<int> { 3, 1 });
        CategoryDic.Add(1, new List<int> { 3 });

        BonePos.Add(0, new List<int>() { 2, 3 });
        BonePos.Add(1,new List<int>(){0,1});
        BonePos.Add(2, new List<int>() { 0, 16 });
        BonePos.Add(3, new List<int>() { 0, 12 });
        BonePos.Add(4, new List<int>() { 16, 17});
        BonePos.Add(5, new List<int>() { 17, 18 });
        BonePos.Add(6, new List<int>() { 18, 19 });
        BonePos.Add(7, new List<int>() { 12, 13 });
        BonePos.Add(8, new List<int>() { 13, 14 });
        BonePos.Add(9, new List<int>() { 14, 15 });
        BonePos.Add(10, new List<int>() { 1, 20 });
        BonePos.Add(11, new List<int>() { 20, 8 });
        BonePos.Add(12, new List<int>() { 8, 9 });
        BonePos.Add(13, new List<int>() { 9, 10 });
        BonePos.Add(14, new List<int>() { 10, 11 });
        BonePos.Add(15, new List<int>() { 20, 4 });
        BonePos.Add(16, new List<int>() { 4, 5 });
        BonePos.Add(17, new List<int>() { 5, 6 });
        BonePos.Add(18, new List<int>() { 6, 7 });
        BonePos.Add(19, new List<int>() { 20, 2 });
        
    }

    /// <summary>
    /// 根据种类所包含的状态码，过滤掉不属于该种类的状态码
    /// </summary>
    /// <param name="stateCode">状态码</param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static void Filter(Action<bool> action)
    {
        //根据不同的交互类型过滤
        if (CategoryDic.ContainsKey(Common.Category))
        {
            List<int> values = CategoryDic[Common.Category];

            if (values.Contains(StateCode)) action(true);
            return;
        }

        action(false);
    }

    /// <summary>
    /// 改变种类
    /// </summary>
    public static void ChangeCategory()
    {
        Category++;

        if (!CategoryDic.ContainsKey(Category))
        {
            Category = 0;//归为初始状态
           
        }

        ChangeStateCode(CategoryDic[Category][0]);//改变一个种类就要设置这个种类的初始状态码
    }

    public static void ChangeStateCode(int code)
    {
       // Debug.LogError("改变了状态码");
        StateCode = code;
    }

    /// <summary>
    /// 改变运动的种类
    /// </summary>
    /// <param name="category"></param>
    public static void ChangeCategory(int category)
    {
        if (CategoryDic.ContainsKey(category))
        {
            Category = category;
            StateCode = CategoryDic[category][0];//改变种类后并把改种类归为第一种状态
        }
    }
    public static float GetCross(Vector2 p1, Vector2 p2, Vector2 p)
    {
        return (p2.x - p1.x) * (p.y - p1.y) - (p.x - p1.x) * (p2.y - p1.y);
    }
    //计算一个点是否在矩形里  2d
    public static bool ContainsQuadrangle(Vector2 leftDownP2, Vector2 leftUpP1, Vector2 rightDownP3, Vector2 rightUpP4, Vector2 p)
    {

        float value1 = GetCross(leftUpP1, leftDownP2, p);

        float value2 = GetCross(rightDownP3, rightUpP4, p);

        if (value1 * value2 < 0) return false;

        float value3 = GetCross(leftDownP2, rightDownP3, p);

        float value4 = GetCross(rightUpP4, leftUpP1, p);

        if (value3 * value4 < 0) return false;
        
        return true;
    }

    public static IEnumerator WaitTime(float time,Action callBack)
    {
        yield return new WaitForSeconds(time);

        if (callBack != null) callBack();
    }
    /// <summary>
    /// 计算在长和宽的矩形里，半径为R的物体能塞多少个，并且不会重叠
    /// </summary>
    /// <param name="width">矩形的长</param>
    /// <param name="height">矩形的宽</param>
    /// <param name="r">小物体半径</param>
    /// <param name="k">计算次数，值越大，越密集，计算量就越大</param>
    /// <returns></returns>
    public static List<Vector2> Sample2D(float width, float height, float r, int k = 30)
    {
        return Sample2D((int)DateTime.Now.Ticks, width, height, r, k);
    }

    public static List<Vector2> Sample2D(int seed, float width, float height, float r, int k = 30)
    {
        // STEP 0

        // 维度，平面就是2维
        var n = 2;

        // 计算出合理的cell大小
        // cell是一个正方形，为了保证每个cell内部不可能出现多个点，那么cell内的任意点最远距离不能大于r
        // 因为cell内最长的距离是对角线，假设对角线长度是r，那边长就是下面的cell_size
        var cell_size = r / Math.Sqrt(n);

        // 计算出有多少行列的cell
        var cols = (int)Math.Ceiling(width / cell_size);
        var rows = (int)Math.Ceiling(height / cell_size);

        // cells记录了所有合法的点
        var cells = new List<Vector2>();

        // grids记录了每个cell内的点在cells里的索引，-1表示没有点
        var grids = new int[rows, cols];
        for (var i = 0; i < rows; ++i)
        {
            for (var j = 0; j < cols; ++j)
            {
                grids[i, j] = -1;
            }
        }

        // STEP 1
        var random = new System.Random(seed);

        // 随机选一个起始点
        var x0 = new Vector2(random.Next((int)width), random.Next((int)height));
        var col = (int)Math.Floor(x0.x / cell_size);
        var row = (int)Math.Floor(x0.y / cell_size);

        var x0_idx = cells.Count;
        cells.Add(x0);
        grids[row, col] = x0_idx;

        var active_list = new List<int>();
        active_list.Add(x0_idx);

        // STEP 2
        while (active_list.Count > 0)
        {
            // 随机选一个待处理的点xi
            var xi_idx = active_list[random.Next(active_list.Count)]; // 区间是[0,1)，不用担心溢出。
            var xi = cells[xi_idx];
            var found = false;

            // 以xi为中点，随机找与xi距离在[r,2r)的点xk，并判断该点的合法性
            // 重复k次，如果都找不到，则把xi从active_list中去掉，认为xi附近已经没有合法点了
            for (var i = 0; i < k; ++i)
            {
                var dir = UnityEngine.Random.insideUnitCircle;
                var xk = xi + (dir.normalized * r + dir * r); // [r,2r)
                if (xk.x < 0 || xk.x >= width || xk.y < 0 || xk.y >= height)
                {
                    continue;
                }

                col = (int)Math.Floor(xk.x / cell_size);
                row = (int)Math.Floor(xk.y / cell_size);

                if (grids[row, col] != -1)
                {
                    continue;
                }

                // 要判断xk的合法性，就是要判断有附近没有点与xk的距离小于r
                // 由于cell的边长小于r，所以只测试xk所在的cell的九宫格是不够的（考虑xk正好处于cell的边缘的情况）
                // 正确做法是以xk为中心，做一个边长为2r的正方形，测试这个正方形覆盖到所有cell
                var ok = true;
                var min_r = (int)Math.Floor((xk.y - r) / cell_size);
                var max_r = (int)Math.Floor((xk.y + r) / cell_size);
                var min_c = (int)Math.Floor((xk.x - r) / cell_size);
                var max_c = (int)Math.Floor((xk.x + r) / cell_size);
                for (var or = min_r; or <= max_r; ++or)
                {
                    if (or < 0 || or >= rows)
                    {
                        continue;
                    }

                    for (var oc = min_c; oc <= max_c; ++oc)
                    {
                        if (oc < 0 || oc >= cols)
                        {
                            continue;
                        }

                        var xj_idx = grids[or, oc];
                        if (xj_idx != -1)
                        {
                            var xj = cells[xj_idx];
                            var dist = (xj - xk).magnitude;
                            if (dist < r)
                            {
                                ok = false;
                                goto end_of_distance_check;
                            }
                        }
                    }
                }

            end_of_distance_check:
                if (ok)
                {
                    var xk_idx = cells.Count;
                    cells.Add(xk);

                    grids[row, col] = xk_idx;
                    active_list.Add(xk_idx);

                    found = true;
                    break;
                }
            }

            if (!found)
            {
                active_list.Remove(xi_idx);
            }
        }

        return cells;
    }

    /// <summary>
    /// 根据图片路径返回图片的字节流byte[]
    /// </summary>
    /// <param name="imagePath">图片路径</param>
    /// <returns>返回的字节流</returns>
    private static byte[] GetImageByte(string imagePath)
    {
        FileStream files = new FileStream(imagePath, FileMode.Open);
        byte[] imgByte = new byte[files.Length];
        files.Read(imgByte, 0, imgByte.Length);
        files.Close();
        return imgByte;
    }


    /// <summary>
    /// 根据T值，计算贝塞尔曲线上面相对应的点
    /// </summary>
    /// <param name="t"></param>T值
    /// <param name="p0"></param>起始点
    /// <param name="p1"></param>控制点
    /// <param name="p2"></param>目标点
    /// <returns></returns>根据T值计算出来的贝赛尔曲线点
    private static Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;

        return p;
    }

    /// <summary>
    /// 获取存储贝塞尔曲线点的数组
    /// </summary>
    /// <param name="startPoint"></param>起始点
    /// <param name="controlPoint"></param>控制点
    /// <param name="endPoint"></param>目标点
    /// <param name="segmentNum"></param>采样点的数量
    /// <returns></returns>存储贝塞尔曲线点的数组
    public static Vector3[] GetBeizerList(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint, int segmentNum)
    {
        Vector3[] path = new Vector3[segmentNum];
        for (int i = 1; i <= segmentNum; i++)
        {
            float t = i / (float)segmentNum;
            Vector3 pixel = CalculateCubicBezierPoint(t, startPoint,
                controlPoint, endPoint);
            path[i - 1] = pixel;
           
        }
        return path;

    }

    /// <summary>
    /// 从图片中获取向量信息
    /// </summary>
    /// <param name="tex"></param>
    /// <param name="maxAlpha"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static  List<Vector3> GetPos(Texture2D tex, float maxAlpha, int count,float scale = 1f)
    {

        var clos = tex.GetPixels();

        int texWidth = tex.width;

        int texHeight = tex.height;

        Color [,]  colors = new Color[tex.width, tex.height];

        int k = 0;
        for (int i = 0; i < clos.Length; i++)
        {

            if (i >= (k + 1) * tex.width)
            {
                k++;
            }

            colors[i - k * tex.height, k] = clos[i];


        }

        List<Vector3> posList = new List<Vector3>();



        for (int i = 0; i < count; i++)
        {
            while (true)
            {
                int width = UnityEngine.Random.Range(0, tex.width);
                int height = UnityEngine.Random.Range(0, tex.height);

                Color col = colors[width, height];

                float val = col.r + col.g + col.b;

                var pos = new Vector2(width, height) - new Vector2(texWidth / 2, texHeight / 2);

               


                if (val >= maxAlpha)
                {
                    pos = pos / scale;

                    float dis = pos.magnitude;

                    float r = texWidth / 2f;

                    float v1 = dis / r;

                    //float h = (1 - v1)*100;//* UnityEngine.Random.Range(-r /scale/25, r / scale/25);

                    float h = UnityEngine.Random.Range(-r / scale / 25, r / scale / 25);

                    h = h / (dis/10);

                    if (h >= 2) h = 2;
                    if (h <= -2) h = -2;

                    posList.Add(new Vector3(pos.x,h,pos.y));

                    break;
                }

            }


        }

        return posList;
    }

    #region Easing Curves

    public static float Linear(float start, float end, float value)
    {
        return Mathf.Lerp(start, end, value);
    }

    public static float Clerp(float start, float end, float value)
    {
        float min = 0.0f;
        float max = 360.0f;
        float half = Mathf.Abs((max - min) / 2.0f);
        float retval = 0.0f;
        float diff = 0.0f;
        if ((end - start) < -half)
        {
            diff = ((max - start) + end) * value;
            retval = start + diff;
        }
        else if ((end - start) > half)
        {
            diff = -((max - end) + start) * value;
            retval = start + diff;
        }
        else retval = start + (end - start) * value;
        return retval;
    }

    public static float Spring(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
        return start + (end - start) * value;
    }

    public static float EaseInQuad(float start, float end, float value)
    {
        end -= start;
        return end * value * value + start;
    }

    public static float EaseOutQuad(float start, float end, float value)
    {
        end -= start;
        return -end * value * (value - 2) + start;
    }

    public static float EaseInOutQuad(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end / 2 * value * value + start;
        value--;
        return -end / 2 * (value * (value - 2) - 1) + start;
    }

    public static float EaseInCubic(float start, float end, float value)
    {
        end -= start;
        return end * value * value * value + start;
    }

    public static float EaseOutCubic(float start, float end, float value)
    {
        value--;
        end -= start;
        return end * (value * value * value + 1) + start;
    }

    public static float EaseInOutCubic(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end / 2 * value * value * value + start;
        value -= 2;
        return end / 2 * (value * value * value + 2) + start;
    }

    public static float EaseInQuart(float start, float end, float value)
    {
        end -= start;
        return end * value * value * value * value + start;
    }

    public static float EaseOutQuart(float start, float end, float value)
    {
        value--;
        end -= start;
        return -end * (value * value * value * value - 1) + start;
    }

    public static float EaseInOutQuart(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end / 2 * value * value * value * value + start;
        value -= 2;
        return -end / 2 * (value * value * value * value - 2) + start;
    }

    public static float EaseInQuint(float start, float end, float value)
    {
        end -= start;
        return end * value * value * value * value * value + start;
    }

    public static float EaseOutQuint(float start, float end, float value)
    {
        value--;
        end -= start;
        return end * (value * value * value * value * value + 1) + start;
    }

    public static float EaseInOutQuint(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end / 2 * value * value * value * value * value + start;
        value -= 2;
        return end / 2 * (value * value * value * value * value + 2) + start;
    }

    public static float EaseInSine(float start, float end, float value)
    {
        end -= start;
        return -end * Mathf.Cos(value / 1 * (Mathf.PI / 2)) + end + start;
    }

    public static float EaseOutSine(float start, float end, float value)
    {
        end -= start;
        return end * Mathf.Sin(value / 1 * (Mathf.PI / 2)) + start;
    }

    public static float EaseInOutSine(float start, float end, float value)
    {
        end -= start;
        return -end / 2 * (Mathf.Cos(Mathf.PI * value / 1) - 1) + start;
    }

    public static float EaseInExpo(float start, float end, float value)
    {
        end -= start;
        return end * Mathf.Pow(2, 10 * (value / 1 - 1)) + start;
    }

    public static float EaseOutExpo(float start, float end, float value)
    {
        end -= start;
        return end * (-Mathf.Pow(2, -10 * value / 1) + 1) + start;
    }

    public static float EaseInOutExpo(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end / 2 * Mathf.Pow(2, 10 * (value - 1)) + start;
        value--;
        return end / 2 * (-Mathf.Pow(2, -10 * value) + 2) + start;
    }

    public static float EaseInCirc(float start, float end, float value)
    {
        end -= start;
        return -end * (Mathf.Sqrt(1 - value * value) - 1) + start;
    }

    public static float EaseOutCirc(float start, float end, float value)
    {
        value--;
        end -= start;
        return end * Mathf.Sqrt(1 - value * value) + start;
    }

    public static float EaseInOutCirc(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return -end / 2 * (Mathf.Sqrt(1 - value * value) - 1) + start;
        value -= 2;
        return end / 2 * (Mathf.Sqrt(1 - value * value) + 1) + start;
    }

    /* GFX47 MOD START */
    public static float EaseInBounce(float start, float end, float value)
    {
        end -= start;
        float d = 1f;
        return end - EaseOutBounce(0, end, d - value) + start;
    }
    /* GFX47 MOD END */

    /* GFX47 MOD START */
    //public static float bounce(float start, float end, float value){
    public static float EaseOutBounce(float start, float end, float value)
    {
        value /= 1f;
        end -= start;
        if (value < (1 / 2.75f))
        {
            return end * (7.5625f * value * value) + start;
        }
        else if (value < (2 / 2.75f))
        {
            value -= (1.5f / 2.75f);
            return end * (7.5625f * (value) * value + .75f) + start;
        }
        else if (value < (2.5 / 2.75))
        {
            value -= (2.25f / 2.75f);
            return end * (7.5625f * (value) * value + .9375f) + start;
        }
        else
        {
            value -= (2.625f / 2.75f);
            return end * (7.5625f * (value) * value + .984375f) + start;
        }
    }
    /* GFX47 MOD END */

    /* GFX47 MOD START */
    public static float EaseInOutBounce(float start, float end, float value)
    {
        end -= start;
        float d = 1f;
        if (value < d / 2) return EaseInBounce(0, end, value * 2) * 0.5f + start;
        else return EaseOutBounce(0, end, value * 2 - d) * 0.5f + end * 0.5f + start;
    }
    /* GFX47 MOD END */

    public static float EaseInBack(float start, float end, float value)
    {
        end -= start;
        value /= 1;
        float s = 1.70158f;
        return end * (value) * value * ((s + 1) * value - s) + start;
    }

    public static float EaseOutBack(float start, float end, float value)
    {
        float s = 1.70158f;
        end -= start;
        value = (value / 1) - 1;
        return end * ((value) * value * ((s + 1) * value + s) + 1) + start;
    }

    public static float EaseInOutBack(float start, float end, float value)
    {
        float s = 1.70158f;
        end -= start;
        value /= .5f;
        if ((value) < 1)
        {
            s *= (1.525f);
            return end / 2 * (value * value * (((s) + 1) * value - s)) + start;
        }
        value -= 2;
        s *= (1.525f);
        return end / 2 * ((value) * value * (((s) + 1) * value + s) + 2) + start;
    }

    public static float Punch(float amplitude, float value)
    {
        float s = 9;
        if (Math.Abs(value) < 0.000001f)
        {
            return 0;
        }
        if (Math.Abs(value - 1) < 0.000001f)
        {
            return 0;
        }
        float period = 1 * 0.3f;
        s = period / (2 * Mathf.PI) * Mathf.Asin(0);
        return (amplitude * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * 1 - s) * (2 * Mathf.PI) / period));
    }

    /* GFX47 MOD START */
    public static float EaseInElastic(float start, float end, float value)
    {
        end -= start;

        float d = 1f;
        float p = d * .3f;
        float s = 0;
        float a = 0;

        if (Math.Abs(value) < 0.000001f) return start;

        if (Math.Abs((value /= d) - 1) < 0.000001f) return start + end;

        if (Math.Abs(a) < 0.000001f || a < Mathf.Abs(end))
        {
            a = end;
            s = p / 4;
        }
        else
        {
            s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
        }

        return -(a * Mathf.Pow(2, 10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
    }
    /* GFX47 MOD END */

    /* GFX47 MOD START */
    //public static float elastic(float start, float end, float value){
    public static float EaseOutElastic(float start, float end, float value)
    {
        /* GFX47 MOD END */
        //Thank you to rafael.marteleto for fixing this as a port over from Pedro's UnityTween
        end -= start;

        float d = 1f;
        float p = d * .3f;
        float s = 0;
        float a = 0;

        if (Math.Abs(value) < 0.000001f) return start;

        if (Math.Abs((value /= d) - 1) < 0.000001f) return start + end;

        if (Math.Abs(a) < 0.0000001f || a < Mathf.Abs(end))
        {
            a = end;
            s = p / 4;
        }
        else
        {
            s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
        }

        return (a * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) + end + start);
    }

    /* GFX47 MOD START */
    public static float EaseInOutElastic(float start, float end, float value)
    {
        end -= start;

        float d = 1f;
        float p = d * .3f;
        float s = 0;
        float a = 0;

        if (Math.Abs(value) < 0.000001f) return start;

        if (Math.Abs((value /= d / 2) - 2) < 0.00000001f) return start + end;

        if (Math.Abs(a) < 0.000001f || a < Mathf.Abs(end))
        {
            a = end;
            s = p / 4;
        }
        else
        {
            s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
        }

        if (value < 1) return -0.5f * (a * Mathf.Pow(2, 10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
        return a * Mathf.Pow(2, -10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) * 0.5f + end + start;
    }
    /* GFX47 MOD END */

    #endregion

}
