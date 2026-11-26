
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class MusicVolumeSlider : MonoBehaviour
{
    private const string PrefsKey = "MusicVolume";
    private const string MusicName = "BackgroundMusic";

    private static MusicVolumeSlider instance;

    public static float Volume { get; private set; } = 1f;

    private readonly Dictionary<AudioSource, float> baseVolumes =
        new Dictionary<AudioSource, float>();

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateOnGameStart()
    {
        if (instance != null) return;

        GameObject go = new GameObject("MusicVolumeManager");
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
        DontDestroyOnLoad(gameObject);

        Volume = Mathf.Clamp01(
            PlayerPrefs.GetFloat(PrefsKey, 1f));

        SceneManager.sceneLoaded += OnSceneLoaded;

        RefreshMusicSources();
        ApplyVolume();
    }

    private void Start()
    {
        InvokeRepeating(nameof(RefreshAndApply), 1f, 1f);
    }

    private void OnDestroy()
    {
        if (instance != this) return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        CancelInvoke();
        instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshAndApply();
    }

    public static void SetVolume(float value)
    {
        Volume = Mathf.Clamp01(value);

        PlayerPrefs.SetFloat(PrefsKey, Volume);
        PlayerPrefs.Save();

        if (instance != null)
            instance.ApplyVolume();
    }

    public static bool IsPointerOver()
    {
        return EventSystem.current != null &&
               EventSystem.current.IsPointerOverGameObject();
    }

    private void RefreshAndApply()
    {
        RefreshMusicSources();
        ApplyVolume();
    }

    private void RefreshMusicSources()
    {
        List<AudioSource> destroyed = new List<AudioSource>();

        foreach (AudioSource source in baseVolumes.Keys)
        {
            if (source == null)
                destroyed.Add(source);
        }

        foreach (AudioSource source in destroyed)
            baseVolumes.Remove(source);

        AudioSource[] sources =
            FindObjectsByType<AudioSource>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        foreach (AudioSource source in sources)
        {
            if (IsMusic(source) &&
                !baseVolumes.ContainsKey(source))
            {
                baseVolumes.Add(source, source.volume);
            }
        }
    }

    private static bool IsMusic(AudioSource source)
    {
        if (source.outputAudioMixerGroup != null &&
            source.outputAudioMixerGroup.name == MusicName)
            return true;

        return source.gameObject.name == MusicName;
    }

    private void ApplyVolume()
    {
        foreach (KeyValuePair<AudioSource, float> pair
                 in baseVolumes)
        {
            if (pair.Key != null)
                pair.Key.volume = pair.Value * Volume;
        }
    }
}
