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
    // �÷��̾ �����ϰ� �ִٸ� �� ƽ���� ȣ��
    public virtual void ControllerUpdate()
    {

    }

    public virtual void OnPossess()
    {

    }

    public virtual void OnUnpossess()
    {

    }

    // return : �÷��̾ �����ϰ� �ִ���
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
    // �ٸ� Ŭ���̾�Ʈ�� ��Ŷ�� ������ FunctionId�� �´� �Լ� ȣ��
    // functionId : ���� ��Ŷ�� �Լ� ���̵�
    // packet : ���� ��Ŷ�� ����Ʈ �迭
    public virtual void RpcFunction_ReceivePacket(RpcObjectFunctionId functionId, byte[] packet)
    {

    }

    // Ŭ���̾�Ʈ���� ���� ��Ŷ�� �Ǽ� ��Ŷ���� Ȯ��
    // functionId : ���� ��Ŷ�� �Լ� ���̵�
    // packet : ���� ��Ŷ�� ����Ʈ �迭
    // return : ���� ��Ŷ�� �Ǽ� ��Ŷ�� �ƴ���
    public virtual bool RpcFunction_Validate(RpcObjectFunctionId functionId, byte[] packet)
    {
        Debug.Log("Receive wrong function id");
        return false;
    }

    #endregion
}
