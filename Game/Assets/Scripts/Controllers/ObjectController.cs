using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Protocol;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ObjectController : MonoBehaviour, RpcObjectFunction
{
    public ObjectController()
    {
        ObjectType = GameObjectType.Knight;
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

    public virtual bool IsPlayerControlled()
    {
        return false;
    }

    #endregion

    #region Sync
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    protected class ObjectSyncInfo
    {
    }

    public virtual void ObjectSync(ByteString syncInfo)
    {
        ObjectSyncInfo info = Util.BytesToObject<ObjectSyncInfo>(syncInfo.ToByteArray());
        ObjectSync(info);
    }

    protected void ObjectSync(ObjectSyncInfo info)
    {
    }

    public virtual ByteString GetObjectSyncInfo()
    {
        ObjectSyncInfo info = new ObjectSyncInfo();
        GetObjectSyncInfo(info);
        return ByteString.CopyFrom(Util.ObjectToBytes<ObjectSyncInfo>(info));
    }

    protected void GetObjectSyncInfo(ObjectSyncInfo info)
    {
    }

    #endregion

    #region RpcFunction
    // 다른 클라이언트로 패킷을 받으면 FunctionId에 맞는 함수 호출
    // functionId : 받은 패킷의 함수 아이디
    // packet : 받은 패킷의 바이트 배열
    public virtual void RpcFunction_ReceivePacket(RpcObjectFunctionId functionId, byte[] packet)
    {

    }

    // 클라이언트에서 받은 패킷이 악성 패킷인지 확인
    // functionId : 받은 패킷의 함수 아이디
    // packet : 받은 패킷의 바이트 배열
    // return : 받은 패킷이 악성 패킷이 아닌지
    public virtual bool RpcFunction_Validate(RpcObjectFunctionId functionId, byte[] packet)
    {
        Debug.Log("Receive wrong function id");
        return false;
    }

    #endregion
}
