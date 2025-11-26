// Purpose: Defines neutral/hostile creature data (SRP: creature tuning)
using UnityEngine;
using Sirenix.OdinInspector;

public enum CreatureType
{
    MeatBeastA,
    MeatBeastB,
    EnemyRaider
}

[CreateAssetMenu(fileName = "CreatureConfig", menuName = "Configs/CreatureConfig")]
public sealed class CreatureConfig : ScriptableObject
{
    [TabGroup("Identity")] [Title("Identity")]
    [TabGroup("Identity")] public CreatureType type = CreatureType.MeatBeastA;

    [TabGroup("Stats")] [Title("Stats")]
    [TabGroup("Stats")] [MinValue(1)] public int maxHealth = 50;
    [TabGroup("Stats")] [MinValue(0f)] public float moveSpeed = 3f;
    [TabGroup("Stats")] [MinValue(0f)] public float attackDamage = 8f;
    [TabGroup("Stats")] [MinValue(0f)] public float attackInterval = 1.5f;

    [TabGroup("Loot")] [Title("Loot")]
    [TabGroup("Loot")] [MinValue(0)] public int meatYield = 3;
}
