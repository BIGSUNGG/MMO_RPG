using Google.Protobuf.Collections;
using Google.Protobuf.Protocol;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectController : MonoBehaviour
{
    public ObjectController()
    {
        ObjectType = GameObjectType.Character;
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {

    }

    #region Object
    public int ObjectId { get; private set; }
    public GameObjectType ObjectType { get; protected set; }

    public virtual void Created(int id)
    {
        ObjectId = id;
    }

    #endregion

    #region Sync
    [Serializable]
    protected class ObjectSyncInfo
    {
    }

    public virtual void ObjectSync(string infoJson)
    {
        ObjectSyncInfo info = JsonUtility.FromJson<ObjectSyncInfo>(infoJson);
        ObjectSync(info);
    }

    protected void ObjectSync(ObjectSyncInfo info)
    {
    }

    public virtual string GetObjectSyncInfo()
    {
        ObjectSyncInfo info = new ObjectSyncInfo();
        return JsonUtility.ToJson(info);
    }

    protected void GetObjectSyncInfo(ObjectSyncInfo info)
    {
    }
    #endregion

    #region Controller
    // 플레이어가 빙의하고 있다면 매 틱마다 호출
    public virtual void ControllerUpdate()
    {

    }

    public virtual void OnPossess()
    {

    }

    public virtual void OnUnpossess()
    {

    }

    // return : 플레이어가 빙의하고 있는지
    public virtual bool IsLocallyControlled()
    {
        return this == Managers.Controller.MyController;
    }
    #endregion

}
