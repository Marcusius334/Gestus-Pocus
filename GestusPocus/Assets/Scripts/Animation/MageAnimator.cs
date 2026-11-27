using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Animates the mage (Idle, Run, Jump) with the sprite sheet of the current level:
/// Level 1 = primavera, Level 2 = verano, Level 3 = otoño, Level 4 = invierno.
///
/// The frames are cut from the sheet when the level starts, so the sheets in Sprites/Mage
/// don't need to be sliced in the Sprite Editor. All four sheets must keep the same layout
/// (1774x887: Idle row with 3 frames, Run row with 6, Jump row with 5).
///
/// Transformations: while PlayerMovement.Animate(n) plays, the form whose Animation Number is n
/// (cat, cow, turtle, dove, car, bike...) is shown instead of the mage, if that form has art for
/// the current level. Forms without art keep the old placeholder effect.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[DisallowMultipleComponent]
public class MageAnimator : MonoBehaviour
{
    [Header("Sprite sheets")]
    [Tooltip("One sheet per level, in order: Level 1 (primavera), 2 (verano), 3 (otoño), 4 (invierno).")]
    [SerializeField] private Texture2D[] levelSheets = new Texture2D[4];
    [Tooltip("0 = automatic: uses the scene name ('Level 3' -> sheet 3), or the level chosen in the menu. " +
             "1-4 = always use that sheet.")]
    [SerializeField, Range(0, 4)] private int forceLevel = 0;

    [Header("Size")]
    [Tooltip("Height of the mage (feet to hat tip) in world units.")]
    [SerializeField, Min(0.1f)] private float characterHeight = 2.3f;

    [Header("Timing")]
    [SerializeField, Min(0.1f)] private float runFps = 12f;
    [Tooltip("Seconds with the eyes open between blinks.")]
    [SerializeField, Min(0.05f)] private float idleOpenTime = 1.2f;
    [SerializeField, Min(0.02f)] private float blinkTime = 0.15f;
    [Tooltip("Seconds the landing frame is shown after touching the ground.")]
    [SerializeField, Min(0f)] private float landingTime = 0.12f;

    [Header("Thresholds")]
    [Tooltip("Horizontal speed above which the mage runs (and turns around).")]
    [SerializeField, Min(0f)] private float runSpeedThreshold = 0.1f;
    [Tooltip("Vertical speed that separates the rising, top and falling jump frames.")]
    [SerializeField, Min(0f)] private float jumpApexSpeed = 2f;

    [Header("Transformations")]
    [Tooltip("Art shown while the mage is transformed (PlayerMovement.Animate).")]
    [SerializeField] private Form[] forms = new Form[0];

    /// <summary>One transformation: a sheet with one row of frames, played in a loop.</summary>
    [Serializable]
    public class Form
    {
        [Tooltip("Only to tell the forms apart in the Inspector.")]
        public string name;
        [Tooltip("Number passed to PlayerMovement.Animate: 1 cat, 2 cow, 3 turtle, 4 dove, 5 car, 6 truck, 7 bike.")]
        public int animationNumber;
        [Tooltip("Level that uses this art (0 = every level).")]
        public int level;
        public Texture2D sheet;
        [Tooltip("Size of the sheet image in pixels (before Unity's import settings).")]
        public Vector2 sheetSize = new Vector2(2172f, 724f);
        [Tooltip("Height of the art in world units, from the ground line to the top of the frames.")]
        [Min(0.1f)] public float height = 1.5f;
        [Min(0.1f)] public float framesPerSecond = 12f;
        [Tooltip("Width of the slot each frame was drawn in. The middle of the slot is the frame's center.")]
        public float slotWidth = 362f;
        [Tooltip("Left edge of each frame and right edge of the last one, in pixels from the LEFT of the sheet.")]
        public float[] frameEdges = new float[0];
        [Tooltip("Top and bottom of the frames, in pixels from the TOP of the sheet.")]
        public float top, bottom;
        [Tooltip("Ground line (feet, wheels or middle of the shadow), in pixels from the TOP of the sheet.")]
        public float ground;
    }

    // ------------------------------------------------------------------ Sheet layout

    // Layout of the sheets, in pixels of a 1774x887 image, measured from the TOP-left corner:
    // left, top, right, bottom of the frame, and the ground line (where the feet / shadow are).
    private const float SheetWidth = 1774f;
    private const float SheetHeight = 887f;
    private const float IdleHeight = 247f; // feet to hat tip in the idle frames

