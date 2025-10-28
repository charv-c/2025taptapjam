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
    public AudioClip bgmLevel3;     // 知音篇：古琴台主题BGM，体现高山流水意境
    public AudioClip bgmLevel4;     // 白蛇传篇：断桥情缘主题BGM

    [Header("音效 (SFX)")]
    public AudioClip sfxUIClick;             // 关卡UI按钮交互音效 [cite: 12]
    public AudioClip sfxButtonHover;             // 主菜单按钮悬停音效
    public AudioClip sfxButtonClick;         // 主菜单按钮点击音效
    public AudioClip sfxTransform;           // “化字”音效 [cite: 12]
    public AudioClip sfxAcquire;             // “取字”音效 [cite: 12]
    public AudioClip sfxSplitSuccess;        // 成功“拆字” [cite: 12]
    public AudioClip sfxCombineSuccess;      // 成功“拼字” [cite: 12]
    public AudioClip sfxOperationFailure;    // “拆/拼”操作失败 [cite: 12]
    public AudioClip sfxSelectWord;          // 在解字台选中文字 [cite: 12]
    public AudioClip sfxGoalFlyIn;           // 目标字飞入诗句的核心奖励音效 [cite: 12]
    public AudioClip sfxWin;                 // 关卡胜利音效
    public AudioClip sfxGuqinPlay;           // 与解语琴交互时的琴弦音效（Level3)
    public AudioClip sfxEasterEgg;           // 触发彩蛋音效
    public AudioClip sfxHunterLeave;         // 猎人离去音效 (Level2)
    public AudioClip sfxChildLaugh;          // 孩童笑声音效 (Level2/Level3)
    public AudioClip sfxScholarEnlighten;    // 书生恍然大悟音效 (Level3)
    public AudioClip sfxBirdCall;            // 鸟叫音效 (Level3)
    public AudioClip sfxApplause;            // 胜利鼓掌音效 (EndLevel)
    public AudioClip sfxBugEatLeaf;          // 虫子吃叶子音效 (Level3)
    public AudioClip sfxSeedSprout;          // 籽发芽音效 (Level3)
    public AudioClip sfxXionghuang;          // 雄黄酒交互音效 (Level4)
    public AudioClip sfxClock;               // 倒计时音效 (Level4)
    public AudioClip sfxSnakeEat;            // 蛇吞食字音效 (Level4)
    public AudioClip sfxWater;               // 清字浇花音效 (Level4)
    public AudioClip sfxBox;                 // 木盒出现音效 (Level4)
    public AudioClip sfxRope;                // 绳子出现音效 (Level4)
    public AudioClip sfxHan;                 // 汉字出现音效 (Level4)
    public AudioClip sfxJiao;                // 骄子出现音效 (Level4)
    public AudioClip sfxBack;                // 恢复人形音效 (Level4)

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

    // --- 知音篇专用音效方法 ---

    /// <summary>
    /// 播放古琴交互音效（含季节切换&文字反转）
    /// </summary>
    public void PlayGuqinInteraction()
    {
        PlaySFX(sfxGuqinPlay);
    }

    /// <summary>
    /// 播放猎人离去音效 (Level2)
    /// </summary>
    public void PlayHunterLeave()
    {
        PlaySFX(sfxHunterLeave);
    }

    /// <summary>
    /// 播放孩童笑声音效 (Level2/Level3)
    /// </summary>
    public void PlayChildLaugh()
    {
        PlaySFX(sfxChildLaugh);
    }

    /// <summary>
    /// 播放书生恍然大悟音效 (Level3)
    /// </summary>
    public void PlayScholarEnlighten()
    {
        PlaySFX(sfxScholarEnlighten);
    }

    /// <summary>
    /// 播放鸟叫音效 (Level3)
    /// </summary>
    public void PlayBirdCall()
    {
        PlaySFX(sfxBirdCall);
    }

    /// <summary>
    /// 播放胜利鼓掌音效 (EndLevel)
    /// </summary>
    public void PlayApplause()
    {
        PlaySFX(sfxApplause);
    }

    /// <summary>
    /// 播放虫子吃叶子音效 (Level3)
    /// </summary>
    public void PlayBugEatLeaf()
    {
        PlaySFX(sfxBugEatLeaf);
    }

    /// <summary>
    /// 播放籽发芽音效 (Level3)
    /// </summary>
    public void PlaySeedSprout()
    {
        PlaySFX(sfxSeedSprout);
    }

    // --- Level4音效专用方法 ---

    /// <summary>
    /// 播放雄黄酒交互音效 (Level4)
    /// </summary>
    public void PlayXionghuang()
    {
        PlaySFX(sfxXionghuang);
    }

    /// <summary>
    /// 播放倒计时音效 (Level4)
    /// </summary>
    public void PlayClock()
    {
        PlaySFX(sfxClock);
    }

    /// <summary>
    /// 播放蛇吞食字音效 (Level4)
    /// </summary>
    public void PlaySnakeEat()
    {
        PlaySFX(sfxSnakeEat);
    }

    /// <summary>
    /// 播放清字浇花音效 (Level4)
    /// </summary>
    public void PlayWater()
    {
        PlaySFX(sfxWater);
    }

    /// <summary>
    /// 播放木盒出现音效 (Level4)
    /// </summary>
    public void PlayBox()
    {
        PlaySFX(sfxBox);
    }

    /// <summary>
    /// 播放绳子出现音效 (Level4)
    /// </summary>
    public void PlayRope()
    {
        PlaySFX(sfxRope);
    }

    /// <summary>
    /// 播放汉字出现音效 (Level4)
    /// </summary>
    public void PlayHan()
    {
        PlaySFX(sfxHan);
    }

    /// <summary>
    /// 播放骄子出现音效 (Level4)
    /// </summary>
    public void PlayJiao()
    {
        PlaySFX(sfxJiao);
    }

    /// <summary>
    /// 播放恢复人形音效 (Level4)
    /// </summary>
    public void PlayBack()
    {
        PlaySFX(sfxBack);
    }

    // --- UI音效专用方法 ---

    /// <summary>
    /// 播放按钮悬停音效
    /// </summary>
    public void PlayButtonHover()
    {
        PlaySFX(sfxButtonHover);
    }

    /// <summary>
    /// 播放按钮点击音效
    /// </summary>
    public void PlayButtonClick()
    {
        PlaySFX(sfxButtonClick);
    }
    
    /// <summary>
    /// 播放通用UI点击音效
    /// </summary>
    public void PlayUIClick()
    {
        PlaySFX(sfxUIClick);
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