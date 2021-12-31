using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
public class DownloadIcon : MonoBehaviour
{
    public bool test;
    public void OnValidate()
    {
        Texture tex = EditorGUIUtility.IconContent("MeshFilter Icon").image;

        RenderTexture tmp = RenderTexture.GetTemporary(
                tex.width,
                tex.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.sRGB
                );

        Graphics.Blit(tex, tmp);


        List<byte> test = new System.Collections.Generic.List<byte>();
        RenderTexture.active = tmp;

        Texture2D texture = new Texture2D(tex.width, tex.height);

        texture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        texture.Apply();

        byte[] pixels = texture.EncodeToPNG();

        System.IO.File.WriteAllBytes(System.IO.Path.Combine(Application.dataPath, "Schema/test.png"), pixels);

    }
}