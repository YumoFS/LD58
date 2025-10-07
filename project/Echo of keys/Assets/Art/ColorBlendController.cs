using UnityEngine;
public class ColorBlendController : MonoBehaviour
{
    //一个材质切换程序，方便不同场景调色
    public Color[] setColor = new Color[5];
   public float duration = 5.0f; // 变化持续时间
   private Color currentColor;
   private Color dstColor;
    private int currentLevel;
   private Renderer objectRenderer;
   private float timer;
   private bool isChanging = false;
   private GameObject player;
    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        currentColor = setColor[0];
        currentLevel = 0;
        timer = 0f;
        player = GameObject.FindGameObjectWithTag("Player");
   }
    int getLevel(float zCoordinate)
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        if (player == null)
        {
            return 0; // 如果仍然找不到玩家，返回默认等级0
        }
        if (zCoordinate < 45)
        {
            return 1;
        }
        else if (zCoordinate >= 45 && zCoordinate <= 98)
        {
            return 2;
        }
        else if (zCoordinate >= 98 && zCoordinate <= 148)
        {
            return 3;
        }
        else if (zCoordinate > 148)
        {
            return 4;
        }
        else
        {
            return 0;
        }
   }
    void Update()
    {
        float currentZPosition = player.transform.position.z;
        int dstLevel = getLevel(currentZPosition);
        timer += Time.deltaTime;
        if (dstLevel != currentLevel)
        {
            dstColor = setColor[dstLevel];
            currentColor = objectRenderer.material.color;
            currentLevel = dstLevel;
            timer = 0f;
            isChanging = true;

        }
        if (isChanging)
        {
            float lerpFactor = Mathf.Clamp01(timer / duration); // 变化因子，范围在0到1之间
            objectRenderer.material.color = Color.Lerp(currentColor, dstColor, lerpFactor); // 平滑过渡
            if (lerpFactor >= 1.0f)
            {
                isChanging = false; // 变化完成
            }
        }
    }
}