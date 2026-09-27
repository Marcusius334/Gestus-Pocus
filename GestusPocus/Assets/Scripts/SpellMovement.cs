using UnityEngine;

public class DragObject : MonoBehaviour
{
    private Vector2 originalPosition;

    private void OnMouseDown()
    {
        originalPosition = transform.position;
    }

    private void OnMouseDrag()
    {
        //Se convierte la posicion del raton en coordenadas 2D
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        transform.position = mousePosition;
    }

    private void OnMouseUp()
    {
        Debug.Log("Hechizo");
        transform.position = originalPosition;
    }
}