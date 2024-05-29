using Google.Protobuf.Protocol;
using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class HealthComponent : ObjectComponent
{
    protected enum RpcFunctionId
    {

    }

    protected override void Start()
    {
        base.Start();

        _curHp = _maxHp;
    }

    protected override void Update()
    {
        base.Update();
    }

    #region Health
    public bool _bDead { get; protected set; } = false; // 오브젝트가 죽어있는지
    public int _curHp { get; protected set; } // 현재 체력
    public int _maxHp { get; protected set; } = 100; // 최대 체력

    public UnityEvent _onTakeDamageEvent = new UnityEvent(); // 대미지를 받고 나서 호출되는 이벤트
    public UnityEvent _onDeathEvent = new UnityEvent(); // 대미지를 받고 나서 호출되는 이벤트

    #region Give Damage
    // 서버에서 오브젝트가 대미지를 줄 때 호출
    // damage : 줄 대미지
    // victim : 대미지를 받을 오브젝트
    // return : 최종적으로 준 대미지
    public int OnServer_GiveDamage(ObjectController victim, int damage)
    {
        if (Util.CheckFuncCalledOnServer() == false) // 서버에서 호출되지않은 경우
            return 0;

        HealthComponent health = victim.GetComponentInChildren<HealthComponent>();
        if (health == null)
            return 0;

        Client_GiveDamage(victim.ObjectId, damage);
        return health.OnServer_TakeDamage(_owner, damage);
    }

    // 서버에서 다른 클라이언트에게 데미지를 준것을 알리는 패킷을 보냄
    // victimId : 공격을 받은 오브젝트의 아이디
    // damage : 최종적으로 준 대미지
    protected virtual void Client_GiveDamage(int victimId, int damage)
    {
        // 패킷 보내기
        if (Managers.Network.IsClient) // 클라이언트에서 호출된 경우 
        {
            Debug.LogError("Client can't call this function");
        }
        else // 서버에서 호출된 경우
        {
            S_RpcComponentFunction rpcFuncPacket = new S_RpcComponentFunction();
            byte[] parameterBuffer = new byte[8];
            Array.Copy(BitConverter.GetBytes((int)victimId), 0, parameterBuffer, 0, sizeof(int));
            Array.Copy(BitConverter.GetBytes((int)damage), 0, parameterBuffer, 4, sizeof(int));

            rpcFuncPacket.ObjectId = _owner.ObjectId;
            rpcFuncPacket.AbsolutelyExcute = true;
            rpcFuncPacket.ComponentType = GameComponentType.HealthComponent;
            rpcFuncPacket.RpcFunctionId = RpcComponentFunctionId.ClientGiveDamage;
            rpcFuncPacket.ParameterBytes = ByteString.CopyFrom(parameterBuffer);

            PlayerController pc = _owner as PlayerController;
            if (pc)
            {
                Managers.Network.SendClient(pc._clientSession, rpcFuncPacket);
            }
        }
    }

    // Rpc 패킷을 받으면 호출
    // packet : 매개변수의 바이트 배열 
    protected virtual void Client_GiveDamage_ReceivePacket(byte[] packet)
    {
        try
        {
            int victimId = BitConverter.ToInt32(packet, 0);
            int damage = BitConverter.ToInt32(packet, 4);
            Client_GiveDamage_Implementation(victimId, damage);
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
        }
    }

    // 서버에서 패킷을 받았을 때 악성 패킷을 감지하기 위한 인증
    // packet : 받은 패킷의 바이트 배열
    // return : 받은 패킷이 악성 패킷이 아닌지
    protected virtual bool Client_GiveDamage_Validate(byte[] packet)
    {
        try
        {
            return false; // 클라이언트에서 받지 않을 Rpc 함수이기 때문에 무조건 false반환
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
            return false;
        }
    }

    // Client_GiveDamage 코드
    // victimId : 공격을 받은 오브젝트의 아이디
    // damage : 최종적으로 받은 대미지
    protected virtual void Client_GiveDamage_Implementation(int victimId, int damage)
    {
        GameObject victim = Managers.Object.FindById(victimId);
        if (victim == null)
        {
            Debug.LogWarning("Victim is not exist object");
            return;
        }

        // Floating Damage 띄우기
        GameObject floatingDamage = Managers.Resource.Instantiate("Effect/Floating Damage");
        if(floatingDamage)
        {
            floatingDamage.transform.position = victim.transform.position + new Vector3(Random.Range(-0.75f, 0.75f), 2.0f, Random.Range(0.0f, 0.75f));
            TextMesh text = floatingDamage.GetComponent<TextMesh>();
            text.text = damage.ToString();
        }
        else
        {
            Debug.LogError("Failed to create floating damage");
        }
    }
    #endregion

    #region TakeDamage
    // 서버에서 오브젝트가 대미지를 받을 때 호출
    // damage : 받은 대미지
    // damageCauser : 대미지를 주는 오브젝트
    // return : 최종적으로 받은 대미지
    public int OnServer_TakeDamage(ObjectController attacker, int damage) 
    {
        if (Util.CheckFuncCalledOnServer() == false) // 서버에서 호출되지않은 경우
            return 0;

        if (_bDead) // 이미 죽어있다면
            return 0;
        int damageResult = damage;

        _curHp -= damageResult;
        Multicast_TakeDamage(_owner.ObjectId, damageResult);

        if(_curHp <= 0) // 체력이 0이하로 떨어졌다면
        {
            _curHp = 0;
            OnServer_Death(_owner, damageResult);
        }

        return damageResult;
    }

    // 서버에서 다른 클라이언트에게 데미지를 받은것을 알리는 패킷을 보냄
    // attackerId : 공격한 오브젝트의 아이디
    // damage : 최종적으로 받은 대미지
    protected virtual void Multicast_TakeDamage(int attackerId, int damage)
    {
        // 패킷 보내기
        if (Managers.Network.IsClient) // 클라이언트에서 호출된 경우 
        {
            Debug.LogError("Client can't call this function");
        }
        else // 서버에서 호출된 경우
        {
            S_RpcComponentFunction rpcFuncPacket = new S_RpcComponentFunction();
            byte[] parameterBuffer = new byte[8];
            Array.Copy(BitConverter.GetBytes((int)attackerId),  0, parameterBuffer, 0, sizeof(int));
            Array.Copy(BitConverter.GetBytes((int)damage),      0, parameterBuffer, 4, sizeof(int));

            rpcFuncPacket.ObjectId = _owner.ObjectId;
            rpcFuncPacket.AbsolutelyExcute = true;
            rpcFuncPacket.ComponentType = GameComponentType.HealthComponent;
            rpcFuncPacket.RpcFunctionId = RpcComponentFunctionId.MulticastTakeDamage;
            rpcFuncPacket.ParameterBytes = ByteString.CopyFrom(parameterBuffer);

            Managers.Network.SendMulticast(rpcFuncPacket);
        }

        Multicast_TakeDamage_Implementation(attackerId, damage);
    }

    // Rpc 패킷을 받으면 호출
    // packet : 매개변수의 바이트 배열 
    protected virtual void Multicast_TakeDamage_ReceivePacket(byte[] packet)
    {
        try
        {
            int attackerId = BitConverter.ToInt32(packet, 0);
            int damage = BitConverter.ToInt32(packet, 4);
            Multicast_TakeDamage_Implementation(attackerId, damage);
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
        }
    }

    // 서버에서 패킷을 받았을 때 악성 패킷을 감지하기 위한 인증
    // packet : 받은 패킷의 바이트 배열
    // return : 받은 패킷이 악성 패킷이 아닌지
    protected virtual bool Multicast_TakeDamage_Validate(byte[] packet)
    {
        try
        {
            return false; // 클라이언트에서 받지 않을 Rpc 함수이기 때문에 무조건 false반환
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
            return false;
        }
    }

    // Multicast_TakeDamage 코드
    // attackerId : 공격한 오브젝트의 아이디
    // damage : 최종적으로 받은 대미지
    protected virtual void Multicast_TakeDamage_Implementation(int attackerId, int damage)
    {
        _onTakeDamageEvent.Invoke();
    }
    #endregion

    #region Death
    // 서버에서 오브젝트의 체력이 0이하로 떨어졌을 때 호출
    // attacker : 대미지를 주는 오브젝트
    // damage : 받은 대미지
    public void OnServer_Death(ObjectController attacker, int damage)
    {
        if (Util.CheckFuncCalledOnServer() == false) // 서버에서 호출되지않은 경우
            return;

        Multicast_Death(attacker.ObjectId, damage);
    }

    // 서버에서 다른 클라이언트에게 데미지를 받은것을 알리는 패킷을 보냄
    // attackerId : 공격한 오브젝트의 아이디
    // damage : 최종적으로 받은 대미지
    protected virtual void Multicast_Death(int attackerId, int damage)
    {
        // 패킷 보내기
        if (Managers.Network.IsClient) // 클라이언트에서 호출된 경우 
        {
            Debug.LogError("Client can't call this function");
        }
        else // 서버에서 호출된 경우
        {
            S_RpcComponentFunction rpcFuncPacket = new S_RpcComponentFunction();
            byte[] parameterBuffer = new byte[8];
            Array.Copy(BitConverter.GetBytes((int)attackerId), 0, parameterBuffer, 0, sizeof(int));
            Array.Copy(BitConverter.GetBytes((int)damage), 0, parameterBuffer, 4, sizeof(int));

            rpcFuncPacket.ObjectId = _owner.ObjectId;
            rpcFuncPacket.AbsolutelyExcute = true;
            rpcFuncPacket.ComponentType = GameComponentType.HealthComponent;
            rpcFuncPacket.RpcFunctionId = RpcComponentFunctionId.MulticastDeath;
            rpcFuncPacket.ParameterBytes = ByteString.CopyFrom(parameterBuffer);

            Managers.Network.SendMulticast(rpcFuncPacket);
        }

        Multicast_Death_Implementation(attackerId, damage);
    }

    // Rpc 패킷을 받으면 호출
    // packet : 매개변수의 바이트 배열 
    protected virtual void Multicast_Death_ReceivePacket(byte[] packet)
    {
        try
        {
            int attackerId = BitConverter.ToInt32(packet, 0);
            int damage = BitConverter.ToInt32(packet, 4);
            Multicast_Death_Implementation(attackerId, damage);
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
        }
    }

    // 서버에서 패킷을 받았을 때 악성 패킷을 감지하기 위한 인증
    // packet : 받은 패킷의 바이트 배열
    // return : 받은 패킷이 악성 패킷이 아닌지
    protected virtual bool Multicast_Death_Validate(byte[] packet)
    {
        try
        {
            return false; // 클라이언트에서 받지 않을 Rpc 함수이기 때문에 무조건 false반환
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
            return false;
        }
    }

    // Multicast_Death 코드
    // attacker : 대미지를 주는 오브젝트
    // damage : 받은 대미지
    protected virtual void Multicast_Death_Implementation(int attackerId, int damage)
    {
        _bDead = true;
        _onDeathEvent.Invoke();
        Debug.Log("Dead");
    }
    #endregion

    #endregion

    #region RpcFunction
    // 다른 클라이언트로 패킷을 받으면 FunctionId에 맞는 함수 호출
    // functionId : 받은 패킷의 함수 아이디
    // packet : 받은 패킷의 바이트 배열
    public override void RpcFunction_ReceivePacket(RpcComponentFunctionId functionId, byte[] packet)
    {
        try
        {
            switch (functionId)
            {
                case RpcComponentFunctionId.ClientGiveDamage:
                    Client_GiveDamage_ReceivePacket(packet);
                    break;
                case RpcComponentFunctionId.MulticastTakeDamage:
                    Multicast_TakeDamage_ReceivePacket(packet);
                    break;
                case RpcComponentFunctionId.MulticastDeath:
                    Multicast_Death_ReceivePacket(packet);
                    break;
                default:
                    break;

            }
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
        }

        base.RpcFunction_ReceivePacket(functionId, packet);
    }

    // 클라이언트에서 받은 패킷이 악성 패킷인지 확인
    // functionId : 받은 패킷의 함수 아이디
    // packet : 받은 패킷의 바이트 배열
    // return : 받은 패킷이 악성 패킷이 아닌지
    public override bool RpcFunction_Validate(RpcComponentFunctionId functionId, byte[] packet)
    {
        try
        {
            switch (functionId)
            {
                case RpcComponentFunctionId.ClientGiveDamage:
                    return Client_GiveDamage_Validate(packet);
                case RpcComponentFunctionId.MulticastTakeDamage:
                    return Multicast_TakeDamage_Validate(packet);
                case RpcComponentFunctionId.MulticastDeath:
                    return Multicast_Death_Validate(packet);
                default:
                    break;

            }
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
        }

        return base.RpcFunction_Validate(functionId, packet);
    }
    #endregion
}
