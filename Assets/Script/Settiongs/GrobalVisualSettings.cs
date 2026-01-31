using UnityEngine; // ïKê{ [cite: 2026-01-24]

[CreateAssetMenu(fileName = "GlobalVisualSettings", menuName = "Settings/GlobalVisualSettings")]
public class GlobalVisualSettings : ScriptableObject
{
    [Header("Damage Flash Settings")]
    public float flashDuration = 0.1f;
    public Color defaultFlashColor = Color.red;
}