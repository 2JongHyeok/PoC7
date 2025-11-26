// Purpose: Defines properties of a controllable flag (SRP: flag data)
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "FlagConfig", menuName = "Configs/FlagConfig")]
public sealed class FlagConfig : ScriptableObject
{
    [TabGroup("Identity")] [Title("Identity")]
    [TabGroup("Identity")] public string colorName = "Red";
    [TabGroup("Identity")] public Color color = Color.red;
    [TabGroup("Identity")] public GameObject flagPrefab;

    [TabGroup("Control")] [Title("Control")]
    [TabGroup("Control")] [MinValue(1f)] public float radius = 10f;
    [TabGroup("Control")] [MinValue(0f)] public float rallyOffset = 0f;
}
