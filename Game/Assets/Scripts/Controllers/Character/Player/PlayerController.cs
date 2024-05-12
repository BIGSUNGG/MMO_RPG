using Google.Protobuf;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.XR;

public class PlayerController : CharacterController
{
    protected PlayerAnimParameter _playerAnim = null;
    protected PlayerMovementComponent _playerMovement = null;

    protected override void Start()
    {
        base.Start();

        _playerAnim = GetComponent<PlayerAnimParameter>();
        if (_playerAnim == null)
            Debug.Log("PlayerAnimParameter is null");

        _playerMovement = GetComponent<PlayerMovementComponent>();
        if (_playerMovement == null)
            Debug.Log("PlayerMovementComponent is null");

    }

    protected override void Update()
    {
        base.Update();

    }

    #region Controller
    public override void ControllerUpdate()
    {
        base.ControllerUpdate();

        if (IsLocallyControlled() == false)
            return;

        if(_playerMovement)
        {
            if (Input.GetKeyDown(KeyCode.Space) && CanDodgeInput())
                _playerMovement.DodgeRollStart();
        }

        if(_camera)
        {
            _camera.transform.position = new UnityEngine.Vector3(0.0f, 10.0f, -3.0f) + transform.position;
            _camera.transform.eulerAngles = new UnityEngine.Vector3(70, 0, 0);
        }

        // ���콺 �������� ȸ��
        if(CanRotationInput())
        {
	        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
	        if (Physics.Raycast(ray, out var hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
	        {
	            Vector3 dir = hit.point - transform.position;
	            transform.eulerAngles = new Vector3(0.0f, Util.GetAngleY(dir), 0.0f);
	        }
        }
    }

    public override void OnPossess()
    {
        base.OnPossess();

        transform.position = new UnityEngine.Vector3(0.0f, 3.0f, 0.0f);

        _camera = GameObject.Find("Main Camera");

    }

    public virtual bool CanDodgeInput()
    {
        return true;
    }

    public override bool CanAttack()
    {
        if (_playerMovement._bIsdodging)
            return false;

        return true;
    }
    #endregion

    #region Object
    GameObject _camera = null;
    #endregion

    #region Sync
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    protected class PlayerSyncInfo : CharacterSyncInfo
    {
    }

    public override void ObjectSync(ByteString syncInfo)
    {
        if (IsLocallyControlled())
            return;

        PlayerSyncInfo info = Util.BytesToObject<PlayerSyncInfo>(syncInfo.ToByteArray());
        ObjectSync(info);
    }

    protected void ObjectSync(PlayerSyncInfo info)
    {
        base.ObjectSync(info);
    }

    public override ByteString GetObjectSyncInfo()
    {
        PlayerSyncInfo info = new PlayerSyncInfo();
        GetObjectSyncInfo(info);
        return ByteString.CopyFrom(Util.ObjectToBytes<PlayerSyncInfo>(info));
    }

    protected void GetObjectSyncInfo(PlayerSyncInfo info)
    {
        base.GetObjectSyncInfo(info);
    }
    #endregion
}
