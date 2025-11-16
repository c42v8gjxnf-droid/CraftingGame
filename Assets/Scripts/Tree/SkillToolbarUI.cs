using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillToolbarUI : MonoBehaviour
{
    [Header("Wiring")]
    public TreePanelController panel; // irgendein Controller aus dem aktiven Tab
    public TreeState state;

    [Header("UI")]
    public TextMeshProUGUI pointsLabel;
    public TextMeshProUGUI queuedLabel;
    public Button confirmButton;
    public Button clearButton;
    public Button respecButton;

    void OnEnable()
    {
        if (!state && panel) state = panel.state;
        Hook(true);
        Refresh();
    }

    void OnDisable() => Hook(false);

    void Hook(bool on)
    {
        if (!state) return;
        if (on)
        {
            state.OnChanged += Refresh;
            state.OnQueueChanged += Refresh;
        }
        else
        {
            state.OnChanged -= Refresh;
            state.OnQueueChanged -= Refresh;
        }

        if (confirmButton)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(Confirm);
        }
        if (clearButton)
        {
            clearButton.onClick.RemoveAllListeners();
            clearButton.onClick.AddListener(Clear);
        }
        if (respecButton)
        {
            respecButton.onClick.RemoveAllListeners();
            respecButton.onClick.AddListener(Respec);
        }
    }

    void Refresh()
    {
        if (!state) return;

        int QueuedCostLookup(string id) => panel ? panel.CostOf(id) : 0;

        int queuedCost = state.GetQueuedTotalCost(QueuedCostLookup);
        int available  = state.GetAvailablePoints(QueuedCostLookup);

        if (pointsLabel) pointsLabel.text = $"Points: {available}";
        if (queuedLabel) queuedLabel.text = queuedCost > 0 ? $"Queued: -{queuedCost}" : "Queued: 0";

        if (confirmButton) confirmButton.interactable = queuedCost > 0 && available + queuedCost >= queuedCost;
        if (clearButton)   clearButton.interactable   = state.Queued.Count > 0;
        if (respecButton)  respecButton.interactable  = state.Unlocked.Count > 0 || state.Queued.Count > 0;
    }

    void Confirm()
    {
        if (!state) return;
        state.ApplyQueue(id => panel ? panel.CostOf(id) : 0);
        panel?.RefreshAll();
        var bj = confirmButton ? confirmButton.GetComponent<ButtonJuice>() : null;
        if (bj) bj.PulseSuccess();
    }

    void Clear()
    {
        if (!state) return;
        state.ClearQueue();
        panel?.RefreshAll();
    }

    void Respec()
    {
        if (!state) return;
        state.Respec(id => panel ? panel.CostOf(id) : 0);
        panel?.RefreshAll();
    }
}
