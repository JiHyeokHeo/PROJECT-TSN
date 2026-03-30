using TMPro;
using UnityEngine;

namespace TST
{
    /// <summary>
    /// Displays the four player parameters and current funds.
    ///
    /// Hover behavior:
    /// - Each parameter panel root is toggled by pointer enter/exit events.
    /// - Hover events are forwarded by UIHoverNotifier on hover-area objects.
    /// </summary>
    public class HUD_Parameters : UIBase
    {
        public override bool IsVisibleCursor { get; set; } = true;

        [Header("Root Objects")]
        [SerializeField] private GameObject fameRoot;
        [SerializeField] private GameObject sanityRoot;
        [SerializeField] private GameObject enlightenmentRoot;
        [SerializeField] private GameObject madnessRoot;

        [Header("Hover Areas (Always Active)")]
        [SerializeField] private RectTransform fameHoverArea;
        [SerializeField] private RectTransform sanityHoverArea;
        [SerializeField] private RectTransform enlightenmentHoverArea;
        [SerializeField] private RectTransform madnessHoverArea;

        [Header("Parameter Texts")]
        [SerializeField] private TextMeshProUGUI fameText;
        [SerializeField] private TextMeshProUGUI sanityText;
        [SerializeField] private TextMeshProUGUI enlightenmentText;
        [SerializeField] private TextMeshProUGUI madnessText;

        [Header("Funds Label")]
        [SerializeField] private TextMeshProUGUI fundsLabel;

        [Header("Hover Cursor")]
        [SerializeField] private bool unlockCursorForHover = true;

        private const string fameFormat = "FAME\n{0:F0}";
        private const string sanityFormat = "SANITY\n{0:F0}";
        private const string enlightenmentFormat = "ENLIGHT\n{0:F0}";
        private const string madnessFormat = "MADNESS\n{0:F0}";
        private const string fundsFormat = "${0:N0}";

        private float fameValue;
        private float sanityValue;
        private float enlightenmentValue;
        private float madnessValue;

        private bool isFameHovered;
        private bool isSanityHovered;
        private bool isEnlightenmentHovered;
        private bool isMadnessHovered;

        private void OnEnable()
        {
            PlayerParameters p = PlayerParameters.Singleton;
            p.OnParameterChanged += HandleParameterChanged;

            ResolveHoverAreas();
            BindHoverNotifiers();
            EnsureCursorUnlockedForHover();

            RefreshAll(p);
            ApplyRootVisibility();
        }

        private void OnDisable()
        {
            if (PlayerParameters.Singleton != null)
                PlayerParameters.Singleton.OnParameterChanged -= HandleParameterChanged;

            isFameHovered = false;
            isSanityHovered = false;
            isEnlightenmentHovered = false;
            isMadnessHovered = false;
            ApplyRootVisibility();
        }

        private void Update()
        {
            EnsureCursorUnlockedForHover();
        }

        private void HandleParameterChanged(ParameterType type, float value)
        {
            switch (type)
            {
                case ParameterType.Fame:
                    fameValue = value;
                    break;
                case ParameterType.Sanity:
                    sanityValue = value;
                    break;
                case ParameterType.Enlightenment:
                    enlightenmentValue = value;
                    break;
                case ParameterType.Madness:
                    madnessValue = value;
                    break;
            }

            ApplyParameterTexts();
            RefreshFunds(PlayerParameters.Singleton);
        }

        public void SetHoverState(ParameterType type, bool isHovered)
        {
            switch (type)
            {
                case ParameterType.Fame:
                    isFameHovered = isHovered;
                    break;
                case ParameterType.Sanity:
                    isSanityHovered = isHovered;
                    break;
                case ParameterType.Enlightenment:
                    isEnlightenmentHovered = isHovered;
                    break;
                case ParameterType.Madness:
                    isMadnessHovered = isHovered;
                    break;
            }

            ApplyRootVisibility();
        }

        private void RefreshAll(PlayerParameters p)
        {
            fameValue = p.Fame;
            sanityValue = p.Sanity;
            enlightenmentValue = p.Enlightenment;
            madnessValue = p.Madness;

            ApplyParameterTexts();
            RefreshFunds(p);
        }

        private void RefreshFunds(PlayerParameters p)
        {
            if (fundsLabel != null)
                fundsLabel.text = string.Format(fundsFormat, p.Funds);
        }

        private void ApplyParameterTexts()
        {
            SetText(fameText, fameFormat, fameValue);
            SetText(sanityText, sanityFormat, sanityValue);
            SetText(enlightenmentText, enlightenmentFormat, enlightenmentValue);
            SetText(madnessText, madnessFormat, madnessValue);
        }

        private void ApplyRootVisibility()
        {
            SetRootActive(fameRoot, isFameHovered);
            SetRootActive(sanityRoot, isSanityHovered);
            SetRootActive(enlightenmentRoot, isEnlightenmentHovered);
            SetRootActive(madnessRoot, isMadnessHovered);
        }

        private static void SetText(TextMeshProUGUI label, string format, float value)
        {
            if (label != null)
                label.text = string.Format(format, value);
        }

        private static void SetRootActive(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active)
                target.SetActive(active);
        }

        private void ResolveHoverAreas()
        {
            fameHoverArea = ResolveHoverArea(fameHoverArea, fameRoot, fameText);
            sanityHoverArea = ResolveHoverArea(sanityHoverArea, sanityRoot, sanityText);
            enlightenmentHoverArea = ResolveHoverArea(enlightenmentHoverArea, enlightenmentRoot, enlightenmentText);
            madnessHoverArea = ResolveHoverArea(madnessHoverArea, madnessRoot, madnessText);
        }

        private static RectTransform ResolveHoverArea(RectTransform explicitArea, GameObject root, TextMeshProUGUI fallbackText)
        {
            if (explicitArea != null)
                return explicitArea;

            if (root != null && root.transform.parent is RectTransform parentRect)
                return parentRect;

            if (root != null)
            {
                RectTransform rootRect = root.GetComponent<RectTransform>();
                if (rootRect != null)
                    return rootRect;
            }

            if (fallbackText != null)
                return fallbackText.rectTransform;

            return null;
        }

        private void BindHoverNotifiers()
        {
            BindHoverNotifier(fameHoverArea, ParameterType.Fame);
            BindHoverNotifier(sanityHoverArea, ParameterType.Sanity);
            BindHoverNotifier(enlightenmentHoverArea, ParameterType.Enlightenment);
            BindHoverNotifier(madnessHoverArea, ParameterType.Madness);
        }

        private void BindHoverNotifier(RectTransform area, ParameterType parameterType)
        {
            if (area == null)
                return;

            UIHoverNotifier notifier = area.GetComponent<UIHoverNotifier>();
            if (notifier == null)
                notifier = area.gameObject.AddComponent<UIHoverNotifier>();

            notifier.Configure(this, parameterType);
        }

        private void EnsureCursorUnlockedForHover()
        {
            if (!unlockCursorForHover)
                return;

            if (Cursor.lockState != CursorLockMode.None)
                Cursor.lockState = CursorLockMode.None;

            if (!Cursor.visible)
                Cursor.visible = true;
        }
    }
}
