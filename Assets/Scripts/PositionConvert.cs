using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Kinect得到的人体位置转换成屏幕位置
/// </summary>
public class PositionConvert : MonoBehaviour
{

    [Tooltip("Camera that will be used to overlay the 3D-objects over the background.")]
    public Camera foregroundCamera;

    [Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
    public int playerIndex = 0;




    public static PositionConvert Instance;
    
    /// <summary>
    /// 每个关节的屏幕位置
    /// </summary>
    public Dictionary<string,Vector3> ScreenPosDictionary = new Dictionary<string, Vector3>();

    

    /// <summary>
    /// 手势事件，双手是否张开或者叠加在胸前
    /// </summary>
    public event Action<bool> HandEvent;

    private bool _isOpenHand = true;
    private Vector4[] _cache;
    /// <summary>
    /// 半径
    /// </summary>
    public float Radius= 1f;
    private WaitForEndOfFrame wfef;
    public Vector3 SpineMidpos;
    private Coroutine _coroutine;

    public float SqrMagnitudeValue = 100;
    private void Awake()
    {
        if(Instance!=null)throw new UnityException("已经有了一个单例了");
        Instance = this;
    }

    void Start()
    {
        KinectManager manager = KinectManager.Instance;
        wfef = new WaitForEndOfFrame();

        if (manager && manager.IsInitialized())
        {
            int jointsCount = manager.GetJointCount();

           
                // array holding the skeleton joints
               

                for (int i = 0; i < jointsCount; i++)
                {
                    
                    string key = ((KinectInterop.JointType)i).ToString();
                    ScreenPosDictionary.Add(key,Vector3.zero);
                   
                }

                



        }

        if (!foregroundCamera)
        {
            // by default - the main camera
            foregroundCamera = Camera.main;
        }
    }

    void Update()
    {
        KinectManager manager = KinectManager.Instance;

        if (manager && manager.IsInitialized() && foregroundCamera)
        {

            // get the background rectangle (use the portrait background, if available)
            Rect backgroundRect = foregroundCamera.pixelRect;
            PortraitBackground portraitBack = PortraitBackground.Instance;

            if (portraitBack && portraitBack.enabled)
            {
                backgroundRect = portraitBack.GetBackgroundRect();
            }

            // overlay all joints in the skeleton
            if (manager.IsUserDetected(playerIndex))
            {
                long userId = manager.GetUserIdByIndex(playerIndex);
                int jointsCount = manager.GetJointCount();

                for (int i = 0; i < jointsCount; i++)
                {
                    int joint = i;

                    if (manager.IsJointTracked(userId, joint))
                    {
                        Vector3 posJoint = manager.GetJointPosColorOverlay(userId, joint, foregroundCamera, backgroundRect);

                        Vector3 screenPos = foregroundCamera.WorldToScreenPoint(posJoint);

                        string jointName = ((KinectInterop.JointType)joint).ToString();

                        if (joint == 1) SpineMidpos = posJoint;

                         ScreenPosDictionary[jointName] = screenPos;
                        

                    }
                   
                }

            }
        }

        CheckHand();
    }

    private void CheckHand()
    {
        Vector3 hl = default, hr = default, spineMid = default;

        if (ScreenPosDictionary.ContainsKey("HandLeft"))
        {
             hl = ScreenPosDictionary["HandLeft"];
           
        }

        if (ScreenPosDictionary.ContainsKey("HandRight"))
        {
             hr = ScreenPosDictionary["HandRight"];
            
        }
        //spineMid
        if (ScreenPosDictionary.ContainsKey("SpineMid"))
        {
            spineMid = ScreenPosDictionary["SpineMid"];

        }

        if (hl == Vector3.zero && hr == Vector3.zero && spineMid == Vector3.zero) return;

        float d1 = Vector3.Distance(hl , spineMid);

        float d2 = Vector3.Distance(hr , spineMid);

       // Debug.Log("d1 is " + d1+"    d2 is " + d2);


        if (d1 <= Radius && d2 <= Radius)
        {
            if (_isOpenHand)
            {
                //Debug.LogError("双手合实");
                _isOpenHand = false;

                if (HandEvent != null) HandEvent(false);
               
            }
           
        }
        else
        {
            if (!_isOpenHand)
            {
               // Debug.LogError("双手张开");
                _isOpenHand = true;
                if (HandEvent != null) HandEvent(true);
                
            }
           
        }
    }
   

    public Vector3 GetScreenPos(string joint)
    {
        if (ScreenPosDictionary.ContainsKey(joint))
        {
            Vector3 temp = ScreenPosDictionary[joint];
            return new Vector2( temp.x, temp.y);
        }

        return Vector3.zero;
    }

   
    public Vector4[] GetPosArray()
    {
        

        List<Vector4> tempList = new List<Vector4>();
       foreach (Vector3 vector3 in ScreenPosDictionary.Values)
       {
           Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(vector3.x, vector3.y, 30.5f));
            tempList.Add(new Vector4(worldPos.x, worldPos.y, worldPos.z,1));
       }


       if (_cache == null)
       {
           _cache = tempList.ToArray();
       }
       else
       {


           //检测位置，如果位置改变太快，则舍弃新的位置

           Vector4[] array = tempList.ToArray();

           for (int i = 0; i < _cache.Length; i++)
           {
               Vector3 v1 = _cache[i];
               Vector3 v2 = array[i];

               Vector3 d = v1 - v2;



               if (d.magnitude <= SqrMagnitudeValue && v2.magnitude>=1)
                   _cache[i] = array[i];
               
            }
        }
        return _cache;

    }
}
