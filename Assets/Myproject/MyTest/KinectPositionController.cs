using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KinectPositionController : MonoBehaviour
{
    public GameObject sphere;//预制体
    private KinectManager manager = null;
    private GameObject[] joints;//关节数组
    private bool isCreate = false;//用于标注骨骼点物体是否创建
    private long userID = 0;

    public Image HandLeft;

    public Image HanlRight;
    // Use this for initialization
    void Start()
    {
        manager = KinectManager.Instance;//初始化KinectManager对象
    }
    // Update is called once per frame
    void Update()
    {
        //Rect backgroundRect = this.mainCamera.pixelRect;
        //PortraitBackground portraitBack = PortraitBackground.Instance;
        //bool flag = portraitBack && portraitBack.enabled;
        //if (flag)
        //{
        //    backgroundRect = portraitBack.GetBackgroundRect();
        //}
        //Vector3 pos = KinectManager.Instance.GetJointPosColorOverlay(obj, 3, this.mainCamera, backgroundRect);
        //Vector3 screenPos = this.mainCamera.WorldToScreenPoint(pos);
    }
   
    
}
