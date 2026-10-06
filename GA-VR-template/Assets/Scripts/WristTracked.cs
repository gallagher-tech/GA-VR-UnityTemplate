using UnityEngine;

public class WristTracked : MonoBehaviour
{
    [Header("Hand Transform Settings")]
    public Transform handTransform; // Drag the hand (wrist) transform here in the Inspector

    [Header("Menu Positioning")]
    public Vector3 positionOffset = new Vector3(0f, 0f, 0f); // Offset the menu's position relative to the hand
    public Vector3 rotationOffset = new Vector3(0f, 0f, 0f); // Offset the menu's rotation relative to the hand

    [Header("Smoothing Settings")]
    public float smoothTime = 0.1f; // Time to smooth the position/rotation movement

    private Vector3 velocity = Vector3.zero;
    private Quaternion rotationVelocity;

    void Update()
    {
        if (handTransform != null)
        {
            UpdateMenuPosition();
        }
        else
        {
            Debug.LogWarning("Hand Transform is not assigned. Please assign the wrist transform in the Inspector.");
        }
    }

    void UpdateMenuPosition()
    {
        // Calculate the target position and rotation with the offsets
        Vector3 targetPosition = handTransform.position + positionOffset;
        Quaternion targetRotation = handTransform.rotation * Quaternion.Euler(rotationOffset);

        // Smoothly update the position and rotation of the menu to follow the hand
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothTime);
    }
}
