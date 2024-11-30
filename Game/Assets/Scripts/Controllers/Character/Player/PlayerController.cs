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
    public ClientSession _clientSession = null;
    public PlayerAnimParameter _playerAnim     { get; protected set; } = null;
    public PlayerMovementComponent  _playerMovement { get; protected set; } = null;

    protected override void Start()
    {
        base.Start();

        _playerAnim = GetComponent<PlayerAnimParameter>();
        if (_playerAnim == null)
            Debug.LogWarning("PlayerAnimParameter is null");

        _playerMovement = GetComponent<PlayerMovementComponent>();
        if (_playerMovement == null)
            Debug.LogWarning("PlayerMovementComponent is null");

    }

    protected override void Update()
    {
        base.Update();

    }

    #region Controller
    public override void ControllerUpdate()
    {
        if (IsLocallyControlled() == false)
            return;

        base.ControllerUpdate();

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

        // 마우스 방향으로 회전
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
        if (CanInput() == false)
            return false;
            
        return true;
    }

    public override bool CanAttack()
    {
        if (CanInput() == false)
            return false;

        if (_playerMovement._bIsdodging)
            return false;

        return base.CanAttack();
    }

    public override bool IsPlayerControlled()
    {
        return true;
    }
    #endregion

    #region Component
    public override void OnRespawnEvent()
    {
        base.OnRespawnEvent();

        transform.position = new UnityEngine.Vector3(0.0f, 3.0f, 0.0f);
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
