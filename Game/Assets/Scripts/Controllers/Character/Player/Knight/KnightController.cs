using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.GridLayoutGroup;

public class KnightController : PlayerController
{
    protected KnightController _knightAnim = null;

    protected override void Start()
    {
        base.Start();

        _knightAnim = GetComponent<KnightController>();
        if (_knightAnim == null)
            Debug.Log("KnightController is null");

        _playerMovement._onDodgeStartEvent.AddListener(Multicast_ComboEnd);
    }

    protected override void Update()
    {
        base.Update();

    }

    #region Attack
    public bool _isAttacking { get; protected set; } = false;
    protected bool _bDoNextCombo = false;

    public int _curCombo { get; protected set; } = 0;
    public int _maxCombo { get; protected set; } = 4;
    protected List<float> _comboDelay = new List<float>() { 0.45f, 0.55f, 0.75f, 0.8f };
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
    // �ٸ� Ŭ���̾�Ʈ�� �޺� ���� ��Ŷ�� ����
    protected virtual void Multicast_ComboStart()
    {
        // ��Ŷ ������
        if (Managers.Network.IsClient) // Ŭ���̾�Ʈ���� ȣ��� ��� 
        {
            C_RpcObjectFunction comboStartPacket = new C_RpcObjectFunction();

            comboStartPacket.ObjectId = ObjectId;
            comboStartPacket.RpcFunctionId = RpcObjectFunctionId.MulticastComboStart;

            Managers.Network.SendServer(comboStartPacket);
        }
        else // �������� ȣ��� ���
        {
            S_RpcObjectFunction comboStartPacket = new S_RpcObjectFunction();

            comboStartPacket.ObjectId = ObjectId;
            comboStartPacket.RpcFunctionId = RpcObjectFunctionId.MulticastComboStart;

            Managers.Network.SendMulticast(comboStartPacket);
        }

        Multicast_ComboStart_Implementation();
    }

