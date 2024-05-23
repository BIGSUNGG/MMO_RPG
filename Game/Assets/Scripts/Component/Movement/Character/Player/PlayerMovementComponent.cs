﻿using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

public class PlayerMovementComponent : CharacterMovementComponent
{
    PlayerController _ownerPlayer = null;
    protected override void Start()
	{
        base.Start();

        _ownerPlayer = gameObject.GetComponent<PlayerController>();
        if (_ownerPlayer == null)
        {
            Debug.Log("Failed to find PlayerController");
            Debug.Assert(false);
        }
    }

    protected override void Update()
	{
        base.Update();

        if(Managers.Network.IsClient && _bIsdodging) // 구르고 있으면
        {
            // 구르는 방향으로 이동
            Vector2 dodgeRollVel = _dodgeDir * _dodgeSpeed;
            _velocity = new Vector3(dodgeRollVel.x, _velocity.y, dodgeRollVel.y);

            // 구르는 방향으로 회전
            transform.eulerAngles = new Vector3(0.0f, Util.GetAngle(_dodgeDir), 0.0f);
        }
    }

    #region Movement

    // Dodge
    bool _bEnableDodge = true;
    public bool _bIsdodging { get; protected set; } = false;           // 구르고 있는지
    Vector2 _dodgeDir = Vector2.zero;   // 구르는 방향
    float _dodgeSpeed = 12.5f;          // 구르는 속도
    const float _dodgeTime = 0.5f;      // 구르는 시간
    TimerHandler _dodgeEndTimer;        // 구르기를 끝내는 타이머

    float _dodgeDelay = 0.75f;          // 구르기 딜레이
    TimerHandler _dodgeDelayTimer;      // 구르기 딜레이 타이머

    public UnityEvent _onDodgeStartEvent; // 구르기 시작 시 호출

    // 구르기 시작 시 호출
    public virtual void DodgeRollStart() 
    {
        if (!_bEnableDodge || _bIsdodging || _ownerPlayer._inputDir == Vector2.zero) // 구르고 있거나 움직일 방향이 없는경우
            return;

        Vector2 moveDir = _ownerPlayer._inputDir;
        moveDir.Normalize();
        Multicast_DodgeRollStart(moveDir);
    }

    // 다른 클라이언트에 구르기 시작 패킷을 보냄
    // dir : 구를 방향
    protected virtual void Multicast_DodgeRollStart(Vector2 dir) 
    {
        // 패킷 보내기
        if (Managers.Network.IsClient) // 클라이언트에서 호출된 경우 
        {
            C_RpcComponentFunction dodgeStartPacket = new C_RpcComponentFunction();
            byte[] parameterBuffer = new byte[9];
            Array.Copy(BitConverter.GetBytes((float)dir.x), 0, parameterBuffer, 0, sizeof(float));
            Array.Copy(BitConverter.GetBytes((float)dir.y), 0, parameterBuffer, 4, sizeof(float));

            dodgeStartPacket.ObjectId = _owner.ObjectId;
            dodgeStartPacket.ComponentType = GameComponentType.PlayerMovementComponent;
            dodgeStartPacket.RpcFunctionId = RpcComponentFunctionId.MulticastDodgeRollStart;
            dodgeStartPacket.ParameterBytes = ByteString.CopyFrom(parameterBuffer);

            Managers.Network.SendServer(dodgeStartPacket);
        }
        else // 서버에서 호출된 경우
        {
            S_RpcComponentFunction dodgeStartPacket = new S_RpcComponentFunction();
            byte[] parameterBuffer = new byte[9];
            Array.Copy(BitConverter.GetBytes((float)dir.x), 0, parameterBuffer, 0, sizeof(float));
            Array.Copy(BitConverter.GetBytes((float)dir.y), 0, parameterBuffer, 4, sizeof(float));

            dodgeStartPacket.ObjectId = _owner.ObjectId;
            dodgeStartPacket.ComponentType = GameComponentType.PlayerMovementComponent;
            dodgeStartPacket.RpcFunctionId = RpcComponentFunctionId.MulticastDodgeRollStart;
            dodgeStartPacket.ParameterBytes = ByteString.CopyFrom(parameterBuffer);

            Managers.Network.SendMulticast(dodgeStartPacket);
        }

        Multicast_DodgeRollStart_Implementation(dir);
    }

    // 다른 클라이언트로 부터 구르기 시작 패킷을 받으면 호출
    // packet : 구를 방향의 바이트 배열 
    protected virtual void Multicast_DodgeRollStart_ReceivePacket(byte[] packet)
    {
        try
        {
            Vector2 dir = new Vector2();
            dir.x = BitConverter.ToSingle(packet, 0);
            dir.y = BitConverter.ToSingle(packet, 4);
            Multicast_DodgeRollStart_Implementation(dir);

            if (Managers.Network.IsServer) // 서버에서 패킷을 받았을 경우
            {
                // 다른 클라이언트에게 패킷 보내기
                S_RpcComponentFunction sendPacket = new S_RpcComponentFunction();
                sendPacket.ObjectId = _owner.ObjectId;
                sendPacket.ComponentType = GameComponentType.PlayerMovementComponent;
                sendPacket.ParameterBytes = ByteString.CopyFrom(packet);
                Managers.Network.SendMulticast(sendPacket);
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
    protected virtual bool Multicast_DodgeRollStart_Validate(byte[] packet)
    {
        try
        {
            Vector2 dir = new Vector2();
            dir.x = BitConverter.ToSingle(packet, 1);
            dir.y = BitConverter.ToSingle(packet, 5);
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
            return false;
        }
        return true;
    }

    // 구르기 시작 패킷을 보낸 이후 또는 다른 클라이언트로 부터 구르기 시작 패킷을 받으면 호출
    // dir : 구를 방향
    protected virtual void Multicast_DodgeRollStart_Implementation(Vector2 dir) 
    {
        Debug.Log("Dodge roll start");

        _bEnableDodge = false;
        _bIsdodging = true;
        _dodgeDir = dir;
        _dodgeDir.Normalize();

        _dodgeDelayTimer = Managers.Timer.SetTimer(_dodgeDelay, DodgeRollDelayEnd, false); // _dodgeDelay 구르기 딜레이 종료
        _dodgeEndTimer = Managers.Timer.SetTimer(_dodgeTime, DodgeRollEnd, false); // _dodgeRollTime이후에 구르기 종료

        _onDodgeStartEvent.Invoke();
    }

    // 구르기 종료 시 호출
    protected virtual void DodgeRollEnd() 
    {
        _bIsdodging = false;
        _dodgeDir = Vector2.zero;
        _dodgeEndTimer = null;
    }

    // 구르기 딜레이 종료 시 호출
    protected virtual void DodgeRollDelayEnd() 
    {
        _bEnableDodge = true;
        _dodgeDelayTimer = null;
    }

    public override bool CanMovementInput()
    {
        if (_bIsdodging)
            return false;

        return base.CanMovementInput();
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
                case RpcComponentFunctionId.MulticastDodgeRollStart:
                    Multicast_DodgeRollStart_ReceivePacket(packet);
                    return;
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
    public override bool RpcFunction_Validate(RpcComponentFunctionId functionId, byte[] packet)
    {
        try
        {
            switch (functionId)
            {
                case RpcComponentFunctionId.MulticastDodgeRollStart:
                    return Multicast_DodgeRollStart_Validate(packet);                    
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
