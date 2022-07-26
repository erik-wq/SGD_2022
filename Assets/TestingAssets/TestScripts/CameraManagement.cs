using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManagement : MonoBehaviour
{
    #region Serialized
    [SerializeField] private float MaxZoom = 25;
    [SerializeField] private float MinZoom = 15;
    [SerializeField] private float DefaultZoom = 20;
    [SerializeField] private float MaxZoomPlayersSpeed = 12;
    [SerializeField] private float MinZoomPlayersSpeed = 4;
    [SerializeField] private float ZoomingSpeed = 0.5f;
    [SerializeField] private CinemachineVirtualCamera _virualCamera;
    #endregion

    #region Private
    private float _playersSpeed = -1;
    private bool _isMoving = false;
    #endregion

    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {

    }

    private void FixedUpdate()
    {
        UpdateZoom();
    }

    private void UpdateZoom()
    {
        if (_playersSpeed == -1)
            return;

        var cameraDistance = _virualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance;

        float targetZoom = 0;
        if (_isMoving)
        {
            targetZoom = GetTargetZoom();
        }
        else
        {
            targetZoom = DefaultZoom;
        }

        if (Mathf.Abs(cameraDistance - targetZoom) > 0.1f)
        {
            var move = ZoomingSpeed * Time.fixedDeltaTime * ((cameraDistance < targetZoom) ? 1f : -1f);
            _virualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = cameraDistance + move;
        }
    }

    private float GetTargetZoom()
    {
        return ((_playersSpeed - MinZoomPlayersSpeed) / (MaxZoomPlayersSpeed - MinZoomPlayersSpeed) * (MaxZoom - MinZoom)) + MinZoom;

    }

    public void SetPlayersSpeed(float speed)
    {
        this._playersSpeed = speed;
        this._isMoving = true;
    }

    public void SetMoving(bool isMoving)
    {
        _isMoving = isMoving;
    }
}
