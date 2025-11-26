// Purpose: Handles player movement, ranged auto-attack, and flag placement commands (SRP: player control)
using UnityEngine;
using Sirenix.OdinInspector;

public sealed class PlayerController : MonoBehaviour
{
    [TabGroup("Refs")] [SerializeField] private GameBalanceConfig balanceConfig;
    [TabGroup("Refs")] [SerializeField] private PlayerStatus playerStatus;
    [TabGroup("Refs")] [SerializeField] private FlagController flagController;
    [TabGroup("Refs")] [SerializeField] private Camera viewCamera;
    [TabGroup("Refs")] [SerializeField] private LayerMask groundMask;
    [TabGroup("Refs")] [SerializeField] private LayerMask enemyMask;

    [TabGroup("Input")] [SerializeField] private KeyCode cancelFlagKey = KeyCode.Mouse1;
    [TabGroup("Input")] [SerializeField] private KeyCode flagKey1 = KeyCode.Alpha1;
    [TabGroup("Input")] [SerializeField] private KeyCode flagKey2 = KeyCode.Alpha2;
    [TabGroup("Input")] [SerializeField] private KeyCode flagKey3 = KeyCode.Alpha3;
    [TabGroup("Input")] [SerializeField] private KeyCode flagKey4 = KeyCode.Alpha4;

    [TabGroup("Combat")] [SerializeField] private GameObject arrowPrefab;
    [TabGroup("Combat")] [SerializeField] private float arrowSpeed = 15f;
    [TabGroup("Combat")] [SerializeField] private Transform arrowSpawnPoint;

    private Vector2 _moveInput;
    private Vector3 _velocity;
    private FlagConfig _selectedFlag;
    private float _attackCooldown;

    private void Awake()
    {
        if (balanceConfig == null) Debug.LogWarning("[Player] Missing GameBalanceConfig");
        if (playerStatus == null) Debug.LogWarning("[Player] Missing PlayerStatus");
        if (viewCamera == null) viewCamera = Camera.main;
        Debug.Log("[Player] Controller initialized");
    }

    private void Update()
    {
        PollInput();
        TickMovement(Time.deltaTime);
        TickAttackCooldown(Time.deltaTime);
        TickAutoAttack();
    }

    private void TryAttack()
    {
        if (balanceConfig == null) return;
        if (_attackCooldown > 0f) return;
        _attackCooldown = balanceConfig.playerAttackInterval;

        Collider[] hits = Physics.OverlapSphere(transform.position, balanceConfig.playerAttackRadius, enemyMask);
        if (hits.Length == 0)
        {
            Debug.Log("[Player] Attack: no targets");
            return;
        }

        Collider target = FindClosest(hits);
        float damage = balanceConfig != null ? balanceConfig.playerAttackDamage : 0f;
        if (target.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.ApplyDamage(damage);
            Debug.Log($"[Player] Ranged attack hit {target.name} for {damage}");
            SpawnArrow(target.transform.position);
        }
        else
        {
            Debug.Log($"[Player] Ranged attack target {target.name} has no IDamageable, damage {damage}");
        }
    }

    public void TryPlaceFlag(FlagConfig flagConfig, Vector3 worldPosition)
    {
        if (flagConfig == null || flagController == null) return;
        flagController.PlaceFlag(flagConfig, worldPosition);
    }

    public bool TryPlaceFlagAtCursor(FlagConfig flagConfig)
    {
        if (viewCamera == null) return false;
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        int mask = groundMask.value == 0 ? Physics.DefaultRaycastLayers : groundMask.value;

        if (Physics.Raycast(ray, out RaycastHit hit, 200f, mask, QueryTriggerInteraction.Ignore))
        {
            TryPlaceFlag(flagConfig, hit.point);
            return true;
        }

        RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray, 200f, mask);
        if (hit2D.collider != null)
        {
            TryPlaceFlag(flagConfig, hit2D.point);
            return true;
        }

        Debug.LogWarning($"[Player] Flag placement failed: no ground hit (mask:{mask})");
        return false;
    }

    #region Input
    private void PollInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        _moveInput = new Vector2(h, v);
        if (_moveInput.sqrMagnitude > 1f)
        {
            _moveInput.Normalize();
        }

        HandleFlagSelection();

        if (_selectedFlag != null && Input.GetMouseButtonDown(0))
        {
            bool placed = TryPlaceFlagAtCursor(_selectedFlag);
            if (!placed)
            {
                Debug.Log("[Player] Flag placement failed (no ground hit)");
            }
            return;
        }

        if (_selectedFlag != null && Input.GetKeyDown(cancelFlagKey))
        {
            RemoveSelectedFlag();
            return;
        }
    }

    private void HandleFlagSelection()
    {
        if (flagController == null) return;
        if (Input.GetKeyDown(flagKey1)) SelectFlagByIndex(0);
        if (Input.GetKeyDown(flagKey2)) SelectFlagByIndex(1);
        if (Input.GetKeyDown(flagKey3)) SelectFlagByIndex(2);
        if (Input.GetKeyDown(flagKey4)) SelectFlagByIndex(3);
    }
    #endregion

    #region Movement
    private void TickMovement(float deltaTime)
    {
        if (balanceConfig == null) return;
        Vector3 move = new Vector3(_moveInput.x, _moveInput.y, 0f);
        _velocity = move * balanceConfig.playerMoveSpeed;
        transform.position += _velocity * deltaTime;
    }
    #endregion

    #region Combat
    private void TickAttackCooldown(float deltaTime)
    {
        if (_attackCooldown > 0f)
        {
            _attackCooldown -= deltaTime;
        }
    }

    private void TickAutoAttack()
    {
        if (_attackCooldown > 0f) return;
        TryAttack();
    }

    private Collider FindClosest(Collider[] hits)
    {
        Collider closest = hits[0];
        float closestSqr = (hits[0].transform.position - transform.position).sqrMagnitude;
        for (int i = 1; i < hits.Length; i++)
        {
            float sqr = (hits[i].transform.position - transform.position).sqrMagnitude;
            if (sqr < closestSqr)
            {
                closestSqr = sqr;
                closest = hits[i];
            }
        }
        return closest;
    }
    #endregion

    #region Flags
    private void SelectFlagByIndex(int index)
    {
        FlagConfig config = flagController.GetEquippedFlag(index);
        if (config == null)
        {
            Debug.Log($"[Player] Flag slot {index + 1} empty");
            return;
        }
        _selectedFlag = config;
        Debug.Log($"[Player] Selected flag {_selectedFlag.colorName}");
    }

    private void RemoveSelectedFlag()
    {
        if (_selectedFlag == null || flagController == null) return;
        flagController.RemoveFlag(_selectedFlag);
        _selectedFlag = null;
    }
    #endregion

    #region Projectile
    private void SpawnArrow(Vector3 targetPosition)
    {
        if (arrowPrefab == null) return;
        Vector3 spawnPos = arrowSpawnPoint != null ? arrowSpawnPoint.position : transform.position;
        Vector3 direction = (targetPosition - spawnPos).normalized;
        GameObject arrow = Object.Instantiate(arrowPrefab, spawnPos, Quaternion.LookRotation(direction));
        if (arrow.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = direction * arrowSpeed;
        }
        Debug.Log($"[Player] Arrow spawned toward {targetPosition}");
    }
    #endregion
}
