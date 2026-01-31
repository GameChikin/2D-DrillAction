using UnityEngine;

public class GimmickPiece : MonoBehaviour, IDamageable // 必須 [cite: 2026-01-31]
{
    private GimmickHealth root;

    private void Awake()
    {
        // 親の Health スクリプトを探して保持
        root = GetComponentInParent<GimmickHealth>();
    }

    public void TakeDamage(int damage, Vector2 hitPoint)
    {
        if (root != null)
        {
            root.RelayDamage(damage);
        }
    }
}