    private struct FrameRect
    {
        public float left, top, right, bottom, ground;
        public FrameRect(float left, float top, float right, float bottom, float ground)
        {
            this.left = left; this.top = top; this.right = right; this.bottom = bottom; this.ground = ground;
        }
    }

    private static readonly FrameRect[] IdleRects =
    {
        new FrameRect(219, 5, 431, 268, 252),  // eyes open
        new FrameRect(449, 5, 659, 268, 252),  // blink
        new FrameRect(676, 5, 885, 268, 252),  // eyes open
    };

    private static readonly FrameRect[] RunRects =
    {
        new FrameRect(226, 285, 456, 541, 523),
        new FrameRect(470, 285, 699, 541, 523),
        new FrameRect(714, 285, 945, 541, 523),
        new FrameRect(971, 285, 1200, 541, 523),
        new FrameRect(1226, 285, 1448, 541, 523),
        new FrameRect(1472, 285, 1707, 541, 523),
    };

    private static readonly FrameRect[] JumpRects =
    {
        new FrameRect(224, 548, 453, 846, 830),   // 0 crouch (not used)
        new FrameRect(471, 548, 708, 846, 830),   // 1 take off / rising
        new FrameRect(727, 548, 956, 790, 775),   // 2 top of the jump (cut above its shadow, ground = boots)
        new FrameRect(986, 548, 1216, 790, 771),  // 3 falling (cut above its shadow, ground = boots)
        new FrameRect(1255, 548, 1485, 846, 830), // 4 landing
    };

    private const int JumpRising = 1;
    private const int JumpTop = 2;
    private const int JumpFalling = 3;
    private const int JumpLanding = 4;

    // Idle: open, blink, open, blink...
    private static readonly int[] IdleSequence = { 0, 1, 2, 1 };

    // ------------------------------------------------------------------ State

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private PlayerMovement player;

    private Sprite[] idleFrames;
    private Sprite[] runFrames;
    private Sprite[] jumpFrames;

    private int idleStep;
    private float idleTimer;
    private int runFrame;
    private float runTimer;
    private bool wasAirborne;
    private float landingTimer;

    // Bottom of the collider, in local units: where the feet go.
    private float feetY;

    // Transformation being shown (null = normal mage).
    private SpriteRenderer formRenderer;
    private readonly Dictionary<Form, Sprite[]> formFrames = new Dictionary<Form, Sprite[]>();
    private Form activeForm;
    private Sprite[] activeFormFrames;
    private int formFrame;
    private float formTimer;

    // ------------------------------------------------------------------ Unity

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<PlayerMovement>();

        Texture2D sheet = ChooseSheet();
        if (sheet == null)
        {
            Debug.LogWarning($"[{nameof(MageAnimator)}] No sprite sheet for this level, the mage keeps its current sprite.", this);
            enabled = false;
            return;
        }

