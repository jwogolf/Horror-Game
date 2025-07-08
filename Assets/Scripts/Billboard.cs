using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] BillboardType type;
    public enum BillboardType {LookAtCamera, CameraForward};

    [SerializeField] private bool lockX;
    [SerializeField] private bool lockY;
    [SerializeField] private bool lockZ;
    private Vector3 originialRotation;

    void Awake()
    {
        originialRotation = transform.rotation.eulerAngles;
    }

    void Update()
    {
        switch (type)
        {
            case BillboardType.LookAtCamera:
                transform.LookAt(Camera.main.transform.position, Vector3.up);
                break;
            case BillboardType.CameraForward:
                transform.forward = Camera.main.transform.forward;
                break;
            default:
                break;
        }

        Vector3 rotation = transform.rotation.eulerAngles;
        if (lockX) { rotation.x = originialRotation.x; }
        if (lockY) { rotation.y = originialRotation.y; }
        if (lockZ) { rotation.z = originialRotation.z; }
        transform.rotation = Quaternion.Euler(rotation);
    }
}
