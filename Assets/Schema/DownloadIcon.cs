using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class DownloadIcon : MonoBehaviour
{
    public static void Download()
    {
        GUIContent c = EditorGUIUtility.IconContent("Favorite On Icon");

        Texture tex = c.image;

        RenderTexture tmp = RenderTexture.GetTemporary(
            tex.width,
            tex.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.sRGB
        );

        Graphics.Blit(tex, tmp);

        List<byte> test = new List<byte>();
        RenderTexture.active = tmp;

        Texture2D texture = new Texture2D(tex.width, tex.height);

        texture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        texture.Apply();

        byte[] pixels = texture.EncodeToPNG();

        File.WriteAllBytes(Path.Combine(Application.dataPath, "Schema/test.png"), pixels);
    }
}