using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

public class CameraControlTrigger : MonoBehaviour
{
    public CustomInspectorObjects customInspectorObjects = new CustomInspectorObjects();

    private Collider2D collider;

    private void Start()
    {
        collider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (customInspectorObjects.PanCameraOnContact)
            {
                CameraManager.Instance.PanCameraOnContact(customInspectorObjects.PanDistance, customInspectorObjects.PanTime, customInspectorObjects.CameraPanDirection, false);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Vector2 exitDirection = (collision.transform.position - collider.bounds.center).normalized;

            if (customInspectorObjects.SwapCameras && customInspectorObjects.CameraOnLeft != null && customInspectorObjects.CameraOnRight != null)
            {
                CameraManager.Instance.SwapCameras(customInspectorObjects.CameraOnLeft, customInspectorObjects.CameraOnRight, exitDirection);
            }

            if (customInspectorObjects.PanCameraOnContact)
            {
                CameraManager.Instance.PanCameraOnContact(customInspectorObjects.PanDistance, customInspectorObjects.PanTime, customInspectorObjects.CameraPanDirection, true);
            }
        }
    }
}

[System.Serializable]
public class CustomInspectorObjects
{
    public bool SwapCameras = false;
    public bool PanCameraOnContact = false;

    [HideInInspector] public CinemachineCamera CameraOnLeft;
    [HideInInspector] public CinemachineCamera CameraOnRight;

    [HideInInspector] public PanDirection CameraPanDirection;
    [HideInInspector] public float PanDistance = 3f;
    [HideInInspector] public float PanTime = .35f;
}

public enum PanDirection
{
    Up,
    Down,
    Left,
    Right,
}

#if UNITY_EDITOR

[CustomEditor(typeof(CameraControlTrigger))]
public class CameraControlTriggerEditor : Editor
{
    private CameraControlTrigger cameraControlTrigger;

    private void OnEnable()
    {
        cameraControlTrigger = (CameraControlTrigger)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (cameraControlTrigger.customInspectorObjects.SwapCameras)
        {
            cameraControlTrigger.customInspectorObjects.CameraOnLeft =
                (CinemachineCamera)EditorGUILayout.ObjectField(
                    "Camera on Left",
                    cameraControlTrigger.customInspectorObjects.CameraOnLeft,
                    typeof(CinemachineCamera),
                    true
                );

            cameraControlTrigger.customInspectorObjects.CameraOnRight =
                (CinemachineCamera)EditorGUILayout.ObjectField(
                    "Camera on Right",
                    cameraControlTrigger.customInspectorObjects.CameraOnRight,
                    typeof(CinemachineCamera),
                    true
                );
        }

        if (cameraControlTrigger.customInspectorObjects.PanCameraOnContact)
        {
            cameraControlTrigger.customInspectorObjects.CameraPanDirection =
                (PanDirection)EditorGUILayout.EnumPopup(
                    "Camera Pan Direction",
                    cameraControlTrigger.customInspectorObjects.CameraPanDirection
                );

            cameraControlTrigger.customInspectorObjects.PanDistance =
                EditorGUILayout.FloatField(
                    "Pan Distance",
                    cameraControlTrigger.customInspectorObjects.PanDistance
                );

            cameraControlTrigger.customInspectorObjects.PanTime =
                EditorGUILayout.FloatField(
                    "Pan Time",
                    cameraControlTrigger.customInspectorObjects.PanTime
                );
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(cameraControlTrigger);
        }
    }
}
#endif