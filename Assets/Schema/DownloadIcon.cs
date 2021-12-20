using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class DownloadIcon : MonoBehaviour
{
	// Start is called before the first frame update
	private void Start()
	{
		Texture tex = EditorGUIUtility.IconContent("GameObject Icon").image;
		Debug.Log(tex);

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

		System.IO.File.WriteAllBytes(Application.dataPath + $"/images/blah.png", pixels);
	}
}
