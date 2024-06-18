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
    public float CurHpRatio { get { return (float)_curHp / (float)_maxHp; } } // 현재 체력 비율
    public int _curHp { get; protected set; } // 현재 체력
    public int _maxHp { get; protected set; } = 100; // 최대 체력
    protected bool _bCancelTakeDamage = false; // 데미지를 무효화할지 여부

    public UnityEvent _onServerBeforeTakeDamageEvent = new UnityEvent(); // 서버에서 대미지를 받기전에 호출되는 이벤트
    public UnityEvent _onTakeDamageEvent = new UnityEvent(); // 대미지를 받고 나서 호출되는 이벤트
    public UnityEvent _onDeathEvent = new UnityEvent(); // 대미지를 받고 나서 호출되는 이벤트
    public UnityEvent _onRespawnEvent = new UnityEvent();

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
        if (health == null || health._bDead)
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
    public void OnServer_CancelTakeDamage() // 데미지를 무효화할 때 호출
    {
        _bCancelTakeDamage = true;
    }

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

        _bCancelTakeDamage = false;
        _onServerBeforeTakeDamageEvent.Invoke();
        if (_bCancelTakeDamage)
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

        Managers.Timer.SetTimer(3.0f, OnServer_Respawn, false);
        OnDeath();
    }

    protected void OnDeath()
    {
        _bDead = true;
        _onDeathEvent.Invoke();
        Debug.Log("Dead");
    }
    #endregion

    #endregion

    #region Respawn
    protected void OnServer_Respawn()
    {
        if (Util.CheckFuncCalledOnServer() == false) // 서버에서 호출되지않은 경우
            return;

        _curHp = _maxHp;
        _bDead = false;
        Multicast_Respawn();
    }

    // 서버에서 다른 클라이언트에게 리스폰을 알리는 패킷을 보냄
    protected virtual void Multicast_Respawn()
    {
        // 패킷 보내기
        if (Managers.Network.IsClient) // 클라이언트에서 호출된 경우 
        {
            Debug.LogError("Client can't call this function");
        }
        else // 서버에서 호출된 경우
        {
            S_RpcComponentFunction rpcFuncPacket = new S_RpcComponentFunction();

            rpcFuncPacket.ObjectId = _owner.ObjectId;
            rpcFuncPacket.AbsolutelyExcute = true;
            rpcFuncPacket.ComponentType = GameComponentType.HealthComponent;
            rpcFuncPacket.RpcFunctionId = RpcComponentFunctionId.MulticastRespawn;

            Managers.Network.SendMulticast(rpcFuncPacket);
        }

        Multicast_Respawn_Implementation();
    }

    // Rpc 패킷을 받으면 호출
    // packet : 매개변수의 바이트 배열 
    protected virtual void Multicast_Respawn_ReceivePacket(byte[] packet)
    {
        try
        {
            Multicast_Respawn_Implementation();
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
        }
    }

    // 서버에서 패킷을 받았을 때 악성 패킷을 감지하기 위한 인증
    // packet : 받은 패킷의 바이트 배열
    // return : 받은 패킷이 악성 패킷이 아닌지
    protected virtual bool Multicast_Respawn_Validate(byte[] packet)
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

    // Multicast_Respawn 코드
    protected virtual void Multicast_Respawn_Implementation()
    {
        Debug.Log("H");
        _onRespawnEvent.Invoke();
    }
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
                case RpcComponentFunctionId.MulticastRespawn:
                    Multicast_Respawn_ReceivePacket(packet);
                    break; ;
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
                case RpcComponentFunctionId.MulticastRespawn:
                    return Multicast_Respawn_Validate(packet);
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

    #region Sync
    public virtual void Sync(int hp, bool dead)
    {
        _curHp = hp;
        if (_bDead != dead) // _bDead의 값이 변했을때
        {
            _bDead = dead;
            OnRep_bDead();        
        }
    }

    protected virtual void OnRep_bDead() // _bDead의 값이 변했을때 호출
    {
        if (_bDead)
        {
            OnDeath();
        }
    }
    #endregion

}
