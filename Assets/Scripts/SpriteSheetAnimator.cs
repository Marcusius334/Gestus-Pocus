using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Plays a sliced sprite sheet as an animation.
/// Add this to an empty GameObject, drag a sprite sheet texture (Sprite Mode = Multiple, already sliced)
/// into "Sprite Sheet", and it will animate. Works in the world (SpriteRenderer) or inside a Canvas (Image).
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public class SpriteSheetAnimator : MonoBehaviour
{
    public enum PlayMode { Loop, Once, PingPong }

    [Header("Sprite sheet")]
    [Tooltip("Drag the sliced sprite sheet texture here. Its frames are loaded automatically into 'Frames'.")]
    [SerializeField] private Texture2D spriteSheet;

    [Tooltip("Frames of the animation, in order. Filled automatically from the sprite sheet, but you can also edit it by hand.")]
    [SerializeField] private Sprite[] frames = Array.Empty<Sprite>();

    // Remembers which sheet 'frames' came from, so manual edits to 'frames' aren't overwritten on reload.
    [SerializeField, HideInInspector] private Texture2D lastLoadedSheet;

    [Header("Playback")]
    [SerializeField, Min(0.01f)] private float framesPerSecond = 12f;
    [SerializeField] private PlayMode playMode = PlayMode.Loop;
    [SerializeField] private bool playOnStart = true;
    [Tooltip("Ignore Time.timeScale (keeps animating while the game is paused).")]
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Look")]
    [SerializeField] private Color tint = Color.white;
    [Tooltip("Sorting order for the SpriteRenderer (ignored for UI Images).")]
    [SerializeField] private int sortingOrder = 0;

    [Header("Events")]
    public UnityEvent onFinished;

    private SpriteRenderer spriteRenderer;
    private Image image;

    private int currentFrame;
    private int direction = 1;
    private float timer;
    private bool isPlaying;

    public bool IsPlaying => isPlaying;
    public int CurrentFrame => currentFrame;
    public int FrameCount => frames?.Length ?? 0;

    public float FramesPerSecond
    {
        get => framesPerSecond;
        set => framesPerSecond = Mathf.Max(0.01f, value);
    }

    public PlayMode Mode
    {
        get => playMode;
        set => playMode = value;
    }

    // ------------------------------------------------------------------ Unity

    private void Awake()
    {
        EnsureRenderer();
        ApplyLook();
        ShowFrame(0);
    }

    private void Start()
    {
        if (Application.isPlaying && playOnStart)
            Play();
    }

    private void Update()
    {
        if (!Application.isPlaying || !isPlaying || FrameCount == 0)
            return;

        timer += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        float frameDuration = 1f / framesPerSecond;

        while (timer >= frameDuration && isPlaying)
        {
            timer -= frameDuration;
            Advance();
        }
    }

    // ------------------------------------------------------------------ Public API

    /// <summary>Starts (or resumes) the animation.</summary>
    public void Play()
    {
        if (FrameCount == 0)
        {
            Debug.LogWarning($"[{nameof(SpriteSheetAnimator)}] '{name}' has no frames to play.", this);
            return;
        }
        isPlaying = true;
    }

    /// <summary>Restarts the animation from the first frame.</summary>
    public void Restart()
    {
        currentFrame = 0;
        direction = 1;
        timer = 0f;
        ShowFrame(0);
        Play();
    }

    /// <summary>Pauses on the current frame.</summary>
    public void Pause() => isPlaying = false;

    /// <summary>Stops and goes back to the first frame.</summary>
    public void Stop()
    {
        isPlaying = false;
        currentFrame = 0;
        direction = 1;
        timer = 0f;
        ShowFrame(0);
    }

    /// <summary>Jumps to a specific frame without changing the play state.</summary>
    public void SetFrame(int index)
    {
        if (FrameCount == 0) return;
        currentFrame = Mathf.Clamp(index, 0, FrameCount - 1);
        timer = 0f;
        ShowFrame(currentFrame);
    }

    /// <summary>Swaps the animation to a new set of frames (e.g. from Resources or another sheet) and plays it.</summary>
    public void SetSheet(Sprite[] newFrames, bool play = true)
    {
        frames = newFrames ?? Array.Empty<Sprite>();
        Restart();
        if (!play) Pause();
    }

    /// <summary>Loads a sliced sheet from a Resources folder (path without extension) and plays it.</summary>
    public void SetSheetFromResources(string resourcesPath, bool play = true)
    {
        Sprite[] loaded = Resources.LoadAll<Sprite>(resourcesPath);
        if (loaded.Length == 0)
        {
            Debug.LogWarning($"[{nameof(SpriteSheetAnimator)}] No sprites found at Resources/{resourcesPath}.", this);
            return;
        }
        SetSheet(SortByFrameNumber(loaded), play);
    }

    public void SetTint(Color color)
    {
        tint = color;
        ApplyLook();
    }

    // ------------------------------------------------------------------ Internals

    private void Advance()
    {
        int last = FrameCount - 1;

        switch (playMode)
        {
            case PlayMode.Loop:
                currentFrame = (currentFrame + 1) % FrameCount;
                break;

            case PlayMode.Once:
                if (currentFrame >= last)
                {
                    isPlaying = false;
                    onFinished?.Invoke();
                    return;
                }
                currentFrame++;
                break;

            case PlayMode.PingPong:
                if (last == 0) break;
                if (currentFrame + direction > last || currentFrame + direction < 0)
                    direction = -direction;
                currentFrame += direction;
                break;
        }

        ShowFrame(currentFrame);
    }

    private void ShowFrame(int index)
    {
        if (FrameCount == 0) return;
        EnsureRenderer();

        Sprite sprite = frames[Mathf.Clamp(index, 0, FrameCount - 1)];
        if (image != null) image.sprite = sprite;
        else if (spriteRenderer != null) spriteRenderer.sprite = sprite;
    }

    /// <summary>Uses an Image if the object is inside a Canvas, otherwise a SpriteRenderer (added automatically).</summary>
    private void EnsureRenderer()
    {
        if (image != null || spriteRenderer != null) return;

        image = GetComponent<Image>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (image != null || spriteRenderer != null) return;

        if (GetComponentInParent<Canvas>() != null)
        {
            image = gameObject.AddComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = false;
        }
        else
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
    }

    private void ApplyLook()
    {
        if (image != null) image.color = tint;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = tint;
            spriteRenderer.sortingOrder = sortingOrder;
        }
    }

    /// <summary>Sorts sprites like "sheet_0, sheet_1, ... sheet_10" numerically instead of alphabetically.</summary>
    private static Sprite[] SortByFrameNumber(Sprite[] sprites)
    {
        return sprites
            .Where(s => s != null)
            .OrderBy(s =>
            {
                Match m = Regex.Match(s.name, @"(\d+)$");
                return m.Success ? int.Parse(m.Groups[1].Value) : int.MaxValue;
            })
            .ThenBy(s => s.name)
            .ToArray();
    }

#if UNITY_EDITOR

    // Runs in the editor whenever something changes in the inspector.
    private void OnValidate()
    {
        if (spriteSheet != null && spriteSheet != lastLoadedSheet)
        {
            LoadFramesFromSheet();
            lastLoadedSheet = spriteSheet;
        }

        // Defer: components can't be added inside OnValidate.
        EditorApplication.delayCall += () =>
        {
            if (this == null) return;
            EnsureRenderer();
            ApplyLook();
            ShowFrame(Application.isPlaying ? currentFrame : 0);
        };
    }

    [ContextMenu("Reload Frames From Sheet")]
    private void LoadFramesFromSheet()
    {
        if (spriteSheet == null) return;

        string path = AssetDatabase.GetAssetPath(spriteSheet);
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();

        if (sprites.Length == 0)
        {
            Debug.LogWarning($"[{nameof(SpriteSheetAnimator)}] '{spriteSheet.name}' has no sprites. " +
                             "Set Texture Type = Sprite, Sprite Mode = Multiple and slice it in the Sprite Editor.", this);
            return;
        }

        Undo.RecordObject(this, "Load sprite sheet frames");
        frames = SortByFrameNumber(sprites);
        EditorUtility.SetDirty(this);
    }
#endif
}
