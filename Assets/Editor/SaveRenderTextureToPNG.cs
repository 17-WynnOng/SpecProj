using UnityEngine;
using UnityEditor;
using System.IO;

public class SaveRenderTextureToPNG
{
    [MenuItem("Tools/Export RenderTexture as PNG")]
    public static void ExportRT()
    {
        RenderTexture rt = Selection.activeObject as RenderTexture;
        if (rt == null)
        {
            Debug.LogError("Select a RenderTexture first!");
            return;
        }

        // Create a temporary Texture2D
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        string path = EditorUtility.SaveFilePanel("Save PNG", Application.dataPath, rt.name + ".png", "png");

        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
            Debug.Log("Saved PNG to: " + path);
        }

        RenderTexture.active = currentRT;
    }
}
