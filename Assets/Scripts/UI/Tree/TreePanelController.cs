using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TreePanelController : MonoBehaviour
{
    [Header("Data")]
    public List<SkillDefinition> nodes = new(); // alle Knoten dieses Baums
    public TreeState state;

    [Header("UI")]
    public RectTransform content;   // ScrollRect Content
    public GameObject nodePrefab;   // SkillNode.prefab
    public float edgeThickness = 4f;
    public Color edgeLocked = new Color(1,1,1,0.15f);
    public Color edgeUnlocked = new Color(0.6f,1f,0.6f,0.6f);

    [Header("Styles")]
    public Color nodeLocked = new Color(1,1,1,0.25f);
    public Color nodeAvailable = Color.white;
    public Color nodeQueued = new Color(1f,0.9f,0.5f);   // gold
    public Color nodeUnlocked = new Color(0.6f,1f,0.6f); // green

    [Header("Zoom")]
    public float minZoom = 0.6f;
    public float maxZoom = 1.6f;
    public float zoomStep = 0.1f;

    readonly Dictionary<string, TreeNodeUI> uiById = new();
    readonly List<Image> edgeImages = new();
    bool builtOnce = false;

    void OnEnable()
    {
        if (state != null)
        {
            state.OnChanged += RefreshAll;
            state.OnQueueChanged += RefreshAll;
        }
        EnsureBuilt();
    }

    void OnDisable()
    {
        if (state != null)
        {
            state.OnChanged -= RefreshAll;
            state.OnQueueChanged -= RefreshAll;
        }
        builtOnce = false; // allow rebuild after domain reload or re-enable
    }

    public void EnsureBuilt()
    {
        if (!builtOnce) { Build(); builtOnce = true; }
        else            { RefreshAll(); }
    }

    public bool ArePrereqsMet(SkillDefinition def, bool considerQueued = false)
    {
        if (def == null || def.prerequisites == null) return true;
        foreach (var pre in def.prerequisites)
        {
            if (!pre) continue;
            bool ok = state != null && (state.IsUnlocked(pre.id) || (considerQueued && state.IsQueued(pre.id)));
            if (!ok) return false;
        }

        return true;
    }

    public int CostOf(string skillId)
    {
        var d = GetById(skillId);
        return d ? Mathf.Max(0, d.cost) : 0;
    }

    bool CanQueue(SkillDefinition def)
    {
        if (def == null || state == null) return false;
        if (state.IsUnlocked(def.id) || state.IsQueued(def.id)) return false;
        if (!ArePrereqsMet(def, considerQueued:true)) return false;
        return state.GetAvailablePoints(CostOf) >= def.cost;
    }

    public void ToggleQueue(SkillDefinition def)
    {
        if (state == null || def == null) return;
        if (state.IsQueued(def.id)) state.Unqueue(def.id);
        else if (CanQueue(def)) state.QueueUnlock(def.id);
        RefreshAll();
    }

    public void ConfirmQueue()
    {
        if (state == null) return;
        state.ApplyQueue(CostOf);
        RefreshAll();
    }

    public void Build()
    {
        if (!content || !nodePrefab) { Debug.LogError("[TreePanel] Missing refs", this); return; }

        // cleanup
        for (int i = content.childCount - 1; i >= 0; i--) Destroy(content.GetChild(i).gameObject);
        edgeImages.Clear();
        uiById.Clear();

        // nodes
        foreach (var def in nodes)
        {
            if (!def) continue;
            var go = Instantiate(nodePrefab, content);
            go.name = def.displayName;
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = def.canvasPosition;

            var ui = go.GetComponent<TreeNodeUI>();
            if (!ui) ui = go.AddComponent<TreeNodeUI>();

            ui.Bind(def, this);
            uiById[def.id] = ui;
        }

        // edges
        foreach (var def in nodes)
        {
            if (!def || def.prerequisites == null) continue;
            var to = def.canvasPosition;
            foreach (var pre in def.prerequisites)
            {
                if (!pre) continue;
                var from = pre.canvasPosition;
                var img = UILineUtil.DrawLine(content, from, to, edgeThickness, edgeLocked, $"Edge_{pre.id}_{def.id}");
                edgeImages.Add(img);
            }
        }

        RefreshAll();
    }

    public void RefreshAll()
    {
        if (state == null) return;

        // Update node visuals
        foreach (var def in nodes)
        {
            if (!def || !uiById.TryGetValue(def.id, out var ui)) continue;

            bool unlocked = state.IsUnlocked(def.id);
            bool queued   = state.IsQueued(def.id);
            bool avail    = !unlocked && !queued && CanQueue(def);

            if (ui.button) ui.button.interactable = (avail || queued); // queued erlauben, um wieder zu entfernen

            var col = unlocked ? nodeUnlocked : queued ? nodeQueued : avail ? nodeAvailable : nodeLocked;
            if (ui.icon) ui.icon.color = col;
            if (ui.frame) ui.frame.color = col;

            if (ui.costText) ui.costText.gameObject.SetActive(!unlocked && def.cost > 0);
        }

        // Update edges: turn on if both nodes satisfied (unlocked or queued)
        foreach (var img in edgeImages)
        {
            if (!img) continue;
            // parse ids aus Name (Edge_pre_to)
            var n = img.name;
            int a = n.IndexOf('_');
            int b = n.LastIndexOf('_');
            if (a < 0 || b <= a) { img.color = edgeLocked; continue; }
            var idA = n.Substring(a+1, b-a-1);
            var idB = n.Substring(b+1);

            bool on = (state.IsUnlocked(idA) || state.IsQueued(idA)) &&
                      (state.IsUnlocked(idB) || ArePrereqsMet(GetById(idB), considerQueued:true));
            img.color = on ? edgeUnlocked : edgeLocked;
        }
    }

    SkillDefinition GetById(string id)
    {
        foreach (var d in nodes) if (d && d.id == id) return d;
        return null;
    }

    // Simple zoom via buttons or mouse wheel (call from UI or EventTrigger)
    public void ZoomIn()  => SetZoom( GetZoom() + zoomStep );
    public void ZoomOut() => SetZoom( GetZoom() - zoomStep );

    float GetZoom() => content ? content.localScale.x : 1f;

    public void SetZoom(float z)
    {
        if (!content) return;
        z = Mathf.Clamp(z, minZoom, maxZoom);
        content.localScale = new Vector3(z,z,1f);
    }
}
