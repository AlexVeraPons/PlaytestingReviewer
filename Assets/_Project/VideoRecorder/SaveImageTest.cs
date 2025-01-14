using System.IO;
using UnityEngine;

public class SaveImageTest : MonoBehaviour
{
    void Start()
    {
        SaveTestImage();
    }

    void SaveTestImage()
    {
        Texture2D blackImage = new Texture2D(1280, 720);
        Color32[] pixels = new Color32[1280 * 720];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.black;
        }

        blackImage.SetPixels32(pixels);
        blackImage.Apply();

        byte[] bytes = blackImage.EncodeToPNG();
        string filePath = System.IO.Path.Combine(Application.dataPath, "CapturedFrames", "test_image.png");
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));
        File.WriteAllBytes(filePath, bytes);

        Debug.Log("Test image saved to: " + filePath);
    }

}