using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    private enum EnemyState { Idle, Chasing, Preparing, Attacking, Recovering, Stunned }
    
    [Header("--- Discovery & Lost ---")]
    [SerializeField] private float detectionRange = 5.0f;
    [SerializeField] private float lostRange = 8.0f;

    [Header("--- UI Indicators ---")]
    [SerializeField] private Transform indicatorPoint;
    [SerializeField] private SpriteRenderer indicatorSR;
    [SerializeField] private Sprite discoveryIcon;
    [SerializeField] private Sprite lostIcon;
    [SerializeField] private float iconDuration = 1.0f;

    [Header("--- Damaged & Stun ---")]
    [Tooltip("チェックを外すと、ダメージを受けても怯まなくなります")]
    [SerializeField] private bool canBeStunned = true;  // 【新規統合】
    [SerializeField] private Sprite damagedSprite;     
    [SerializeField] private float stunDuration = 0.4f; 
    [SerializeField] private float shakeAmount = 0.08f; 

    [Header("--- Movement ---")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stoppingDistance = 0.8f;

    [Header("--- Attack Timing ---")]
    [SerializeField] private float prepareDuration = 0.5f;
    [SerializeField] private float attackDuration = 0.5f;
    [SerializeField] private float minRecoverTime = 0.7f;
    [SerializeField] private float maxRecoverTime = 1.2f;

    [Header("--- Combat ---")]
    [SerializeField] private float attackRadius = 1.0f;
    [SerializeField] private int attackDamage = 1;

    [Header("--- Visuals ---")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite prepareSprite;
    [SerializeField] private Sprite attackSprite;

    private NavMeshAgent agent;
    private SpriteRenderer bodySR;
    private Transform player;
    private EnemyState currentState = EnemyState.Idle;
    private Coroutine iconCoroutine;
    private Coroutine actionCoroutine; 
    private Vector3 originalBodyPos;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        bodySR = GetComponentInChildren<SpriteRenderer>();
        
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance;

        if (bodySR != null) originalBodyPos = bodySR.transform.localPosition;
        if (indicatorSR != null) indicatorSR.enabled = false;
    }

    private void Start() => player = GameObject.FindGameObjectWithTag("Player")?.transform;

    private void Update()
    {
        if (player == null || currentState == EnemyState.Stunned) return;

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState();
                break;
            case EnemyState.Chasing:
                HandleChasingState();
                break;
        }
    }

    private void HandleIdleState()
    {
        agent.isStopped = true;
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= detectionRange) TriggerDiscovery();
    }

    private void HandleChasingState()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > lostRange)
        {
            TriggerLost();
        }
        else if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (actionCoroutine == null) actionCoroutine = StartCoroutine(AttackRoutine());
        }
    }

    /// <summary>
    /// 被ダメージ時に外部（EnemyHealth）から呼ばれる
    /// </summary>
    public void OnTakeDamage()
    {
        // 【新規統合】怯み無効設定なら、ダメージを受けても思考を中断しない
        if (!canBeStunned) return;

        if (actionCoroutine != null) StopCoroutine(actionCoroutine);
        actionCoroutine = StartCoroutine(StunRoutine());
    }

    private IEnumerator StunRoutine()
    {
        currentState = EnemyState.Stunned;
        agent.isStopped = true;

        if (damagedSprite != null) bodySR.sprite = damagedSprite;

        float timer = 0;
        while (timer < stunDuration)
        {
            // 小刻みに震える視覚フィードバック
            float x = Random.Range(-1f, 1f) * shakeAmount;
            float y = Random.Range(-1f, 1f) * shakeAmount;
            bodySR.transform.localPosition = originalBodyPos + new Vector3(x, y, 0);

            timer += Time.deltaTime;
            yield return null;
        }

        bodySR.transform.localPosition = originalBodyPos;
        if (normalSprite != null) bodySR.sprite = normalSprite;

        float dist = Vector2.Distance(transform.position, player.position);
        currentState = (dist <= lostRange) ? EnemyState.Chasing : EnemyState.Idle;
        
        agent.isStopped = (currentState == EnemyState.Idle);
        actionCoroutine = null;
    }

    private void TriggerDiscovery() { currentState = EnemyState.Chasing; ShowIcon(discoveryIcon); }
    private void TriggerLost() { currentState = EnemyState.Idle; ShowIcon(lostIcon); }

    private void ShowIcon(Sprite icon)
    {
        if (indicatorSR == null || icon == null) return;
        if (iconCoroutine != null) StopCoroutine(iconCoroutine);
        iconCoroutine = StartCoroutine(IconDisplayRoutine(icon));
    }

    private IEnumerator IconDisplayRoutine(Sprite icon)
    {
        indicatorSR.sprite = icon;
        indicatorSR.enabled = true;
        yield return new WaitForSeconds(iconDuration);
        indicatorSR.enabled = false;
    }

    private IEnumerator AttackRoutine()
    {
        currentState = EnemyState.Preparing;
        agent.isStopped = true;
        if (prepareSprite != null) bodySR.sprite = prepareSprite;
        yield return new WaitForSeconds(prepareDuration);

        currentState = EnemyState.Attacking;
        if (attackSprite != null) bodySR.sprite = attackSprite;
        
        float timer = 0;
        while (timer < attackDuration)
        {
            CheckAttackHit();
            timer += Time.deltaTime;
            yield return null;
        }

        currentState = EnemyState.Recovering;
        if (normalSprite != null) bodySR.sprite = normalSprite;
        yield return new WaitForSeconds(Random.Range(minRecoverTime, maxRecoverTime));

        currentState = EnemyState.Chasing;
        agent.isStopped = false;
        actionCoroutine = null;
    }

    private void CheckAttackHit()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRadius, LayerMask.GetMask("Default"));
        if (hit != null && hit.CompareTag("Player"))
        {
            if (hit.TryGetComponent<PlayerController>(out var pc)) pc.TakeDamage();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, lostRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}