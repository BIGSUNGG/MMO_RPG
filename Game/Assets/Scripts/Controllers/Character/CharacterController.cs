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
    protected CharacterAnimParameter _anim = null;
    protected CharacterMovementComponent _movement = null;
    protected HealthComponent _health = null;

    public CharacterController()
    {
        ObjectType = GameObjectType.Character;
    }

    protected override void Start()
    {
        base.Start();

        _anim = GetComponent<CharacterAnimParameter>();
        if (_anim == null)
            Debug.Log("CharacterAnimParameter is null");

        _movement = GetComponent<CharacterMovementComponent>();
        if (_movement == null)
            Debug.Log("MovementComponent is null");

        _health = GetComponent<HealthComponent>();
        if (_health == null)
            Debug.Log("HealthComponent is null");
    }

    protected override void Update()
    {
        base.Update();

    }

    #region Controller
    public override void ControllerUpdate()
    {
        base.ControllerUpdate();

        if (_movement)
        {
            if (Input.GetKey(KeyCode.W))
                _movement.MoveForward(1.0f);
            if (Input.GetKey(KeyCode.S))
                _movement.MoveForward(-1.0f);

            if (Input.GetKey(KeyCode.A))
                _movement.MoveRight(-1.0f);
            if (Input.GetKey(KeyCode.D))
                _movement.MoveRight(1.0f);

            _movement._bIsRunning = Input.GetKey(KeyCode.LeftShift);
        }
    }

    public override void OnPossess()
    {
        base.OnPossess();

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

        if(_movement)
        {
            _movement.Sync(info.position, info.rotation, info.inputDir, info.bIsRunning);
        }

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
        info.inputDir  = _movement._lastInputDir;
        info.bIsRunning = _movement._bIsRunning;

        base.GetObjectSyncInfo(info);
    }
    #endregion
}
