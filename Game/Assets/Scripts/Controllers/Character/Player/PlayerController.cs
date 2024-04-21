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
    protected override void Start()
    {
        base.Start();
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

            if (Input.GetKey(KeyCode.Space))
                _movement.Jump();

            _movement._bIsRunning = Input.GetKey(KeyCode.LeftShift);
                
        }

        if(_camera)
        {
            _camera.transform.position = new UnityEngine.Vector3(0.0f, 10.0f, -3.0f) + transform.position;
            _camera.transform.eulerAngles = new UnityEngine.Vector3(70, 0, 0);
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // ¸¶¿ì½º ÁÂÇ¥¿¡¼­ ½î´Â ray
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
