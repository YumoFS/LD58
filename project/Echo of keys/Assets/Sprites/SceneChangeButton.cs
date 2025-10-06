using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonPressed : MonoBehaviour
{
    public string targetSceneName;
    
    private Button button;
    
    void Start()
    {
        button = GetComponent<Button>();
        
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
        else
        {
            Debug.LogError("ButtonPressed脚本需要附加到Button对象上");
        }
    }
    
    public void OnButtonClicked()
    {
        if (targetSceneName == null)
        {
            Debug.LogError("未分配目标场景！请在Inspector中分配目标场景");
            return;
        }
        
        LoadTargetScene(targetSceneName);
    }
    
    public void LoadTargetScene(string sceneName)
    {
        if (IsSceneInBuildSettings(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"场景 '{sceneName}' 不存在于Build Settings中");
        }
    }
    
    private bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string scene = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (scene == sceneName)
                return true;
        }
        return false;
    }

}