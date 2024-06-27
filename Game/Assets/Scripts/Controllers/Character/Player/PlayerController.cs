using Google.Protobuf;
using Google.Protobuf.Protocol;
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
    public PlayerController()
    {
        _characterType = CharacterType.Player;
    }

    protected override void Start()
    {
        base.Start();

        PlayerAnim = GetComponent<PlayerAnimParameter>();
        if (PlayerAnim == null)
            Debug.LogWarning("PlayerAnimParameter is null");

        _playerMovement = GetComponent<PlayerMovementComponent>();
        if (_playerMovement == null)
            Debug.LogWarning("PlayerMovementComponent is null");

        _playerMovement._onDodgeStartEvent.AddListener(() =>
        {
            if (IsLocallyControlled())
                Multicast_ComboEnd();
        });

        Inventory = GetComponent<InventoryComponent>();
        if (Inventory == null)
            Debug.LogWarning("InventoryComponent is null");
    }

    protected override void Update()
    {
        base.Update();

    }

    #region Player
    public ClientSession Session = null;
    #endregion

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
        if(CanRotate())
        {
            LookMousePos();
        }
    }

    protected virtual void LookMousePos()
    {
        if (IsLocallyControlled() == false)
        {
            Debug.LogError("This function must called on locally controller");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            Vector3 dir = hit.point - transform.position;
            transform.eulerAngles = new Vector3(0.0f, Util.GetAngleY(dir), 0.0f);
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
    public InventoryComponent Inventory
    {
        get
        {
            if (_inventory == null)
                _inventory = GetComponent<InventoryComponent>();

            return _inventory;
        }
        set { _inventory = value; }
    }
    private InventoryComponent _inventory;

    public PlayerMovementComponent PlayerMovement
    {
        get
        {
            if (_playerMovement == null)
                _playerMovement = GetComponent<PlayerMovementComponent>();

            return _playerMovement;
        }
        set { _playerMovement = value; }
    }
    private PlayerMovementComponent _playerMovement;

    public PlayerAnimParameter PlayerAnim
    {
        get
        {
            if (_playerAnim == null)
                _playerAnim = GetComponent<PlayerAnimParameter>();

            return _playerAnim;
        }
        set { _playerAnim = value; }
    }
    private PlayerAnimParameter _playerAnim;
    #endregion

    #region Object
    GameObject _camera = null;
    #endregion

    #region Attack
    protected override void Multicast_ComboAttack_Implementation(int combo)
    {
        base.Multicast_ComboAttack_Implementation(combo);

        if(IsLocallyControlled())
        {
            LookMousePos();
        }
    }
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
