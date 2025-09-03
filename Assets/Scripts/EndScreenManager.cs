using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 临时的游戏结束页面管理器
/// 负责监听快捷键，返回主菜单或退出游戏
/// </summary>
public class EndScreenManager : MonoBehaviour
{
    [Header("场景设置")]
    [Tooltip("主菜单场景的文件名")]
    public string mainMenuSceneName = "StartMenu";

    private void Start()
    {
        // 播放一次性的胜利音效
        if (AudioManager.Instance != null && AudioManager.Instance.sfxWin != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxWin);
        }
    }

    void Update()
    {
        // 检测是否按下ESC键
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToMainMenu();
        }

        // 检测是否按下Q键
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ExitGame();
        }
    }

    public void ReturnToMainMenu()
    {
        // 加载主菜单场景
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ExitGame()
    {
        // 退出游戏
        // 注意：此功能在Unity编辑器中无效，只在打包后的游戏中有效
        Application.Quit();
    }
}