        BuildFrames(sheet);
        spriteRenderer.sprite = idleFrames[0];
    }

    private void Update()
    {
        if (activeForm != null)
        {
            if (player != null && player.IsAnimating)
            {
                AnimateForm();
                return;
            }
            EndForm();
        }

        Vector2 velocity = rb != null ? rb.linearVelocity : Vector2.zero;
        bool grounded = player == null || player.IsGrounded;

        // Face the direction of movement (the sheets face right).
        if (Mathf.Abs(velocity.x) > runSpeedThreshold)
            spriteRenderer.flipX = velocity.x < 0f;

        if (!grounded)
        {
            wasAirborne = true;
            ShowJump(velocity.y);
            return;
        }

        if (wasAirborne)
        {
            wasAirborne = false;
            landingTimer = landingTime;
        }

        if (landingTimer > 0f)
        {
            landingTimer -= Time.deltaTime;
            spriteRenderer.sprite = jumpFrames[JumpLanding];
            return;
        }

        if (Mathf.Abs(velocity.x) > runSpeedThreshold)
            ShowRun();
        else
            ShowIdle();
    }

    private void LateUpdate()
    {
        if (activeForm != null) PlaceFormRenderer();
    }

    // ------------------------------------------------------------------ Transformations

    /// <summary>
    /// Shows the form for this animation number until PlayerMovement stops animating.
    /// False if this level has no art for it (then the mage stays visible).
    /// </summary>
    public bool StartForm(int animationNumber, bool faceLeft)
    {
        if (!isActiveAndEnabled) return false;

        Form form = FindForm(animationNumber);
        if (form == null) return false;

        Sprite[] frames = GetFormFrames(form);
        if (frames == null || frames.Length == 0) return false;

        EnsureFormRenderer();
        activeForm = form;
        activeFormFrames = frames;
        formFrame = 0;
        formTimer = 0f;

        formRenderer.sprite = frames[0];
        formRenderer.flipX = faceLeft;
        formRenderer.gameObject.SetActive(true);
        spriteRenderer.enabled = false;
        PlaceFormRenderer();
        return true;
    }

    private void EndForm()
    {
        if (formRenderer != null)
        {
            // Keep looking the way the form was going.
            spriteRenderer.flipX = formRenderer.flipX;
            formRenderer.gameObject.SetActive(false);
        }
        spriteRenderer.enabled = true;
        activeForm = null;
        activeFormFrames = null;
    }

    private void AnimateForm()
    {
        formTimer += Time.deltaTime;
        float frameTime = 1f / activeForm.framesPerSecond;
        while (formTimer >= frameTime)
        {
            formTimer -= frameTime;
            formFrame = (formFrame + 1) % activeFormFrames.Length;
        }
        formRenderer.sprite = activeFormFrames[formFrame];
    }

    /// <summary>
    /// The form keeps its own size even though PlayerMovement scales the mage while animating,
    /// and stands on the bottom of the (scaled) collider.
    /// </summary>
    private void PlaceFormRenderer()
    {
        Vector3 scale = transform.lossyScale;
        formRenderer.transform.localScale = new Vector3(
            scale.x != 0f ? 1f / scale.x : 1f,
            scale.y != 0f ? 1f / scale.y : 1f,
            1f);
        formRenderer.transform.localPosition = new Vector3(0f, feetY, 0f);
    }

    private Form FindForm(int animationNumber)
    {
        if (forms == null) return null;

        int level = LevelSheets.CurrentLevel(forceLevel);
        foreach (Form form in forms)
        {
            if (form != null && form.sheet != null && form.animationNumber == animationNumber &&
                (form.level == 0 || form.level == level))
                return form;
        }
        return null;
    }

    private void EnsureFormRenderer()
    {
        if (formRenderer != null) return;

        GameObject go = new GameObject("FormSprite");
        go.transform.SetParent(transform, false);
        go.layer = gameObject.layer;

        formRenderer = go.AddComponent<SpriteRenderer>();
        formRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
        formRenderer.sortingOrder = spriteRenderer.sortingOrder;
        formRenderer.sharedMaterial = spriteRenderer.sharedMaterial;
        go.SetActive(false);
    }

    /// <summary>Cuts the form's frames from its sheet (once).</summary>
    private Sprite[] GetFormFrames(Form form)
    {
        if (formFrames.TryGetValue(form, out Sprite[] cached)) return cached;

        Sprite[] frames = CutFormFrames(form);
        formFrames[form] = frames;
        return frames;
    }

    private static Sprite[] CutFormFrames(Form form)
    {
        int count = form.frameEdges != null ? form.frameEdges.Length - 1 : 0;
        float artHeight = form.ground - form.top;
        if (count <= 0 || form.sheetSize.x <= 0f || form.sheetSize.y <= 0f ||
            form.bottom <= form.top || artHeight <= 0f)
        {
            Debug.LogWarning($"[{nameof(MageAnimator)}] Form '{form.name}' has no valid frame layout.");
            return new Sprite[0];
        }

        Texture2D sheet = form.sheet;
        // Works even if Unity imported the sheet at a smaller size.
        float scaleX = sheet.width / form.sheetSize.x;
        float scaleY = sheet.height / form.sheetSize.y;
        float pixelsPerUnit = artHeight * scaleY / form.height;

        Sprite[] frames = new Sprite[count];
        for (int i = 0; i < count; i++)
        {
            float left = form.frameEdges[i];
            float right = form.frameEdges[i + 1];

            // Unity measures sprite rects from the BOTTOM-left corner.
            float x = Mathf.Round(left * scaleX);
            float y = Mathf.Round((form.sheetSize.y - form.bottom) * scaleY);
            float width = Mathf.Min(Mathf.Round((right - left) * scaleX), sheet.width - x);
            float height = Mathf.Min(Mathf.Round((form.bottom - form.top) * scaleY), sheet.height - y);

            // Pivot: middle of the slot the frame was drawn in, on the ground line.
            float slotCenter = form.slotWidth * (i + 0.5f);
            Vector2 pivot = new Vector2(
                (slotCenter - left) * scaleX / width,
                (form.bottom - form.ground) * scaleY / height);

            Sprite sprite = Sprite.Create(sheet, new Rect(x, y, width, height), pivot, pixelsPerUnit, 0, SpriteMeshType.FullRect);
            sprite.name = sheet.name + "_" + i;
            frames[i] = sprite;
        }
        return frames;
    }

    // ------------------------------------------------------------------ States

    private void ShowJump(float verticalSpeed)
    {
        int frame;
        if (verticalSpeed > jumpApexSpeed) frame = JumpRising;
        else if (verticalSpeed > -jumpApexSpeed) frame = JumpTop;
        else frame = JumpFalling;

        spriteRenderer.sprite = jumpFrames[frame];

        // Start the other animations from their first frame next time.
        runFrame = 0;
        runTimer = 0f;
        idleStep = 0;
        idleTimer = 0f;
    }

    private void ShowRun()
    {
        runTimer += Time.deltaTime;
        float frameTime = 1f / runFps;
        while (runTimer >= frameTime)
        {
            runTimer -= frameTime;
            runFrame = (runFrame + 1) % runFrames.Length;
        }
        spriteRenderer.sprite = runFrames[runFrame];

        idleStep = 0;
        idleTimer = 0f;
    }

    private void ShowIdle()
    {
        idleTimer += Time.deltaTime;
        float stepTime = IdleSequence[idleStep] == 1 ? blinkTime : idleOpenTime;
        if (idleTimer >= stepTime)
        {
            idleTimer = 0f;
            idleStep = (idleStep + 1) % IdleSequence.Length;
        }
        spriteRenderer.sprite = idleFrames[IdleSequence[idleStep]];

        runFrame = 0;
        runTimer = 0f;
    }

    // ------------------------------------------------------------------ Setup

    /// <summary>Sheet for this level: forced level, else "Level N" scene name, else the level chosen in the menu.</summary>
    private Texture2D ChooseSheet()
    {
        return LevelSheets.Choose(levelSheets, forceLevel);
    }

    private void BuildFrames(Texture2D sheet)
    {
        // Works even if Unity imported the sheet at a smaller size.
        float scaleX = sheet.width / SheetWidth;
        float scaleY = sheet.height / SheetHeight;
        float pixelsPerUnit = IdleHeight * scaleY / characterHeight;

        // The ground line of each frame goes to the bottom of the collider, so the feet touch the floor.
        feetY = 0f;
        CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
        if (capsule != null) feetY = capsule.offset.y - capsule.size.y * 0.5f;

        idleFrames = CutFrames(sheet, IdleRects, scaleX, scaleY, pixelsPerUnit, feetY);
        runFrames = CutFrames(sheet, RunRects, scaleX, scaleY, pixelsPerUnit, feetY);
        jumpFrames = CutFrames(sheet, JumpRects, scaleX, scaleY, pixelsPerUnit, feetY);
    }

    private static Sprite[] CutFrames(Texture2D sheet, FrameRect[] rects, float scaleX, float scaleY,
                                      float pixelsPerUnit, float feetY)
    {
        Sprite[] sprites = new Sprite[rects.Length];

        for (int i = 0; i < rects.Length; i++)
        {
            FrameRect f = rects[i];

            // Unity measures sprite rects from the BOTTOM-left corner.
            float x = Mathf.Round(f.left * scaleX);
            float y = Mathf.Round((SheetHeight - f.bottom) * scaleY);
            float width = Mathf.Round((f.right - f.left) * scaleX);
            float height = Mathf.Round((f.bottom - f.top) * scaleY);

            width = Mathf.Min(width, sheet.width - x);
            height = Mathf.Min(height, sheet.height - y);
            Rect rect = new Rect(x, y, width, height);

            // Pivot = the object's position. Put it above the ground line by the distance
            // between the collider's center and its bottom, so the feet stand on the floor.
            float groundFromBottom = (f.bottom - f.ground) * scaleY;
            float pivotY = (groundFromBottom - feetY * pixelsPerUnit) / height;
            Vector2 pivot = new Vector2(0.5f, pivotY);

            Sprite sprite = Sprite.Create(sheet, rect, pivot, pixelsPerUnit, 0, SpriteMeshType.FullRect);
            sprite.name = sheet.name + "_" + i;
            sprites[i] = sprite;
        }

        return sprites;
    }
}
