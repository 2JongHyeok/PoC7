// Purpose: Instantly follows target in 2D (SRP: camera follow)
using UnityEngine;

public sealed class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    private void LateUpdate()
    {
        if (target == null) return;
        transform.position = target.position + offset;
    }

    public void SetTarget(Transform followTarget)
    {
        target = followTarget;
    }
}
