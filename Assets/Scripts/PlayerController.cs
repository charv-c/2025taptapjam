using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("玩家切换设置")]
    [SerializeField] private List<Player> players = new List<Player>();
    [SerializeField] private KeyCode switchKey = KeyCode.Space;
    
    [Header("颜色管理设置")]
    [SerializeField] private bool enableColorManagement = true; // 是否启用颜色管理

    private int currentPlayerIndex = 0;
    private Player currentPlayer;

    void Start()
    {
        // 如果没有手动设置玩家，自动查找场景中的所有Player组件
        if (players.Count == 0)
        {
            Player[] foundPlayers = FindObjectsOfType<Player>();
            players.AddRange(foundPlayers);
        }

        // 首先禁用所有玩家的输入，并设置玩家类型
        for (int i = 0; i < players.Count; i++)
        {
            Player player = players[i];
            if (player != null)
            {
                player.SetInputEnabled(false);
                // 设置玩家类型：第一个为Player1（左半边），第二个为Player2（右半边）
                player.SetPlayerType(i == 0);
            }
        }

        // 初始化第一个玩家为当前控制的玩家，但不启用输入（由TutorialManager控制）
        if (players.Count > 0)
        {
            currentPlayerIndex = 0;
            currentPlayer = players[0];
            // 不调用 SetCurrentPlayer(0)，因为那会启用输入
            
            // 只有在启用颜色管理时才初始化颜色状态
            if (enableColorManagement)
            {
                UpdatePlayerColors();
            }
        }
        else
        {
            GameLogger.LogWarning("场景中没有找到Player组件！");
        }
    }

    void Update()
    {
        // 检测切换按键
        if (Input.GetKeyDown(switchKey) && players.Count > 1)
        {
            SwitchPlayer();
        }

        // 只有在当前玩家输入启用时才处理输入
        if (currentPlayer != null && currentPlayer.IsInputEnabled())
        {
            HandleCurrentPlayerInput();
        }
    }

    void SwitchPlayer()
    {
        // 禁用当前玩家的输入
        if (currentPlayer != null)
        {
            currentPlayer.SetInputEnabled(false);
        }

        // 切换到下一个玩家
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        SetCurrentPlayer(currentPlayerIndex);
        
        // 只有在启用颜色管理时才更新颜色状态
        if (enableColorManagement)
        {
            UpdatePlayerColors();
        }

        GameLogger.LogDev($"切换到玩家 {currentPlayerIndex + 1}");
    }

    void SetCurrentPlayer(int index)
    {
        if (index >= 0 && index < players.Count)
        {
            currentPlayerIndex = index;
            currentPlayer = players[index];
            currentPlayer.SetInputEnabled(true);
            
            // 只有在启用颜色管理时才更新颜色状态
            if (enableColorManagement)
            {
                UpdatePlayerColors();
            }
        }
    }

    void HandleCurrentPlayerInput()
    {
        // 获取输入
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // 将输入传递给当前玩家
        currentPlayer.SetInput(horizontalInput, verticalInput);
    }

    // 公共方法：添加玩家到列表
    public void AddPlayer(Player player)
    {
        if (player != null && !players.Contains(player))
        {
            players.Add(player);
            player.SetInputEnabled(false); // 新添加的玩家默认禁用输入
        }
    }

    // 公共方法：移除玩家
    public void RemovePlayer(Player player)
    {
        if (players.Contains(player))
        {
            int index = players.IndexOf(player);
            players.Remove(player);

            // 如果移除的是当前玩家，切换到下一个
            if (index == currentPlayerIndex && players.Count > 0)
            {
                SetCurrentPlayer(0);
            }
        }
    }

    // 公共方法：获取当前玩家
    public Player GetCurrentPlayer()
    {
        return currentPlayer;
    }

    // 公共方法：获取指定索引的玩家
    public Player GetPlayerByIndex(int index)
    {
        if (index >= 0 && index < players.Count)
        {
            return players[index];
        }
        return null;
    }

    // 公共方法：获取当前玩家索引
    public int GetCurrentPlayerIndex()
    {
        return currentPlayerIndex;
    }

    // 公共方法：获取玩家总数
    public int GetPlayerCount()
    {
        return players.Count;
    }

    // 公共方法：设置所有玩家的初始位置
    public void SetAllPlayersStartPositions(Vector3 player1Position, Vector3 player2Position)
    {
        if (players.Count >= 1)
        {
            players[0].SetCustomStartPosition(player1Position);
        }

        if (players.Count >= 2)
        {
            players[1].SetCustomStartPosition(player2Position);
        }
    }

    // 公共方法：重置所有玩家为默认位置
    public void ResetAllPlayersToDefaultPositions()
    {
        foreach (Player player in players)
        {
            if (player != null)
            {
                player.ResetToDefaultPosition();
            }
        }
    }

    // 公共方法：禁用当前玩家移动
    public void DisableCurrentPlayerMovement()
    {
        if (currentPlayer != null)
        {
            currentPlayer.SetInputEnabled(false);
            // 重置输入值，确保玩家停止移动
            currentPlayer.SetInput(0f, 0f);
            GameLogger.LogDev($"PlayerController: 已禁用玩家 {currentPlayerIndex + 1} 的移动");
        }
        else
        {
            GameLogger.LogWarning("PlayerController: currentPlayer 为 null，无法禁用移动");
        }
    }

    // 公共方法：设置当前玩家索引
    public void SetCurrentPlayerIndex(int index)
    {
        if (index >= 0 && index < players.Count)
        {
            // 禁用当前玩家的输入
            if (currentPlayer != null)
            {
                currentPlayer.SetInputEnabled(false);
            }

            // 设置新的当前玩家
            SetCurrentPlayer(index);
        }
        else
        {
            GameLogger.LogWarning($"无效的玩家索引: {index}，玩家总数: {players.Count}");
        }
    }

    // 公共方法：启用当前玩家移动
    public void EnableCurrentPlayerMovement()
    {
        if (currentPlayer != null)
        {
            currentPlayer.SetInputEnabled(true);
        }
    }

    // 公共方法：禁用玩家切换功能
    public void DisablePlayerSwitching()
    {
        // 通过将switchKey设置为None来禁用切换
        switchKey = KeyCode.None;
    }

    // 公共方法：启用玩家切换功能
    public void EnablePlayerSwitching()
    {
        // 重新启用切换键为空格键
        switchKey = KeyCode.Space;
    }
    
    /// <summary>
    /// 更新所有玩家的颜色状态（当前操控的玩家正常颜色，其他玩家灰色）
    /// </summary>
    public void UpdatePlayerColors()
    {
        if (!enableColorManagement)
        {
            GameLogger.LogWarning("PlayerController: 颜色管理已禁用，跳过更新玩家颜色。");
            return;
        }

        for (int i = 0; i < players.Count; i++)
        {
            Player player = players[i];
            if (player != null)
            {
                if (i == currentPlayerIndex)
                {
                    // 当前操控的玩家恢复正常颜色
                    player.RestoreNormalColor();
                }
                else
                {
                    // 其他玩家设置为灰色
                    player.SetGrayedOut();
                }
            }
        }
        GameLogger.LogDev($"PlayerController: 已更新玩家颜色状态，当前操控玩家: {currentPlayerIndex + 1}");
    }
    
    /// <summary>
    /// 将所有玩家恢复正常颜色
    /// </summary>
    public void RestoreAllPlayerColors()
    {
        if (!enableColorManagement)
        {
            GameLogger.LogWarning("PlayerController: 颜色管理已禁用，跳过恢复玩家颜色。");
            return;
        }

        foreach (Player player in players)
        {
            if (player != null)
            {
                player.RestoreNormalColor();
            }
        }
        GameLogger.LogDev("PlayerController: 已恢复所有玩家正常颜色");
    }
    
    /// <summary>
    /// 将所有玩家设置为灰色
    /// </summary>
    public void SetAllPlayersGrayedOut()
    {
        if (!enableColorManagement)
        {
            GameLogger.LogWarning("PlayerController: 颜色管理已禁用，跳过设置玩家为灰色。");
            return;
        }

        foreach (Player player in players)
        {
            if (player != null)
            {
                player.SetGrayedOut();
            }
        }
        GameLogger.LogDev("PlayerController: 已将所有玩家设置为灰色");
    }
    
    /// <summary>
    /// 启用颜色管理
    /// </summary>
    public void EnableColorManagement()
    {
        enableColorManagement = true;
        GameLogger.LogDev("PlayerController: 已启用颜色管理");
    }
    
    /// <summary>
    /// 禁用颜色管理
    /// </summary>
    public void DisableColorManagement()
    {
        enableColorManagement = false;
        GameLogger.LogDev("PlayerController: 已禁用颜色管理");
    }
    
    /// <summary>
    /// 检查颜色管理是否启用
    /// </summary>
    /// <returns>颜色管理是否启用</returns>
    public bool IsColorManagementEnabled()
    {
        return enableColorManagement;
    }
}
