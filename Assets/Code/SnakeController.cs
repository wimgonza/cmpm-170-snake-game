using System.Collections;
using System.Collections.Generic;
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

    // Initialize inputs
    void OnEnable()
    {
        steerAction = new InputAction("Steer", binding: "<Keyboard>/a");
        steerAction.AddBinding("<Keyboard>/d");
        steerAction.AddBinding("<Keyboard>/arrowLeft");
        steerAction.AddBinding("<Keyboard>/arrowRight");

        steerAction.Enable();
    }

    // Set up disable inputs when no longer needed
    void OnDisable()
    {
        steerAction.Disable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GrowSnake();
        GrowSnake();
        GrowSnake();
        GrowSnake();
        GrowSnake();

        SpawnApple();
    }

    // Update is called once per frame
    void Update()
    {
        // Moving forward
        transform.position += transform.forward * MoveSpeed * Time.deltaTime;

        // Steering
        float steerDirection = 0f;
        if (Keyboard.current.aKey.isPressed)
        {
            steerDirection = -1f;
        }
        else if (Keyboard.current.dKey.isPressed)
        {
            steerDirection = 1f;
        }

        if (Keyboard.current.leftArrowKey.isPressed)
        {
            steerDirection = -1;
        }
        else if (Keyboard.current.rightArrowKey.isPressed)
        {
            steerDirection = 1;
        }
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
    }

    // Adding snake body
    private void GrowSnake()
    {
        GameObject body = Instantiate(BodyPrefab);
        BodyParts.Add(body);
    }

    // Spawning the apple
    private void SpawnApple()
    {
        float xPos = Random.Range(-5f, 5f);
        float zPos = Random.Range(-5f, 5f);

        Vector3 applePosition = new Vector3(xPos, -2f, zPos);

        Instantiate(ApplePrefab, applePosition, Quaternion.identity);
    }

    // Collision check for apple
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered with: " + other.gameObject.name);

        if (other.gameObject.tag == "Apple")
        {
            Debug.Log("Apple hit!");
            Destroy(other.gameObject);
            SpawnApple();
            StartCoroutine(EnlargeSnakeTemporarily());
        }
    }
    
    // Enlarge snake for short period of time
    private IEnumerator EnlargeSnakeTemporarily()
    {
        transform.localScale = EnlargedScale;

        foreach (var body in BodyParts)
        {
            body.transform.localScale = EnlargedScale;
        }

        yield return new WaitForSeconds(EnlargeDuration);

        transform.localScale = Vector3.one;
        foreach (var body in BodyParts)
        {
            body.transform.localScale = Vector3.one;
        }
    }
}
