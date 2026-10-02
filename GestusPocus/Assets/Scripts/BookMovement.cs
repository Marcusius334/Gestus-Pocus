using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class Bookdisplacement : MonoBehaviour, IPointerClickHandler
{
    private RectTransform rectTransform;
    private RectTransform canvasRect;
    private Vector2 closedPosition;
    private Vector2 openPosition;
    private bool open;

    [SerializeField] private float duration = 0.3f;
    [SerializeField, Range(0f, 1f)] private float percentage = 0.3f;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasRect = rectTransform.parent as RectTransform;

        open = false;

        CalcPositions();

        rectTransform.anchoredPosition = closedPosition;
    }

    private void CalcPositions()
    {
        float canvasHeight = canvasRect.rect.height;

        openPosition = Vector2.zero;

        // Queremos que quede visible el percentage superior.
        float displacement = canvasHeight * (1f - percentage);

        closedPosition = new Vector2(0f, -displacement);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        StopAllCoroutines();

        if (open)
        {
            open = false;
            StartCoroutine(MovePanel(rectTransform.anchoredPosition, closedPosition));
        }
        else
        {
            open = true;
            StartCoroutine(MovePanel(rectTransform.anchoredPosition, openPosition));
        }
    }

    private IEnumerator MovePanel(Vector2 start, Vector2 end)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            rectTransform.anchoredPosition =
                Vector2.Lerp(start, end, t);

            yield return null;
        }

        rectTransform.anchoredPosition = end;
    }
}