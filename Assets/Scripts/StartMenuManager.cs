using System.Collections; // 必须引用此命名空间才能使用协程
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 开始菜单的管理器脚本
/// 负责处理UI交互，如点击“开始游戏”按钮，并播放相关音效
/// </summary>
public class StartMenuManager : MonoBehaviour
{
    [Header("场景设置")]
    public string sceneToLoad = "FormalLevel_Cowherd";

    [Header("音效设置")]
    public AudioClip hoverSound;
    public AudioClip clickSound;
    public AudioSource sfxSource;

    /// <summary>
    /// “开始游戏”按钮被点击时，会调用此方法。
    /// 它不再直接加载场景，而是启动一个协程。
    /// </summary>
    public void OnStartGameClicked()
    {
        // 确保音频源和音频片段都已设置
        if (sfxSource != null && clickSound != null)
        {
            // 启动“在音效后加载场景”的协程
            StartCoroutine(LoadSceneAfterSound());
        }
        else
        {
            // 如果没有设置音效，则直接加载场景，避免卡住
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    /// <summary>
    /// 这是一个协程，用于播放音效并等待其播放完毕后再加载场景
    /// </summary>
    private IEnumerator LoadSceneAfterSound()
    {
        // 1. 播放点击音效
        sfxSource.PlayOneShot(clickSound);

        // 2. 等待一段时间，等待的时间正好是音效的长度
        yield return new WaitForSeconds(clickSound.length);

        // 3. 音效播放得差不多了，现在加载下一个场景
        SceneManager.LoadScene(sceneToLoad);
    }

    public void PlayHoverSound()
    {
        if (hoverSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(hoverSound);
        }
    }

    // 点击音效的播放现在由协程管理，所以这个方法可以移除了
    // public void PlayClickSound() { ... }
}