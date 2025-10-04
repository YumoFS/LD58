using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSelector : MonoBehaviour
{
    [Header("Selection Settings")]
    public KeyCode toggleKey = KeyCode.Delete; // 切换显示边框的按键
    public Material outlineMaterial; // 边框材质
    public Color outlineColor = Color.red; // 边框颜色
    public float outlineWidth = 0.05f; // 边框宽度
    
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
        // 切换选择模式
        if (Input.GetKeyDown(toggleKey))
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
            Debug.Log("进入选择模式 - 点击物体可将其隐藏");
            Cursor.lockState = CursorLockMode.None; // 解锁鼠标
            Cursor.visible = true; // 显示鼠标
        }
        else
        {
            Debug.Log("退出选择模式");
            Cursor.lockState = CursorLockMode.Locked; // 锁定鼠标到屏幕中心
            Cursor.visible = false; // 隐藏鼠标
            
            // 清除选择
            ClearSelection();
        }
    }
    
    void HandleSelection()
    {
        // 检测鼠标点击
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
        
        // 右键点击隐藏选中的物体
        if (Input.GetMouseButtonDown(1) && selectedObject != null) // 右键点击
        {
            HideSelectedObject();
        }
        
        // 按H键直接隐藏选中的物体
        if (Input.GetKeyDown(KeyCode.H) && selectedObject != null)
        {
            HideSelectedObject();
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
    
    void HideSelectedObject()
    {
        if (selectedObject != null)
        {
            Debug.Log($"隐藏物体: {selectedObject.name}");
            selectedObject.SetActive(false);
            ClearSelection();
        }
    }
    
    // 在编辑器中显示选择状态
    void OnGUI()
    {
        if (selectionMode)
        {
            GUI.Box(new Rect(10, 10, 200, 100), "选择模式");
            GUI.Label(new Rect(20, 40, 180, 20), "左键点击: 选择物体");
            GUI.Label(new Rect(20, 60, 180, 20), "右键/H键: 隐藏物体");
            GUI.Label(new Rect(20, 80, 180, 20), "Delete键: 退出模式");
            
            if (selectedObject != null)
            {
                GUI.Box(new Rect(10, 120, 200, 50), $"已选择: {selectedObject.name}");
            }
        }
    }
}