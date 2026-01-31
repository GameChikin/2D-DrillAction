using UnityEngine;
using System.Collections; // 必須 [cite: 2026-01-31]

public class GimmickHealth : MonoBehaviour
{
    [SerializeField] private int maxHP = 10;
    [SerializeField] private GameObject destroyEffect;
    
    private int currentHP;

    private void Awake()
    {
        currentHP = maxHP;
    }

    // 子（Piece）から呼ばれる窓口
    public void RelayDamage(int damage)
    {
        currentHP -= damage;

        // 全ての子にある DamageFlasher を一斉に光らせる [cite: 2026-01-31]
        foreach (var flasher in GetComponentsInChildren<DamageFlasher>())
        {
            flasher.CallFlash();
        }

        if (currentHP <= 0) BreakGimmick();
    }

    private void BreakGimmick()
    {
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
        }
        Destroy(gameObject); // 親ごと消える
    }
}