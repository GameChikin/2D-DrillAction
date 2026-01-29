using UnityEngine;

// 壁も敵もボスも、この「ダメージを受けられる」というルールを守る
public interface IDamageable
{
    void TakeDamage(int damage, Vector2 hitPoint);
}