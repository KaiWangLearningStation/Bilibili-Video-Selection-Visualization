using UnityEditor;
using UnityEngine;
using System.IO;

public class MaterialBatchCreator : EditorWindow
{
    [MenuItem("Tools/Batch Create Materials")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(MaterialBatchCreator), false, "Batch Material Creator");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Create Materials from Textures"))
        {
            CreateMaterials();
        }
    }

    private void CreateMaterials()
    {
        // 获取所有纹理文件（假设你的纹理存放在 "Assets/Textures" 文件夹中）
        string[] texturePaths = Directory.GetFiles("Assets/Resources/Pic", "*.png", SearchOption.AllDirectories);

        foreach (string texturePath in texturePaths)
        {
            // 加载纹理
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture != null)
            {
                // 创建新的材质
                Material material = new Material(Shader.Find("Standard"));

                // 将纹理应用到材质的MainTex属性
                material.mainTexture = texture;

                // 设置材质的颜色，并设置Alpha通道为0.7
                Color materialColor = material.color; // 获取当前颜色
                materialColor.a = 0.5f; // 设置Alpha值为0.7
                material.color = materialColor; // 应用新的颜色

                // 设置渲染模式为透明
                material.SetInt("_Mode", 3); // 3 表示透明模式 (Standard Shader 的透明模式)
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_BlendSrc", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_BlendDst", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000; // 透明队列

                // 获取纹理文件名（去掉扩展名）
                string materialName = Path.GetFileNameWithoutExtension(texturePath);

                // 保存材质到Assets目录
                string materialPath = "Assets/Resources/Pic/Materials/" + materialName + ".mat";
                AssetDatabase.CreateAsset(material, materialPath);
            }
        }

        // 刷新AssetDatabase
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 给出提示
        EditorUtility.DisplayDialog("Batch Material Creation", "Materials have been created!", "OK");
    }
}
