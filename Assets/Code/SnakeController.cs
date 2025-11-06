using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

public class SnakeController : MonoBehaviour
{
    public float MoveSpeed = 5;
    public float SteerSpeed = 180f;
    public float BodySpeed = 5;
    public int Gap = 100;
    private InputAction steerAction;

    public GameObject BodyPrefab;
    public GameObject ApplePrefab;

    private List<GameObject> BodyParts = new List<GameObject>();
    private List<Vector3> PositionsHistory = new List<Vector3>();

    public Vector3 EnlargedScale = new Vector3(2f, 2f, 2f);
    public float EnlargeDuration = 5f;

    // explosion settings
    public float DetachImpulse = 200f;
    public float DetachedLifetime = 3f;

    void OnEnable()
    {
        steerAction = new InputAction("Steer", binding: "<Keyboard>/a");
        steerAction.AddBinding("<Keyboard>/d");
        steerAction.AddBinding("<Keyboard>/arrowLeft");
        steerAction.AddBinding("<Keyboard>/arrowRight");
        steerAction.Enable();
    }

    void OnDisable()
    {
        steerAction.Disable();
    }

    void Start()
    {
        GrowSnake();
        GrowSnake();
        GrowSnake();
        GrowSnake();
        GrowSnake();

        SpawnApple();
    }

    void Update()
    {
        // Moving forward
        transform.position += transform.forward * MoveSpeed * Time.deltaTime;

        // Steering
        float steerDirection = 0f;
        if (Keyboard.current.aKey.isPressed)
            steerDirection = -1f;
        else if (Keyboard.current.dKey.isPressed)
            steerDirection = 1f;

        if (Keyboard.current.leftArrowKey.isPressed)
            steerDirection = -1;
        else if (Keyboard.current.rightArrowKey.isPressed)
            steerDirection = 1;

        transform.Rotate(Vector3.up * steerDirection * SteerSpeed * Time.deltaTime);

        // Store position history
        PositionsHistory.Insert(0, transform.position);

        // Move body parts
        int index = 0;
        foreach (var body in BodyParts)
        {
            Vector3 point = PositionsHistory[Mathf.Min(index * Gap, PositionsHistory.Count - 1)];
            Vector3 moveDirection = point - body.transform.position;
            body.transform.position += moveDirection * BodySpeed * Time.deltaTime;
            body.transform.LookAt(point);
            index++;
        }

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            DetachLastBody();
        }
    }

    private void GrowSnake()
    {
        GameObject body = Instantiate(BodyPrefab);
        BodyParts.Add(body);
    }

    private void SpawnApple()
    {
        float xPos = Random.Range(-5f, 5f);
        float zPos = Random.Range(-5f, 5f);
        Vector3 applePosition = new Vector3(xPos, -2f, zPos);
        Instantiate(ApplePrefab, applePosition, Quaternion.identity);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered with: " + other.gameObject.name);

        if (other.gameObject.CompareTag("Apple"))
        {
            Debug.Log("Apple hit!");
            Destroy(other.gameObject);

            GrowSnake();

            SpawnApple();
            StartCoroutine(EnlargeSnakeTemporarily());
        }
        else if (other.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Wall hit");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private IEnumerator EnlargeSnakeTemporarily()
    {
        transform.localScale = EnlargedScale;
        foreach (var body in BodyParts)
            body.transform.localScale = EnlargedScale;

        yield return new WaitForSeconds(EnlargeDuration);

        transform.localScale = Vector3.one;
        foreach (var body in BodyParts)
            body.transform.localScale = Vector3.one;
    }


    private void DetachLastBody()
    {
        if (BodyParts.Count == 0) return;

        GameObject lastBody = BodyParts[BodyParts.Count - 1];
        BodyParts.RemoveAt(BodyParts.Count - 1);

        // Detach from the snake
        lastBody.transform.parent = null;

        // Ensure collider exists for visuals (optional)
        Collider col = lastBody.GetComponent<Collider>();
        if (col == null)
            col = lastBody.AddComponent<BoxCollider>();
        col.isTrigger = false;

        // Make sure physics doesn't move it
        Rigidbody rb = lastBody.GetComponent<Rigidbody>();
        if (rb == null)
            rb = lastBody.AddComponent<Rigidbody>();

        rb.isKinematic = true; // stay still
        rb.useGravity = false;

        // Add and initialize explosive behavior
        ExplosiveSegment explosive = lastBody.AddComponent<ExplosiveSegment>();
        explosive.Initialize(DetachedLifetime);
    }


}


public class ExplosiveSegment : MonoBehaviour
{
    public float explosionRadius = 5f; // how far the explosion reaches
    private float lifetime;

    public void Initialize(float lifetime)
    {
        this.lifetime = lifetime;
        StartCoroutine(ExplosionCountdown());
    }

    private IEnumerator ExplosionCountdown()
    {
        // Optional: small indicator or glow before explosion
        yield return new WaitForSeconds(lifetime);

        // Find all colliders within the radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in hitColliders)
        {
            if (hit.CompareTag("Wall"))
            {
                Debug.Log("Destroyed wall: " + hit.name);
                Destroy(hit.gameObject);
            }
        }

        // Optional: explosion visual or sound effect here

        Destroy(gameObject); // destroy the explosive segment after the blast
    }

    // For debugging — visualize the blast radius in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}


