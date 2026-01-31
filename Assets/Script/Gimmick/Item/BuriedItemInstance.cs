using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuriedItemInstance : MonoBehaviour
{
    [SerializeField] private BuriedItemData data;
    [SerializeField] private SpriteRenderer glimpseSR;
    [SerializeField] private SpriteRenderer fullSR;

    [Header("--- Animation Settings ---")]
    [SerializeField] private float bounceHeight = 0.5f; 
    [SerializeField] private float duration = 0.6f;     
    [SerializeField] private float destroyDelay = 0.2f; 

    [Header("--- Visual Effects ---")]
    [SerializeField] private GameObject foundEffectPrefab; 

    [Header("--- Debug Settings ---")]
    [SerializeField] private Color debugGizmoColor = Color.green; // 緑色 [cite: 2026-01-24]

    private bool isFound = false;
    private List<Vector2Int> occupiedCoords = new List<Vector2Int>(); 
    private int clearedTileCount = 0; 

    private void Start()
    {
        InitializeSizeAndRegistration();

        if (data != null)
        {
            glimpseSR.sprite = data.glimpseSprite;
            fullSR.sprite = data.fullSprite;
            fullSR.enabled = false;
        }
    }

    private void InitializeSizeAndRegistration()
    {
        if (data == null) return;

        // 1. 基点座標の取得（補正なしの標準計算）
        Vector2Int origin = new Vector2Int(
            Mathf.FloorToInt(transform.position.x),
            Mathf.FloorToInt(transform.position.y)
        );

        // 2. サイズ分だけループして登録
        for (int x = 0; x < data.size.x; x++)
        {
            for (int y = 0; y < data.size.y; y++)
            {
                Vector2Int targetCoord = origin + new Vector2Int(x, y);
                occupiedCoords.Add(targetCoord);
            
                if (BuriedItemManager.Instance != null)
                    BuriedItemManager.Instance.RegisterItem(targetCoord, this);
            }
        }

        // 3. 視覚的な中心補正（1.0 基準）
        // 矩形の中心 = サイズ / 2
        Vector3 visualOffset = new Vector3(
            data.size.x * 0.5f, 
            data.size.y * 0.5f, 
            0
        );
    
        if (fullSR != null) fullSR.transform.localPosition = visualOffset;
        if (glimpseSR != null) glimpseSR.transform.localPosition = visualOffset;
    }

        public void CheckTileClearing()
        {
            clearedTileCount++;

            if (clearedTileCount >= occupiedCoords.Count)
            {
                if (isFound) return;
                isFound = true;

                if (foundEffectPrefab != null)
                {
                    // 【修正】エフェクトも矩形の中心（fullSRの位置）で生成 [cite: 2026-01-31]
                    Instantiate(foundEffectPrefab, fullSR.transform.position, Quaternion.identity);
                }

                StartCoroutine(PopUpRoutine());
            }
        }

    private IEnumerator PopUpRoutine()
    {
        if (glimpseSR != null) glimpseSR.enabled = false;
        if (fullSR != null) fullSR.enabled = true;

        Debug.Log($"[獲得確定] {data.itemName}");

        Vector3 startPos = fullSR.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float yOffset = Mathf.Sin(progress * Mathf.PI) * bounceHeight;
            fullSR.transform.localPosition = startPos + new Vector3(0, yOffset, 0);
            yield return null;
        }

        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || occupiedCoords == null || occupiedCoords.Count == 0)
        {
            DrawDebugCellsFromTransform();
            return;
        }

        Gizmos.color = debugGizmoColor;
        foreach (var coord in occupiedCoords)
        {
            Vector3 cellCenter = new Vector3(coord.x + 0.5f, coord.y + 0.5f, 0);
            Gizmos.DrawWireCube(cellCenter, new Vector3(0.95f, 0.95f, 0.1f));
            
            Gizmos.color = new Color(debugGizmoColor.r, debugGizmoColor.g, debugGizmoColor.b, 0.2f);
            Gizmos.DrawCube(cellCenter, new Vector3(0.9f, 0.9f, 0.1f));
            Gizmos.color = debugGizmoColor;
        }
    }

    private void DrawDebugCellsFromTransform()
    {
        if (data == null) return;
        Gizmos.color = debugGizmoColor;
        Vector2Int origin = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));
        for (int x = 0; x < data.size.x; x++)
        {
            for (int y = 0; y < data.size.y; y++)
            {
                Vector3 cellCenter = new Vector3(origin.x + x + 0.5f, origin.y + y + 0.5f, 0);
                Gizmos.DrawWireCube(cellCenter, new Vector3(0.95f, 0.95f, 0.1f));
            }
        }
    }
}