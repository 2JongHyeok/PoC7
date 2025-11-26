// Purpose: Defines unit role stats and hooks (SRP: unit data)
using UnityEngine;
using Sirenix.OdinInspector;

public enum UnitRole
{
    Gatherer,
    Shield,
    Archer,
    Swordsman,
    Cavalry
}

[CreateAssetMenu(fileName = "UnitConfig", menuName = "Configs/UnitConfig")]
public sealed class UnitConfig : ScriptableObject
{
    [TabGroup("Identity")] [Title("Identity")]
    [TabGroup("Identity")] public UnitRole role = UnitRole.Gatherer;

    [TabGroup("Stats")] [Title("Stats")]
    [TabGroup("Stats")] [MinValue(1)] public int maxHealth = 100;
    [TabGroup("Stats")] [MinValue(0f)] public float moveSpeed = 3.5f;
    [TabGroup("Stats")] [MinValue(0f)] public float attackDamage = 10f;
    [TabGroup("Stats")] [MinValue(0f)] public float attackInterval = 1.2f;

    [TabGroup("Gathering")] [Title("Gathering")]
    [TabGroup("Gathering")] public bool canGather = false;
    [TabGroup("Gathering")] [ShowIf("canGather")]
    [TabGroup("Gathering")] [MinValue(0f)] public float gatherSpeedMultiplier = 1f;
}
