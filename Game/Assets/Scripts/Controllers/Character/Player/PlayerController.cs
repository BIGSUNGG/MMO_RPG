using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    #region Sync
    [Serializable]
    protected class PlayerSyncInfo : CharacterSyncInfo
    {
    }

    public override void ObjectSync(string infoJson)
    {
        if (IsLocallyControlled())
            return;

        PlayerSyncInfo info = JsonUtility.FromJson<PlayerSyncInfo>(infoJson);
        ObjectSync(info);
    }

    protected void ObjectSync(PlayerSyncInfo info)
    {
        base.ObjectSync(info);
    }

    public override string GetObjectSyncInfo()
    {
        PlayerSyncInfo info = new PlayerSyncInfo();
        GetObjectSyncInfo(info);
        return JsonUtility.ToJson(info);
    }

    protected void GetObjectSyncInfo(PlayerSyncInfo info)
    {
        base.GetObjectSyncInfo(info);
    }
    #endregion

    #region Controller
    public override void ControllerUpdate()
    {
        base.ControllerUpdate();

        if (IsLocallyControlled() == false)
            return;

        if (_movement == null)
            return;

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
    }

    public override void OnPossess()
    {
        base.OnPossess();

        transform.position = new UnityEngine.Vector3(1.0f, 3.0f, 1.0f);
    }
    #endregion
}
