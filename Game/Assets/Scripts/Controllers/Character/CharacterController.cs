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
    protected MovementComponent _movement = null;

    public CharacterController()
    {
        ObjectType = GameObjectType.Character;
    }

    protected override void Start()
    {
        base.Start();

        _movement = GetComponent<MovementComponent>();
        if (_movement == null)
            Debug.Log("MovementComponent is null");
    }

    protected override void Update()
    {
        base.Update();

    }

    #region Controller
    public override void ControllerUpdate()
    {
        base.ControllerUpdate();

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
        public Vector3 angle;
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
            _movement.Sync(info.position, info.angle);
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
        info.angle = transform.eulerAngles;

        base.GetObjectSyncInfo(info);
    }
    #endregion
}
