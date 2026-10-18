using UnityEngine;
using System.Collections;

public class CutsceneManager : MonoBehaviour
{
    public GameObject exit;
    public SceneLoader sceneLoader;
    public string sceneLoad;
    public float duracion = 20f;
    public float disableExitTime = 2f;

    private RectTransform contenido;
    private RectTransform pantalla;
    private bool exitEnabled;

    void Start()
    {
        contenido = GetComponent<RectTransform>();
        pantalla = contenido.parent as RectTransform;
        
        exitEnabled = false;

        StartCoroutine(SubirContenido());
    }

    IEnumerator SubirContenido()
    {
        float altoPantalla = pantalla.rect.height;
        float altoContenido = contenido.rect.height;

        Vector2 inicio = new Vector2(
            contenido.anchoredPosition.x,
            -altoPantalla / 2f - altoContenido / 2f
        );

        Vector2 final = new Vector2(
            contenido.anchoredPosition.x,
            contenido.anchoredPosition.y
        );

        contenido.anchoredPosition = inicio;

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float progreso = Mathf.Clamp01(tiempo / duracion);

            contenido.anchoredPosition =
                Vector2.Lerp(inicio, final, progreso);

            yield return null;
        }

        contenido.anchoredPosition = final;

        sceneLoader.LoadScene(sceneLoad);
    }

    private void OnMouseDown()
    {
        if(!exitEnabled){
            exitEnabled = true;
            exit.SetActive(true);
            StartCoroutine(disableExit());
        }else{
            sceneLoader.LoadScene(sceneLoad);
        }
    }

    IEnumerator disableExit()
    {
        yield return new WaitForSeconds(disableExitTime);

        exitEnabled = false;
        exit.SetActive(false);
    }
}