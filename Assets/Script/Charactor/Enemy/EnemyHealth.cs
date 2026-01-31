using UnityEngine;
using System.Collections; // コルーチンを使用しない場合でも、将来的な拡張性を考慮し保持 [cite: 2026-01-31]

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("--- HP Settings ---")]
    [SerializeField] private int maxHP = 3;
    
    [Header("--- Visual Effects ---")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject destroyEffectPrefab;

    private int currentHP;
    private bool isDead = false;
    private EnemyAI ai;

    private void Awake()
    {
        currentHP = maxHP;
        ai = GetComponent<EnemyAI>();
    }

    public void TakeDamage(int damage, Vector2 hitPoint)
    {
        if (isDead) return;

        currentHP -= damage;

        // 1. ヒットエフェクトの生成
        if (hitEffectPrefab != null)
            Instantiate(hitEffectPrefab, hitPoint, Quaternion.identity);

        // 2. 【統合】DamageFlasher による点滅処理 [cite: 2026-01-31]
        // 自身および子オブジェクト（SpriteVisual等）から全ての DamageFlasher を探して実行
        DamageFlasher[] flashers = GetComponentsInChildren<DamageFlasher>();
        foreach (var f in flashers)
        {
            f.CallFlash();
        }

        // 3. AIを怯ませる
        if (ai != null) ai.OnTakeDamage();

        // 4. 死亡判定
        if (currentHP <= 0) Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (destroyEffectPrefab != null)
            Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
        
        Destroy(gameObject);
    }
}