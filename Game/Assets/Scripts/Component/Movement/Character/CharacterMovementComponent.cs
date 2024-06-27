using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovementComponent : ObjectComponent
{
    CharacterController _character = null;
    Rigidbody _rigidbody = null;

    protected override void Start()
    {
        base.Start();

        _character = GetComponent<CharacterController>();
        _rigidbody = Util.GetOrAddComponent<Rigidbody>(gameObject);
	}

    protected override void Update()
    {
        base.Update();

        _curMoveSpeed = _bIsRunning ? _runMaxSpeed : _walkMaxSpeed;

        if (Owner.IsLocallyControlled()) // 클라이언트가 빙의한 오브젝트거나 서버의 Ai컨트롤러일 경우
        {
            _rigidbody.useGravity = true;
            _rigidbody.isKinematic = false;

            Vector2 _moveDir = _character._moveDir;
            _moveDir.Normalize();
            if (_character.CanMove() && this.CanMove()) // 캐릭터가 움직일 수 있는지
            {
                // 입력 방향으로 이동
                _velocity = new Vector3(_moveDir.x * _curMoveSpeed, _velocity.y < 2.0f ? _velocity.y : 2.0f, _moveDir.y * _curMoveSpeed);
            }
        }
        else if (Managers.Network.IsClient) // 클라이언트가 빙의하지않은 오브젝트인 경우
        {
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = true;

            _curPosSyncLerpTime += Time.deltaTime * _syncPosLerpMultiply;
            _curRotSyncLerpTime += Time.deltaTime * _syncRotLerpMultiply;

            if (_curPosSyncLerpTime >= 1)
                _character._moveDir = Vector3.zero;

            transform.position = Vector3.Lerp(_syncStartPos, _syncEndPos, _curPosSyncLerpTime);
            transform.rotation = Quaternion.Lerp(_syncStartRot, _syncEndRot, _curRotSyncLerpTime);
        }
        else // 서버인 경우
        {
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = true;
        }
    }

	#region Movement
    // State
	public Vector3 _velocity { get { return _rigidbody.velocity; }  set { _rigidbody.velocity = value; } }

    // Move
    public bool _bIsRunning = false; // 달리고 있는지
    float _curMoveSpeed = 4.0f; // 현재 이동 속도
	float _walkMaxSpeed = 4.0f; // 걷기 속도
    float _runMaxSpeed = 7.5f; // 뛰기 속도

    // Input
	float _jumpPower = 5.0f;
	public virtual void Jump()
	{
		if (_rigidbody.velocity.y != 0)
			return;

		_rigidbody.velocity = new Vector3(_rigidbody.velocity.x, _jumpPower, _rigidbody.velocity.z);
	}

    public virtual bool CanMove()
    {
        return true;
    }

    public bool IsFalling()
	{
		return _rigidbody.velocity.y < 0;
	}

    #endregion

    #region Sync
    public Vector3 _syncStartPos = Vector3.zero;
	public Vector3 _syncEndPos = Vector3.zero;

	Quaternion _syncStartRot = new Quaternion();
    Quaternion _syncEndRot = new Quaternion();

    float _curPosSyncLerpTime = 0.0f;
	float _curRotSyncLerpTime = 0.0f;
    float _syncPosLerpMultiply = 5.75f;
	float _syncRotLerpMultiply = 5.75f; 

    public virtual void Sync(Vector3 pos, Quaternion rot, Vector3 moveDir, bool IsRunnung)
	{
        if (Managers.Network.IsServer)
		{
			transform.position = pos;
			transform.rotation = rot;

            _bIsRunning = IsRunnung;
            _character._moveDir = moveDir;
        }
        else
        {
            if (pos != _syncEndPos)
            {
                _curPosSyncLerpTime = 0.0f;
                _syncStartPos = transform.position;
                if(moveDir != Vector3.zero)
                    _character._moveDir = moveDir;
            }

            if(rot != _syncEndRot)
            {
                _curRotSyncLerpTime = 0.0f;
                _syncStartRot = transform.rotation;
            }

            _syncEndPos = pos;
			_syncEndRot = rot;

            _bIsRunning = IsRunnung;
        }
    }
	#endregion

}
