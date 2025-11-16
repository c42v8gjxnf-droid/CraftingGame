using UnityEngine;

public class TabHotkeys : MonoBehaviour
{
    public TabView tabView;
    [Header("Keys")]
    public KeyCode nextKey = KeyCode.E;
    public KeyCode prevKey = KeyCode.Q;

    void Reset() { tabView = GetComponent<TabView>(); }

    void Update()
    {
        if (!tabView) return;
        if (!Application.isFocused) return;

        if (Input.GetKeyDown(nextKey)) Step(+1);
        if (Input.GetKeyDown(prevKey)) Step(-1);
    }

    void Step(int dir)
    {
        int count = tabView.pages.Count;
        if (count == 0) return;
        int cur = tabView.CurrentIndex < 0 ? 0 : tabView.CurrentIndex;
        int nxt = Mathf.Clamp(cur + dir, 0, count - 1);
        if (nxt != cur) tabView.Select(nxt);
    }
}
