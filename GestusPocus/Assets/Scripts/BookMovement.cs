using UnityEngine;
using System.Collections;

public class Bookdisplacement : MonoBehaviour
{
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool open;

    [Header("Book movement")]
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private float displacement = 3f;
    [SerializeField] private string[] layerNames;
    
    [Header("Managers")]
    public ErrorsManager errorManager;
    public AudioManager audioManager;

    private void Start()
    {
        // Guardamos la posición inicial del objeto.
        openPosition = transform.position;

        // Posición a la que se desplazará al cerrarse.
        closedPosition = openPosition + Vector3.down * displacement;

        open = true;

        transform.position = openPosition;
    }

    private void OnMouseDown()
    {
        if(errorManager.bookUses <= 0){
            //Si no quedan usos de libros no hace nada
            return;
        }
        
        StopAllCoroutines();
        audioManager.PlayOne(audioManager.soundBook);

        if (open)//Cerrar
        {
            errorManager.BookUsed();
            ActiveLayerObjects(true);
            open = false;
            StartCoroutine(MoveObject(transform.position, closedPosition));
        }
        else//Abrir
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
        
        int layer0 = LayerMask.NameToLayer(layerNames[0]);
        int layer1 = LayerMask.NameToLayer(layerNames[1]);

        foreach (GameObject obj in FindObjectsByType<GameObject>(FindObjectsInactive.Include))
        {
            if (obj.layer == layer0 || obj.layer == layer1)
            {
                Debug.Log(obj.layer);
                obj.SetActive(active);

                if(obj.layer == layer1){
                    Debug.Log("AA");
                    Owl owl = obj.GetComponent<Owl>();

                    if (owl != null)
                    {
                        Debug.Log("BB");
                        StartCoroutine(disableClue(owl));
                    }
                }
            }
        }
    }

    IEnumerator disableClue(Owl owl)
    {
        yield return new WaitForSeconds(0.01f);

        owl.disableClueBook();
    }
        
}