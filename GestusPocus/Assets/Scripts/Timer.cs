using UnityEngine;
using System.Collections;

public class Timer : MonoBehaviour
{
    public float duration;
    public SceneLoader sceneLoader;
    public string sceneLoad;

    void Awake(){
        StartCoroutine(Timeado(duration));
    }

    private IEnumerator Timeado(float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            yield return null;
        }

        sceneLoader.LoadScene(sceneLoad);
    }
}
