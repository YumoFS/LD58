using UnityEngine;
using UnityEngine.UI;

public class QuitButton : MonoBehaviour
{
    private Button quitButton;

    void Start()
    {
        quitButton = GetComponent<Button>();
        quitButton.onClick.AddListener(Quit);
    }

    public void Quit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}