using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthUI : MonoBehaviour
{
    [Header("--- Target ---")]
    [SerializeField] private PlayerController player;

    [Header("--- UI Sliders ---")]
    [Tooltip("手前にある緑色の即時反映ゲージ")]
    [SerializeField] private Slider greenHpSlider;
    [Tooltip("背後にある赤色の後追いゲージ")]
    [SerializeField] private Slider redDamageSlider;

    [Header("--- Animation Settings ---")]
    [Tooltip("赤いゲージが減少するスピード（値が大きいほど速い）")]
    [SerializeField] private float decreaseSpeed = 0.5f;
    [Tooltip("ダメージを受けてから赤いゲージが減り始めるまでの遅延時間")]
    [SerializeField] private float damageDelay = 0.5f;

    private Coroutine damageCoroutine;

    private void Awake()
    {
        if (player == null)
        {
            Debug.LogError("HealthUI: PlayerController がアサインされていません！");
            return;
        }

        // プレイヤーの HP 変化イベントを購読
        player.OnHPChanged += UpdateHPGauge;
    }

    private void OnDestroy()
    {
        if (player != null) player.OnHPChanged -= UpdateHPGauge;
    }

    /// <summary>
    /// HPが変化したときに呼ばれる
    /// </summary>
    private void UpdateHPGauge(int currentHP, int maxHP)
    {
        if (greenHpSlider == null || redDamageSlider == null) return;

        // 0除算回避
        if (maxHP <= 0) maxHP = 1;

        // 目標となるHP割合を計算 (0.0 ～ 1.0)
        float targetFillAmount = currentHP / (float)maxHP;

        // 1. 緑色のメインゲージは即座に更新する
        greenHpSlider.value = targetFillAmount;

        // 2. 赤色の後追いゲージの処理を開始する
        // 既存のアニメーションが動いていれば止める
        if (damageCoroutine != null) StopCoroutine(damageCoroutine);
        // 新しいアニメーションを開始
        damageCoroutine = StartCoroutine(SmoothDecreaseRoutine(targetFillAmount));
    }

    // 赤いゲージを滑らかに減らすコルーチン
    private IEnumerator SmoothDecreaseRoutine(float targetValue)
    {
        // ダメージの余韻を感じさせるため、少し待つ
        yield return new WaitForSeconds(damageDelay);

        // 赤いゲージの値が、目標値（緑ゲージの位置）より大きい間ループ
        while (redDamageSlider.value > targetValue)
        {
            // 現在値から目標値に向かって、一定のスピードで近づける
            redDamageSlider.value = Mathf.MoveTowards(redDamageSlider.value, targetValue, decreaseSpeed * Time.deltaTime);
            
            // 次のフレームまで待機
            yield return null;
        }

        // 念のため、最終的に値をぴったり合わせる
        redDamageSlider.value = targetValue;
        damageCoroutine = null;
    }
}