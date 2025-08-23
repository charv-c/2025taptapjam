using UnityEngine;
using System.Collections;

// ������Ч����ĵ�
public class AudioManager : MonoBehaviour
{
    // ����ģʽ��������κνű��з���
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [Tooltip("�������ֲ�����")]
    [SerializeField] private AudioSource bgmSource;
    [Tooltip("��Ч������")]
    [SerializeField] private AudioSource sfxSource;
    [Tooltip("������������")]
    [SerializeField] private AudioSource ambientSource;

    [Header("��������")]
    [Range(0f, 1f)]
    public float initialBgmVolume = 0.5f; // BGM�ĳ�ʼ������Ĭ��50%
    [Range(0f, 1f)]
    public float initialAmbientVolume = 1f; // �������ĳ�ʼ����

    [Header("�������� (BGM)")]
    public AudioClip bgmMenu;       // �������˵���BGM [cite: 8]
    public AudioClip bgmRainy;      // ���ڹؿ�����׶ε�BGM [cite: 8]
    public AudioClip bgmSunny;      // ��ͣ���BGM [cite: 8]
    public AudioClip bgmTutorial;   // �̳̽׶ε�BGM [cite: 8]

    [Header("��Ч (SFX)")]
    public AudioClip sfxUIClick;             // UI��ť������Ч [cite: 12]
    public AudioClip sfxTransform;           // �����֡���Ч [cite: 12]
    public AudioClip sfxAcquire;             // ��ȡ�֡���Ч [cite: 12]
    public AudioClip sfxSplitSuccess;        // �ɹ������֡� [cite: 12]
    public AudioClip sfxCombineSuccess;      // �ɹ���ƴ�֡� [cite: 12]
    public AudioClip sfxOperationFailure;    // ����/ƴ������ʧ�� [cite: 12]
    public AudioClip sfxSelectWord;          // �ڽ���̨ѡ������ [cite: 12]
    public AudioClip sfxGoalFlyIn;           // Ŀ���ַ���ʫ��ĺ��Ľ�����Ч [cite: 12]

    [Header("������ (Ambient)")]
    public AudioClip ambientRain;            // ѭ�����ŵı������� [cite: 12]

    private void Awake()
    {
        // ʵ�ֵ���ģʽ��ȷ��ȫ��Ψһ
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Ӧ�ó�ʼ��������
        bgmSource.volume = initialBgmVolume;
        ambientSource.volume = initialAmbientVolume;
    }

    // --- BGM ���� ---

    public void PlayBGM(AudioClip clip)
    {
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    // �����л����µ�BGM
    public void CrossfadeToBGM(AudioClip newClip, float fadeDuration)
    {
        StartCoroutine(FadeBGMCoroutine(newClip, fadeDuration));
    }

    private IEnumerator FadeBGMCoroutine(AudioClip newClip, float duration)
    {
        float startVolume = bgmSource.volume;

        // ����
        while (bgmSource.volume > 0)
        {
            bgmSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        bgmSource.Stop();

        // �л�������
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

    // --- SFX ���� ---

    // ����һ����Ч��PlayOneShot���Դ�������Чͬʱ���Ŷ������ϱ˴�
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // --- ���������� ---

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
        ambientSource.volume = initialAmbientVolume; // �ָ������Ա��´β���
    }
}