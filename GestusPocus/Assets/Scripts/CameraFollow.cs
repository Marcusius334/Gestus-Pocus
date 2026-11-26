using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Transform book;
    public Transform owl;
    public Vector3 offset;

    void LateUpdate()
    {
        transform.position = new Vector3(player.transform.position.x, transform.position.y, transform.position.z);
        book.transform.position = new Vector3(transform.position.x, book.transform.position.y, book.transform.position.z);
        owl.transform.position = new Vector3(transform.position.x + offset.x, owl.transform.position.y, owl.transform.position.z);
    }
}
