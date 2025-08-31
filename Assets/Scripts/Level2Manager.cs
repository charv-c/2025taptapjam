using UnityEngine;

/// <summary>
/// Level2场景管理器
/// 负责在level2场景加载时启用所有玩家操作
/// </summary>
public class Level2Manager : MonoBehaviour
{
    private PlayerController playerController;

    void Start()
    {
        Debug.Log("Level2Manager: 开始初始化level2场景");
        
        // 设置Level2的BGM
        SetupLevel2BGM();
        
        // 延迟一帧后启用所有操作，确保PlayerController已完全初始化
        StartCoroutine(DelayedEnableAllOperations());
    }
    
    private System.Collections.IEnumerator DelayedEnableAllOperations()
    {
        // 等待一帧，确保PlayerController已完全初始化
        yield return null;
        
        // 查找PlayerController
        playerController = FindObjectOfType<PlayerController>();
        
        if (playerController != null)
        {
            EnableAllOperations();
        }
        else
        {
            Debug.LogError("Level2Manager: 未找到PlayerController");
        }
    }
    
    // 设置Level2的BGM
    private void SetupLevel2BGM()
    {
        if (AudioManager.Instance != null)
        {
            // 播放雨天BGM
            if (AudioManager.Instance.bgmRainy != null)
            {
                AudioManager.Instance.PlayBGM(AudioManager.Instance.bgmRainy);
                Debug.Log("Level2Manager: 已设置Level2 BGM为bgmRainy");
            }
            else
            {
                Debug.LogWarning("Level2Manager: bgmRainy音频片段未设置");
            }
            
            // 播放雨声环境音
            if (AudioManager.Instance.ambientRain != null)
            {
                AudioManager.Instance.PlayAmbient(AudioManager.Instance.ambientRain);
                Debug.Log("Level2Manager: 已播放雨声环境音");
            }
            else
            {
                Debug.LogWarning("Level2Manager: ambientRain音频片段未设置");
            }
        }
        else
        {
            Debug.LogWarning("Level2Manager: 未找到AudioManager实例");
        }
    }
    
    // 启用所有操作（移动、切换、回车、空格）
    private void EnableAllOperations()
    {
        Debug.Log("Level2Manager: 启用所有操作");
        
        if (playerController != null)
        {
            // 启用所有玩家的移动和回车键响应
            for (int i = 0; i < playerController.GetPlayerCount(); i++)
            {
                Player player = playerController.GetPlayerByIndex(i);
                if (player != null)
                {
                    // 启用移动
                    player.SetInputEnabled(true);
                    // 启用回车键响应
                    player.SetEnterKeyEnabled(true);
                }
            }
            
            // 设置第一个玩家为当前玩家
            if (playerController.GetPlayerCount() > 0)
            {
                playerController.SetCurrentPlayerIndex(0);
            }
            
            // 启用玩家切换功能
            playerController.EnablePlayerSwitching();
            
            // 更新玩家颜色状态（当前操控的玩家正常颜色，其他玩家灰色）
            playerController.UpdatePlayerColors();
            
            Debug.Log("Level2Manager: 已启用所有移动、切换、回车、空格操作，并设置玩家颜色状态");
        }
        else
        {
            Debug.LogWarning("Level2Manager: PlayerController为null，无法启用操作");
        }
    }
}
