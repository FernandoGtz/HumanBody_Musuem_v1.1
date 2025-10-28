using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public float speed = 60f;

    void Update()
    {
        transform.Rotate(Vector3.up * speed * Time.deltaTime);
    }
}
