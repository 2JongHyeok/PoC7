// Purpose: Handles player movement, ranged auto-attack, and flag placement commands (SRP: player control)
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public sealed class PlayerController : MonoBehaviour
{
    [TabGroup("Refs")] [SerializeField] private GameBalanceConfig balanceConfig;
    [TabGroup("Refs")] [SerializeField] private PlayerStatus playerStatus;
    [TabGroup("Refs")] [SerializeField] private FlagController flagController;
    [TabGroup("Refs")] [SerializeField] private Camera viewCamera;
    [TabGroup("Refs")] [SerializeField] private LayerMask groundMask;
    [TabGroup("Refs")] [SerializeField] private LayerMask enemyMask;
    [TabGroup("Refs")] [SerializeField] private SpriteRenderer attackRangeIndicator;
    [TabGroup("Refs")] [SerializeField] private SpriteRenderer flagRangeIndicator;
    [TabGroup("Refs")] [SerializeField] private List<GameObject> flagHighlights = new List<GameObject>();

    [TabGroup("Input")] [SerializeField] private KeyCode cancelFlagKey = KeyCode.Mouse1;
    [TabGroup("Input")] [SerializeField] private KeyCode flagKey1 = KeyCode.Alpha1;
    [TabGroup("Input")] [SerializeField] private KeyCode flagKey2 = KeyCode.Alpha2;
    [TabGroup("Input")] [SerializeField] private KeyCode flagKey3 = KeyCode.Alpha3;
    [TabGroup("Input")] [SerializeField] private KeyCode flagKey4 = KeyCode.Alpha4;

    [TabGroup("Combat")] [SerializeField] private GameObject arrowPrefab;
    [TabGroup("Combat")] [SerializeField] private float arrowSpeed = 15f;
    [TabGroup("Combat")] [SerializeField] private Transform arrowSpawnPoint;
    [TabGroup("Combat")] [SerializeField] private float rangeIndicatorAlpha = 0.15f;
    [TabGroup("Combat")] [SerializeField] private float attackRangeBonus = 0f;

    [TabGroup("Flags")] [SerializeField] private float flagPreviewAlpha = 0.35f;
    [TabGroup("Flags")] [SerializeField] private float flagPlacementBonus = 0f;

    private Vector2 _moveInput;
    private Vector3 _velocity;
    private FlagConfig _selectedFlag;
    private int _selectedFlagIndex = -1;
    private float _attackCooldown;
    private GameObject _flagPreviewInstance;

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
        UpdateAttackRangeIndicator();
        UpdateFlagPreview();
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
        Vector3 clamped = ClampPositionToAttackRange(worldPosition);
        flagController.PlaceFlag(flagConfig, clamped);
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
        if (Input.GetKeyDown(flagKey1)) ToggleFlagByIndex(0);
        if (Input.GetKeyDown(flagKey2)) ToggleFlagByIndex(1);
        if (Input.GetKeyDown(flagKey3)) ToggleFlagByIndex(2);
        if (Input.GetKeyDown(flagKey4)) ToggleFlagByIndex(3);
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
    private void ToggleFlagByIndex(int index)
    {
        if (_selectedFlagIndex == index)
        {
            DeselectFlag();
            return;
        }
        SelectFlagByIndex(index);
    }

    private void SelectFlagByIndex(int index)
    {
        FlagConfig config = flagController.GetEquippedFlag(index);
        if (config == null)
        {
            Debug.Log($"[Player] Flag slot {index + 1} empty");
            return;
        }
        _selectedFlag = config;
        _selectedFlagIndex = index;
        Debug.Log($"[Player] Selected flag {_selectedFlag.colorName}");
        EnsureFlagPreview();
        flagController?.SelectFlagInstance(config);
        UpdateFlagHighlights();
    }

    private void RemoveSelectedFlag()
    {
        if (_selectedFlag == null || flagController == null) return;
        flagController.RemoveFlag(_selectedFlag);
        DeselectFlag();
    }

    private void DeselectFlag()
    {
        _selectedFlag = null;
        _selectedFlagIndex = -1;
        DestroyFlagPreview();
        UpdateFlagHighlights();
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

    #region Range & Preview
    private void UpdateAttackRangeIndicator()
    {
        if (attackRangeIndicator == null || balanceConfig == null) return;
        if (!attackRangeIndicator.gameObject.activeSelf)
        {
            attackRangeIndicator.gameObject.SetActive(true);
        }
        float radius = balanceConfig.playerAttackRadius + attackRangeBonus;
        if (radius < 0f) radius = 0f;
        float diameter = radius * 2f;
        attackRangeIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
        Color c = attackRangeIndicator.color;
        c.a = rangeIndicatorAlpha;
        attackRangeIndicator.color = c;
    }

    private void UpdateFlagPreview()
    {
        if (_selectedFlag == null)
        {
            DestroyFlagPreview();
            return;
        }

        Vector3 targetPos = transform.position;
        if (TryGetCursorWorldPoint(out Vector3 worldHit))
        {
            targetPos = ClampPositionToAttackRange(worldHit);
        }
        else
        {
            targetPos = ClampPositionToAttackRange(targetPos);
        }

        EnsureFlagPreview();
        if (_flagPreviewInstance != null)
        {
            _flagPreviewInstance.transform.position = targetPos;
            ApplyPreviewAlpha(_flagPreviewInstance, flagPreviewAlpha);
        }

        float flagRadius = _selectedFlag.radius + flagPlacementBonus;
        if (flagRadius < 0f) flagRadius = 0f;
        UpdateFlagRangeIndicator(targetPos, flagRadius);
    }

    private void EnsureFlagPreview()
    {
        if (_selectedFlag == null) return;
        if (_flagPreviewInstance != null)
        {
            // If existing preview is for a different flag, rebuild.
            if (_flagPreviewInstance.name.Replace("(Clone)", "").Trim() != _selectedFlag.flagPrefab.name)
            {
                DestroyFlagPreview();
            }
            else
            {
                ApplyPreviewAlpha(_flagPreviewInstance, flagPreviewAlpha);
                return;
            }
        }
        if (_selectedFlag.flagPrefab == null)
        {
            Debug.LogWarning($"[Player] No flag prefab for preview: {_selectedFlag.colorName}");
            return;
        }
        _flagPreviewInstance = Object.Instantiate(_selectedFlag.flagPrefab, transform.position, Quaternion.identity);
        ApplyPreviewAlpha(_flagPreviewInstance, flagPreviewAlpha);
    }

    private void DestroyFlagPreview()
    {
        if (_flagPreviewInstance != null)
        {
            Destroy(_flagPreviewInstance);
            _flagPreviewInstance = null;
        }
        if (flagRangeIndicator != null)
        {
            flagRangeIndicator.gameObject.SetActive(false);
        }
    }

    private void UpdateFlagHighlights()
    {
        for (int i = 0; i < flagHighlights.Count; i++)
        {
            GameObject highlight = flagHighlights[i];
            if (highlight == null) continue;
            bool active = (_selectedFlagIndex == i);
            highlight.SetActive(active);
        }
    }

    private void ApplyPreviewAlpha(GameObject target, float alpha)
    {
        if (target == null) return;
        var renderers = target.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var r in renderers)
        {
            Color c = _selectedFlag != null ? _selectedFlag.color : r.color;
            c.a = alpha;
            r.color = c;
        }
    }

    private void UpdateFlagRangeIndicator(Vector3 position, float radius)
    {
        if (flagRangeIndicator == null) return;
        flagRangeIndicator.gameObject.SetActive(true);
        flagRangeIndicator.transform.position = position;
        float diameter = radius * 2f;
        flagRangeIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
        Color c = flagRangeIndicator.color;
        if (_selectedFlag != null)
        {
            c = _selectedFlag.color;
        }
        c.a = rangeIndicatorAlpha;
        flagRangeIndicator.color = c;
    }

    private bool TryGetCursorWorldPoint(out Vector3 point)
    {
        point = transform.position;
        if (viewCamera == null) return false;
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        int mask = groundMask.value == 0 ? Physics.DefaultRaycastLayers : groundMask.value;

        if (Physics.Raycast(ray, out RaycastHit hit, 200f, mask, QueryTriggerInteraction.Ignore))
        {
            point = hit.point;
            return true;
        }

        RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray, 200f, mask);
        if (hit2D.collider != null)
        {
            point = hit2D.point;
            return true;
        }
        return false;
    }

    public Vector3 ClampPositionToAttackRange(Vector3 position)
    {
        if (balanceConfig == null) return position;
        Vector2 center = new Vector2(transform.position.x, transform.position.y);
        Vector2 target = new Vector2(position.x, position.y);
        Vector2 delta = target - center;
        float radius = balanceConfig.playerAttackRadius + attackRangeBonus;
        if (radius < 0f) radius = 0f;
        if (delta.sqrMagnitude > radius * radius)
        {
            delta = delta.normalized * radius;
        }
        Vector2 clamped = center + delta;
        return new Vector3(clamped.x, clamped.y, transform.position.z);
    }
    #endregion

    private void OnDisable()
    {
        DestroyFlagPreview();
    }
}
