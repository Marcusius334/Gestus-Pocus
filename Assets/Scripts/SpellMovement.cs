using UnityEngine;

public class SpellMovement : MonoBehaviour
{
    [SerializeField] private LayerMask triggerLayers;
    private Collider2D myCollider;
    private ContactFilter2D contactFilter;
    private readonly Collider2D[] results = new Collider2D[10];

    [SerializeField] private string spellName;
    private Vector3 originalLocalPosition;
    private Vector3 dragOffset;
    private Camera mainCamera;
    private bool dragging;

    [Header("Explanation animation")]
    [Tooltip("Sliced sprite sheet shown animated in the Explanation object when this spell is clicked. " +
             "If empty, the spell's own sprite is shown instead.")]
    [SerializeField] private Texture2D explanationSheet;
    [SerializeField] private float explanationFps = 12f;
    // Frames taken from explanationSheet in the editor, saved so they also work in builds.
    [SerializeField, HideInInspector] private Sprite[] explanationFrames = new Sprite[0];
    [SerializeField, HideInInspector] private Texture2D loadedExplanationSheet;

    public Sprite[] ExplanationFrames => explanationFrames;
    public float ExplanationFps => explanationFps;

    [Header("Managers")]
    public ErrorsManager errorManager;
    public AudioManager audioManager;
    public Explanation explanation;
    public bool explaining;

    private void Awake()
    {
        mainCamera = Camera.main;
        originalLocalPosition = transform.localPosition;

        myCollider = GetComponent<Collider2D>();
        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(triggerLayers);
        contactFilter.useTriggers = true;

        explaining = false;
    }

    private void OnMouseDown()
    {
        if (mainCamera == null) return;

        dragging = true;

        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(Input.mousePosition);

        mouseWorldPosition.z = transform.position.z;

        // Mantener el punto exacto donde se agarró el objeto.
        dragOffset = transform.position - mouseWorldPosition;

        //Poner explicación
        if(explaining == false){
            explaining = true;
            explanation.Explain(gameObject);
        }
    }

    private void OnMouseDrag()
    {
        if (!dragging || mainCamera == null) return;

        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(Input.mousePosition);

        mouseWorldPosition.z = transform.position.z;

        transform.position = mouseWorldPosition + dragOffset;
    }

    private void OnMouseUp()
    {
        if (!dragging) return;

        dragging = false;

        //Detectar si colisiona con spellCheck
        if (myCollider != null)
        {
            Debug.Log("hay collider");
            int count = myCollider.Overlap(contactFilter, results);
            Debug.Log("count -> "+count);

            for (int i = 0; i < count; i++)
            {
                Collider2D other = results[i];

                SpellCheck spellCheck = other.GetComponent<SpellCheck>();

                Debug.Log("spellCheck -> "+spellCheck);

                if (spellCheck != null)
                {
                    if (spellCheck.spellRequired == spellName){
                        if (spellCheck.linkedObject != null)
                        {
                            //Activar el hechizo si tiene el mismo nombre
                            audioManager.PlayOne(audioManager.soundCorrect);
                            spellCheck.ActivateSpell();
                        }
                    }else{
                        audioManager.PlayOne(audioManager.soundWrong);
                        errorManager.SpellFailed();
                    }
                }
            }
        }

        // Volver a la posición original dentro del padre.
        transform.localPosition = originalLocalPosition;

        Debug.Log("Hechizo");
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (explanationSheet == null)
        {
            explanationFrames = new Sprite[0];
            loadedExplanationSheet = null;
        }
        else if (explanationSheet != loadedExplanationSheet)
        {
            explanationFrames = SpriteSheetAnimator.LoadFramesFromTexture(explanationSheet, this);
            loadedExplanationSheet = explanationSheet;
        }
    }
#endif
}