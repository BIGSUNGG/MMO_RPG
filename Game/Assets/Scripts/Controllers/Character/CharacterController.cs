using Google.Protobuf;
using Google.Protobuf.Protocol;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class CharacterController : ObjectController
{
    public CharacterAnimParameter       _anim       { get; protected set; } = null;
    public CharacterMovementComponent   _movement   { get; protected set; } = null;
    public HealthComponent              _health     { get; protected set; } = null;
    public InventoryComponent           _inventory  { get; protected set; } = null;
    public CapsuleCollider              _capsule    { get; protected set; } = null;

    public CharacterController()
    {
        ObjectType = GameObjectType.Knight;
    }

    protected override void Start()
    {
        base.Start();

        _anim = GetComponent<CharacterAnimParameter>();
        if (_anim == null)
            Debug.LogWarning("CharacterAnimParameter is null");

        _movement = GetComponent<CharacterMovementComponent>();
        if (_movement == null)
            Debug.LogWarning("MovementComponent is null");

        _health = GetComponent<HealthComponent>();
        if (_health == null)
            Debug.LogWarning("HealthComponent is null");

        _inventory = GetComponent<InventoryComponent>();
        if (_inventory == null)
            Debug.LogWarning("InventoryComponent is null");

        _capsule = GetComponentInChildren<CapsuleCollider>();
        if (_capsule == null)
            Debug.LogWarning("CapsuleCollider is null");
    }

    protected override void Update()
    {
        base.Update();

    }

    #region Controller
    public Vector2 _inputDir = Vector2.zero;
    public override void ControllerUpdate()
    {
        base.ControllerUpdate();

        _inputDir = Vector2.zero;
        if (_movement)
        {
            _inputDir = Vector2.zero;

            if (CanMovementInput())
            {
                if (Input.GetKey(KeyCode.W))
                    _inputDir.y += 1.0f;
                if (Input.GetKey(KeyCode.S))
                    _inputDir.y -= 1.0f;

                if (Input.GetKey(KeyCode.A))
                    _inputDir.x -= 1.0f;
                if (Input.GetKey(KeyCode.D))
                    _inputDir.x += 1.0f;
            }

            _movement._bIsRunning = Input.GetKey(KeyCode.LeftShift);
        }
    }

    public override void OnPossess()
    {
        base.OnPossess();

    }

    public virtual bool CanAttack()
    {
        if (_health._bDead)
            return false;

        return true;
    }

    public virtual bool CanRotationInput()
    {
        if (_health._bDead)
            return false;

        return true;
    }

    public virtual bool CanMovementInput()
    {
        if (_health._bDead)
            return false;

        return true;
    }
    #endregion

    #region Sync
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    protected class CharacterSyncInfo : ObjectSyncInfo
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector2 inputDir;
        public bool bIsRunning;
    }

    public override void ObjectSync(ByteString syncInfo)
    {
        if (IsLocallyControlled())
            return;

        CharacterSyncInfo info = Util.BytesToObject<CharacterSyncInfo>(syncInfo.ToByteArray());
        ObjectSync(info);
    }

    protected void ObjectSync(CharacterSyncInfo info)
    {
        if (info == null)
            return;

        _inputDir = info.inputDir;

        if(_movement)
            _movement.Sync(info.position, info.rotation, info.bIsRunning);

        base.ObjectSync(info);
    }

    public override ByteString GetObjectSyncInfo()
    {
        CharacterSyncInfo info = new CharacterSyncInfo();
        GetObjectSyncInfo(info);
        return ByteString.CopyFrom(Util.ObjectToBytes<CharacterSyncInfo>(info));
    }

    protected void GetObjectSyncInfo(CharacterSyncInfo info)
    {
        if (_movement == null)
            return;

        info.position = transform.position;
        info.rotation = transform.rotation;
        info.inputDir  = _inputDir;
        info.bIsRunning = _movement._bIsRunning;

        base.GetObjectSyncInfo(info);
    }
    #endregion
}
