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
    public float maxDistance = 6f; // 最大操作距离
    
    [Header("Object Settings")]
    public string deletionTag = "Stone"; // 删除模式下可删除的标签
    public GameObject createdBlockPrefab; // 要生成的预制体
    public string ignoreTag = "AirWall"; // 射线会穿透的标签
    
    [Header("Add Mode Settings")]
    public float blockLifetimeAfterExit = 3f; // 玩家离开后方块保持的时间
    
    [Header("Delete Mode Settings")]
    public float deleteDelay = 0.1f; // 删除延迟时间
    
    [Header("Preview Settings")]
    public Material previewMaterial; // 预览框材质
    public Color validPositionColor = Color.green; // 有效位置颜色
    public Color invalidPositionColor = Color.red; // 无效位置颜色


    [Header("Air Wall Settings")]
    public GameObject airWallPrefabX; // 空气墙预制体
    public GameObject airWallPrefabZ; // 空气墙预制体
    
    private Move_Controller moveController;
    private bool selectionMode = false;
    private GameObject selectedObject;
    private Material originalMaterial;
    private Material outlineMatInstance;
    private List<GameObject> createdBlocks = new List<GameObject>(); // 存储已创建的方块
    private Vector3 previewPosition; // 预览位置
    private bool isPositionValid = true; // 位置是否有效（在距离范围内）
    private GameObject currentAddedBlock; // 当前添加的方块
    private bool canAddNewBlock = true; // 是否可以添加新方块
    private Coroutine blockLifetimeCoroutine; // 方块生命周期协程
    private Coroutine deleteCoroutine; // 删除协程
    
    // 预览相关变量
    private GameObject previewObject; // 预览对象
    private Renderer previewRenderer; // 预览渲染器
    
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
        
        // 创建预览对象
        CreatePreviewObject();
    }
    
    // 创建预览对象
    void CreatePreviewObject()
    {
        if (createdBlockPrefab != null)
        {
            // 基于要创建的方块预制体创建预览对象
            previewObject = Instantiate(createdBlockPrefab);
            previewObject.name = "BlockPreview";
            
            // 移除预览对象的碰撞体，避免影响游戏物理
            Collider previewCollider = previewObject.GetComponent<Collider>();
            if (previewCollider != null)
            {
                previewCollider.enabled = false;
            }
            
            // 获取渲染器并设置预览材质
            previewRenderer = previewObject.GetComponent<Renderer>();
            if (previewRenderer != null && previewMaterial != null)
            {
                previewRenderer.material = new Material(previewMaterial);
                previewRenderer.material.color = validPositionColor;
            }
            
            // 初始时隐藏预览对象
            previewObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("未设置createdBlockPrefab，无法创建预览对象");
        }
    }
    
    void Update()
    {
        // 切换选择模式 - 使用 Input Manager
        if (Input.GetKeyDown(KeyCode.R) && CanEnterSelectionMode())
        {
            ToggleSelectionMode();
        }
        
        // 在选择模式下处理鼠标点击
        if (selectionMode)
        {
            HandleSelection();
        }
        
        // 检查当前方块是否需要消失
        CheckCurrentBlockStatus();
        
        // 更新预览显示
        UpdatePreview();
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
            Debug.Log($"进入{modeName}模式");
            
            // 更新边框颜色
            UpdateOutlineColor();
            
            // 如果是添加模式，显示预览
            if (moveController.canAdd)
            {
                ShowPreview(true);
            }
        }
        else
        {
            Debug.Log("退出选择模式");
            
            // 清除选择
            ClearSelection();
            
            // 停止删除协程
            if (deleteCoroutine != null)
            {
                StopCoroutine(deleteCoroutine);
                deleteCoroutine = null;
            }
            
            // 隐藏预览
            ShowPreview(false);
        }
    }
    
    void HandleSelection()
    {
        if (moveController.canAdd)
        {
            // 添加模式下，显示鼠标位置并允许放置方块
            ShowMousePosition();
        }
        
        // 检测鼠标点击 - 使用 Input Manager
        if (Input.GetMouseButtonDown(0)) // 左键点击
        {
            if (moveController.canDelete)
            {
                // 删除模式下选择物体
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                GameObject hitObject = RaycastIgnoreTag(ray, ignoreTag);
                
                if (hitObject != null)
                {
                    // 检查距离是否在允许范围内
                    float distance = Vector3.Distance(transform.position, hitObject.transform.position);
                    if (distance <= maxDistance)
                    {
                        // 清除之前的选择和删除协程
                        ClearSelection();
                        if (deleteCoroutine != null)
                        {
                            StopCoroutine(deleteCoroutine);
                            deleteCoroutine = null;
                        }
                        
                        // 设置新的选择
                        SelectObject(hitObject);
                        
                        // 启动删除协程
                        deleteCoroutine = StartCoroutine(DeleteAfterDelay());
                    }
                    else
                    {
                        Debug.Log($"物体距离太远 ({distance:F1} > {maxDistance})，无法选择");
                        StartCoroutine(FlashOutlineColor(Color.yellow, 0.5f));
                    }
                }
            }
            else if (moveController.canAdd && canAddNewBlock)
            {
                // 添加模式下放置方块
                PlaceBlockAtGridPosition();
            }
            else if (moveController.canAdd && !canAddNewBlock)
            {
                Debug.Log("请等待当前方块消失后再添加新方块");
                StartCoroutine(FlashOutlineColor(Color.yellow, 0.5f));
            }
        }
        
        // 按H键直接执行操作 - 使用 Input Manager
        if (Input.GetKeyDown(KeyCode.H) && selectedObject != null && moveController.canDelete)
        {
            ExecuteImmediateDelete();
        }
        
        // 添加模式下，按H键也可以放置方块
        if (Input.GetKeyDown(KeyCode.H) && moveController.canAdd && canAddNewBlock)
        {
            PlaceBlockAtGridPosition();
        }
        else if (Input.GetKeyDown(KeyCode.H) && moveController.canAdd && !canAddNewBlock)
        {
            Debug.Log("请等待当前方块消失后再添加新方块");
            StartCoroutine(FlashOutlineColor(Color.yellow, 0.5f));
        }
    }
    
    // 更新预览显示
    void UpdatePreview()
    {
        if (selectionMode && moveController != null && moveController.canAdd && previewObject != null)
        {
            // 更新预览位置
            previewObject.transform.position = previewPosition;
            
            // 更新预览颜色
            if (previewRenderer != null)
            {
                float distance = Vector3.Distance(transform.position, previewPosition);
                bool isValid = distance <= maxDistance && !IsPositionOccupied(previewPosition);
                
                Color previewColor = isValid ? validPositionColor : invalidPositionColor;
                previewRenderer.material.color = previewColor;
                
                // 添加透明度效果
                Color semiTransparent = previewColor;
                semiTransparent.a = 0.5f;
                previewRenderer.material.color = semiTransparent;
            }
        }
    }
    
    // 显示/隐藏预览
    void ShowPreview(bool show)
    {
        if (previewObject != null)
        {
            previewObject.SetActive(show);
        }
    }
    
    // 延迟删除协程
    IEnumerator DeleteAfterDelay()
    {
        if (selectedObject == null) yield break;
        
        // 显示倒计时效果
        float timer = deleteDelay;
        while (timer > 0 && selectedObject != null)
        {
            // 可以在这里添加视觉反馈，比如边框闪烁
            float pulse = Mathf.PingPong(Time.time * 10f, 0.5f) + 0.5f;
            Color pulseColor = Color.Lerp(deleteModeColor, Color.white, pulse);
            outlineMatInstance.SetColor("_OutlineColor", pulseColor);
            
            timer -= Time.deltaTime;
            yield return null;
        }
        
        // 恢复原始颜色
        if (outlineMatInstance != null)
        {
            outlineMatInstance.SetColor("_OutlineColor", deleteModeColor);
        }
        
        // 执行删除
        if (selectedObject != null)
        {
            DeleteSelectedObject();
        }
        
        deleteCoroutine = null;
    }
    
    // 立即删除（H键触发）
    void ExecuteImmediateDelete()
    {
        if (selectedObject != null && moveController != null && moveController.canDelete)
        {
            // 检查距离是否在允许范围内
            float distance = Vector3.Distance(transform.position, selectedObject.transform.position);
            if (distance > maxDistance)
            {
                Debug.Log($"物体距离太远 ({distance:F1} > {maxDistance})，无法删除");
                StartCoroutine(FlashOutlineColor(Color.yellow, 0.5f));
                return;
            }
            
            // 停止删除协程
            if (deleteCoroutine != null)
            {
                StopCoroutine(deleteCoroutine);
                deleteCoroutine = null;
            }
            
            // 删除模式 - 删除标签为 Stone 的物体
            DeleteSelectedObject();
        }
    }
    
    // 显示鼠标位置（用于添加模式）
    void ShowMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;
        
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 worldPosition = ray.GetPoint(rayDistance);
            
            // 计算网格位置
            Vector3 gridPosition = CalculateGridPosition(worldPosition);
            
            // 更新预览位置
            previewPosition = gridPosition;
            
            // 检查距离是否在允许范围内
            float distance = Vector3.Distance(transform.position, gridPosition);
            isPositionValid = distance <= maxDistance;
            
            // 在Scene视图中显示位置（调试用）
            Debug.DrawRay(gridPosition, Vector3.up * 2, isPositionValid ? Color.green : Color.red);
            
            // 绘制网格线（调试用）
            DrawGridLines(isPositionValid ? Color.green : Color.red);
            
            // 绘制距离指示线
            Debug.DrawLine(transform.position, gridPosition, isPositionValid ? Color.green : Color.red);
        }
    }
    
    // 计算网格位置 (2x-1, 0, 2z-1)
    Vector3 CalculateGridPosition(Vector3 worldPosition)
    {
        // 计算最近的网格坐标
        int x = Mathf.RoundToInt((worldPosition.x) / 2f) * 2;
        int z = Mathf.RoundToInt((worldPosition.z) / 2f) * 2;
        
        return new Vector3(x, 0, z);
    }
    
    // 在网格位置放置方块
    void PlaceBlockAtGridPosition()
    {
        if (createdBlockPrefab == null)
        {
            Debug.LogError("请为ObjectSelector分配要生成的预制体");
            return;
        }
        
        // 检查距离是否在允许范围内
        float distance = Vector3.Distance(transform.position, previewPosition);
        if (distance > maxDistance)
        {
            Debug.Log($"放置位置距离太远 ({distance:F1} > {maxDistance})，无法放置");
            StartCoroutine(FlashOutlineColor(Color.yellow, 0.5f));
            return;
        }
        
        // 检查该位置是否已经有方块
        if (!IsPositionOccupied(previewPosition))
        {
            // 生成方块
            GameObject newBlock = Instantiate(createdBlockPrefab, previewPosition, Quaternion.identity);
            newBlock.name = "createdBlock"; // 设置名称为createdBlock
            
            // 设置为当前添加的方块
            currentAddedBlock = newBlock;
            canAddNewBlock = false;
            
            // 添加到已创建方块列表
            createdBlocks.Add(newBlock);
            
            Debug.Log($"在网格位置 {previewPosition} 生成了方块 (距离: {distance:F1})");
            Debug.Log($"方块将在玩家经过并离开后 {blockLifetimeAfterExit} 秒消失");
        }
        else
        {
            Debug.Log("该网格位置已有方块，无法放置新方块");
            StartCoroutine(FlashOutlineColor(Color.yellow, 0.5f));
        }
    }
    
    // 检查当前方块状态
    void CheckCurrentBlockStatus()
    {
        if (currentAddedBlock != null && !canAddNewBlock)
        {
            // 检查玩家是否在当前方块上
            float distanceToBlock = Vector3.Distance(transform.position, currentAddedBlock.transform.position);
            bool isPlayerOnBlock = distanceToBlock < 2f; // 假设玩家在方块上时的距离阈值
            
            if (isPlayerOnBlock)
            {
                // 玩家在方块上，重置协程
                if (blockLifetimeCoroutine != null)
                {
                    StopCoroutine(blockLifetimeCoroutine);
                    blockLifetimeCoroutine = null;
                }
            }
            else
            {
                // 玩家不在方块上，开始计时
                if (blockLifetimeCoroutine == null)
                {
                    blockLifetimeCoroutine = StartCoroutine(BlockLifetimeCountdown());
                }
            }
        }
    }
    
    // 方块生命周期倒计时
    IEnumerator BlockLifetimeCountdown()
    {
        float timer = blockLifetimeAfterExit;
        
        while (timer > 0)
        {
            // 检查玩家是否重新进入方块区域
            float distanceToBlock = Vector3.Distance(transform.position, currentAddedBlock.transform.position);
            if (distanceToBlock < 1.5f)
            {
                // 玩家重新进入，取消倒计时
                yield break;
            }
            
            timer -= Time.deltaTime;
            yield return null;
        }
        
        // 时间到，销毁方块
        if (currentAddedBlock != null)
        {
            Vector3 blockPosition = currentAddedBlock.transform.position;

            Debug.Log($"方块存在时间结束，已销毁");
            Destroy(currentAddedBlock);
            createdBlocks.Remove(currentAddedBlock);

            SpawnAirWalls(blockPosition);
            
            currentAddedBlock = null;
            canAddNewBlock = true;
        }
        
        blockLifetimeCoroutine = null;
    }

    void SpawnAirWalls(Vector3 centerPosition)
    {
        if (airWallPrefabX == null || airWallPrefabZ == null)
        {
            Debug.LogWarning("未分配空气墙预制体，无法生成空气墙");
            return;
        }
        
        // 四个方向：前、后、左、右
        Vector3[] directions = {
            Vector3.forward * 0.9f,   // 前方
            Vector3.back * 0.9f,      // 后方
            Vector3.left * 0.9f,      // 左方
            Vector3.right * 0.9f      // 右方
        };

        Quaternion yRotation90 = Quaternion.Euler(0, 90, 0);
        
        List<GameObject> spawnedAirWalls = new List<GameObject>();

        // 在每个方向生成空气墙
        foreach (Vector3 direction in directions)
        {
            Vector3 spawnPosition = centerPosition + direction;
            spawnPosition.y += 1;

            if (direction == Vector3.forward * 0.9f || direction == Vector3.back * 0.9f)
            {
                GameObject airWall = Instantiate(airWallPrefabX, spawnPosition, yRotation90);
                airWall.name = "AirWall";
                spawnedAirWalls.Add(airWall);
            }
            else
            {
                GameObject airWall = Instantiate(airWallPrefabZ, spawnPosition, Quaternion.identity);
                airWall.name = "AirWall";
                spawnedAirWalls.Add(airWall);
            }
            
            // Debug.Log($"在位置 {spawnPosition} 生成了空气墙");
        }
        
    }

    
    // 检查位置是否已被占用
    bool IsPositionOccupied(Vector3 position)
    {
        float checkRadius = 0.5f; // 检查半径，根据方块大小调整
        
        // 检查该位置附近是否有碰撞体
        Collider[] colliders = Physics.OverlapSphere(position, checkRadius);
        foreach (Collider collider in colliders)
        {
            // 忽略AirWall和其他不需要考虑的物体
            if (!collider.CompareTag(ignoreTag) && collider.gameObject != this.gameObject)
            {
                return true;
            }
        }
        
        return false;
    }
    
    // 绘制网格线（调试用）
    void DrawGridLines(Color color)
    {
        // 绘制当前网格位置的边框
        Vector3 center = previewPosition;
        float halfSize = 1f; // 网格大小为2，所以半大小为1
        
        // 绘制四条边
        Debug.DrawLine(
            new Vector3(center.x - halfSize, center.y, center.z - halfSize),
            new Vector3(center.x + halfSize, center.y, center.z - halfSize),
            color
        );
        Debug.DrawLine(
            new Vector3(center.x + halfSize, center.y, center.z - halfSize),
            new Vector3(center.x + halfSize, center.y, center.z + halfSize),
            color
        );
        Debug.DrawLine(
            new Vector3(center.x + halfSize, center.y, center.z + halfSize),
            new Vector3(center.x - halfSize, center.y, center.z + halfSize),
            color
        );
        Debug.DrawLine(
            new Vector3(center.x - halfSize, center.y, center.z + halfSize),
            new Vector3(center.x - halfSize, center.y, center.z - halfSize),
            color
        );
    }
    
    // 自定义射线检测函数，忽略指定标签的物体
    GameObject RaycastIgnoreTag(Ray ray, string tagToIgnore)
    {
        // 获取所有射线命中的物体
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
        
        // 按距离排序
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        
        // 找到第一个不是要忽略标签的物体
        foreach (RaycastHit hit in hits)
        {
            if (!hit.collider.CompareTag(tagToIgnore))
            {
                return hit.collider.gameObject;
            }
        }
        
        return null;
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
        
        float distance = Vector3.Distance(transform.position, selectedObject.transform.position);
        Debug.Log($"选中物体: {selectedObject.name} (Tag: {selectedObject.tag}, 距离: {distance:F1})");
        Debug.Log($"物体将在 {deleteDelay} 秒后自动删除，按 H 键可立即删除");
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
        if (selectedObject.CompareTag(deletionTag))
        {
            float distance = Vector3.Distance(transform.position, selectedObject.transform.position);
            Debug.Log($"删除 {deletionTag} 物体: {selectedObject.name} (距离: {distance:F1})");
            
            // 如果是当前添加的方块，重置状态
            if (selectedObject == currentAddedBlock)
            {
                currentAddedBlock = null;
                canAddNewBlock = true;
                
                // 停止协程
                if (blockLifetimeCoroutine != null)
                {
                    StopCoroutine(blockLifetimeCoroutine);
                    blockLifetimeCoroutine = null;
                }
            }
            
            // 如果是创建的方块，从列表中移除
            if (createdBlocks.Contains(selectedObject))
            {
                createdBlocks.Remove(selectedObject);
            }
            
            // 销毁物体
            Destroy(selectedObject);
            ClearSelection();
        }
        else
        {
            Debug.Log($"无法删除此物体: {selectedObject.name} (标签: {selectedObject.tag})，只能删除标签为 {deletionTag} 的物体");
            
            // 给用户视觉反馈，改变边框颜色
            StartCoroutine(FlashOutlineColor(Color.yellow, 0.5f));
            
            // 清除选择
            ClearSelection();
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
 
    // 清理资源
    void OnDestroy()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
        }
    }
}