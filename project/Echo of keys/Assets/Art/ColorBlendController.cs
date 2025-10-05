using UnityEngine;
public class ColorBlendController : MonoBehaviour
{
    //一个材质切换程序，方便不同场景调色
    public Color[] setColor = new Color[5];
   public float duration = 50.0f; // 变化持续时间
   private Color currentColor;
   private Color dstColor;
    private int currentLevel;
   private Renderer objectRenderer;
   private float timer;
   private bool isChanging = false;
    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        currentColor = setColor[0];
       currentLevel = 0;
        timer = 0f;
   }
    void Update()
    {
        int dstLevel = 2; //!!!从程序那边获取当前切换的关卡!!!
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