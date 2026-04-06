using UnityEngine;

public class UISpinner : MonoBehaviour
{
    [SerializeField] private float speed = 300f;

    void Update()
    {
        transform.Rotate(0f, 0f, -speed * Time.unscaledDeltaTime);
    }
}