using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("玩家切换设置")]
    [SerializeField] private List<Player> players = new List<Player>();
    [SerializeField] private KeyCode switchKey = KeyCode.Space;
    
    private int currentPlayerIndex = 0;
    private Player currentPlayer;
    
    void Start()
    {
        // 根据当前关卡播放对应的BGM
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName == "level1")
        {
            if (AudioManager.Instance != null && AudioManager.Instance.bgmTutorial != null)
            {
                AudioManager.Instance.PlayBGM(AudioManager.Instance.bgmTutorial);
            }
        }
        else // 默认为第二个关卡或其他关卡的处理方式
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
        
        // 初始化第一个玩家为当前控制的玩家
        if (players.Count > 0)
        {
            SetCurrentPlayer(0);
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
        
        // 将输入传递给当前控制的玩家
        if (currentPlayer != null)
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
}
