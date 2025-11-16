using UnityEngine;
using UnityEngine.UI;

public static class UILineUtil
{
    public static Image DrawLine(RectTransform parent, Vector2 a, Vector2 b, float thickness, Color color, string name="Edge")
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        var rt = go.GetComponent<RectTransform>();
        var img = go.GetComponent<Image>();
        img.color = color;
        rt.SetParent(parent, false);
        rt.SetAsFirstSibling(); // keep edges behind dynamically spawned nodes

        Vector2 dir = (b - a);
        float len = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        rt.sizeDelta = new Vector2(len, thickness);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); // gleiche Referenz wie Nodes im Tree
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = a;
        rt.localRotation = Quaternion.Euler(0,0,angle);

        return img;
    }
}
