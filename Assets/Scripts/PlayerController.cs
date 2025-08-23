using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("玩家切换设置")]
    [SerializeField] private List<Player> players = new List<Player>();
    [SerializeField] private KeyCode switchKey = KeyCode.Space;

    private int currentPlayerIndex = 0;
    private Player currentPlayer;

    void Start()
    {
        // 游戏开始时，根据设计文档播放下雨场景的BGM
        if (AudioManager.Instance != null && AudioManager.Instance.bgmRainy != null)
        {
            AudioManager.Instance.PlayBGM(AudioManager.Instance.bgmRainy);
        }

        // 同时播放下雨的环境音
        if (AudioManager.Instance != null && AudioManager.Instance.ambientRain != null)
        {
            AudioManager.Instance.PlayAmbient(AudioManager.Instance.ambientRain);
        }

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
        }
        else
        {
            Debug.LogWarning("场景中没有找到Player组件！");
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

        Debug.Log($"切换到玩家 {currentPlayerIndex + 1}");
    }

    void SetCurrentPlayer(int index)
    {
        if (index >= 0 && index < players.Count)
        {
            currentPlayerIndex = index;
            currentPlayer = players[index];
            currentPlayer.SetInputEnabled(true);
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
            Debug.Log($"PlayerController: 已禁用玩家 {currentPlayerIndex + 1} 的移动");
        }
        else
        {
            Debug.LogWarning("PlayerController: currentPlayer 为 null，无法禁用移动");
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
            Debug.LogWarning($"无效的玩家索引: {index}，玩家总数: {players.Count}");
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
}
