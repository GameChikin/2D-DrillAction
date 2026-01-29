using UnityEngine;
using System.Collections;

public class BuriedItemInstance : MonoBehaviour
{
    [SerializeField] private BuriedItemData data;
    [SerializeField] private SpriteRenderer glimpseSR;
    [SerializeField] private SpriteRenderer fullSR;

    [Header("--- Animation Settings ---")]
    [SerializeField] private float bounceHeight = 0.5f; 
    [SerializeField] private float duration = 0.6f;     
    [SerializeField] private float destroyDelay = 0.2f; // 演出終了から消えるまでの余韻

    [Header("--- Visual Effects ---")]
    [SerializeField] private GameObject foundEffectPrefab; // 掘り出した時のエフェクト

    private bool isFound = false;

    private void Start()
    {
        // 座標登録（1x1の暫定）
        Vector2Int myCoord = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));
        if (BuriedItemManager.Instance != null)
            BuriedItemManager.Instance.RegisterItem(myCoord, this);

        if (data != null)
        {
            glimpseSR.sprite = data.glimpseSprite;
            fullSR.sprite = data.fullSprite;
            fullSR.enabled = false;
        }
    }

    public void CheckTileClearing()
    {
        if (isFound) return;
        isFound = true;

        // 1. エフェクトの生成
        if (foundEffectPrefab != null)
        {
            Instantiate(foundEffectPrefab, transform.position, Quaternion.identity);
        }

        StartCoroutine(PopUpRoutine());
    }

    private IEnumerator PopUpRoutine()
    {
        if (glimpseSR != null) glimpseSR.enabled = false;
        if (fullSR != null) fullSR.enabled = true;

        // 【将来の課題：獲得処理】ここでスコア加算やインベントリ追加を呼ぶ
        Debug.Log($"[獲得確定] {data.itemName}");

        Vector3 startPos = fullSR.transform.localPosition;
        float elapsed = 0f;

        // 2. ポップアップ演出
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float yOffset = Mathf.Sin(progress * Mathf.PI) * bounceHeight;
            fullSR.transform.localPosition = startPos + new Vector3(0, yOffset, 0);
            yield return null;
        }

        // 3. 一定時間待機して消滅
        yield return new WaitForSeconds(destroyDelay);
        
        Debug.Log($"[消滅] {data.itemName} の演出が完了しました");
        Destroy(gameObject);
    }
}