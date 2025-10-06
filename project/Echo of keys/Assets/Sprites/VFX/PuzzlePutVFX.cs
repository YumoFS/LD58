using System.Collections;
using UnityEngine;

public class PuzzlePutVFX : MonoBehaviour
{
    private float MaxSize = 1.05f;
    public float AnimationSpeed = 4f;
    private bool hasExist = false;
    
    void OnEnable()
    {
        StartCoroutine(PutPuzzle());
        hasExist = true;
    }
    
    IEnumerator PutPuzzle()
    {
        if (hasExist) yield break;
        
        float t = 0f;
        Vector3 baseVec = Vector3.one * MaxSize;
        
        while (t < 1)
        {
            // 使用二次缓动函数实现先慢后快的效果
            float easedT = EaseInExpo(t);
            
            this.transform.localScale = Vector3.Lerp(baseVec, Vector3.one, easedT);
            t += Time.deltaTime * AnimationSpeed;
            yield return null;
        }
        
        this.transform.localScale = Vector3.one;
        yield return null;
    }
    
    // 二次缓入函数：先慢后快
    // private float EaseInQuad(float t)
    // {
    //     return t * t;
    // }
    
    // 可选：其他缓动函数，可以根据需要选择
    // 三次缓入：更明显的先慢后快
    // private float EaseInCubic(float t)
    // {
    //     return t * t * t;
    // }
    
    // 指数缓入：更加极端的先慢后快
    private float EaseInExpo(float t)
    {
        return t == 0 ? 0 : Mathf.Pow(2, 10 * (t - 1));
    }
    
    // 正弦缓入：相对柔和
    // private float EaseInSine(float t)
    // {
    //     return 1 - Mathf.Cos((t * Mathf.PI) / 2);
    // }
}