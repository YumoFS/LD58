using UnityEngine;
using TMPro; // 添加 TextMeshPro 命名空间

public class LevelTextController : MonoBehaviour
{
    [Header("UI设置")]
    public GameObject textLevel0Object; // 这是包含TextMeshPro组件的GameObject
    
    [Header("等级UI对象")]
    public GameObject level1Object;
    public GameObject level2Object;
    public GameObject level3Object;
    public GameObject level4Object;
    
    [Header("调试信息")]
    public float currentZPosition;
    public string currentLevel;
    
    private TMP_Text textLevel0; // 使用TMP_Text而不是Text
    private GameObject player;
    private bool playerFound = false;
    private string lastLevel = ""; // 用于检测等级变化
    
    void Start()
    {
        // 尝试通过tag查找玩家
        FindPlayer();
        
        // 获取TextMeshPro组件
        if (textLevel0Object != null)
        {
            textLevel0 = textLevel0Object.GetComponent<TMP_Text>();
            if (textLevel0 == null)
            {
                Debug.LogWarning("TextLevel0对象上没有找到TextMeshPro组件");
            }
        }
        else
        {
            // 如果Inspector中没有赋值，尝试自动查找
            textLevel0Object = GameObject.Find("TextLevel0");
            if (textLevel0Object != null)
            {
                textLevel0 = textLevel0Object.GetComponent<TMP_Text>();
            }
            else
            {
                Debug.LogWarning("未找到名为TextLevel0的对象");
            }
        }
        
        // 如果Inspector中没有赋值，尝试自动查找等级UI对象
        if (level1Object == null)
            level1Object = GameObject.Find("Level1");
        if (level2Object == null)
            level2Object = GameObject.Find("Level2");
        if (level3Object == null)
            level3Object = GameObject.Find("Level3");
        if (level4Object == null)
            level4Object = GameObject.Find("Level4");
            
        // 初始时禁用所有等级UI对象
        SetAllLevelObjects(false);
        
    }
    
    void Update()
    {
        // 如果之前没找到玩家，再次尝试查找
        if (!playerFound)
        {
            FindPlayer();
            return;
        }
        
        // 获取玩家Z坐标并更新文本
        currentZPosition = player.transform.position.z;
        currentLevel = GetLevelByZ(currentZPosition);
        
        // 更新UI文本
        if (textLevel0 != null)
        {
            textLevel0.text = currentLevel;
        }
        
        // 如果等级发生变化，更新UI对象状态
        if (currentLevel != lastLevel)
        {
            UpdateLevelObjects(currentLevel);
            lastLevel = currentLevel;
        }
    }
    
    void FindPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerFound = true;
            Debug.Log("成功找到玩家对象");
        }
        else
        {
            Debug.LogWarning("未找到tag为Player的对象，请确保场景中有玩家且tag设置正确");
        }
    }
    
    string GetLevelByZ(float zCoordinate)
    {
        if (zCoordinate < 45)
        {
            return "LEVEL 1";
        }
        else if (zCoordinate >= 45 && zCoordinate <= 98)
        {
            return "LEVEL 2";
        }
        else if (zCoordinate >= 98 && zCoordinate <= 148)
        {
            return "LEVEL 3";
        }
        else if (zCoordinate > 148)
        {
            return "LEVEL 4";
        }
        else
        {
            return "undefined";
        }
    }
    
    void SetAllLevelObjects(bool active)
    {
        if (level1Object != null) level1Object.SetActive(active);
        if (level2Object != null) level2Object.SetActive(active);
        if (level3Object != null) level3Object.SetActive(active);
        if (level4Object != null) level4Object.SetActive(active);
    }
    
    void UpdateLevelObjects(string level)
    {
        // 首先禁用所有等级对象
        SetAllLevelObjects(false);
        
        // 然后根据当前等级激活对应的对象
        switch(level)
        {
            case "LEVEL 1":
                if (level1Object != null) level1Object.SetActive(true);
                break;
            case "LEVEL 2":
                if (level2Object != null) level2Object.SetActive(true);
                break;
            case "LEVEL 3":
                if (level3Object != null) level3Object.SetActive(true);
                break;
            case "LEVEL 4":
                if (level4Object != null) level4Object.SetActive(true);
                break;
            default:
                // 对于undefined等级，不激活任何对象
                break;
        }
        
        Debug.Log($"切换到等级: {level}");
    }
    
    // 在Scene视图中可视化显示当前Z坐标和等级（调试用）
    void OnDrawGizmos()
    {
        if (playerFound)
        {
            Vector3 playerPos = player.transform.position;
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(playerPos + Vector3.up * 2, 
                $"Z: {currentZPosition:F1}\nLevel: {currentLevel}", style);
            #endif
        }
    }
}