using UnityEngine;
using System.Collections.Generic;

public class BuriedItemManager : MonoBehaviour
{
    public static BuriedItemManager Instance { get; private set; }
    
    // 座標（Vector2Int）とアイテムの紐付け
    private Dictionary<Vector2Int, BuriedItemInstance> itemMap = new Dictionary<Vector2Int, BuriedItemInstance>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // アイテムが自分を登録する
    public void RegisterItem(Vector2Int coord, BuriedItemInstance instance)
    {
        itemMap[coord] = instance;
    }

    // 壁が壊れた時に呼ばれる
    public void OnTileDestroyed(Vector2Int cellPos)
    {
        if (itemMap.TryGetValue(cellPos, out var item))
        {
            item.CheckTileClearing(); // 1x1なので即チェック
        }
    }
}