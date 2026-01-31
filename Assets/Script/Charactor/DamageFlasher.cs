using UnityEngine;         // SpriteRenderer, Color, MonoBehaviour用 [cite: 2026-01-24]
using System.Collections;  // IEnumerator用 [cite: 2026-01-31]

public class DamageFlasher : MonoBehaviour
{
    [SerializeField] private GlobalVisualSettings settings; // ここにおおもとの設定を紐付ける
    [SerializeField] private SpriteRenderer targetRenderer;

    private Color originalColor;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        if (targetRenderer == null) targetRenderer = GetComponent<SpriteRenderer>();
        if (targetRenderer != null) originalColor = targetRenderer.color;
    }

    public void CallFlash()
    {
        // 設定がアタッチされていない、あるいは無効化されている場合は何もしない [cite: 2026-01-24]
        if (settings == null || targetRenderer == null || !enabled) return;
        
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        // おおもとの設定（ScriptableObject）から値を取得する [cite: 2026-01-31]
        targetRenderer.color = settings.defaultFlashColor;
        yield return new WaitForSeconds(settings.flashDuration);
        targetRenderer.color = originalColor;
        flashCoroutine = null;
    }
}