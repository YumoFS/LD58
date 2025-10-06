using System;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Interactable : MonoBehaviour
{
    [Header("距离阈值")]
    public float farDistance = 10f;
    public float nearDistance = 3f;

    [Header("角度阈值")]
    public float largeAngle = 90f;
    public float smallAngle = 30f;

    [Header("输出变量")]
    public float df = 0f; // 距离因子
    // public float da = 0f; // 角度因子

    private GameObject player;
    private Transform playerTransform;

    public GameObject HintPrefab;
    private GameObject m_hint;
    private GameObject m_text;
    private Image m_image;
    private TextMeshProUGUI m_textMeshProUGUI;
    public AnimationCurve sizeOverDistance;
    public AnimationCurve fadeOverDistance;

    [Header("内容与切换")]
    [Range(0, 1)]
    public float Transition = 0.9f;
    public float Range = 0.2f;
    private float t_threshold;
    private float t_lerpRange;
    [TextArea]
    public String content;
    void Start()
    {
        t_threshold = Transition - Range;
        t_lerpRange = 1 - t_threshold;
        // 查找带有Player Tag的玩家对象
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("未找到带有Player标签的对象！");
        }
        m_hint = Instantiate(HintPrefab, InteractableTextMng.Instance.transform);
        m_image = m_hint.GetComponent<Image>();
        m_text = m_hint.transform.GetChild(0).gameObject;
        m_textMeshProUGUI = m_text.GetComponent<TextMeshProUGUI>();

        m_textMeshProUGUI.text = content;
    }

    void Update()
    {
        if (playerTransform == null) return;

        CalculateDistanceFactor();
        // CalculateAngleFactor();

        if (m_hint)
        {
            // UI跟随场景设定位置
            m_hint.transform.position = GetScreenPositionWithDepth(transform.position);

            // 计算提示气泡透明度
            float alpha = df > t_threshold ? Mathf.Lerp(1, 0, (df - t_threshold) / t_lerpRange) : Mathf.Clamp01(fadeOverDistance.Evaluate(df));
            Color new_color = new(m_image.color.r, m_image.color.g, m_image.color.b, alpha);
            m_image.color = new_color;

            // 应用大小变化曲线
            float factor = sizeOverDistance.Evaluate(df) * 5 - 4;
            Vector3 new_scale = Vector3.one * (factor > 1 ? factor : 1);
            m_hint.transform.localScale = new_scale;

            // 切换到文字
            if (df > t_threshold)
            {
                if (!m_text.activeSelf) { m_text.SetActive(true); }
                float text_alpha = Mathf.Clamp01(Mathf.Lerp(0, 1, (df - t_threshold) / Range));
                Color new_textColor = new(m_textMeshProUGUI.color.r,
                                            m_textMeshProUGUI.color.g,
                                            m_textMeshProUGUI.color.b,
                                            text_alpha);
                m_textMeshProUGUI.color = new_textColor;
            }
            else
            {
                m_text.SetActive(false);
            }
        }
    }

    void CalculateDistanceFactor()
    {
        // 获取X-Z平面上的位置（忽略Y轴）
        Vector3 currentPosXZ = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 playerPosXZ = new Vector3(playerTransform.position.x, 0f, playerTransform.position.z);

        // 计算在X-Z平面上的距离
        float distance = Vector3.Distance(currentPosXZ, playerPosXZ);

        // 计算距离因子df（线性插值）
        if (distance <= nearDistance)
        {
            df = 1f;
        }
        else if (distance >= farDistance)
        {
            df = 0f;
        }
        else
        {
            // 在近距离和远距离之间进行线性插值
            df = 1f - (distance - nearDistance) / (farDistance - nearDistance);
        }
    }

    // void CalculateAngleFactor()
    // {
    //     // 获取X-Z平面上的位置（忽略Y轴）
    //     Vector3 currentPosXZ = new Vector3(transform.position.x, 0f, transform.position.z);
    //     Vector3 playerPosXZ = new Vector3(playerTransform.position.x, 0f, playerTransform.position.z);

    //     // 计算从玩家到当前物体在X-Z平面上的方向向量
    //     Vector3 directionToObjectXZ = (currentPosXZ - playerPosXZ).normalized;

    //     // 获取玩家的前向向量在X-Z平面上的投影（忽略Y轴）
    //     Vector3 playerForwardXZ = new Vector3(playerTransform.forward.x, 0f, playerTransform.forward.z).normalized;

    //     // 计算两个向量在X-Z平面上的夹角（0-180度）
    //     float angle = Vector3.Angle(playerForwardXZ, directionToObjectXZ);

    //     // 计算角度因子da
    //     if (angle <= smallAngle)
    //     {
    //         da = 1f;
    //     }
    //     else if (angle >= largeAngle)
    //     {
    //         da = 0f;
    //     }
    //     else
    //     {
    //         // 在大小角度之间进行线性插值
    //         da = 1f - (angle - smallAngle) / (largeAngle - smallAngle);
    //     }
    // }

    public Vector3 GetScreenPositionWithDepth(Vector3 worldPosition)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("未找到主摄像机！");
            return Vector3.zero;
        }

        return mainCamera.WorldToScreenPoint(worldPosition);
    }

#if UNITY_EDITOR
    // 在Scene视图中绘制调试信息（可选）
    void OnDrawGizmosSelected()
    {
        // 获取X-Z平面上的位置（忽略Y轴）
        Vector3 currentPosXZ = new Vector3(transform.position.x, 0f, transform.position.z);
        // 绘制距离阈值范围（在X-Z平面上）
        Gizmos.color = Color.yellow;
        DrawWireCircle(currentPosXZ, nearDistance, 20);
        DrawWireCircle(currentPosXZ, farDistance, 20);
    }

    // 绘制圆形辅助方法
    void DrawWireCircle(Vector3 center, float radius, int segments)
    {
        float angle = 0f;
        float angleIncrement = 360f / segments;

        Vector3 prevPoint = center + new Vector3(Mathf.Cos(0) * radius, 0f, Mathf.Sin(0) * radius);

        for (int i = 1; i <= segments; i++)
        {
            angle += angleIncrement;
            Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * radius, 0f, Mathf.Sin(angle * Mathf.Deg2Rad) * radius);
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }
#endif
}
