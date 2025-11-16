using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[DefaultExecutionOrder(50)]
public class PanelToggle : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("Sliding container (Wrapper/Dock, z.B. InventoryDock / CraftingDock).")]
    public RectTransform panel;

    [Tooltip("Button in der BottomBar, der angezeigt wird, wenn das Panel versteckt ist.")]
    public GameObject openButton;

    [Tooltip("Close-Button im Panel (optional).")]
    public Button closeButton;

    [Tooltip("CanvasGroup am Dock (optional) für Raycast/Interactable Umschaltung).")]
    public CanvasGroup canvasGroup;

    [Header("Mutual Exclusion")]
    [Tooltip("Wenn true, schließt dieses Panel beim Show() alle anderen registrierten Panels.")]
    public bool closeOthersOnShow = false;

    [Tooltip("Wenn true, versteckt dieses Panel beim Hide() auch alle anderen Panels.")]
    public bool closeOthersOnHide = false;

    public event System.Action<PanelToggle> OnShown;
    public event System.Action<PanelToggle> OnHidden;

    [Header("Persistence")]
    public bool persistState = false;
    public UIPrefKey prefsKeyAsset;

    string PrefsKey => (persistState && prefsKeyAsset) ? prefsKeyAsset.ResolveKey() : null;

    [Header("Animation")]
    [Min(0f)] public float slideDuration = 0.20f;
    [Min(0f)] public float bottomPadding = 16f; // wird nur bei Auto genutzt
    public bool startHidden = false;

    [Header("Slide Distance Override")]
    [Tooltip("Wenn an, wird 'customSlideDistance' benutzt statt Auto-Höhe.")]
    public bool useCustomSlideDistance = false;

    [Tooltip("Pixel nach UNTEN, die das Panel zum Verstecken bewegt wird.")]
    [Min(0f)] public float customSlideDistance = 360f;

    public bool IsVisible { get; private set; }

    Vector2 shownPos;
    Vector2 hiddenPos;
    Coroutine slideCo;
    bool initialized;

    void Awake()
    {
        if (!panel) panel = GetComponent<RectTransform>();
        if (!canvasGroup && panel) canvasGroup = panel.GetComponent<CanvasGroup>();
        if (closeButton) closeButton.onClick.AddListener(Hide);
    }

    void Start()
    {
        RecalculatePositions();
        var key = PrefsKey;
        if (!string.IsNullOrEmpty(key) && PlayerPrefs.HasKey(key))
        {
            bool wantVisible = PlayerPrefs.GetInt(key, 0) == 1;
            if (wantVisible) SetShownInstant();
            else SetHiddenInstant();
        }
        else
        {
            if (startHidden) SetHiddenInstant();
            else SetShownInstant();
        }
    }

    /// <summary>
    /// Wenn du im UI rumschiebst (Größen/Position/Pivots), ruf das auf, um die Slide-Positionen neu zu bestimmen.
    /// </summary>
    public void RecalculatePositions()
    {
        if (!panel) return;
        Canvas.ForceUpdateCanvases();

        // Sichtbare Position ist die aktuelle Position
        shownPos = panel.anchoredPosition;

        float slideDown;
        if (useCustomSlideDistance)
        {
            slideDown = customSlideDistance;
        }
        else
        {
            // Auto: komplette Höhe + Padding nach unten
            float h = panel.rect.height;
            slideDown = h + bottomPadding;
        }

        hiddenPos = shownPos + new Vector2(0f, -slideDown);
        initialized = true;
    }

    void SetHiddenInstant()
    {
        EnsureSetup();
        IsVisible = false;
        panel.anchoredPosition = hiddenPos;
        if (openButton) openButton.SetActive(true);
        if (canvasGroup) { canvasGroup.interactable = false; canvasGroup.blocksRaycasts = false; }
        SavePersisted(false);
    }

    void SetShownInstant()
    {
        EnsureSetup();
        IsVisible = true;
        panel.anchoredPosition = shownPos;
        if (openButton) openButton.SetActive(false);
        if (canvasGroup) { canvasGroup.interactable = true; canvasGroup.blocksRaycasts = true; }
        SavePersisted(true);
    }

    public void Show()
    {
        EnsureSetup();
        if (IsVisible) return;
        IsVisible = true;
        if (openButton) openButton.SetActive(false);
        SlideTo(shownPos, enableRaycasts: true, onEnd: () =>
        {
            OnShown?.Invoke(this);
        });
        SavePersisted(true);
    }

    public void Hide()
    {
        EnsureSetup();
        if (!IsVisible) return;
        IsVisible = false;
        SlideTo(hiddenPos, enableRaycasts: false, onEnd: () =>
        {
            if (openButton) openButton.SetActive(true);
            OnHidden?.Invoke(this);
        });
        SavePersisted(false);
    }

    public void Toggle() { if (IsVisible) Hide(); else Show(); }

    void EnsureSetup()
    {
        if (!initialized) RecalculatePositions();
    }

    void SlideTo(Vector2 target, bool enableRaycasts, System.Action onEnd = null)
    {
        if (slideCo != null) StopCoroutine(slideCo);
        slideCo = StartCoroutine(CoSlide(panel.anchoredPosition, target, enableRaycasts, onEnd));
    }

    IEnumerator CoSlide(Vector2 from, Vector2 to, bool enableRaycasts, System.Action onEnd)
    {
        if (canvasGroup && !enableRaycasts)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        float t = 0f, dur = Mathf.Max(0.0001f, slideDuration);
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float s = Mathf.SmoothStep(0f, 1f, t / dur);
            panel.anchoredPosition = Vector2.LerpUnclamped(from, to, s);
            yield return null;
        }
        panel.anchoredPosition = to;

        if (canvasGroup && enableRaycasts)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        onEnd?.Invoke();
    }

    void SavePersisted(bool visible)
    {
        var key = PrefsKey;
        if (string.IsNullOrEmpty(key)) return;
        PlayerPrefs.SetInt(key, visible ? 1 : 0);
    }
}
