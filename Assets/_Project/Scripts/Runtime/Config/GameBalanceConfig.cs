// Purpose: Holds global tuning values for tribe progression (SRP: balance data container)
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "GameBalanceConfig", menuName = "Configs/GameBalanceConfig")]
public sealed class GameBalanceConfig : ScriptableObject
{
    [TabGroup("Start")] [Title("Start Values")]
    [MinValue(1)] public int startPopulation = 5;
    [TabGroup("Start")] [MinValue(0)] public int startFood = 20;
    [TabGroup("Start")] [MinValue(0)] public int startStone = 10;
    [TabGroup("Start")] [MinValue(0)] public int startWood = 10;

    [TabGroup("Player")] [Title("Player")]
    [TabGroup("Player")] [MinValue(1)] public int playerStartHealth = 100;
    [TabGroup("Player")] [MinValue(1)] public int playerMaxHealth = 100;
    [TabGroup("Player")] [MinValue(1f)] public float playerMoveSpeed = 5f;
    [TabGroup("Player")] [MinValue(0f)] public float playerAttackDamage = 10f;
    [TabGroup("Player")] [MinValue(0.1f)] public float playerAttackInterval = 1f;
    [TabGroup("Player")] [MinValue(0.1f)] public float playerAttackRadius = 2.5f;
    [TabGroup("Player")] [MinValue(1f)] public float playerMaxHunger = 100f;

    [TabGroup("Hunger")] [Title("Hunger")]
    [MinValue(0f)] public float hungerDecayPerSecond = 0.5f;
    [MinValue(0f)] public float hungerDamagePerSecond = 2f;
    [MinValue(0f)] public float hungerRevivePercent = 0.3f;

    [TabGroup("Gathering")] [Title("Gathering")]
    [TabGroup("Gathering")] [MinValue(0f)] public float gatherIntervalSeconds = 2f;
    [TabGroup("Gathering")] [MinValue(1)] public int woodPerTick = 2;
    [TabGroup("Gathering")] [MinValue(1)] public int stonePerTick = 1;

    [TabGroup("Combat")] [Title("Combat")]
    [TabGroup("Combat")] [MinValue(0f)] public float baseDamage = 5f;
    [TabGroup("Combat")] [MinValue(0f)] public float playerBuffMultiplier = 1.2f;
}