    // �ٸ� Ŭ���̾�Ʈ�� ���� �޺� ���� ��Ŷ�� ������ ȣ��
    // packet : ���� �Ű� ������ ����Ʈ �迭 
    protected virtual void Multicast_ComboStart_ReceivePacket(byte[] packet)
    {
        try
        {
            Multicast_ComboStart_Implementation();

            if (Managers.Network.IsServer) // �������� ��Ŷ�� �޾��� ���
            {
                // �ٸ� Ŭ���̾�Ʈ���� ��Ŷ ������
                S_RpcObjectFunction comboStartPacket = new S_RpcObjectFunction();
                comboStartPacket.ObjectId = ObjectId;
                comboStartPacket.RpcFunctionId = RpcObjectFunctionId.MulticastComboStart;
                Managers.Network.SendMulticast(comboStartPacket);
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
        }
    }

    // �������� ��Ŷ�� �޾��� �� �Ǽ� ��Ŷ�� �����ϱ� ���� ����
    // packet : ���� ��Ŷ�� ����Ʈ �迭
    // return : ���� ��Ŷ�� �Ǽ� ��Ŷ�� �ƴ���
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

    // �޺� ���� �� ȣ��
    protected virtual void Multicast_ComboStart_Implementation()
    {
        Debug.Log("Combo Start");

        _curCombo = 0;
        _isAttacking = true;
        _bDoNextCombo = false;

        _onComboStartEvent.Invoke();
    }
    #endregion

    public virtual void OnComboDelayTimerEnd()
    {
        if(_bDoNextCombo)
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

    #region ComboAttack
    public virtual void Multicast_ComboAttack(int combo)
    {
        // ��Ŷ ������
        if (Managers.Network.IsClient) // Ŭ���̾�Ʈ���� ȣ��� ��� 
        {
            C_RpcObjectFunction comboAttackPacket = new C_RpcObjectFunction();
            byte[] parameterBuffer = new byte[4];
            Array.Copy(BitConverter.GetBytes((int)combo), 0, parameterBuffer, 0, sizeof(int));

            comboAttackPacket.ObjectId = ObjectId;
            comboAttackPacket.RpcFunctionId = RpcObjectFunctionId.MulticastComboAttack;
            comboAttackPacket.ParameterBytes = ByteString.CopyFrom(parameterBuffer);

            Managers.Network.SendServer(comboAttackPacket);
        }
        else // �������� ȣ��� ���
        {
            S_RpcObjectFunction comboAttackPacket = new S_RpcObjectFunction();
            byte[] parameterBuffer = new byte[4];
            Array.Copy(BitConverter.GetBytes((int)combo), 0, parameterBuffer, 0, sizeof(int));

            comboAttackPacket.ObjectId = ObjectId;
            comboAttackPacket.RpcFunctionId = RpcObjectFunctionId.MulticastComboAttack;
            comboAttackPacket.ParameterBytes = ByteString.CopyFrom(parameterBuffer);

            Managers.Network.SendMulticast(comboAttackPacket);
        }

        Multicast_ComboAttack_Implementation(combo);
    }


    // �ٸ� Ŭ���̾�Ʈ�� ���� �޺� ���� ��Ŷ�� ������ ȣ��
    // packet : ���� �Ű� ������ ����Ʈ �迭 
    protected virtual void Multicast_ComboAttack_ReceivePacket(byte[] packet)
    {
        try
        {
            int combo = BitConverter.ToInt32(packet, 0);
            Multicast_ComboAttack_Implementation(combo);

            if (Managers.Network.IsServer) // �������� ��Ŷ�� �޾��� ���
            {
                // �ٸ� Ŭ���̾�Ʈ���� ��Ŷ ������
                S_RpcObjectFunction comboAttackPacket = new S_RpcObjectFunction();
                comboAttackPacket.ObjectId = ObjectId;
                comboAttackPacket.RpcFunctionId = RpcObjectFunctionId.MulticastComboAttack;
                comboAttackPacket.ParameterBytes = ByteString.CopyFrom(packet);

                Managers.Network.SendMulticast(comboAttackPacket);
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
        }
    }

    // �������� ��Ŷ�� �޾��� �� �Ǽ� ��Ŷ�� �����ϱ� ���� ����
    // packet : ���� ��Ŷ�� ����Ʈ �迭
    // return : ���� ��Ŷ�� �Ǽ� ��Ŷ�� �ƴ���
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

    // �޺� ���� �� ȣ��
    protected virtual void Multicast_ComboAttack_Implementation(int combo)
    {
        if (_isAttacking == false)
            return;

        if (combo > _maxCombo || combo == 0)
        {
            if(IsLocallyControlled())
                Multicast_ComboEnd();
            
            return;
        }

        Debug.Log($"Start Combo Attack {combo}");

        _curCombo = combo;
        _bDoNextCombo = false;

        if(IsLocallyControlled())
        {
	        if (_comboTimer != null)
	            Managers.Timer.RemoveTimer(_comboTimer);
	
	        _comboTimer = Managers.Timer.SetTimer(_comboDelay[combo - 1], OnComboDelayTimerEnd, false);
	
	        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
	        if (Physics.Raycast(ray, out var hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
	        {
	            Vector3 dir = hit.point - transform.position;
	            transform.eulerAngles = new Vector3(0.0f, Util.GetAngleY(dir), 0.0f);
	        }
        }
    }
    #endregion

    #region ComboEnd
    // �ٸ� Ŭ���̾�Ʈ�� �޺� ���� ��Ŷ�� ����
    public virtual void Multicast_ComboEnd() 
    {
        // ��Ŷ ������
        if (Managers.Network.IsClient) // Ŭ���̾�Ʈ���� ȣ��� ��� 
        {
            C_RpcObjectFunction comboEndPacket = new C_RpcObjectFunction();

            comboEndPacket.ObjectId = ObjectId;
            comboEndPacket.RpcFunctionId = RpcObjectFunctionId.MulticastComboEnd;

            Managers.Network.SendServer(comboEndPacket);
        }
        else // �������� ȣ��� ���
        {
            S_RpcObjectFunction comboEndPacket = new S_RpcObjectFunction();

            comboEndPacket.ObjectId = ObjectId;
            comboEndPacket.RpcFunctionId = RpcObjectFunctionId.MulticastComboEnd;

            Managers.Network.SendMulticast(comboEndPacket);
        }

        Multicast_ComboEnd_Implementation();
    }    
    
    // �ٸ� Ŭ���̾�Ʈ�� ���� �޺� ���� ��Ŷ�� ������ ȣ��
    // packet : �Ű� ���� ����Ʈ �迭 
    protected virtual void Multicast_ComboEnd_ReceivePacket(byte[] packet)
    {
        try
        {
            Multicast_ComboEnd_Implementation();

            if (Managers.Network.IsServer) // �������� ��Ŷ�� �޾��� ���
            {
                // �ٸ� Ŭ���̾�Ʈ���� ��Ŷ ������
                S_RpcObjectFunction comboEndPacket = new S_RpcObjectFunction();
                comboEndPacket.ObjectId = ObjectId;
                comboEndPacket.RpcFunctionId = RpcObjectFunctionId.MulticastComboEnd;
                Managers.Network.SendMulticast(comboEndPacket);
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
        }
    }

    // �������� ��Ŷ�� �޾��� �� �Ǽ� ��Ŷ�� �����ϱ� ���� ����
    // packet : ���� ��Ŷ�� ����Ʈ �迭
    // return : ���� ��Ŷ�� �Ǽ� ��Ŷ�� �ƴ���
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

    // �޺� ���� �� ȣ��
    protected virtual void Multicast_ComboEnd_Implementation()
    {
        Debug.Log("Combo End");

        _curCombo = 0;
        _isAttacking = false;
        _bDoNextCombo = false;

        if(_comboTimer != null)
        {
	        Managers.Timer.RemoveTimer(_comboTimer);
	        _comboTimer = null;
        }

        _onComboEndEvent.Invoke();
    }
    #endregion

    public virtual void OnComboAttackSwing(string attackName)
    {
        Debug.Log($"{attackName}");

    }

    #endregion

    #region Controller
    public override void ControllerUpdate()
    {
        base.ControllerUpdate();

        if (IsLocallyControlled() == false)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }

    public override bool CanDodgeInput()
    {
        return base.CanDodgeInput();
    }

    public override bool CanRotationInput()
    {
        if (_isAttacking)
            return false;

        return base.CanRotationInput();
    }

    public override bool CanMovementInput()
    {
        if (_isAttacking)
            return false;

        return base.CanMovementInput();
    }
    #endregion

    #region RpcFunction
    // �ٸ� Ŭ���̾�Ʈ�� ��Ŷ�� ������ FunctionId�� �´� �Լ� ȣ��
    // functionId : ���� ��Ŷ�� �Լ� ���̵�
    // packet : ���� ��Ŷ�� ����Ʈ �迭
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

        }

        base.RpcFunction_ReceivePacket(functionId, packet);
    }

    // Ŭ���̾�Ʈ���� ���� ��Ŷ�� �Ǽ� ��Ŷ���� Ȯ��
    // functionId : ���� ��Ŷ�� �Լ� ���̵�
    // packet : ���� ��Ŷ�� ����Ʈ �迭
    // return : ���� ��Ŷ�� �Ǽ� ��Ŷ�� �ƴ���
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
}
