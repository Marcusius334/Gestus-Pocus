using UnityEngine;
using UnityEngine.UI;

public class LevelSelectManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Transform padre = transform;

        for (int i = 0; i < padre.childCount; i++)
        {
            if (i >= GameManager.levelsUnlocked)
            {
                Transform hijo = padre.GetChild(i);

                // Desactivar el botón
                Button boton = hijo.GetComponent<Button>();
                if (boton != null)
                {
                    boton.interactable = false;
                }

                // Reducir la transparencia de la imagen
                Image imagen = hijo.GetComponent<Image>();
                if (imagen != null)
                {
                    Color color = imagen.color;
                    color.a = 0.3f; // 30% de opacidad
                    imagen.color = color;
                }
            }
        }
    }
}
