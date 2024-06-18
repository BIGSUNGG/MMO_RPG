using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Protocol;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class ObjectController : MonoBehaviour, RpcObjectFunction
{
    public ObjectController()
    {
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {

    }

    #region Object
    public bool bCreated { get; private set; } = false;
    public int ObjectId { get; private set; }
    public GameObjectType ObjectType { get; protected set; } = GameObjectType.Unknown;

    public virtual void Registered(int id)
    {
        if (bCreated)
        {
            Debug.LogWarning("Already created");
            return;
        }

        bCreated = true;
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

    public virtual void Multicast_SetPosition(Vector3 position)
    {
        // ��Ŷ ������
        if (Managers.Network.IsClient) // Ŭ���̾�Ʈ���� ȣ��� ��� 
        {
            Debug.LogError("Client can't call this function");
            return;
        }
        else // �������� ȣ��� ���
        {
            S_RpcObjectFunction rpcFuncPacket = new S_RpcObjectFunction();

            byte[] parameterBuffer = new byte[12];
            Array.Copy(BitConverter.GetBytes((float)position.x), 0, parameterBuffer, 0, sizeof(float));
            Array.Copy(BitConverter.GetBytes((float)position.y), 0, parameterBuffer, 4, sizeof(float));
            Array.Copy(BitConverter.GetBytes((float)position.z), 0, parameterBuffer, 8, sizeof(float));

            rpcFuncPacket.ObjectId = ObjectId;
            rpcFuncPacket.AbsolutelyExcute = true;
            rpcFuncPacket.RpcFunctionId = RpcObjectFunctionId.MulticastSetPosition;
            rpcFuncPacket.ParameterBytes = ByteString.CopyFrom(parameterBuffer);
            Managers.Network.SendMulticast(rpcFuncPacket);
        }

        Multicast_SetPosition_Implementation(position);
    }

    protected virtual void Multicast_SetPosition_ReceivePacket(byte[] packet)
    {
        try
        {
            Vector3 position = new Vector3();
            position.x = BitConverter.ToSingle(packet, 0); 
            position.y = BitConverter.ToSingle(packet, 4); 
            position.z = BitConverter.ToSingle(packet, 8);

            Multicast_SetPosition_Implementation(position);

            if (Managers.Network.IsServer) // �������� ��Ŷ�� �޾��� ���
            {
                // �ٸ� Ŭ���̾�Ʈ���� ��Ŷ ������
                S_RpcObjectFunction rpcFuncPacket = new S_RpcObjectFunction();
                rpcFuncPacket.ObjectId = ObjectId;
                rpcFuncPacket.RpcFunctionId = RpcObjectFunctionId.MulticastSetPosition;
                Managers.Network.SendMulticast(rpcFuncPacket);
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
        }
    }

    protected virtual bool Multicast_SetPosition_Validate(byte[] packet)
    {
        try
        {
            return false; // �� �Լ��� Ŭ���̾�Ʈ�κ��� ���� ������
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
            return false;
        }
    }

    protected virtual void Multicast_SetPosition_Implementation(Vector3 position)
    {
        gameObject.transform.position = position;
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
        try
        {
            switch (functionId)
            {
                case RpcObjectFunctionId.MulticastSetPosition:
                    Multicast_SetPosition_ReceivePacket(packet);
                    break;
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
        }
    }

    // Ŭ���̾�Ʈ���� ���� ��Ŷ�� �Ǽ� ��Ŷ���� Ȯ��
    // functionId : ���� ��Ŷ�� �Լ� ���̵�
    // packet : ���� ��Ŷ�� ����Ʈ �迭
    // return : ���� ��Ŷ�� �Ǽ� ��Ŷ�� �ƴ���
    public virtual bool RpcFunction_Validate(RpcObjectFunctionId functionId, byte[] packet)
    {
        try
        {
            switch (functionId)
            {
                case RpcObjectFunctionId.MulticastSetPosition:
                    return Multicast_SetPosition_Validate(packet);
                default:
                    break;
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
        }

        Debug.Log("Receive wrong function id");
        return false;
    }

    #endregion
}
