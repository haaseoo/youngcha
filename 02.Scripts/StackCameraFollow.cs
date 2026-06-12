using UnityEngine;

public class StackCameraFollow : MonoBehaviour
{
    [Header("Follow Target")]
    public Transform followTarget;

    [Header("Follow Setting")]
    public float smoothSpeed = 2.5f;

    [Header("Follow Axis")]
    public bool followYOnly = true;

    [Header("Optional Look At")]
    public bool useLookAt = false;

    private Vector3 initialCameraPosition;
    private Quaternion initialCameraRotation;
    private float yOffset;

    private void Start()
    {
        initialCameraPosition = transform.position;
        initialCameraRotation = transform.rotation;

        if (followTarget != null)
        {
            yOffset = transform.position.y - followTarget.position.y;
        }
    }

    private void LateUpdate()
    {
        if (followTarget == null) return;

        Vector3 targetPosition;

        if (followYOnly)
        {
            // 기존 45도 구도는 유지하고, 카메라 높이만 따라 올라감
            targetPosition = new Vector3(
                initialCameraPosition.x,
                followTarget.position.y + yOffset,
                initialCameraPosition.z
            );
        }
        else
        {
            // 필요하면 X, Y, Z 전체를 따라가게 할 수 있음
            targetPosition = followTarget.position + (initialCameraPosition - followTarget.position);
        }

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            Time.deltaTime * smoothSpeed
        );

        if (useLookAt)
        {
            transform.LookAt(followTarget);
        }
        else
        {
            // 카메라 회전값 고정
            transform.rotation = initialCameraRotation;
        }
    }
}