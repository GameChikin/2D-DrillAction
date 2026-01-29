using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    private enum PlayerState { Normal, Drill }
    private enum FacingDirection { Up, Down, Left, Right }

    // --- ライフサイクル・イベント（GameManagerやUIが購読します） ---
    public delegate void HPChangedHandler(int currentHP, int maxHP);
    public event HPChangedHandler OnHPChanged; 
    public event System.Action OnPlayerDeath;  

    [Header("--- HP Settings ---")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("--- Visuals (Normal) ---")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite spriteUp, spriteDown, spriteLeft, spriteRight;

    [Header("--- Visuals (Drill) ---")]
    [SerializeField] private Sprite drillUp, drillDown, drillLeft, drillRight;

    [Header("--- Visual Feedback ---")]
    [SerializeField] private float shakeAmount = 0.05f;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float flashDuration = 0.15f;

    [Header("--- Movement Settings ---")]
    [SerializeField] private float normalSpeed = 5f;
    [SerializeField] private float drillSpeed = 8f;

    [Header("--- Drill Combat Settings ---")]
    [SerializeField] private Transform drillHitPoint;
    [SerializeField] private float drillHitRadius = 0.4f;
    [SerializeField] private LayerMask destructibleLayer; // Inspectorで Wall と Enemy を選択すること
    [SerializeField] private int drillDamage = 1;
    [SerializeField] private float damageInterval = 0.2f;

    private Rigidbody2D rb;
    private PlayerInputActions inputActions; 
    private PlayerState currentState = PlayerState.Normal;
    private FacingDirection currentFacing = FacingDirection.Down;

    private Vector2 moveInput;
    private Vector2 lastFacingDirection = Vector2.down;
    private float nextDamageTime;
    private bool isFlashing = false;
    private Vector3 originalSpriteLocalPos;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputActions = new PlayerInputActions();

        // 【修正】HPの初期化を Awake に移動。
        // これにより、敵の攻撃が Start より先に走っても「HP 0 による無視」が発生しません。
        currentHealth = maxHealth;

        if (spriteRenderer != null)
            originalSpriteLocalPos = spriteRenderer.transform.localPosition;

        inputActions.Player.Drill.started += ctx => StartDrill();
        inputActions.Player.Drill.canceled += ctx => StopDrill();
    }

    private void Start()
    {
        // 初期HPを通知（UIが準備できているタイミングで実行）
        OnHPChanged?.Invoke(currentHealth, maxHealth);
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Update()
    {
        // 死亡時は入力を一切受け付けない
        if (currentHealth <= 0) return;

        moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        if (currentState == PlayerState.Normal && moveInput.sqrMagnitude > 0.01f)
        {
            lastFacingDirection = moveInput.normalized;
            UpdateFacing(lastFacingDirection);
        }

        UpdateVisualsAndHitPoint();
        HandleDrillShake();

        if (currentState == PlayerState.Drill)
        {
            HandleDrillCollision();
        }
    }

    private void FixedUpdate() => ApplyMovement();

    private void ApplyMovement()
    {
        // 死亡時は物理移動を停止。linearVelocityを0にする。
        if (currentHealth <= 0)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (currentState == PlayerState.Drill)
            rb.linearVelocity = lastFacingDirection * drillSpeed;
        else
            rb.linearVelocity = moveInput.normalized * normalSpeed;
    }

    private void UpdateFacing(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            currentFacing = (dir.x > 0) ? FacingDirection.Right : FacingDirection.Left;
        else
            currentFacing = (dir.y > 0) ? FacingDirection.Up : FacingDirection.Down;
    }

    private void UpdateVisualsAndHitPoint()
    {
        if (spriteRenderer == null) return;

        if (currentState == PlayerState.Drill)
        {
            spriteRenderer.sprite = currentFacing switch
            {
                FacingDirection.Up    => drillUp,
                FacingDirection.Down  => drillDown,
                FacingDirection.Left  => drillLeft,
                FacingDirection.Right => drillRight,
                _ => drillDown
            };
        }
        else
        {
            spriteRenderer.sprite = currentFacing switch
            {
                FacingDirection.Up    => spriteUp,
                FacingDirection.Down  => spriteDown,
                FacingDirection.Left  => spriteLeft,
                FacingDirection.Right => spriteRight,
                _ => spriteDown
            };
        }

        if (drillHitPoint != null)
        {
            float offset = 0.6f;
            drillHitPoint.localPosition = GetFacingVector() * offset;
        }
    }

    private void HandleDrillShake()
    {
        if (spriteRenderer == null) return;
        if (currentState == PlayerState.Drill)
        {
            float x = Random.Range(-1f, 1f) * shakeAmount;
            float y = Random.Range(-1f, 1f) * shakeAmount;
            spriteRenderer.transform.localPosition = originalSpriteLocalPos + new Vector3(x, y, 0);
        }
        else
            spriteRenderer.transform.localPosition = originalSpriteLocalPos;
    }

    private void StartDrill()
    {
        if (currentHealth <= 0) return;
        currentState = PlayerState.Drill;
        lastFacingDirection = GetFacingVector(); 
        nextDamageTime = 0f;
    }

    private void StopDrill() => currentState = PlayerState.Normal;

    private void HandleDrillCollision()
    {
        if (Time.time < nextDamageTime) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(drillHitPoint.position, drillHitRadius, destructibleLayer);
        
        if (hits.Length > 0)
        {
            var closest = hits
                .OrderBy(h => Vector2.Distance(transform.position, h.transform.position))
                .FirstOrDefault();

            if (closest != null && closest.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(drillDamage, drillHitPoint.position);
                nextDamageTime = Time.time + damageInterval;
            }
        }
    }

    /// <summary>
    /// ダメージを受け、死亡判定を行う。
    /// 以前の「フラッシュ演出」も完全に統合しています。
    /// </summary>
    public void TakeDamage()
    {
        // 修正：点滅中（無敵）または死亡済みなら無視する
        if (isFlashing || currentHealth <= 0) return;

        currentHealth--;
        OnHPChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else if (gameObject.activeInHierarchy)
        {
            StartCoroutine(DamageFlashRoutine());
        }
    }

    private void Die()
    {
        Debug.Log("Player Death State Triggered");
        
        // 【修正】ドリル状態を解除し、物理慣性を殺す
        StopDrill();
        rb.linearVelocity = Vector2.zero;
        
        // 死亡イベントの発火（GameManagerがこれを拾ってUIを出します）
        OnPlayerDeath?.Invoke();

        // 自身のスクリプトを止める
        this.enabled = false;
    }

    private IEnumerator DamageFlashRoutine()
    {
        if (spriteRenderer == null) yield break; // Nullチェック追加

        isFlashing = true;
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(flashDuration);
        
        if (spriteRenderer != null) // 待機中にオブジェクトが破棄された場合の安全策
            spriteRenderer.color = originalColor;
            
        isFlashing = false;
    }

    public Vector2 GetFacingVector()
    {
        return currentFacing switch
        {
            FacingDirection.Up => Vector2.up,
            FacingDirection.Down => Vector2.down,
            FacingDirection.Left => Vector2.left,
            FacingDirection.Right => Vector2.right,
            _ => Vector2.down
        };
    }
}