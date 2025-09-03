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

    private void Start()
    {
        // 进入开始场景时播放菜单BGM（通过AudioManager统一控制）
        if (AudioManager.Instance != null && AudioManager.Instance.bgmMenu != null)
        {
            AudioManager.Instance.PlayBGM(AudioManager.Instance.bgmMenu);
        }
    }

    public void OnStartGameClicked()
    {
        // 统一经由AudioManager播放点击音效，并在音效结束后加载场景
        StartCoroutine(LoadSceneAfterSound());
    }

    /// <summary>
    /// 播放点击音效并等待其播放完毕后再加载场景
    /// </summary>
    private IEnumerator LoadSceneAfterSound()
    {
        AudioClip clickClip = (AudioManager.Instance != null) ? AudioManager.Instance.sfxButtonClick : null;

        if (AudioManager.Instance != null && clickClip != null)
        {
            AudioManager.Instance.PlaySFX(clickClip);
            yield return new WaitForSeconds(clickClip.length);
        }
        // 若没有可用的AudioManager或音效未配置，则直接进入下一场景

        PublicData.OnBeforeSceneTransition();
        SceneManager.LoadScene(sceneToLoad);
    }

    public void PlayHoverSound()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.sfxButtonHover != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxButtonHover);
        }
    }
}