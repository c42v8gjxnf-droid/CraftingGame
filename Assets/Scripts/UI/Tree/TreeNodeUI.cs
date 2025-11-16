using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TreeNodeUI : MonoBehaviour
{
    [Header("Refs")]
    public Image icon;
    public Image frame;
    public Button button;
    public TextMeshProUGUI label;
    public TextMeshProUGUI costText;

    [HideInInspector] public SkillDefinition def;
    [HideInInspector] public TreePanelController controller;

    public void Bind(SkillDefinition d, TreePanelController c)
    {
        def = d; controller = c;
        if (icon) icon.sprite = d.icon;
        if (label) label.text = d.displayName;
        if (costText) costText.text = d.cost > 0 ? d.cost.ToString() : "";
        if (button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    void OnClick()
    {
        if (controller) controller.ToggleQueue(def);
    }
}
