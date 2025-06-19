using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 7.0f;
    public float sensitivity = 1.0f;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();   
    }

    // Update is called once per frame
    void Update()
    {
        rb.linearVelocity = new Vector3(Input.GetAxisRaw("Vertical") * speed, rb.linearVelocity.y, Input.GetAxisRaw("Horizontal") * speed * (-1));

        float mouse = Input.GetAxis("Mouse X");
        transform.Rotate(new Vector3(0.0f, mouse * sensitivity, 0.0f));

        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * 800.0f);
        }
    }
}
