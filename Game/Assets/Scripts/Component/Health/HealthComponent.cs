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
    public bool _bDead { get; protected set; } = false; // ������Ʈ�� �׾��ִ���
    public float CurHpRatio { get { return (float)_curHp / (float)_maxHp; } } // ���� ü�� ����
    public int _curHp { get; protected set; } // ���� ü��
    public int _maxHp { get; protected set; } = 100; // �ִ� ü��
    protected bool _bCancelTakeDamage = false; // �������� ��ȿȭ���� ����

    public UnityEvent _onServerBeforeTakeDamageEvent = new UnityEvent(); // �������� ������� �ޱ����� ȣ��Ǵ� �̺�Ʈ
    public UnityEvent _onTakeDamageEvent = new UnityEvent(); // ������� �ް� ���� ȣ��Ǵ� �̺�Ʈ
    public UnityEvent _onDeathEvent = new UnityEvent(); // ������� �ް� ���� ȣ��Ǵ� �̺�Ʈ
    public UnityEvent _onRespawnEvent = new UnityEvent();

    #region Give Damage
    // �������� ������Ʈ�� ������� �� �� ȣ��
    // damage : �� �����
    // victim : ������� ���� ������Ʈ
    // return : ���������� �� �����
    public int OnServer_GiveDamage(ObjectController victim, int damage)
    {
        if (Util.CheckFuncCalledOnServer() == false) // �������� ȣ��������� ���
            return 0;

        HealthComponent health = victim.GetComponentInChildren<HealthComponent>();
        if (health == null || health._bDead)
            return 0;

        Client_GiveDamage(victim.ObjectId, damage);
        return health.OnServer_TakeDamage(_owner, damage);
    }

    // �������� �ٸ� Ŭ���̾�Ʈ���� �������� �ذ��� �˸��� ��Ŷ�� ����
    // victimId : ������ ���� ������Ʈ�� ���̵�
    // damage : ���������� �� �����
    protected virtual void Client_GiveDamage(int victimId, int damage)
    {
        // ��Ŷ ������
        if (Managers.Network.IsClient) // Ŭ���̾�Ʈ���� ȣ��� ��� 
        {
            Debug.LogError("Client can't call this function");
        }
        else // �������� ȣ��� ���
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

    // Rpc ��Ŷ�� ������ ȣ��
    // packet : �Ű������� ����Ʈ �迭 
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

    // �������� ��Ŷ�� �޾��� �� �Ǽ� ��Ŷ�� �����ϱ� ���� ����
    // packet : ���� ��Ŷ�� ����Ʈ �迭
    // return : ���� ��Ŷ�� �Ǽ� ��Ŷ�� �ƴ���
    protected virtual bool Client_GiveDamage_Validate(byte[] packet)
    {
        try
        {
            return false; // Ŭ���̾�Ʈ���� ���� ���� Rpc �Լ��̱� ������ ������ false��ȯ
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
            return false;
        }
    }

    // Client_GiveDamage �ڵ�
    // victimId : ������ ���� ������Ʈ�� ���̵�
    // damage : ���������� ���� �����
    protected virtual void Client_GiveDamage_Implementation(int victimId, int damage)
    {
        GameObject victim = Managers.Object.FindById(victimId);
        if (victim == null)
        {
            Debug.LogWarning("Victim is not exist object");
            return;
        }

        // Floating Damage ����
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
    public void OnServer_CancelTakeDamage() // �������� ��ȿȭ�� �� ȣ��
    {
        _bCancelTakeDamage = true;
    }

    // �������� ������Ʈ�� ������� ���� �� ȣ��
    // damage : ���� �����
    // damageCauser : ������� �ִ� ������Ʈ
    // return : ���������� ���� �����
    public int OnServer_TakeDamage(ObjectController attacker, int damage) 
    {
        if (Util.CheckFuncCalledOnServer() == false) // �������� ȣ��������� ���
            return 0;

        if (_bDead) // �̹� �׾��ִٸ�
            return 0;

        _bCancelTakeDamage = false;
        _onServerBeforeTakeDamageEvent.Invoke();
        if (_bCancelTakeDamage)
            return 0;

        int damageResult = damage;

        _curHp -= damageResult;
        Multicast_TakeDamage(_owner.ObjectId, damageResult);

        if(_curHp <= 0) // ü���� 0���Ϸ� �������ٸ�
        {
            _curHp = 0;
            OnServer_Death(_owner, damageResult);
        }

        return damageResult;
    }

    // �������� �ٸ� Ŭ���̾�Ʈ���� �������� �������� �˸��� ��Ŷ�� ����
    // attackerId : ������ ������Ʈ�� ���̵�
    // damage : ���������� ���� �����
    protected virtual void Multicast_TakeDamage(int attackerId, int damage)
    {
        // ��Ŷ ������
        if (Managers.Network.IsClient) // Ŭ���̾�Ʈ���� ȣ��� ��� 
        {
            Debug.LogError("Client can't call this function");
        }
        else // �������� ȣ��� ���
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

    // Rpc ��Ŷ�� ������ ȣ��
    // packet : �Ű������� ����Ʈ �迭 
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

    // �������� ��Ŷ�� �޾��� �� �Ǽ� ��Ŷ�� �����ϱ� ���� ����
    // packet : ���� ��Ŷ�� ����Ʈ �迭
    // return : ���� ��Ŷ�� �Ǽ� ��Ŷ�� �ƴ���
    protected virtual bool Multicast_TakeDamage_Validate(byte[] packet)
    {
        try
        {
            return false; // Ŭ���̾�Ʈ���� ���� ���� Rpc �Լ��̱� ������ ������ false��ȯ
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
            return false;
        }
    }

    // Multicast_TakeDamage �ڵ�
    // attackerId : ������ ������Ʈ�� ���̵�
    // damage : ���������� ���� �����
    protected virtual void Multicast_TakeDamage_Implementation(int attackerId, int damage)
    {
        _onTakeDamageEvent.Invoke();
    }
    #endregion

    #region Death
    // �������� ������Ʈ�� ü���� 0���Ϸ� �������� �� ȣ��
    // attacker : ������� �ִ� ������Ʈ
    // damage : ���� �����
    public void OnServer_Death(ObjectController attacker, int damage)
    {
        if (Util.CheckFuncCalledOnServer() == false) // �������� ȣ��������� ���
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
        if (Util.CheckFuncCalledOnServer() == false) // �������� ȣ��������� ���
            return;

        _curHp = _maxHp;
        _bDead = false;
        Multicast_Respawn();
    }

    // �������� �ٸ� Ŭ���̾�Ʈ���� �������� �˸��� ��Ŷ�� ����
    protected virtual void Multicast_Respawn()
    {
        // ��Ŷ ������
        if (Managers.Network.IsClient) // Ŭ���̾�Ʈ���� ȣ��� ��� 
        {
            Debug.LogError("Client can't call this function");
        }
        else // �������� ȣ��� ���
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

    // Rpc ��Ŷ�� ������ ȣ��
    // packet : �Ű������� ����Ʈ �迭 
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

    // �������� ��Ŷ�� �޾��� �� �Ǽ� ��Ŷ�� �����ϱ� ���� ����
    // packet : ���� ��Ŷ�� ����Ʈ �迭
    // return : ���� ��Ŷ�� �Ǽ� ��Ŷ�� �ƴ���
    protected virtual bool Multicast_Respawn_Validate(byte[] packet)
    {
        try
        {
            return false; // Ŭ���̾�Ʈ���� ���� ���� Rpc �Լ��̱� ������ ������ false��ȯ
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
            return false;
        }
    }

    // Multicast_Respawn �ڵ�
    protected virtual void Multicast_Respawn_Implementation()
    {
        Debug.Log("H");
        _onRespawnEvent.Invoke();
    }
    #endregion

    #region RpcFunction
    // �ٸ� Ŭ���̾�Ʈ�� ��Ŷ�� ������ FunctionId�� �´� �Լ� ȣ��
    // functionId : ���� ��Ŷ�� �Լ� ���̵�
    // packet : ���� ��Ŷ�� ����Ʈ �迭
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

    // Ŭ���̾�Ʈ���� ���� ��Ŷ�� �Ǽ� ��Ŷ���� Ȯ��
    // functionId : ���� ��Ŷ�� �Լ� ���̵�
    // packet : ���� ��Ŷ�� ����Ʈ �迭
    // return : ���� ��Ŷ�� �Ǽ� ��Ŷ�� �ƴ���
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
        if (_bDead != dead) // _bDead�� ���� ��������
        {
            _bDead = dead;
            OnRep_bDead();        
        }
    }

    protected virtual void OnRep_bDead() // _bDead�� ���� �������� ȣ��
    {
        if (_bDead)
        {
            OnDeath();
        }
    }
    #endregion

}
