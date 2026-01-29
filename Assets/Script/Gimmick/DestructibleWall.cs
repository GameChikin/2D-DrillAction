using UnityEngine;
using UnityEngine.AI;

public class DestructibleWall : MonoBehaviour, IDamageable
{
    [Header("--- HP Settings ---")]
    [SerializeField] private int hp = 3;

    [Header("--- Visual Effects (Prefabs) ---")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject destroyEffectPrefab;

    private Collider2D wallCollider;
    private NavMeshObstacle obstacle; 
    private bool isDestroyed = false;

    private void Awake()
    {
        wallCollider = GetComponent<Collider2D>();
        obstacle = GetComponent<NavMeshObstacle>();
    }

    public void TakeDamage(int damage, Vector2 hitPoint)
    {
        if (isDestroyed) return;
        hp -= damage;

        if (hitEffectPrefab != null)
            Instantiate(hitEffectPrefab, hitPoint, Quaternion.identity);

        if (hp <= 0) Break();
    }

    private void Break()
    {
        isDestroyed = true;



        if (wallCollider != null) wallCollider.enabled = false;
        if (obstacle != null) obstacle.enabled = false;

        if (destroyEffectPrefab != null)
            Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);

        if (TryGetComponent<Renderer>(out var renderer))
            renderer.enabled = false;
        
        Destroy(gameObject, 1.0f);
    }
}