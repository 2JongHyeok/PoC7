// Purpose: Defines a resource node type (SRP: resource data)
using UnityEngine;
using Sirenix.OdinInspector;

public enum ResourceType
{
    Wood,
    Stone
}

[CreateAssetMenu(fileName = "ResourceNodeConfig", menuName = "Configs/ResourceNodeConfig")]
public sealed class ResourceNodeConfig : ScriptableObject
{
    [TabGroup("Type")] [Title("Type")]
    [TabGroup("Type")] public ResourceType type = ResourceType.Wood;

    [TabGroup("Harvest")] [Title("Harvest")]
    [TabGroup("Harvest")] [MinValue(1)] public int yieldPerCycle = 2;
    [TabGroup("Harvest")] [MinValue(0f)] public float respawnTimeSeconds = 30f;
}
