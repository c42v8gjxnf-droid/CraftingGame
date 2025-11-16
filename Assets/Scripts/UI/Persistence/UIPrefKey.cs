using UnityEngine;

[CreateAssetMenu(menuName = "UI/Persistence/Pref Key", fileName = "UIPrefKey_")]
public class UIPrefKey : ScriptableObject
{
    [Tooltip("Nur fürs Auffinden im Projekt/Inspector. Hat keinen Einfluss auf die Stabilität.")]
    public string displayName = "InventoryDock";

    [SerializeField, HideInInspector] string stableId;

    [Header("Key Format")]
    public bool prefixWithProductName = true;
    public string namespacePrefix = "UI.";
    public string optionalSuffix = "";

    void OnValidate()
    {
        if (string.IsNullOrEmpty(stableId))
            stableId = System.Guid.NewGuid().ToString("N");
    }

    public string ResolveKey()
    {
        string key = "";
        if (prefixWithProductName) key += Application.productName + ".";
        if (!string.IsNullOrEmpty(namespacePrefix)) key += namespacePrefix;
        key += stableId;
        if (!string.IsNullOrEmpty(optionalSuffix)) key += optionalSuffix;
        return key;
    }

    [ContextMenu("Log Resolved Key")]
    void LogKey() => Debug.Log($"[UIPrefKey] {name} -> {ResolveKey()}", this);
}
