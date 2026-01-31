using UnityEngine;

public class ArmorPiece : MonoBehaviour, IDamageable
{
    [SerializeField] private int hp = 1;
    [SerializeField] private GameObject breakEffect;

    public void TakeDamage(int damage, Vector2 hitPoint)
    {
        hp -= damage;

        // 【自動検知】DamageFlasherコンポーネントがあれば光らせる [cite: 2026-01-31]
        if (TryGetComponent<DamageFlasher>(out var flasher))
        {
            flasher.CallFlash();
        }

        if (hp <= 0) Break();
    }

    public void Break()
    {
        if (breakEffect != null) Instantiate(breakEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}