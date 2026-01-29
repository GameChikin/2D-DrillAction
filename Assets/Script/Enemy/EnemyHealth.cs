using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("--- HP Settings ---")]
    [SerializeField] private int maxHP = 3;
    
    [Header("--- Visual Effects ---")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject destroyEffectPrefab;

    private int currentHP;
    private SpriteRenderer sr;
    private Color originalColor;
    private bool isDead = false;
    private EnemyAI ai; // í«â¡

    private void Awake()
    {
        currentHP = maxHP;
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
        
        ai = GetComponent<EnemyAI>(); // í«â¡
    }

    public void TakeDamage(int damage, Vector2 hitPoint)
    {
        if (isDead) return;

        currentHP -= damage;

        if (hitEffectPrefab != null)
            Instantiate(hitEffectPrefab, hitPoint, Quaternion.identity);

        if (sr != null) StartCoroutine(FlashRed());

        // AIÇãØÇ‹ÇπÇÈÅií«â¡Åj
        if (ai != null) ai.OnTakeDamage();

        if (currentHP <= 0) Die();
    }

    private void Die()
    {
        isDead = true;
        if (destroyEffectPrefab != null)
            Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private IEnumerator FlashRed()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        if (sr != null) sr.color = originalColor;
    }
}