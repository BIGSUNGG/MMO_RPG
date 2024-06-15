using Google.Protobuf;
using Google.Protobuf.Protocol;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class CharacterController : ObjectController
{
    public CharacterAnimParameter       _anim       { get; protected set; } = null;
    public CharacterMovementComponent   _movement   { get; protected set; } = null;
    public HealthComponent              _health     { get; protected set; } = null;
    public InventoryComponent           _inventory  { get; protected set; } = null;
    public CapsuleCollider              _capsule    { get; protected set; } = null;

    public CharacterController()
    {
    }

    protected override void Start()
    {
        base.Start();

        _anim = GetComponent<CharacterAnimParameter>();
        if (_anim == null)
            Debug.LogWarning("CharacterAnimParameter is null");

        _movement = GetComponent<CharacterMovementComponent>();
        if (_movement == null)
            Debug.LogWarning("MovementComponent is null");

        _health = GetComponent<HealthComponent>();
        if (_health == null)
            Debug.LogWarning("HealthComponent is null");
        else
        {
            _health._onTakeDamageEvent.AddListener(OnTakeDamageEvent);
            _health._onDeathEvent.AddListener(OnDeathEvent);
            _health._onRespawnEvent.AddListener(OnRespawnEvent);
        }

        _inventory = GetComponent<InventoryComponent>();
        if (_inventory == null)
            Debug.LogWarning("InventoryComponent is null");

        _capsule = GetComponentInChildren<CapsuleCollider>();
        if (_capsule == null)
            Debug.LogWarning("CapsuleCollider is null");
    }

    protected override void Update()
    {
        base.Update();

    }

    #region Controller
    public Vector2 _moveDir = Vector2.zero;
    public override void ControllerUpdate()
    {
        base.ControllerUpdate();

        _moveDir = Vector2.zero;
        if (_movement)
        {
            _moveDir = Vector2.zero;
         
            if (Input.GetKey(KeyCode.W))
                _moveDir.y += 1.0f;
            if (Input.GetKey(KeyCode.S))
                _moveDir.y -= 1.0f;

            if (Input.GetKey(KeyCode.A))
                _moveDir.x -= 1.0f;
            if (Input.GetKey(KeyCode.D))
                _moveDir.x += 1.0f;

            _movement._bIsRunning = Input.GetKey(KeyCode.LeftShift);
        }
    }

    public override void OnPossess()
    {
        base.OnPossess();

    }

    public virtual bool CanInput()
    {
        if (_health._bDead)
            return false;

        return true;
    }

    public virtual bool CanAttack()
    {
        if (CanInput() == false)
            return false;

        return true;
    }

    public virtual bool CanRotate()
    {
        if (CanInput() == false)
            return false;

        return true;
    }

    public virtual bool CanMove()
    {
        if (CanInput() == false)
            return false;

        return true;
    }
    #endregion

    #region Component
    protected virtual void OnTakeDamageEvent()
    {
        if(Managers.Network.IsServer)
        {
            Debug.Log("TakeDamage");
            Multicast_ComboEnd();
        }
    }

    protected virtual void OnDeathEvent()
    {
        _movement._velocity = Vector3.zero;
    }

    protected virtual void OnRespawnEvent()
    {

    }
    #endregion

    #region Attack
    public bool _isAttacking { get; protected set; } = false;
    protected bool _bDoNextCombo = false;

    public int _curCombo { get; protected set; } = 0;
    public int _maxCombo = 1;
    public List<float> _comboDelay = new List<float>();
    protected TimerHandler _comboTimer = null;

    public UnityEvent _onComboStartEvent = new UnityEvent();
    public UnityEvent _onComboEndEvent = new UnityEvent();

    public virtual void Attack()
    {
        if (CanAttack() == false)
            return;

        if (_isAttacking == false)
        {
            Multicast_ComboStart();
            Multicast_ComboAttack(1);
        }
        else
        {
            _bDoNextCombo = true;
        }

    }

    #region ComboStart
    // 다른 클라이언트에 콤보 시작 패킷을 보냄
    protected virtual void Multicast_ComboStart()
    {
        // 패킷 보내기
        if (Managers.Network.IsClient) // 클라이언트에서 호출된 경우 
        {
            C_RpcObjectFunction rpcFuncPacket = new C_RpcObjectFunction();

            rpcFuncPacket.ObjectId = ObjectId;
            rpcFuncPacket.RpcFunctionId = RpcObjectFunctionId.MulticastComboStart;

            Managers.Network.SendServer(rpcFuncPacket);
        }
        else // 서버에서 호출된 경우
        {
            S_RpcObjectFunction rpcFuncPacket = new S_RpcObjectFunction();

            rpcFuncPacket.ObjectId = ObjectId;
            rpcFuncPacket.AbsolutelyExcute = true;
            rpcFuncPacket.RpcFunctionId = RpcObjectFunctionId.MulticastComboStart;

            Managers.Network.SendMulticast(rpcFuncPacket);
        }

        Multicast_ComboStart_Implementation();
    }

    // 다른 클라이언트로 부터 콤보 시작 패킷을 받으면 호출
    // packet : 받은 매개 변수의 바이트 배열 
    protected virtual void Multicast_ComboStart_ReceivePacket(byte[] packet)
    {
        try
        {
            Multicast_ComboStart_Implementation();

            if (Managers.Network.IsServer) // 서버에서 패킷을 받았을 경우
            {
                // 다른 클라이언트에게 패킷 보내기
                S_RpcObjectFunction rpcFuncPacket = new S_RpcObjectFunction();
                rpcFuncPacket.ObjectId = ObjectId;
                rpcFuncPacket.RpcFunctionId = RpcObjectFunctionId.MulticastComboStart;
                Managers.Network.SendMulticast(rpcFuncPacket);
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
        }
    }

    // 서버에서 패킷을 받았을 때 악성 패킷을 감지하기 위한 인증
    // packet : 받은 패킷의 바이트 배열
    // return : 받은 패킷이 악성 패킷이 아닌지
    protected virtual bool Multicast_ComboStart_Validate(byte[] packet)
    {
        try
        {
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
            return false;
        }
    }

    // 콤보 시작 시 호출
    protected virtual void Multicast_ComboStart_Implementation()
    {
        Debug.Log("Combo Start");

        _curCombo = 0;
        _isAttacking = true;
        _bDoNextCombo = false;

        _onComboStartEvent.Invoke();
    }
    #endregion

    #region ComboAttack
    public virtual void OnComboDelayTimerEnd()
    {
        if (_isAttacking == false)
            return;

        if (_bDoNextCombo)
        {
            if (_curCombo < _maxCombo)
                Multicast_ComboAttack(_curCombo + 1);
            else
                Multicast_ComboAttack(1);
        }
        else
        {
            Multicast_ComboEnd();
        }
    }


    public virtual void Multicast_ComboAttack(int combo)
    {
        // 패킷 보내기
        if (Managers.Network.IsClient) // 클라이언트에서 호출된 경우 
        {
            C_RpcObjectFunction comboAttackPacket = new C_RpcObjectFunction();
            byte[] parameterBuffer = new byte[4];
            Array.Copy(BitConverter.GetBytes((int)combo), 0, parameterBuffer, 0, sizeof(int));

            comboAttackPacket.ObjectId = ObjectId;
            comboAttackPacket.RpcFunctionId = RpcObjectFunctionId.MulticastComboAttack;
            comboAttackPacket.ParameterBytes = ByteString.CopyFrom(parameterBuffer);

            Managers.Network.SendServer(comboAttackPacket);
        }
        else // 서버에서 호출된 경우
        {
            S_RpcObjectFunction rpcFuncPacket = new S_RpcObjectFunction();
            byte[] parameterBuffer = new byte[4];
            Array.Copy(BitConverter.GetBytes((int)combo), 0, parameterBuffer, 0, sizeof(int));

            rpcFuncPacket.ObjectId = ObjectId;
            rpcFuncPacket.AbsolutelyExcute = true;
            rpcFuncPacket.RpcFunctionId = RpcObjectFunctionId.MulticastComboAttack;
            rpcFuncPacket.ParameterBytes = ByteString.CopyFrom(parameterBuffer);

            Managers.Network.SendMulticast(rpcFuncPacket);
        }

        Multicast_ComboAttack_Implementation(combo);
    }


    // 다른 클라이언트로 부터 콤보 시작 패킷을 받으면 호출
    // packet : 받은 매개 변수의 바이트 배열 
    protected virtual void Multicast_ComboAttack_ReceivePacket(byte[] packet)
    {
        try
        {
            int combo = BitConverter.ToInt32(packet, 0);
            Multicast_ComboAttack_Implementation(combo);

            if (Managers.Network.IsServer) // 서버에서 패킷을 받았을 경우
            {
                // 다른 클라이언트에게 패킷 보내기
                S_RpcObjectFunction rpcFuncPacket = new S_RpcObjectFunction();
                rpcFuncPacket.ObjectId = ObjectId;
                rpcFuncPacket.RpcFunctionId = RpcObjectFunctionId.MulticastComboAttack;
                rpcFuncPacket.ParameterBytes = ByteString.CopyFrom(packet);

                Managers.Network.SendMulticast(rpcFuncPacket);
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
        }
    }

    // 서버에서 패킷을 받았을 때 악성 패킷을 감지하기 위한 인증
    // packet : 받은 패킷의 바이트 배열
    // return : 받은 패킷이 악성 패킷이 아닌지
    protected virtual bool Multicast_ComboAttack_Validate(byte[] packet)
    {
        try
        {
            int combo = BitConverter.ToInt32(packet, 0);

            if (_isAttacking == false)
            {
                Debug.Log("Is not attacking");
                return false;
            }

            if (combo > _maxCombo || combo == 0)
            {
                Debug.Log("Wrong combo excute");
                return false;
            }

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
            return false;
        }
    }

    // 콤보 시작 시 호출
    protected virtual void Multicast_ComboAttack_Implementation(int combo)
    {
        if (combo > _maxCombo || combo == 0)
        {
            if (IsLocallyControlled())
                Multicast_ComboEnd();

            return;
        }

        _curCombo = combo;
        _bDoNextCombo = false;

        Debug.Log($"Start Combo Attack {combo}");
        if (IsLocallyControlled())
        {
            if (_comboTimer != null)
                Managers.Timer.RemoveTimer(_comboTimer);

            _comboTimer = Managers.Timer.SetTimer(_comboDelay[combo - 1], OnComboDelayTimerEnd, false);
        }
    }
    #endregion

    #region ComboAttackSwing
    public virtual void OnComboAttackSwing(string attackName) // 무기를 휘두르는 타이밍에 호출
    {
        if(Managers.Network.IsServer && _isAttacking)
            OnServer_ComboAttackSwing(attackName);
    }

    // 서버에 공격 결과 패킷을 보냄
    // attackName : 공격 이름
    public virtual void OnServer_ComboAttackSwing(string attackName)
    {

    }
    #endregion

    #region ComboEnd
    // 다른 클라이언트에 콤보 종료 패킷을 보냄
    public virtual void Multicast_ComboEnd()
    {
        // 패킷 보내기
        if (Managers.Network.IsClient) // 클라이언트에서 호출된 경우 
        {
            C_RpcObjectFunction rpcFuncPacket = new C_RpcObjectFunction();

            rpcFuncPacket.ObjectId = ObjectId;
            rpcFuncPacket.RpcFunctionId = RpcObjectFunctionId.MulticastComboEnd;

            Managers.Network.SendServer(rpcFuncPacket);
        }
        else // 서버에서 호출된 경우
        {
            S_RpcObjectFunction rpcFuncPacket = new S_RpcObjectFunction();

            rpcFuncPacket.ObjectId = ObjectId;
            rpcFuncPacket.AbsolutelyExcute = true;
            rpcFuncPacket.RpcFunctionId = RpcObjectFunctionId.MulticastComboEnd;

            Managers.Network.SendMulticast(rpcFuncPacket);
        }

        Multicast_ComboEnd_Implementation();
    }

    // 다른 클라이언트로 부터 콤보 종료 패킷을 받으면 호출
    // packet : 매개 변수 바이트 배열 
    protected virtual void Multicast_ComboEnd_ReceivePacket(byte[] packet)
    {
        try
        {
            Multicast_ComboEnd_Implementation();

            if (Managers.Network.IsServer) // 서버에서 패킷을 받았을 경우
            {
                // 다른 클라이언트에게 패킷 보내기
                S_RpcObjectFunction rpcFuncPacket = new S_RpcObjectFunction();
                rpcFuncPacket.ObjectId = ObjectId;
                rpcFuncPacket.RpcFunctionId = RpcObjectFunctionId.MulticastComboEnd;
                Managers.Network.SendMulticast(rpcFuncPacket);
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
        }
    }

    // 서버에서 패킷을 받았을 때 악성 패킷을 감지하기 위한 인증
    // packet : 받은 패킷의 바이트 배열
    // return : 받은 패킷이 악성 패킷이 아닌지
    protected virtual bool Multicast_ComboEnd_Validate(byte[] packet)
    {
        try
        {
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
            return false;
        }
    }

    // 콤보 종료 시 호출
    protected virtual void Multicast_ComboEnd_Implementation()
    {
        _curCombo = 0;
        _isAttacking = false;
        _bDoNextCombo = false;

        Debug.Log("Combo End");
        if (IsLocallyControlled())
        {
            if (_comboTimer != null)
            {
                Managers.Timer.RemoveTimer(_comboTimer);
                _comboTimer = null;
            }
        }

        _onComboEndEvent.Invoke();
    }
    #endregion
    #endregion


    #region RpcFunction
    // 다른 클라이언트로 패킷을 받으면 FunctionId에 맞는 함수 호출
    // functionId : 받은 패킷의 함수 아이디
    // packet : 받은 패킷의 바이트 배열
    public override void RpcFunction_ReceivePacket(RpcObjectFunctionId functionId, byte[] packet)
    {
        try
        {
            switch (functionId)
            {
                case RpcObjectFunctionId.MulticastComboStart:
                    Multicast_ComboStart_ReceivePacket(packet);
                    break;
                default:
                    break;
                case RpcObjectFunctionId.MulticastComboAttack:
                    Multicast_ComboAttack_ReceivePacket(packet);
                    break;
                case RpcObjectFunctionId.MulticastComboEnd:
                    Multicast_ComboEnd_ReceivePacket(packet);
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
    public override bool RpcFunction_Validate(RpcObjectFunctionId functionId, byte[] packet)
    {
        try
        {
            switch (functionId)
            {
                case RpcObjectFunctionId.MulticastComboStart:
                    return Multicast_ComboStart_Validate(packet);
                case RpcObjectFunctionId.MulticastComboAttack:
                    return Multicast_ComboAttack_Validate(packet);
                case RpcObjectFunctionId.MulticastComboEnd:
                    return Multicast_ComboEnd_Validate(packet);
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
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    protected class CharacterSyncInfo : ObjectSyncInfo
    {
        // Movement
        public Vector3 position;
        public Quaternion rotation;
        public Vector2 moveDir;
        public bool bIsRunning;

        // Health
        public int curHp;
        public bool bDead;
    }

    public override void ObjectSync(ByteString syncInfo)
    {
        CharacterSyncInfo info = Util.BytesToObject<CharacterSyncInfo>(syncInfo.ToByteArray());
        ObjectSync(info);
    }

    protected void ObjectSync(CharacterSyncInfo info)
    {
        if (info == null)
            return;

        if(!IsLocallyControlled())
        {
            if (_movement)
                _movement.Sync(info.position, info.rotation, info.moveDir, info.bIsRunning);
        }

        if (Managers.Network.IsClient && _health)
            _health.Sync(info.curHp, info.bDead);

        base.ObjectSync(info);
    }

    public override ByteString GetObjectSyncInfo()
    {
        CharacterSyncInfo info = new CharacterSyncInfo();
        GetObjectSyncInfo(info);
        return ByteString.CopyFrom(Util.ObjectToBytes<CharacterSyncInfo>(info));
    }

    protected void GetObjectSyncInfo(CharacterSyncInfo info)
    {
        if (_movement == null)
            return;

        info.position = transform.position;
        info.rotation = transform.rotation;
        info.moveDir  = _moveDir;
        info.bIsRunning = _movement._bIsRunning;

        if(Managers.Network.IsServer)
        {
            info.curHp = _health._curHp;
            info.bDead = _health._bDead;
        }

        base.GetObjectSyncInfo(info);
    }
    #endregion
}
