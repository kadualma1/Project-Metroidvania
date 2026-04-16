using UnityEngine;
using Unity.Cinemachine;
using System;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [SerializeField] private CinemachineCamera[] allVirtualCameras;

    [Header("Y Damping during player vertical movement")]
    [SerializeField] private float fallPanAmount = .25f;
    [SerializeField] private float fallYPanTime = .35f;
    public float FallSpeedYDampingChangeThreshold = -15f;

    public bool IsLerpingYDamping { get; private set; }
    public bool LerpedFromPlayerFalling { get; private set; }

    private Coroutine lerpYPanCoroutine;

    private CinemachineCamera currentCamera;
    private CinemachinePositionComposer positionComposer;

    private float normYPanAmount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        for (int i = 0; i < allVirtualCameras.Length; i++)
        {
            if (allVirtualCameras[i].enabled)
            {
                currentCamera = allVirtualCameras[i];
                positionComposer = currentCamera.GetComponent<CinemachinePositionComposer>();
                break;
            }
        }

        normYPanAmount = positionComposer.Damping.y;
    }

    public void SetLerpedFromPlayerFalling(bool v)
    {
        LerpedFromPlayerFalling = v;
    }

    #region Lerping Y Damping on Camera

    public void LerpYDamping(bool isPlayerFalling)
    {
        lerpYPanCoroutine = StartCoroutine(LerpYCoroutine(isPlayerFalling));
    }

    private IEnumerator LerpYCoroutine(bool isPlayerFalling)
    {
        IsLerpingYDamping = true;
        float startDampAmount = positionComposer.Damping.y;
        float endDampAmount = 0;

        if (isPlayerFalling)
        {
            endDampAmount = fallPanAmount;
            LerpedFromPlayerFalling = true;
        }
        else
        {
            endDampAmount = normYPanAmount;
        }

        float elapsedTime = 0f;
        while (elapsedTime < fallYPanTime)
        {
            elapsedTime += Time.deltaTime;

            float lerpedAmount = Mathf.Lerp(startDampAmount, endDampAmount, (elapsedTime / fallYPanTime));
            positionComposer.Damping.y = lerpedAmount;

            yield return null;
        }

        IsLerpingYDamping = false;
    }

    #endregion
}
