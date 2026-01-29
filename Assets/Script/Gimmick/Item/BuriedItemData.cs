using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Gimmick/BuriedItemData")]
public class BuriedItemData : ScriptableObject
{
    public string itemName;
    public Sprite fullSprite;    // ”­Œ@Œã‚Ì‰æ‘œ
    public Sprite glimpseSprite; // ƒqƒ“ƒg‰æ‘œ
    public GameObject itemPrefab; // ŽÀ‘ÌƒvƒŒƒnƒu
}