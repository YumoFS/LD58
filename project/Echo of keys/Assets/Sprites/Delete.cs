using UnityEngine;
using System.Collections;

public class ObjectSelector : MonoBehaviour
{
    [Header("Selection Settings")]
    public Material outlineMaterial; // 边框材质
    public Color outlineColor = Color.red; // 边框颜色
    public float outlineWidth = 0.05f; // 边框宽度
    
    [Header("Deletion Settings")]
    public string allowedDeletionTag = "Stone"; // 只允许删除此标签的物体
    
    private bool selectionMode = false;
    private GameObject selectedObject;
    private Material originalMaterial;
    private Material outlineMatInstance;
    
    void Start()
    {
        // 创建边框材质实例
        if (outlineMaterial != null)
        {
            outlineMatInstance = new Material(outlineMaterial);
            outlineMatInstance.SetColor("_OutlineColor", outlineColor);
            outlineMatInstance.SetFloat("_OutlineWidth", outlineWidth);
        }
        else
        {
            Debug.LogWarning("请为ObjectSelector脚本分配边框材质");
        }
    }
    
    void Update()
    {
        // 切换选择模式 - 使用 Input Manager
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            ToggleSelectionMode();
        }
        
        // 在选择模式下处理鼠标点击
        if (selectionMode)
        {
            HandleSelection();
        }
    }
    
    void ToggleSelectionMode()
    {
        selectionMode = !selectionMode;
        
        if (selectionMode)
        {
            Debug.Log("进入选择模式 - 只能删除标签为 " + allowedDeletionTag + " 的物体");
            // Cursor.lockState = CursorLockMode.None; // 解锁鼠标
            // Cursor.visible = true; // 显示鼠标
            
            // 禁用玩家移动
            // DisablePlayerMovement();
        }
        else
        {
            Debug.Log("退出选择模式");
            // Cursor.lockState = CursorLockMode.Locked; // 锁定鼠标到屏幕中心
            // Cursor.visible = false; // 隐藏鼠标
            
            // 启用玩家移动
            // EnablePlayerMovement();
            
            // 清除选择
            ClearSelection();
        }
    }
    
    void HandleSelection()
    {
        // 检测鼠标点击 - 使用 Input Manager
        if (Input.GetMouseButtonDown(0)) // 左键点击
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                // 清除之前的选择
                ClearSelection();
                
                // 设置新的选择
                SelectObject(hit.collider.gameObject);
            }
        }
        
        // 右键点击删除选中的物体 - 使用 Input Manager
        if (Input.GetMouseButtonDown(1) && selectedObject != null) // 右键点击
        {
            DeleteSelectedObject();
        }
        
        // 按H键直接删除选中的物体 - 使用 Input Manager
        if (Input.GetKeyDown(KeyCode.H) && selectedObject != null)
        {
            DeleteSelectedObject();
        }
    }
    
    void SelectObject(GameObject obj)
    {
        selectedObject = obj;
        
        // 保存原始材质并应用边框材质
        Renderer renderer = selectedObject.GetComponent<Renderer>();
        if (renderer != null && outlineMatInstance != null)
        {
            originalMaterial = renderer.material;
            renderer.material = outlineMatInstance;
        }
        
        Debug.Log($"选中物体: {selectedObject.name} (Tag: {selectedObject.tag})");
    }
    
    void ClearSelection()
    {
        if (selectedObject != null)
        {
            // 恢复原始材质
            Renderer renderer = selectedObject.GetComponent<Renderer>();
            if (renderer != null && originalMaterial != null)
            {
                renderer.material = originalMaterial;
            }
            
            selectedObject = null;
            originalMaterial = null;
        }
    }
    
    void DeleteSelectedObject()
    {
        if (selectedObject != null)
        {
            // 检查物体标签是否为允许删除的标签
            if (selectedObject.CompareTag(allowedDeletionTag))
            {
                Debug.Log($"删除 Stone 物体: {selectedObject.name}");
                Destroy(selectedObject); // 或者使用 selectedObject.SetActive(false);
                ClearSelection();
            }
            else
            {
                Debug.Log($"无法删除此物体: {selectedObject.name} (标签: {selectedObject.tag})，只能删除标签为 {allowedDeletionTag} 的物体");
                
                // 给用户视觉反馈，改变边框颜色
                Renderer renderer = selectedObject.GetComponent<Renderer>();
                if (renderer != null && outlineMatInstance != null)
                {
                    // 临时改变边框颜色为黄色表示警告
                    StartCoroutine(FlashOutlineColor(Color.yellow, 0.5f));
                }
            }
        }
    }
    
    // 闪烁边框颜色的协程
    IEnumerator FlashOutlineColor(Color flashColor, float duration)
    {
        Color originalColor = outlineMatInstance.GetColor("_OutlineColor");
        outlineMatInstance.SetColor("_OutlineColor", flashColor);
        
        yield return new WaitForSeconds(duration);
        
        if (outlineMatInstance != null)
        {
            outlineMatInstance.SetColor("_OutlineColor", originalColor);
        }
    }
    
    // void DisablePlayerMovement()
    // {
    //     // 禁用玩家移动组件
    //     var moveController = FindObjectOfType<Move_Controller>();
    //     if (moveController != null)
    //     {
    //         moveController.enabled = false;
    //     }
        
    //     // 如果有角色控制器，也可以禁用它
    //     var characterController = FindObjectOfType<CharacterController>();
    //     if (characterController != null)
    //     {
    //         characterController.enabled = false;
    //     }
    // }
    
    // void EnablePlayerMovement()
    // {
    //     // 启用玩家移动组件
    //     var moveController = FindObjectOfType<Move_Controller>();
    //     if (moveController != null)
    //     {
    //         moveController.enabled = true;
    //     }
        
    //     // 启用角色控制器
    //     var characterController = FindObjectOfType<CharacterController>();
    //     if (characterController != null)
    //     {
    //         characterController.enabled = true;
    //     }
    // }
    
    // 在编辑器中显示选择状态
    void OnGUI()
    {
        if (selectionMode)
        {
            GUI.Box(new Rect(10, 10, 300, 140), "选择模式");
            GUI.Label(new Rect(20, 40, 280, 20), "左键点击: 选择物体");
            GUI.Label(new Rect(20, 60, 280, 20), "右键点击: 删除物体");
            GUI.Label(new Rect(20, 80, 280, 20), "H 键: 删除物体");
            GUI.Label(new Rect(20, 100, 280, 20), "Delete 键: 退出模式");
            GUI.Label(new Rect(20, 120, 280, 20), $"只能删除标签为 {allowedDeletionTag} 的物体");
            
            if (selectedObject != null)
            {
                string canDeleteText = selectedObject.CompareTag(allowedDeletionTag) ? "可删除" : "不可删除";
                GUI.Box(new Rect(10, 160, 300, 50), $"已选择: {selectedObject.name}\n标签: {selectedObject.tag} ({canDeleteText})");
            }
        }
    }
}