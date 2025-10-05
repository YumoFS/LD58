using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectSelector : MonoBehaviour
{
    [Header("Selection Settings")]
    public Material outlineMaterial; // 边框材质
    public Color deleteModeColor = Color.red; // 删除模式边框颜色
    public Color addModeColor = Color.green; // 添加模式边框颜色
    public float outlineWidth = 0.05f; // 边框宽度
    
    [Header("Object Settings")]
    public string deletionTag = "Stone"; // 删除模式下可删除的标签
    public string additionTag = "hiddenBlock"; // 添加模式下可激活的标签
    
    private Move_Controller moveController;
    private bool selectionMode = false;
    private GameObject selectedObject;
    private Material originalMaterial;
    private Material outlineMatInstance;
    private List<GameObject> hiddenObjects = new List<GameObject>(); // 存储被隐藏的物体
    
    void Start()
    {
        // 获取 Move_Controller 引用
        moveController = FindObjectOfType<Move_Controller>();
        if (moveController == null)
        {
            Debug.LogError("未找到 Move_Controller 组件！");
        }
        
        // 创建边框材质实例
        if (outlineMaterial != null)
        {
            outlineMatInstance = new Material(outlineMaterial);
            outlineMatInstance.SetFloat("_OutlineWidth", outlineWidth);
        }
        else
        {
            Debug.LogWarning("请为ObjectSelector脚本分配边框材质");
        }
        
        // 查找所有被隐藏的物体并添加到列表
        FindAllHiddenObjects();
    }
    
    void Update()
    {
        // 切换选择模式 - 使用 Input Manager
        if (Input.GetKeyDown(KeyCode.Delete) && CanEnterSelectionMode())
        {
            ToggleSelectionMode();
        }
        
        // 在选择模式下处理鼠标点击
        if (selectionMode)
        {
            HandleSelection();
        }
    }
    
    bool CanEnterSelectionMode()
    {
        if (moveController == null) return false;
        
        // 只有在 canDelete 为 true 或 canAdd 为 true 时才能进入选择模式
        return moveController.canDelete || moveController.canAdd;
    }
    
    void ToggleSelectionMode()
    {
        selectionMode = !selectionMode;
        
        if (selectionMode)
        {
            string modeName = moveController.canDelete ? "删除" : "添加";
            string targetTag = moveController.canDelete ? deletionTag : additionTag;
            Debug.Log($"进入{modeName}模式 - 目标标签: {targetTag}");
            
            // Cursor.lockState = CursorLockMode.None; // 解锁鼠标
            // Cursor.visible = true; // 显示鼠标
            
            // 禁用玩家移动
            // DisablePlayerMovement();
            
            // 更新边框颜色
            UpdateOutlineColor();
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
        
        // 右键点击执行操作 - 使用 Input Manager
        if (Input.GetMouseButtonDown(1) && selectedObject != null) // 右键点击
        {
            ExecuteAction();
        }
        
        // 按H键直接执行操作 - 使用 Input Manager
        if (Input.GetKeyDown(KeyCode.H) && selectedObject != null)
        {
            ExecuteAction();
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
    
    void ExecuteAction()
    {
        if (selectedObject != null && moveController != null)
        {
            if (moveController.canDelete)
            {
                // 删除模式 - 删除标签为 Stone 的物体
                DeleteSelectedObject();
            }
            else if (moveController.canAdd)
            {
                // 添加模式 - 激活标签为 Block 的隐藏物体
                ActivateSelectedObject();
            }
        }
    }
    
    void DeleteSelectedObject()
    {
        if (selectedObject.CompareTag(deletionTag))
        {
            Debug.Log($"删除 {deletionTag} 物体: {selectedObject.name}");
            
            // 添加到隐藏物体列表
            hiddenObjects.Add(selectedObject);
            
            // 隐藏物体而不是销毁，以便后续可以重新激活
            selectedObject.SetActive(false);
            ClearSelection();
        }
        else
        {
            Debug.Log($"无法删除此物体: {selectedObject.name} (标签: {selectedObject.tag})，只能删除标签为 {deletionTag} 的物体");
            
            // 给用户视觉反馈，改变边框颜色
            StartCoroutine(FlashOutlineColor(Color.yellow, 0.5f));
        }
    }
    
    void ActivateSelectedObject()
    {
        if (selectedObject.CompareTag(additionTag) && !selectedObject.activeInHierarchy)
        {
            Debug.Log($"激活 {additionTag} 物体: {selectedObject.name}");
            selectedObject.SetActive(true);
            
            // 从隐藏物体列表中移除
            if (hiddenObjects.Contains(selectedObject))
            {
                hiddenObjects.Remove(selectedObject);
            }
            
            ClearSelection();
        }
        else if (!selectedObject.CompareTag(additionTag))
        {
            Debug.Log($"无法激活此物体: {selectedObject.name} (标签: {selectedObject.tag})，只能激活标签为 {additionTag} 的物体");
            StartCoroutine(FlashOutlineColor(Color.yellow, 0.5f));
        }
        else if (selectedObject.activeInHierarchy)
        {
            Debug.Log($"此物体已经是激活状态: {selectedObject.name}");
            StartCoroutine(FlashOutlineColor(Color.yellow, 0.5f));
        }
    }
    
    void UpdateOutlineColor()
    {
        if (outlineMatInstance != null && moveController != null)
        {
            Color targetColor = moveController.canDelete ? deleteModeColor : addModeColor;
            outlineMatInstance.SetColor("_OutlineColor", targetColor);
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
    
    void DisablePlayerMovement()
    {
        // 禁用玩家移动组件
        if (moveController != null)
        {
            moveController.enabled = false;
        }
        
        // 如果有角色控制器，也可以禁用它
        var characterController = FindObjectOfType<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = false;
        }
    }
    
    void EnablePlayerMovement()
    {
        // 启用玩家移动组件
        if (moveController != null)
        {
            moveController.enabled = true;
        }
        
        // 启用角色控制器
        var characterController = FindObjectOfType<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = true;
        }
    }
    
    void FindAllHiddenObjects()
    {
        // 查找所有标签为 additionTag 的隐藏物体
        GameObject[] allObjects = FindObjectsOfType<GameObject>(true); // 包括非激活物体
        foreach (GameObject obj in allObjects)
        {
            if (obj.CompareTag(additionTag) && !obj.activeInHierarchy)
            {
                hiddenObjects.Add(obj);
            }
        }
        
        Debug.Log($"找到 {hiddenObjects.Count} 个隐藏的 {additionTag} 物体");
    }
    
    // 在编辑器中显示选择状态
    void OnGUI()
    {
        if (selectionMode && moveController != null)
        {
            string modeName = moveController.canDelete ? "删除" : "添加";
            string targetTag = moveController.canDelete ? deletionTag : additionTag;
            string actionDescription = moveController.canDelete ? "删除物体" : "激活物体";
            
            GUI.Box(new Rect(10, 10, 300, 140), $"{modeName}模式");
            GUI.Label(new Rect(20, 40, 280, 20), "左键点击: 选择物体");
            GUI.Label(new Rect(20, 60, 280, 20), "右键点击: " + actionDescription);
            GUI.Label(new Rect(20, 80, 280, 20), "H 键: " + actionDescription);
            GUI.Label(new Rect(20, 100, 280, 20), "Delete 键: 退出模式");
            GUI.Label(new Rect(20, 120, 280, 20), $"目标标签: {targetTag}");
            
            if (selectedObject != null)
            {
                bool isValidTarget = moveController.canDelete ? 
                    selectedObject.CompareTag(deletionTag) : 
                    (selectedObject.CompareTag(additionTag) && !selectedObject.activeInHierarchy);
                    
                string statusText = isValidTarget ? "可操作" : "不可操作";
                GUI.Box(new Rect(10, 160, 300, 50), $"已选择: {selectedObject.name}\n标签: {selectedObject.tag} ({statusText})");
            }
        }
    }
}