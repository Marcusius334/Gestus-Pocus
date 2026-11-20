using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Background music volume control with an on-screen slider in the top right corner.
///
/// It creates itself automatically when the game starts (no need to add it to any scene)
/// and survives scene changes. It controls every AudioSource that outputs to the
/// "BackgroundMusic" mixer group (or whose GameObject is called "BackgroundMusic").
/// The chosen volume is saved in PlayerPrefs, so it is remembered between sessions.
/// </summary>
[DisallowMultipleComponent]
public class MusicVolumeSlider : MonoBehaviour
{
    private const string PrefsKey = "MusicVolume";
    private const string MusicName = "BackgroundMusic";

    // Layout (in reference pixels, 1920x1080).
    private const float PanelWidth = 360f;
    private const float PanelHeight = 56f;
    private const float Margin = 20f;

    private static MusicVolumeSlider instance;

    /// <summary>Current music volume, from 0 (silent) to 1 (original volume).</summary>
    public static float Volume { get; private set; } = 1f;

    // Music sources found so far and the volume each one had before we touched it.
    private readonly Dictionary<AudioSource, float> baseVolumes = new Dictionary<AudioSource, float>();

    private RectTransform panel;
    private Slider slider;
    private TMP_Text percentText;

    // ------------------------------------------------------------------ Startup

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateOnGameStart()
    {
        if (instance != null) return;

        GameObject go = new GameObject("MusicVolumeSlider");
        DontDestroyOnLoad(go);
        go.AddComponent<MusicVolumeSlider>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        Volume = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefsKey, 1f));
        BuildUI();
        FindMusicSources();
        ApplyVolume();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (instance != this) return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindMusicSources();
        ApplyVolume();
    }

    // ------------------------------------------------------------------ Public API

    /// <summary>Sets the music volume (0..1), updates the slider and saves it.</summary>
    public static void SetVolume(float value)
    {
        Volume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(PrefsKey, Volume);

        if (instance == null) return;

        if (instance.slider != null) instance.slider.SetValueWithoutNotify(Volume);
        instance.UpdatePercentText();
        instance.ApplyVolume();
    }

    /// <summary>
    /// True when the mouse is over the volume slider. World objects that react to
    /// OnMouseDown use this to ignore clicks meant for the slider.
    /// </summary>
    public static bool IsPointerOver()
    {
        if (instance == null || instance.panel == null || !instance.panel.gameObject.activeInHierarchy)
            return false;

        // Screen Space Overlay canvas: no camera needed.
        return RectTransformUtility.RectangleContainsScreenPoint(instance.panel, Input.mousePosition, null);
    }

    // ------------------------------------------------------------------ Music sources

    private void FindMusicSources()
    {
        // Forget sources destroyed with the previous scene.
        List<AudioSource> destroyed = new List<AudioSource>();
        foreach (AudioSource source in baseVolumes.Keys)
        {
            if (source == null) destroyed.Add(source);
        }
        foreach (AudioSource source in destroyed)
        {
            baseVolumes.Remove(source);
        }

        foreach (AudioSource source in FindObjectsByType<AudioSource>(FindObjectsInactive.Include))
        {
            if (IsMusic(source) && !baseVolumes.ContainsKey(source))
                baseVolumes.Add(source, source.volume);
        }
    }

    private static bool IsMusic(AudioSource source)
    {
        if (source.outputAudioMixerGroup != null && source.outputAudioMixerGroup.name == MusicName)
            return true;

        return source.gameObject.name == MusicName;
    }

    private void ApplyVolume()
    {
        foreach (KeyValuePair<AudioSource, float> pair in baseVolumes)
        {
            if (pair.Key != null)
                pair.Key.volume = pair.Value * Volume;
        }
    }

    // ------------------------------------------------------------------ UI

    private void BuildUI()
    {
        // Canvas drawn on top of everything, scaled with the screen.
        GameObject canvasGO = new GameObject("MusicVolumeCanvas", typeof(RectTransform));
        canvasGO.transform.SetParent(transform, false);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // Background panel in the top right corner.
        panel = CreateRect("MusicVolumePanel", canvasGO.transform);
        panel.anchorMin = new Vector2(1f, 0.15f);
        panel.anchorMax = new Vector2(1f, 0.15f);
        panel.pivot = new Vector2(1f, 0f);
        panel.anchoredPosition = new Vector2(-Margin, -Margin);
        panel.sizeDelta = new Vector2(PanelWidth, PanelHeight);

        Image panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 1f);

        // "Música" label on the left.
        TMP_Text label = CreateText("Label", panel, "Música", TextAlignmentOptions.MidlineLeft);
        PlaceLeft(label.rectTransform, 16f, 90f, PanelHeight);

        // Slider in the middle.
        GameObject sliderGO = DefaultControls.CreateSlider(new DefaultControls.Resources());
        sliderGO.name = "MusicVolumeSlider";
        sliderGO.transform.SetParent(panel, false);
        PlaceLeft((RectTransform)sliderGO.transform, 110f, 175f, 20f);

        slider = sliderGO.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.navigation = new Navigation { mode = Navigation.Mode.None };
        slider.SetValueWithoutNotify(Volume);
        slider.onValueChanged.AddListener(SetVolume);

        StyleSlider(slider);

        // Keeps the arrow keys / WASD from moving the slider after it was clicked.
        sliderGO.AddComponent<DeselectOnRelease>();

        // Percentage on the right.
        percentText = CreateText("Percent", panel, "", TextAlignmentOptions.MidlineRight);
        PlaceLeft(percentText.rectTransform, PanelWidth - 16f - 60f, 60f, PanelHeight);
        UpdatePercentText();
    }

    private static void StyleSlider(Slider s)
    {
        Image background = s.transform.Find("Background")?.GetComponent<Image>();
        if (background != null) background.color = new Color(1f, 1f, 1f, 0.25f);

        if (s.fillRect != null)
        {
            Image fill = s.fillRect.GetComponent<Image>();
            if (fill != null) fill.color = new Color(1f, 0.75f, 0.25f, 1f);
        }

        if (s.handleRect != null)
        {
            s.handleRect.sizeDelta = new Vector2(16f, 0f);
            Image handle = s.handleRect.GetComponent<Image>();
            if (handle != null) handle.color = Color.white;
        }
    }

    private void UpdatePercentText()
    {
        if (percentText != null)
            percentText.text = Mathf.RoundToInt(Volume * 100f) + "%";
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    private static TMP_Text CreateText(string name, Transform parent, string text, TextAlignmentOptions alignment)
    {
        RectTransform rect = CreateRect(name, parent);
        TextMeshProUGUI tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 24f;
        tmp.color = Color.white;
        tmp.alignment = alignment;
        tmp.raycastTarget = false;
        return tmp;
    }

    /// <summary>Places a child at a horizontal offset from the panel's left edge, vertically centered.</summary>
    private static void PlaceLeft(RectTransform rect, float x, float width, float height)
    {
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = new Vector2(x, 0f);
        rect.sizeDelta = new Vector2(width, height);
    }

    /// <summary>Clears the UI selection when the slider is released, and saves the volume.</summary>
    private class DeselectOnRelease : MonoBehaviour, IPointerUpHandler
    {
        public void OnPointerUp(PointerEventData eventData)
        {
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
                EventSystem.current.SetSelectedGameObject(null);

            PlayerPrefs.Save();
        }
    }
}
