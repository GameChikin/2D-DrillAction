using UnityEngine;

public class EnemyCore : MonoBehaviour, IDamageable
{
    [SerializeField] private int coreHp = 3;
    [SerializeField] private Transform armorPivot; 
    [SerializeField] private GameObject explosionEffect;

    public void TakeDamage(int damage, Vector2 hitPoint)
    {
        coreHp -= damage;

        // 【自動検知】コンポーネントがあれば光る [cite: 2026-01-31]
        if (TryGetComponent<DamageFlasher>(out var flasher))
        {
            flasher.CallFlash();
        }

        if (coreHp <= 0) Die();
    }

    private void Die()
    {
        if (armorPivot != null)
        {
            ArmorPiece[] remainingPieces = armorPivot.GetComponentsInChildren<ArmorPiece>();
            foreach (var piece in remainingPieces) piece.Break();
        }

        if (explosionEffect != null) Instantiate(explosionEffect, transform.position, Quaternion.identity);

        if (transform.parent != null) Destroy(transform.parent.gameObject);
        else Destroy(gameObject);
    }
}