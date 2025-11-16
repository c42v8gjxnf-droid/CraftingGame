using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Coordinates visibility for panels that should close others when they open/close.
/// </summary>
public class PanelCoordinator : MonoBehaviour
{
    [Tooltip("Wenn leer, werden automatisch alle PanelToggle in den Children gefunden.")]
    public List<PanelToggle> panels = new List<PanelToggle>();

    void OnEnable()
    {
        AutoCollectIfNeeded();
        Subscribe(true);
    }

    void OnDisable()
    {
        Subscribe(false);
    }

    void AutoCollectIfNeeded()
    {
        if (panels == null || panels.Count == 0)
        {
            panels = new List<PanelToggle>(GetComponentsInChildren<PanelToggle>(true));
        }
    }

    void Subscribe(bool on)
    {
        if (panels == null) return;
        foreach (var p in panels)
        {
            if (!p) continue;
            if (on)
            {
                p.OnShown += HandleShown;
                p.OnHidden += HandleHidden;
            }
            else
            {
                p.OnShown -= HandleShown;
                p.OnHidden -= HandleHidden;
            }
        }
    }

    void HandleShown(PanelToggle shown)
    {
        if (shown == null || !shown.closeOthersOnShow) return;

        foreach (var other in panels)
        {
            if (!other || other == shown) continue;
            if (other.IsVisible)
                other.Hide();
        }
    }

    void HandleHidden(PanelToggle hidden)
    {
        if (hidden == null || !hidden.closeOthersOnHide) return;

        foreach (var other in panels)
        {
            if (!other || other == hidden) continue;
            if (other.IsVisible)
                other.Hide();
        }
    }
}
