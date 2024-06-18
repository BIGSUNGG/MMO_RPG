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

    public virtual void Multicast_SetPosition(Vector3 position)
    {
        // 패킷 보내기
        if (Managers.Network.IsClient) // 클라이언트에서 호출된 경우 
        {
            Debug.LogError("Client can't call this function");
            return;
        }
        else // 서버에서 호출된 경우
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

            if (Managers.Network.IsServer) // 서버에서 패킷을 받았을 경우
            {
                // 다른 클라이언트에게 패킷 보내기
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
            return false; // 이 함수는 클라이언트로부터 받지 않을것
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

    // 클라이언트에서 받은 패킷이 악성 패킷인지 확인
    // functionId : 받은 패킷의 함수 아이디
    // packet : 받은 패킷의 바이트 배열
    // return : 받은 패킷이 악성 패킷이 아닌지
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
