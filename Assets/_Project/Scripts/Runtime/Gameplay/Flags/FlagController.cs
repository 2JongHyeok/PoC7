// Purpose: Manages equipped flags and placement/removal events (SRP: flag management)
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public sealed class FlagController : MonoBehaviour
{
    [TabGroup("Setup")] [SerializeField] private List<FlagConfig> equippedFlags = new List<FlagConfig>();
    [TabGroup("Setup")] [SerializeField] private int maxEquipped = 4;
    [TabGroup("Setup")] [SerializeField] private Transform flagContainer;
    [TabGroup("Setup")] [SerializeField] private SpriteRenderer rangeIndicatorPrefab;
    [TabGroup("Setup")] [SerializeField] private float rangeIndicatorAlpha = 0.15f;

    public event System.Action<FlagConfig, Vector3> OnFlagPlaced;
    public event System.Action<FlagConfig> OnFlagRemoved;
    public event System.Action<FlagConfig, Vector3> OnFlagSelected;

    private readonly Dictionary<FlagConfig, GameObject> _placedFlags = new Dictionary<FlagConfig, GameObject>();
    private readonly Dictionary<FlagConfig, SpriteRenderer> _rangeIndicators = new Dictionary<FlagConfig, SpriteRenderer>();

    public bool EquipFlag(FlagConfig config)
    {
        if (config == null || equippedFlags.Contains(config)) return false;
        if (equippedFlags.Count >= maxEquipped)
        {
            Debug.LogWarning("[Flag] Equip failed: slots full");
            return false;
        }
        equippedFlags.Add(config);
        Debug.Log($"[Flag] Equipped {config.colorName}");
        return true;
    }

    [Button("Unequip All")]
    private void UnequipAll()
    {
        equippedFlags.Clear();
        Debug.Log("[Flag] All flags unequipped");
    }

    public bool UnequipFlag(FlagConfig config)
    {
        if (config == null) return false;
        bool removed = equippedFlags.Remove(config);
        if (removed) Debug.Log($"[Flag] Unequipped {config.colorName}");
        return removed;
    }

    public void PlaceFlag(FlagConfig config, Vector3 position)
    {
        if (config == null) return;
        if (!equippedFlags.Contains(config))
        {
            Debug.LogWarning("[Flag] Place failed: flag not equipped");
            return;
        }

        if (_placedFlags.TryGetValue(config, out GameObject existing))
        {
            existing.transform.position = position;
            Debug.Log($"[Flag] Moved {config.colorName} to {position}");
        }
        else
        {
            if (config.flagPrefab == null)
            {
                Debug.LogWarning($"[Flag] No prefab for {config.colorName}");
            }
            else
            {
                GameObject instance = Object.Instantiate(config.flagPrefab, position, Quaternion.identity, flagContainer);
                _placedFlags[config] = instance;
                Debug.Log($"[Flag] Spawned {config.colorName} prefab at {position}");
            }
        }

        OnFlagPlaced?.Invoke(config, position);
        Debug.Log($"[Flag] Placed {config.colorName} at {position}");
    }

    public void RemoveFlag(FlagConfig config)
    {
        if (config == null) return;
        if (_placedFlags.TryGetValue(config, out GameObject instance))
        {
            Object.Destroy(instance);
            _placedFlags.Remove(config);
            Debug.Log($"[Flag] Destroyed {config.colorName} instance");
        }
        if (_rangeIndicators.TryGetValue(config, out var indicator))
        {
            Object.Destroy(indicator.gameObject);
            _rangeIndicators.Remove(config);
        }
        OnFlagRemoved?.Invoke(config);
        Debug.Log($"[Flag] Removed {config.colorName}");
    }

    public FlagConfig GetEquippedFlag(int index)
    {
        if (index < 0 || index >= equippedFlags.Count) return null;
        return equippedFlags[index];
    }

    public IReadOnlyList<FlagConfig> EquippedFlags => equippedFlags;

    private void CreateRangeIndicator(FlagConfig config, Vector3 position)
    {
        if (rangeIndicatorPrefab == null || config == null) return;
        SpriteRenderer indicator = Object.Instantiate(rangeIndicatorPrefab, position, Quaternion.identity, flagContainer);
        _rangeIndicators[config] = indicator;
        UpdateRangeIndicator(config, position);
    }

    private void UpdateRangeIndicator(FlagConfig config, Vector3 position)
    {
        if (config == null) return;
        if (!_rangeIndicators.TryGetValue(config, out var indicator))
        {
            CreateRangeIndicator(config, position);
            return;
        }

        indicator.transform.position = position;
        float diameter = config.radius * 2f;
        indicator.transform.localScale = new Vector3(diameter, diameter, 1f);

        Color c = config.color;
        c.a = rangeIndicatorAlpha;
        indicator.color = c;
        indicator.gameObject.SetActive(true);
    }

    public void SelectFlagInstance(FlagConfig config)
    {
        HideAllIndicators();

        if (config == null) return;
        if (!_placedFlags.TryGetValue(config, out GameObject instance))
        {
            Debug.LogWarning("[Flag] Select failed: no placed instance");
            return;
        }
        if (!_rangeIndicators.ContainsKey(config))
        {
            CreateRangeIndicator(config, instance.transform.position);
        }
        else
        {
            UpdateRangeIndicator(config, instance.transform.position);
        }
        HideOtherIndicators(config);
        OnFlagSelected?.Invoke(config, instance.transform.position);
    }

    public void DeselectAllFlags()
    {
        HideAllIndicators();
    }

    private void HideOtherIndicators(FlagConfig except)
    {
        foreach (var kv in _rangeIndicators)
        {
            if (kv.Key == except) continue;
            if (kv.Value != null)
            {
                kv.Value.gameObject.SetActive(false);
            }
        }
    }

    private void HideAllIndicators()
    {
        foreach (var kv in _rangeIndicators)
        {
            if (kv.Value != null)
            {
                kv.Value.gameObject.SetActive(false);
            }
        }
    }
}
