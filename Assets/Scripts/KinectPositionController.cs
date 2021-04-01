using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KinectPositionController : MonoBehaviour
{

    public Button ChangeButton;
   
    // Use this for initialization
    void Start()
    {
        
        ChangeButton.onClick.AddListener((() =>
        {
            Debug.Log("onClick");
            Common.ChangeCategory();

        }));
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
        CheckUserRange(0.65f, 0.45f, 4f);
    }

    private void CheckUserRange(float x, float y, float z)
    {
        bool flag = KinectManager.Instance.GetAllUserIds().Count > 0;
        if (flag)
        {
            List<long> ids = KinectManager.Instance.GetAllUserIds();
            foreach (long id in ids)
            {
                Vector3 pos = KinectManager.Instance.GetUserPosition(id);
                bool flag2 = Mathf.Abs(pos.x) > x;
                if (flag2)
                {
                    KinectManager.Instance.ClearKinectUsers();
                }
                bool flag3 = Mathf.Abs(pos.z) > z;
                if (flag3)
                {
                    KinectManager.Instance.ClearKinectUsers();
                }
            }
        }
    }
}
