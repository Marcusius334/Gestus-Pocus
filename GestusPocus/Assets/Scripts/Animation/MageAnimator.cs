using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Animates the mage (Idle, Run, Jump) with the sprite sheet of the current level:
/// Level 1 = primavera, Level 2 = verano, Level 3 = otoño, Level 4 = invierno.
///
/// The frames are cut from the sheet when the level starts, so the sheets in Sprites/Mage
/// don't need to be sliced in the Sprite Editor. All four sheets must keep the same layout
/// (1774x887: Idle row with 3 frames, Run row with 6, Jump row with 5).
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
        int level = forceLevel;

        if (level == 0)
        {
            Match match = Regex.Match(SceneManager.GetActiveScene().name, @"^Level\s*(\d+)$");
            level = match.Success ? int.Parse(match.Groups[1].Value) : GameManager.actualLevel;
        }

        if (levelSheets == null || levelSheets.Length == 0) return null;

        int index = Mathf.Clamp(level, 1, levelSheets.Length) - 1;
        return levelSheets[index];
    }

    private void BuildFrames(Texture2D sheet)
    {
        // Works even if Unity imported the sheet at a smaller size.
        float scaleX = sheet.width / SheetWidth;
        float scaleY = sheet.height / SheetHeight;
        float pixelsPerUnit = IdleHeight * scaleY / characterHeight;

        // The ground line of each frame goes to the bottom of the collider, so the feet touch the floor.
        float feetY = 0f;
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
