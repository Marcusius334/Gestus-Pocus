
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenuUI : MonoBehaviour
{
    [Header("Panel de configuración")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Control de música")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TMP_Text musicPercentText;

    private void Awake()
    {
        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;
            musicSlider.wholeNumbers = false;

            musicSlider.onValueChanged.AddListener(
                OnMusicVolumeChanged);
        }

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(
                OnMusicVolumeChanged);
    }

    public void OpenSettings()
    {
        if (settingsPanel == null) return;

        UpdateMusicSlider();
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void ToggleSettings()
    {
        if (settingsPanel == null) return;

        if (settingsPanel.activeSelf)
            CloseSettings();
        else
            OpenSettings();
    }

    private void OnMusicVolumeChanged(float value)
    {
        MusicVolumeSlider.SetVolume(value);
        UpdatePercentText(value);
    }

    private void UpdateMusicSlider()
    {
        float volume = MusicVolumeSlider.Volume;

        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(volume);

        UpdatePercentText(volume);
    }

    private void UpdatePercentText(float volume)
    {
        if (musicPercentText != null)
        {
            musicPercentText.text =
                Mathf.RoundToInt(volume * 100f) + "%";
        }
    }
}
