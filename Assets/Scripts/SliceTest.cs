using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class SliceTest : MonoBehaviour
{
    public List<Sprite> Sprites;

    public string SpriteName;
    // Start is called before the first frame update
    void Start()
    {
        if(SpriteName==null)throw new UnityException("名字不能为null");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SvaeSprite()
    {
        string path = Application.streamingAssetsPath + "/flower/" + SpriteName;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        int count = 0;
        foreach (Sprite sprite in Sprites)
        {
           
            var targetTex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            var pixels = sprite.texture.GetPixels(
                (int)sprite.textureRect.x,
                (int)sprite.textureRect.y,
                (int)sprite.textureRect.width,
                (int)sprite.textureRect.height);
            targetTex.SetPixels(pixels);
            targetTex.Apply();
                

            byte [] bytes = targetTex.EncodeToPNG();

            File.WriteAllBytes(path+"/"+ count+".png", bytes);
            count++;
        }
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0f, 0f, 100f, 100f), "slice"))
        {
            SvaeSprite();
        }
    }
}
