using UnityEngine;
using UnityEngine.SceneManagement; // シーン切り替えに必要 [cite: 2026-01-24]

public class GameManager : MonoBehaviour
{
    [Header("--- References ---")]
    [Tooltip("監視対象のプレイヤー")]
    [SerializeField] private PlayerController player;
    
    [Tooltip("ゲームオーバー時に表示するUIパネル")]
    [SerializeField] private GameObject gameOverUI;

    private void Start()
    {
        // プレイヤーの死亡イベントを購読
        if (player != null)
        {
            player.OnPlayerDeath += HandlePlayerDeath;
        }
        
        // 開始時はUIを隠しておく
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // メモリリーク防止のため購読を解除
        if (player != null)
        {
            player.OnPlayerDeath -= HandlePlayerDeath;
        }
    }

    /// <summary>
    /// プレイヤー死亡時に呼ばれる処理
    /// </summary>
    private void HandlePlayerDeath()
    {
        Debug.Log("Game Over!");
        
        // 1. ゲーム内の時間を止める
        Time.timeScale = 0f;
        
        // 2. ゲームオーバー画面を表示
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
    }

    /// <summary>
    /// UIのボタンから呼び出すリスタート処理
    /// </summary>
    public void RestartGame()
    {
        // 止めた時間を戻してからシーンを再読み込みする
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}