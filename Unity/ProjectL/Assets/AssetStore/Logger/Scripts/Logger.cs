using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

#nullable enable

/* -------------------------------------------------
     Created by : Hamza Herbou

     Youtube : https://youtube.com/hamza-herbou
   ------------------------------------------------- */

namespace EasyUI
{
    public enum LogPosition { Top, Bottom }

    public class Logger : MonoBehaviour
    {
        [SerializeField] LogPosition position = LogPosition.Top;
        [Range(200f, 800f)][SerializeField] private float height = 300f;
        [Range(20f, 50f)][SerializeField] private float iconWidth = 40f;
        [SerializeField] private Sprite? spriteOpenIcon;
        [SerializeField] private Sprite? spriteCloseIcon;
        [Space(50f)]
        [SerializeField] private GameObject? uiEventSystem;
        [SerializeField] private Button? uiToggleButton;
        [SerializeField] private TextMeshProUGUI? uiLogText;
        [SerializeField] private GameObject? uiViewport;
        [SerializeField] private GameObject? uiScrollBar;
        [SerializeField] private ScrollRect? uiScrollRect;

        private VerticalLayoutGroup? uiContentVerticalLayoutGroup;
        private RectTransform? uiScrollRectTransform;
        private Image? uiToggleButtonImage;
        private RectTransform? uiToggleButtonRectTransform;

        public bool IsOpen { get; private set; } = false;

        private string[] colors = new string[3]{
            "#aaaaaa", // White
            "#ffdd33", // Yellow
            "#ff6666"  // Red
        };

        public void ClearLog() => uiLogText!.text = string.Empty;
        public void Hide() => uiScrollRect!.gameObject.SetActive(false);
        public void Show() => uiScrollRect!.gameObject.SetActive(true);

        private void OnEnable()
        {
            Application.logMessageReceived += LogCallback;
            uiToggleButton!.onClick.AddListener(ToggleLogUI);
        }

        private void Awake()
        {
            if (FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length == 0) {
                uiEventSystem!.SetActive(true);
            }

            uiContentVerticalLayoutGroup = transform.GetChild(0).GetComponent<VerticalLayoutGroup>();
            uiScrollRectTransform = uiScrollRect!.GetComponent<RectTransform>();
            uiToggleButtonImage = uiToggleButton!.GetComponent<Image>();
            uiToggleButtonRectTransform = uiToggleButton!.GetComponent<RectTransform>();

            IsOpen = true;
            ToggleLogUI();
        }

        private void ScrollDown()
        {
            uiScrollRect!.verticalNormalizedPosition = 0f;
        }

        private void LogCallback(string message, string stackTrace, LogType type)
        {
            // random TMP error which sometimes happens
            if (message.StartsWith("Importer(NativeFormatImporter) generated inconsistent result for asset")) {
                return;
            }
            //logTypeIndex => normal:0 , warning:1 , error:2
            int logTypeIndex = (type == LogType.Log) ? 0 : (type == LogType.Warning) ? 1 : 2;
            uiLogText!.text += $"<sprite={logTypeIndex}><color={colors[logTypeIndex]}> {message}</color>\n\n";
            ScrollDown();
        }

        public void ToggleLogUI()
        {
            IsOpen = !IsOpen;
            if (IsOpen) {
                SetupUI(new Vector2(1f, height), spriteCloseIcon!);
                ScrollDown();
            }
            else {
                SetupUI(Vector2.one * iconWidth, spriteOpenIcon!);
            }
        }

        private void SetupUI(Vector2 size, Sprite icon)
        {
            uiScrollRect!.enabled = IsOpen;
            uiViewport!.SetActive(IsOpen);
            uiScrollBar!.SetActive(IsOpen);
            uiContentVerticalLayoutGroup!.childForceExpandWidth = IsOpen;
            uiContentVerticalLayoutGroup.childControlWidth = IsOpen;
            uiScrollRectTransform!.sizeDelta = size;
            uiToggleButtonImage!.sprite = icon;
            uiToggleButtonRectTransform!.sizeDelta = 0.7f * iconWidth * Vector2.one;
            uiToggleButtonRectTransform.anchoredPosition = -Vector2.one * iconWidth / 2f;
        }

        private void OnValidate()
        {
            if (uiContentVerticalLayoutGroup != null) {
                if (position == LogPosition.Top)
                    uiContentVerticalLayoutGroup.childAlignment = TextAnchor.UpperRight;
                else
                    uiContentVerticalLayoutGroup.childAlignment = TextAnchor.LowerRight;
            }
        }


        private void OnDisable()
        {
            // singleton consistency
            if (this.gameObject == null) {
                return;
            }
            // hide the log UI
            Application.logMessageReceived -= LogCallback;
            uiToggleButton!.onClick.RemoveListener(ToggleLogUI);
            IsOpen = true;
            ToggleLogUI();
        }
    }
}
