using UnityEngine;

public class CameraLock : MonoBehaviour
{
    public Transform player;    
    public Vector3 offset = new Vector3(0, 5, -10); 
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (player == null)
            return;

        // Desired position based on player position + offset
        Vector3 targetPosition = player.position + offset;

        // Smoothly move the camera toward the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);


    }
}
