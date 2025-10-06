using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;
    
    [SerializeField] private float defaultTransitionDelay = 3f;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void ScheduleTransitionToTitle(string titleSceneName, GameObject[] objectsToActivate = null, float delay = 3f)
    {
        StartCoroutine(TransitionCoroutine(titleSceneName, objectsToActivate, delay));
    }
    
    private IEnumerator TransitionCoroutine(string sceneName, GameObject[] objectsToActivate, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 加载新场景
        SceneManager.LoadScene(sceneName);
        
        // 等待一帧让新场景加载完成
        yield return null;
        
        // 激活指定对象
        if (objectsToActivate != null)
        {
            foreach (GameObject obj in objectsToActivate)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }
    }
}