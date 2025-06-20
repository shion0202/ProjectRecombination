using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 20.0f;
    [SerializeField] private int damage = 0;
    [SerializeField] private float lifetime = 1.0f;
    
    private Vector3 _direction;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("DetectionCollider")) return;
        
        if (!other.TryGetComponent(out Monster monster)) return;
        monster.TakeDamage(damage);
        Destroy(gameObject);
    }
    
    public void SetBulletDirection(Vector3 direction)
    {
        _direction = direction.normalized;
        transform.forward = _direction; // Set the bullet's forward direction
        GetComponent<Rigidbody>().linearVelocity = _direction * speed; // Set the bullet's velocity
    }
}
