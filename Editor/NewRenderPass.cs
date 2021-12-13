using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class NewRenderPass : EditorWindow
{

    private string customName = "New Render Pass";

    private const string SearchFilter_Feature = "t:TextAsset RenderFeatureTemplete";
    private const string SearchFilter_Pass = "t:TextAsset RenderPassTemplete";
    private const string SearchFilter_Volume = "t:TextAsset RenderVolumeTemplete";
    private const string SearchFilter_Shader = "t:TextAsset RenderShaderTemplete";

    private string featureData;
    private string passData;
    private string volumeData;
    private string shaderData;

    [MenuItem("Assets/Create/Render Pass", false, 0)]
    public static void OpenCreateWindow()
    {
        float width = 400;
        float height = 50;
        Rect rect = new Rect(Screen.width * 0.5f - width, Screen.height * 0.5f - height, width, height);
        NewRenderPass window = GetWindowWithRect<NewRenderPass>(rect, true, "New Render Pass");
        window.Show();
    }
    private void OnGUI()
    {
        customName = EditorGUILayout.TextField("RenderPassName", customName);
        if (GUILayout.Button("Create"))
        {
            GetTempleteData();
            CreateRenderFile();
            AssetDatabase.Refresh();
            Debug.Log("Create render pass <" + customName + "> succeed");
            Close();
        }
    }
    private void GetTempleteData()
    {
        string featureTempletePath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets(SearchFilter_Feature)[0]);
        string passTempletePath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets(SearchFilter_Pass)[0]);
        string volumeTempletePath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets(SearchFilter_Volume)[0]);
        string shaderTempletePath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets(SearchFilter_Shader)[0]);
        featureData = AssetDatabase.LoadAssetAtPath<TextAsset>(featureTempletePath).text.Replace("#CUSTOMNAME#",customName);
        passData = AssetDatabase.LoadAssetAtPath<TextAsset>(passTempletePath).text.Replace("#CUSTOMNAME#", customName);
        volumeData = AssetDatabase.LoadAssetAtPath<TextAsset>(volumeTempletePath).text.Replace("#CUSTOMNAME#", customName);
        shaderData = AssetDatabase.LoadAssetAtPath<TextAsset>(shaderTempletePath).text.Replace("#CUSTOMNAME#", customName);
    }
    private bool TryGetCurrentFolderPath(out string currPath)
    {
        currPath=null;
        string[] selGUIDS = Selection.assetGUIDs;
        if (selGUIDS.Length > 0)
        {
            currPath = AssetDatabase.GUIDToAssetPath(selGUIDS[0]);
            if (!AssetDatabase.IsValidFolder(currPath))
            {
                currPath = System.IO.Path.GetDirectoryName(currPath);
                Regex regex = new Regex(@"\\");
                currPath = regex.Replace(currPath, "/");
            }
            return true;
        }
        return false;
    }
    private void CreateRenderFile()
    {
        if(TryGetCurrentFolderPath(out string currentPath))
        {
            if (!AssetDatabase.IsValidFolder(customName))
            {
                AssetDatabase.CreateFolder(currentPath, customName);
            }
            string featurePath = currentPath + "/" + customName + "/" + customName + "Feature.cs";
            string passPath = currentPath + "/" + customName + "/" + customName + "Pass.cs";
            string volumePath = currentPath + "/" + customName + "/" + customName + "Volume.cs";
            string shaderPath = currentPath + "/" + customName + "/" + customName + "Shader.shader";
            CreateFile(featurePath, featureData);
            CreateFile(passPath, passData);
            CreateFile(volumePath, volumeData);
            CreateFile(shaderPath, shaderData);
        }
        else
        {
            throw new System.IO.DirectoryNotFoundException("No folder selected");
        }
    }
    private void CreateFile(string filePath,string text)
    {
        if (!System.IO.File.Exists(filePath))
        {
            System.IO.File.WriteAllText(filePath, text);
        }
    }
}
