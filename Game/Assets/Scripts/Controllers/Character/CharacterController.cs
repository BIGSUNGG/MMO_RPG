using Google.Protobuf.Protocol;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
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

    #region Sync
    protected class CharacterSyncInfo : ObjectSyncInfo
    {
        public Vector3 pos;
        public Vector3 angle;
        public Vector3 velocity;
    }

    public override void ObjectSync(string infoJson)
    {
        CharacterSyncInfo info = JsonConvert.DeserializeObject<CharacterSyncInfo>(infoJson);
        ObjectSync(info);
    }

    protected void ObjectSync(CharacterSyncInfo info)
    {
        transform.position = info.pos;
        transform.eulerAngles = info.angle;
        _movement._velocity = info.velocity;

        base.ObjectSync(info);
    }

    public override string GetObjectSyncInfo()
    {
        CharacterSyncInfo info = new CharacterSyncInfo();
        return JsonConvert.SerializeObject(info);
    }

    protected void GetObjectSyncInfo(CharacterSyncInfo info)
    {
        info.pos = transform.position;
        info.angle = transform.eulerAngles;
        info.velocity = _movement._velocity;

        base.GetObjectSyncInfo(info);
    }
    #endregion

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
}
