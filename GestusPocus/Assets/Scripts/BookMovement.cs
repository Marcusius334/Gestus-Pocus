using UnityEngine;
using System.Collections;

public class Bookdisplacement : MonoBehaviour
{
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool open;

    [SerializeField] private float duration = 0.3f;
    [SerializeField] private float displacement = 3f;
    [SerializeField] private string layerName = "SpellCheck";

    private void Start()
    {
        open = false;

        // Guardamos la posición inicial del objeto.
        openPosition = transform.position;

        // Posición a la que se desplazará al cerrarse.
        closedPosition = openPosition + Vector3.down * displacement;

        transform.position = closedPosition;
    }

    private void OnMouseDown()
    {
        StopAllCoroutines();

        if (open)
        {
            ActiveLayerObjects(true);
            open = false;
            StartCoroutine(MoveObject(transform.position, closedPosition));
        }
        else
        {
            ActiveLayerObjects(false);
            open = true;
            StartCoroutine(MoveObject(transform.position, openPosition));
        }
    }

    private IEnumerator MoveObject(Vector3 start, Vector3 end)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(start, end, t);

            yield return null;
        }

        transform.position = end;
    }

    //Esto es para que no se puedan activar hechizos si el libro está abierto
    private void ActiveLayerObjects(bool active)
    {
        int layer = LayerMask.NameToLayer(layerName);

        foreach (GameObject obj in FindObjectsByType<GameObject>(FindObjectsInactive.Include))
        {
            if (obj.layer == layer)
            {
                obj.SetActive(active);
            }
        }
    }
}