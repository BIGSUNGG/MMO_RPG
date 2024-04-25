using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerMovementComponent : CharacterMovementComponent
{
    public override void Start()
	{
        base.Start();
	}

	public override void Update()
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
    bool _bIsdodging = false;           // 구르고 있는지
    Vector2 _dodgeDir = Vector2.zero;   // 구르는 방향
    float _dodgeSpeed = 12.5f;          // 구르는 속도
    const float _dodgeTime = 0.5f;      // 구르는 시간
    TimerHandler _dodgeEndTimer;        // 구르기를 끝내는 타이머

    float _dodgeDelay = 0.75f;          // 구르기 딜레이
    TimerHandler _dodgeDelayTimer;      // 구르기 딜레이 타이머

    public UnityEvent _onDodgeStartEvent; // 구르기 시작 시 호출

    public virtual void DodgeRollStart() // 구르기 시작 시 호출
    {
        if (!_bEnableDodge || _bIsdodging || _lastInputDir == Vector2.zero) // 구르고 있거나 움직일 방향이 없는경우
            return;

        Multicast_DodgeRollStart(_lastInputDir);
    }

    protected virtual void Multicast_DodgeRollStart(Vector2 dir) // 다른 클라이언트에 구르기 시작 패킷을 보냄
    {
        // 패킷 보내기
        C_DodgeStart dodgeStartPacket = new C_DodgeStart();
        dodgeStartPacket.X = dir.x;
        dodgeStartPacket.Y = dir.y;
        Managers.Network.SendServer(dodgeStartPacket);

        Multicast_DodgeRollStart_Implementation(dir);
    }

    public virtual void Multicast_DodgeRollStart_ReceivePacket(Vector2 dir) // 다른 클라이언트로 부터 구르기 시작 패킷을 받으면 호출
    {
        Multicast_DodgeRollStart_Implementation(dir);
    }

    protected virtual void Multicast_DodgeRollStart_Implementation(Vector2 dir) // 구르기 시작 패킷을 보낸 이후 또는 다른 클라이언트로 부터 구르기 시작 패킷을 받으면 호출
    {
        _bEnableDodge = false;
        _bIsdodging = true;
        _dodgeDir = dir;
        _dodgeDir.Normalize();

        _dodgeDelayTimer = Managers.Timer.SetTimer(_dodgeDelay, DodgeRollDelayEnd, false); // _dodgeDelay 구르기 딜레이 종료
        _dodgeEndTimer = Managers.Timer.SetTimer(_dodgeTime, DodgeRollEnd, false); // _dodgeRollTime이후에 구르기 종료

        OnDodgeRollStartEvent();
    }

    public virtual void OnDodgeRollStartEvent() // 구르기 시작 시 호출
    {
        _onDodgeStartEvent.Invoke();
    }

    protected virtual void DodgeRollEnd() // 구르기 종료 시 호출
    {
        _bIsdodging = false;
        _dodgeDir = Vector2.zero;
        _dodgeEndTimer = null;
    }

    protected virtual void DodgeRollDelayEnd() // 구르기 딜레이 종료 시 호출
    {
        _bEnableDodge = true;
        _dodgeDelayTimer = null;
    }

    public override bool CanInputMove()
    {
        if (_bIsdodging)
            return false;

        return base.CanInputMove();
    }

    #endregion

    #region Sync
    #endregion

}
