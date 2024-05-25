using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using static UnityEngine.UI.GridLayoutGroup;

public class KnightController : PlayerController
{
	protected KnightController _knightAnim = null;

	protected override void Start()
	{
		base.Start();

		_knightAnim = GetComponent<KnightController>();
		if (_knightAnim == null)
			Debug.LogWarning("KnightController is null");

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
		if(IsLocallyControlled())
		{
			Debug.Log("Combo Start");   
		}

		_curCombo = 0;
		_isAttacking = true;
		_bDoNextCombo = false;

		_onComboStartEvent.Invoke();
	}
	#endregion

	#region ComboAttack
	public virtual void OnComboDelayTimerEnd()
	{
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
		if (_isAttacking == false)
			return;

		if (combo > _maxCombo || combo == 0)
		{
			if(IsLocallyControlled())
				Multicast_ComboEnd();
			
			return;
		}

		_curCombo = combo;
		_bDoNextCombo = false;

		if(IsLocallyControlled())
		{
			Debug.Log($"Start Combo Attack {combo}");
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

	#region ComboAttackSwing
	public virtual void OnComboAttackSwing(string attackName) // 무기를 휘두르는 타이밍에 호출
	{
		if (IsLocallyControlled() == false)
			return;

		switch(attackName)
		{
			case "1":
				break;
			case "2":
				break;
			case "3-1":
				break;
			case "3-2":
				break;
			case "4":
				break;
			default:
				Debug.LogWarning("Recv wrong attack name");
				return;
		}

        // 공격 범위에 있는 오브젝트 찾기
        int targetLayer = LayerMask.NameToLayer("Character");
        int layerMask = 1 << targetLayer;
        if (targetLayer == -1) // 레이어를 못 찾았을 경우
        {
            Debug.LogWarning("레이어 이름이 유효하지 않습니다: " + layerMask);
            return;
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position + new Vector3(0.0f, _capsule.height / 2, 0.0f), 2.0f, layerMask);
        List<int> objectIdList = new List<int>();

        foreach (var hitCollider in hitColliders)
        {
            ObjectController oc = hitCollider.gameObject.GetComponentInParent<ObjectController>();
            if (oc && oc == this)
                continue;

            objectIdList.Add(oc.ObjectId);
        }

        if (objectIdList.Count > 0)
            Server_ComboAttackResult(attackName, objectIdList);
    }

	public virtual void Server_ComboAttackResult(string attackName, List<int> objectIdArr)
	{
		// 패킷 보내기
		if (Managers.Network.IsClient) // 클라이언트에서 호출된 경우 
		{
			C_RpcObjectFunction rpcFuncPacket = new C_RpcObjectFunction();
			byte nameLength = (byte)attackName.Length; // attackName의 str 길이
			byte objectIdArrCount = (byte)objectIdArr.Count; // object Id 개수

			byte[] parameterBuffer = new byte[1 + nameLength + 1 + (objectIdArrCount * 4)];
			Array.Copy(BitConverter.GetBytes((byte)nameLength)  , 0, parameterBuffer, 0, sizeof(byte)); // attackName 길이 복사
			Array.Copy(Encoding.UTF8.GetBytes(attackName)       , 0, parameterBuffer, 1, nameLength); // attackName 복사

			Array.Copy(BitConverter.GetBytes((byte)objectIdArrCount), 0, parameterBuffer, 1 + nameLength, sizeof(byte)); // object id 개수 복사
			for (int i = 0; i < objectIdArr.Count; i++)
                Array.Copy(BitConverter.GetBytes((int)objectIdArr[i]), 0, parameterBuffer, 2 + nameLength + (sizeof(int) * i), sizeof(int)); // object id 복사

			rpcFuncPacket.ObjectId = ObjectId;
			rpcFuncPacket.RpcFunctionId = RpcObjectFunctionId.ServerComboAttackResult;
			rpcFuncPacket.ParameterBytes = ByteString.CopyFrom(parameterBuffer);

			Managers.Network.SendServer(rpcFuncPacket);
		}
		else // 서버에서 호출된 경우
		{
			S_RpcObjectFunction rpcFuncPacket = new S_RpcObjectFunction();
			byte nameLength = (byte)attackName.Length; // attackName의 str 길이
			byte objectIdArrCount = (byte)objectIdArr.Count; // object Id 개수

            byte[] parameterBuffer = new byte[1 + nameLength + 1 + (objectIdArrCount * 4)];
            Array.Copy(BitConverter.GetBytes((byte)nameLength), 0, parameterBuffer, 0, sizeof(byte)); // attackName 길이 복사
			Array.Copy(Encoding.UTF8.GetBytes(attackName), 0, parameterBuffer, 1, nameLength); // attackName 복사

			Array.Copy(BitConverter.GetBytes((byte)objectIdArrCount), 0, parameterBuffer, 1 + nameLength, sizeof(byte)); // object id 개수 복사
			for (int i = 0; i < objectIdArr.Count; i++)
				Array.Copy(BitConverter.GetBytes((int)objectIdArr[i]), 0, parameterBuffer, 2 + nameLength + (sizeof(int) * i), sizeof(int)); // object id 복사

			rpcFuncPacket.ObjectId = ObjectId;
			rpcFuncPacket.RpcFunctionId = RpcObjectFunctionId.ServerComboAttackResult;
			rpcFuncPacket.ParameterBytes = ByteString.CopyFrom(parameterBuffer);
		}
	}


	// 다른 클라이언트로 부터 콤보 시작 패킷을 받으면 호출
	// packet : 받은 매개 변수의 바이트 배열 
	protected virtual void Server_ComboAttackResult_ReceivePacket(byte[] packet)
	{
		try
        {
            byte nameLength = packet[0]; // attackName의 str 길이
            string attackName = BitConverter.ToString(packet, 1, nameLength); ;
            byte objectIdArrCount = packet[1 + nameLength]; // object Id 개수
            List<int> objectIdArr = Util.BytesToIntList(packet, 2 + nameLength, objectIdArrCount);

			Server_ComboAttackResult_Implementation(attackName, objectIdArr);
		}
		catch (System.Exception ex)
		{
			Debug.Log($"{ex}");
		}
	}

	// 서버에서 패킷을 받았을 때 악성 패킷을 감지하기 위한 인증
	// packet : 받은 패킷의 바이트 배열
	// return : 받은 패킷이 악성 패킷이 아닌지
	protected virtual bool Server_ComboAttackResult_Validate(byte[] packet)
	{
		try
        {
            byte nameLength = packet[0]; // attackName의 str 길이
            string attackName = BitConverter.ToString(packet, 1, nameLength); ;
            byte objectIdArrCount = packet[1 + nameLength]; // object Id 개수
            List<int> objectIdArr = Util.BytesToIntList(packet, 2 + nameLength, objectIdArrCount);

            switch (attackName)
            {
                case "1":
                    break;
                case "2":
                    break;
                case "3-1":
                    break;
                case "3-2":
                    break;
                case "4":
                    break;
                default:
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
	protected virtual void Server_ComboAttackResult_Implementation(string attackName, List<int> objectIdArr)
	{
        List<ObjectController> objects = new List<ObjectController>();
        foreach(int id in objectIdArr)
        {
            GameObject go = Managers.Object.FindById(id);
            if (go == null)
                continue;

            gameObject.GiveDamage(go, 30.0f);
        }
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

		if(IsLocallyControlled())
		{
			Debug.Log("Combo End");
			if(_comboTimer != null)
			{
				Managers.Timer.RemoveTimer(_comboTimer);
				_comboTimer = null;
			}
		}

		_onComboEndEvent.Invoke();
	}
	#endregion
	void OnDrawGizmos()
	{
		// 트레이스 범위 그리기
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position + new Vector3(0.0f, _capsule.height / 2, 0.0f), 2.0f);
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
                case RpcObjectFunctionId.ServerComboAttackResult:
                    Server_ComboAttackResult_ReceivePacket(packet);
                    break;
            }
		}
		catch (System.Exception ex)
		{

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
                case RpcObjectFunctionId.ServerComboAttackResult:
                    return Server_ComboAttackResult_Validate(packet);
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
