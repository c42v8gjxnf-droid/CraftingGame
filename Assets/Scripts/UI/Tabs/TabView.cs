using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class TabView : MonoBehaviour
{
    [Header("Wiring")]
    public List<Button> tabButtons = new();       // Reihenfolge = Tab-Reihenfolge
    public List<GameObject> pages = new();        // gleicher Index wie Buttons

    [Header("Animation")]
    [Min(0f)] public float fadeTime = 0.12f;
    [Min(0f)] public float slidePx = 24f;         // 0 = kein Slide
    public bool startSelectFirst = true;

    [Header("Persistence")]
    [Tooltip("Wenn aktiv, merkt sich dieses TabView den zuletzt ausgewählten Tab.")]
    public bool persistState = false;

    [Tooltip("ScriptableObject mit stabilem PlayerPrefs-Key.")]
    public UIPrefKey prefsKeyAsset;

    string PrefsKey => (persistState && prefsKeyAsset) ? prefsKeyAsset.ResolveKey() : null;

    [Header("State (read-only)")]
    [SerializeField] private int currentIndex = -1; // shown in Inspector
    public int CurrentIndex { get => currentIndex; private set => currentIndex = value; }

    Coroutine animCo;

    void OnEnable()
    {
        // Buttons mit Handlern verdrahten
        for (int i = 0; i < tabButtons.Count; i++)
        {
            int idx = i;
            if (tabButtons[i] == null) continue;
            tabButtons[i].onClick.RemoveAllListeners();
            tabButtons[i].onClick.AddListener(() => Select(idx));
        }

        int target = 0;
        var key = PrefsKey;
        if (!string.IsNullOrEmpty(key) && PlayerPrefs.HasKey(key))
            target = Mathf.Clamp(PlayerPrefs.GetInt(key, 0), 0, pages.Count - 1);
        else if (!startSelectFirst && CurrentIndex >= 0)
            target = Mathf.Clamp(CurrentIndex, 0, pages.Count - 1);

        if (pages.Count > 0)
            Select(target);
    }

    public void Select(int index)
    {
        if (index < 0 || index >= pages.Count) return;
        if (index == CurrentIndex && pages[index].activeSelf) return;

        int prev = CurrentIndex;
        CurrentIndex = index;

        // Buttons visual state (optional: Interactable off für aktive)
        for (int i = 0; i < tabButtons.Count; i++)
            if (tabButtons[i]) tabButtons[i].interactable = (i != index);

        // Seiten togglen + animieren
        if (animCo != null) StopCoroutine(animCo);
        animCo = StartCoroutine(CoSwitch(prev, index));

        SavePersisted(index);
    }

    void SavePersisted(int index)
    {
        var key = PrefsKey;
        if (string.IsNullOrEmpty(key)) return;
        PlayerPrefs.SetInt(key, index);
    }

    IEnumerator CoSwitch(int prev, int cur)
    {
        GameObject prevPage = (prev >= 0 && prev < pages.Count) ? pages[prev] : null;
        GameObject curPage  = pages[cur];

        // sicherstellen, dass beide CanvasGroup haben
        var cgPrev = prevPage ? GetOrAddCanvasGroup(prevPage) : null;
        var cgCur  = GetOrAddCanvasGroup(curPage);

        // Vorbereiten
        if (prevPage) cgPrev.interactable = false;
        curPage.SetActive(true);
        cgCur.alpha = 0f;
        cgCur.interactable = false;
        cgCur.blocksRaycasts = false;

        // optionale Slide-Offsets
        Vector2 curBasePos = Vector2.zero;
        Vector2 prevBasePos = Vector2.zero;
        RectTransform rtPrev = prevPage ? prevPage.GetComponent<RectTransform>() : null;
        RectTransform rtCur  = curPage.GetComponent<RectTransform>();

        if (rtCur) curBasePos = rtCur.anchoredPosition;
        if (rtPrev) prevBasePos = rtPrev.anchoredPosition;

        float t = 0f;
        float dur = Mathf.Max(0.0001f, fadeTime);
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / dur);
            float s = Mathf.SmoothStep(0, 1, u);

            if (cgPrev) cgPrev.alpha = 1f - s;
            if (cgCur)  cgCur.alpha  = s;

            if (slidePx > 0f)
            {
                if (rtPrev) rtPrev.anchoredPosition = prevBasePos + new Vector2(-slidePx * s, 0);
                if (rtCur)  rtCur.anchoredPosition  = curBasePos  + new Vector2(+slidePx * (1f - s), 0);
            }
            yield return null;
        }

        // finalisieren
        if (cgPrev)
        {
            cgPrev.alpha = 0f;
            cgPrev.blocksRaycasts = false;
            if (prevPage) prevPage.SetActive(false);
            if (rtPrev) rtPrev.anchoredPosition = prevBasePos;
        }
        cgCur.alpha = 1f;
        cgCur.blocksRaycasts = true;
        cgCur.interactable = true;
        if (rtCur) rtCur.anchoredPosition = curBasePos;

        // Optional: Wenn Seite einen TreePanelController enthält -> sicherstellen, dass er gebaut wurde
        var tree = curPage.GetComponentInChildren<TreePanelController>(true);
        if (tree) tree.EnsureBuilt();

        animCo = null;
    }

    CanvasGroup GetOrAddCanvasGroup(GameObject go)
    {
        var cg = go.GetComponent<CanvasGroup>();
        if (!cg) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }
}
