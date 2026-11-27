using UnityEngine;

/// <summary>
/// Animates the owl (wing flap loop) with the sprite sheet of the current level:
/// Level 1 = primavera, Level 2 = verano, Level 3 = otoño, Level 4 = invierno.
///
/// The sheets draw a shadow under each frame. The shadow is cut off the owl and shown as a
/// separate object ON THE GROUND below the owl (found with a raycast against the ground layers),
/// not at a fixed distance under it. If there is no ground below, the shadow is hidden.
///
/// The frames are cut from the sheet when the owl first appears, so the sheets in Sprites/Buho
/// don't need to be sliced in the Sprite Editor. All four sheets must keep the same layout
/// (2172x724: 4 frames side by side, owl on top, shadow below).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[DisallowMultipleComponent]
public class OwlAnimator : MonoBehaviour
{
    [Header("Sprite sheets")]
    [Tooltip("One sheet per level, in order: Level 1 (primavera), 2 (verano), 3 (otoño), 4 (invierno).")]
    [SerializeField] private Texture2D[] levelSheets = new Texture2D[4];
    [Tooltip("0 = automatic: uses the scene name ('Level 3' -> sheet 3), or the level chosen in the menu. " +
             "1-4 = always use that sheet.")]
    [SerializeField, Range(0, 4)] private int forceLevel = 0;

    [Header("Animation")]
    [Tooltip("Height of the owl (feet to hat tip) in world units.")]
    [SerializeField, Min(0.1f)] private float owlHeight = 1.6f;
    [SerializeField, Min(0.1f)] private float framesPerSecond = 8f;

    [Header("Shadow on the ground")]
    [Tooltip("Layers the shadow can land on.")]
    [SerializeField] private LayerMask groundLayers = 1 << 6; // "Ground"
    [Tooltip("How far below the owl to look for ground, in world units.")]
    [SerializeField, Min(0.1f)] private float maxShadowDistance = 30f;
    [Tooltip("Moves the shadow up (+) or down (-) from the top of the ground, in world units.")]
    [SerializeField] private float shadowYOffset = 0f;

    // ------------------------------------------------------------------ Sheet layout

    // Layout of the sheets, in pixels of a 2172x724 image, measured from the TOP-left corner.
    private const float SheetWidth = 2172f;
    private const float SheetHeight = 724f;
    private const float OwlArtHeight = 510f; // hat tip to feet

    // One column per frame.
    private static readonly float[] ColumnLeft = { 0f, 543f, 1086f, 1629f };
    private const float ColumnWidth = 543f;

    // Owl part: rows 40-590, pivot (the owl's position) in the middle of the owl.
    private const float OwlTop = 40f;
    private const float OwlBottom = 590f;
    private const float OwlPivotY = 317f;

    // Shadow part: rows 593-662, pivot on the middle line of the shadow (that line touches the ground).
    private const float ShadowTop = 593f;
    private const float ShadowBottom = 662f;
    private const float ShadowPivotY = 627f;

    // ------------------------------------------------------------------ State

    private SpriteRenderer spriteRenderer;
    private SpriteRenderer shadowRenderer;
    private Sprite[] owlFrames;
    private Sprite[] shadowFrames;
    private int frame;
    private float timer;

    // ------------------------------------------------------------------ Unity

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        Texture2D sheet = LevelSheets.Choose(levelSheets, forceLevel);
        if (sheet == null)
        {
            Debug.LogWarning($"[{nameof(OwlAnimator)}] No sprite sheet for this level, the owl keeps its current sprite.", this);
            enabled = false;
            return;
        }

        BuildFrames(sheet);
        CreateShadow();
        ShowFrame(0);
    }

    private void OnEnable()
    {
        if (shadowRenderer != null) shadowRenderer.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        // The shadow is a child, so it also hides when the owl is deactivated;
        // this covers disabling only this component.
        if (shadowRenderer != null) shadowRenderer.gameObject.SetActive(false);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float frameTime = 1f / framesPerSecond;
        if (timer >= frameTime)
        {
            timer -= frameTime;
            if (timer >= frameTime) timer = 0f; // after a long pause, don't skip frames
            ShowFrame((frame + 1) % owlFrames.Length);
        }
    }

    private void LateUpdate()
    {
        PlaceShadowOnGround();
    }

    // ------------------------------------------------------------------ Animation

    private void ShowFrame(int index)
    {
        frame = index;
        spriteRenderer.sprite = owlFrames[frame];
        if (shadowRenderer != null) shadowRenderer.sprite = shadowFrames[frame];
    }

    /// <summary>Puts the shadow on the first ground below the owl, or hides it if there is none.</summary>
    private void PlaceShadowOnGround()
    {
        if (shadowRenderer == null) return;

        Vector2 origin = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, maxShadowDistance, groundLayers);

        shadowRenderer.enabled = hit.collider != null;
        if (hit.collider == null) return;

        // Slightly behind the owl (in case they overlap), same flip as the owl.
        shadowRenderer.transform.position = new Vector3(origin.x, hit.point.y + shadowYOffset, transform.position.z + 0.01f);
        shadowRenderer.flipX = spriteRenderer.flipX;
    }

    // ------------------------------------------------------------------ Setup

    private void BuildFrames(Texture2D sheet)
    {
        // Works even if Unity imported the sheet at a smaller size.
        float scaleX = sheet.width / SheetWidth;
        float scaleY = sheet.height / SheetHeight;
        float pixelsPerUnit = OwlArtHeight * scaleY / owlHeight;

        owlFrames = new Sprite[ColumnLeft.Length];
        shadowFrames = new Sprite[ColumnLeft.Length];

        for (int i = 0; i < ColumnLeft.Length; i++)
        {
            owlFrames[i] = Cut(sheet, i, OwlTop, OwlBottom, OwlPivotY, scaleX, scaleY, pixelsPerUnit, "_owl_");
            shadowFrames[i] = Cut(sheet, i, ShadowTop, ShadowBottom, ShadowPivotY, scaleX, scaleY, pixelsPerUnit, "_shadow_");
        }
    }

    private static Sprite Cut(Texture2D sheet, int column, float top, float bottom, float pivotRow,
                              float scaleX, float scaleY, float pixelsPerUnit, string suffix)
    {
        // Unity measures sprite rects from the BOTTOM-left corner.
        float x = Mathf.Round(ColumnLeft[column] * scaleX);
        float y = Mathf.Round((SheetHeight - bottom) * scaleY);
        float width = Mathf.Min(Mathf.Round(ColumnWidth * scaleX), sheet.width - x);
        float height = Mathf.Min(Mathf.Round((bottom - top) * scaleY), sheet.height - y);

        // Pivot in the middle of the column, at the given row.
        Vector2 pivot = new Vector2(0.5f, (bottom - pivotRow) * scaleY / height);

        Sprite sprite = Sprite.Create(sheet, new Rect(x, y, width, height), pivot, pixelsPerUnit, 0, SpriteMeshType.FullRect);
        sprite.name = sheet.name + suffix + column;
        return sprite;
    }

    private void CreateShadow()
    {
        // Child of the owl: same scale as the owl, and hidden together with it.
        GameObject shadow = new GameObject("OwlShadow");
        shadow.transform.SetParent(transform, false);

        shadowRenderer = shadow.AddComponent<SpriteRenderer>();
        shadowRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
        shadowRenderer.sortingOrder = spriteRenderer.sortingOrder;
        shadowRenderer.sharedMaterial = spriteRenderer.sharedMaterial;
        shadowRenderer.enabled = false; // shown once it is on the ground
    }
}
