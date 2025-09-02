using UnityEngine;
using System.Collections;

// 基于音效设计文档
public class AudioManager : MonoBehaviour
{
    // 单例模式，方便从任何脚本中访问
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [Tooltip("背景音乐播放器")]
    [SerializeField] private AudioSource bgmSource;
    [Tooltip("音效播放器")]
    [SerializeField] private AudioSource sfxSource;
    [Tooltip("环境音播放器")]
    [SerializeField] private AudioSource ambientSource;

    [Header("音量控制")]
    [Range(0f, 1f)]
    public float initialBgmVolume = 0.5f; // BGM的初始音量，默认50%
    [Range(0f, 1f)]
    public float initialAmbientVolume = 1f; // 环境音的初始音量

    [Header("背景音乐 (BGM)")]
    public AudioClip bgmMenu;       // 用于主菜单的BGM [cite: 8]
    public AudioClip bgmRainy;      // 用于关卡下雨阶段的BGM [cite: 8]
    public AudioClip bgmSunny;      // 雨停后的BGM [cite: 8]
    public AudioClip bgmTutorial;   // 教程阶段的BGM [cite: 8]

    [Header("音效 (SFX)")]
    public AudioClip sfxUIClick;             // UI按钮交互音效 [cite: 12]
    public AudioClip sfxTransform;           // “化字”音效 [cite: 12]
    public AudioClip sfxAcquire;             // “取字”音效 [cite: 12]
    public AudioClip sfxSplitSuccess;        // 成功“拆字” [cite: 12]
    public AudioClip sfxCombineSuccess;      // 成功“拼字” [cite: 12]
    public AudioClip sfxOperationFailure;    // “拆/拼”操作失败 [cite: 12]
    public AudioClip sfxSelectWord;          // 在解字台选中文字 [cite: 12]
    public AudioClip sfxGoalFlyIn;           // 目标字飞入诗句的核心奖励音效 [cite: 12]

    [Header("环境音 (Ambient)")]
    public AudioClip ambientRain;            // 循环播放的背景雨声 [cite: 12]

    private void Awake()
    {
        // 实现单例模式，确保全局唯一
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // 应用初始音量设置
        bgmSource.volume = initialBgmVolume;
        ambientSource.volume = initialAmbientVolume;
    }

    // --- BGM 控制 ---

    public void PlayBGM(AudioClip clip)
    {
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    // 渐变切换到新的BGM
    public void CrossfadeToBGM(AudioClip newClip, float fadeDuration)
    {
        StartCoroutine(FadeBGMCoroutine(newClip, fadeDuration));
    }

    private IEnumerator FadeBGMCoroutine(AudioClip newClip, float duration)
    {
        float startVolume = bgmSource.volume;

        // 淡出
        while (bgmSource.volume > 0)
        {
            bgmSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        bgmSource.Stop();
        
        // 切换并淡入
        bgmSource.clip = newClip;
        bgmSource.Play();
        bgmSource.volume = 0f;
        while (bgmSource.volume < initialBgmVolume)
        {
            bgmSource.volume += initialBgmVolume * Time.deltaTime / duration;
            yield return null;
        }
        bgmSource.volume = initialBgmVolume;
    }

    // --- SFX 控制 ---

    // 播放一次音效，PlayOneShot可以处理多个音效同时播放而不会打断彼此
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // --- 环境音控制 ---

    public void PlayAmbient(AudioClip clip)
    {
        ambientSource.clip = clip;
        ambientSource.Play();
    }
    
    public void StopAmbient(float fadeDuration)
    {
        StartCoroutine(FadeOutAmbientCoroutine(fadeDuration));
    }

    private IEnumerator FadeOutAmbientCoroutine(float duration)
    {
        float startVolume = ambientSource.volume;
        while (ambientSource.volume > 0)
        {
            ambientSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }
        ambientSource.Stop();
        ambientSource.volume = initialAmbientVolume; // 恢复音量以便下次播放
    }

    // --- BGM停止控制 ---

    /// <summary>
    /// 停止当前播放的BGM
    /// </summary>
    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    /// <summary>
    /// 渐变停止当前BGM
    /// </summary>
    /// <param name="fadeDuration">渐变时长</param>
    public void StopBGMWithFade(float fadeDuration = 1f)
    {
        StartCoroutine(FadeOutBGMCoroutine(fadeDuration));
    }

    private IEnumerator FadeOutBGMCoroutine(float duration)
    {
        float startVolume = bgmSource.volume;
        while (bgmSource.volume > 0)
        {
            bgmSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }
        bgmSource.Stop();
        bgmSource.volume = initialBgmVolume; // 恢复音量以便下次播放
    }
}