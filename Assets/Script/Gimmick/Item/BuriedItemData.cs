using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Gimmick/BuriedItemData")]
public class BuriedItemData : ScriptableObject
{
    public string itemName;
    public Vector2Int size = new Vector2Int(1, 1); // 追加：タイル単位のサイズ
    public Sprite fullSprite;    // 発掘後の画像
    public Sprite glimpseSprite; // ヒント画像
    public GameObject itemPrefab; // 実体プレハブ
}