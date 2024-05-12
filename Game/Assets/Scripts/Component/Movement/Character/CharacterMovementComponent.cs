using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovementComponent : ObjectComponent
{
    protected enum RpcFunctionId
    {
        Multicast_DodgeRollStart = 0,
    }

	Rigidbody _rigidbody = null;

    protected override void Start()
    {
        base.Start();

        _rigidbody = Util.GetOrAddComponent<Rigidbody>(gameObject);
	}

    protected override void Update()
    {
        base.Update();

        _curMoveSpeed = _bIsRunning ? _runMaxSpeed : _walkMaxSpeed;

        if (Managers.Network.IsServer) // 서버인 경우
        {
            _rigidbody.useGravity = false;
        }
        else if (_owner.IsLocallyControlled()) // 클라이언트가 빙의한 오브젝트인 경우
        {
            _moveDir.Normalize();
            if (CanMovementInput()) // 입력이 가능한지
            {
                // 입력 방향으로 이동
	            _velocity = new Vector3(_moveDir.x * _curMoveSpeed, _velocity.y, _moveDir.y * _curMoveSpeed);
            }
            _moveDir = Vector2.zero;
        }
        else // 클라이언트가 빙의하지않은 오브젝트인 경우
        {
            _rigidbody.useGravity = false;

            _curSyncLerpTime += Time.deltaTime;
            float lerpPosVal = _curSyncLerpTime * _syncPosLerpMultiply;
            float lerpRotVal = _curSyncLerpTime * _syncRotLerpMultiply;

            transform.position = Vector3.Lerp(_syncStartPos, _syncEndPos, lerpPosVal);
            transform.rotation = Quaternion.Lerp(_syncStartRot, _syncEndRot, lerpRotVal);
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
    Vector2 _moveDir = Vector2.zero;

    public virtual void MoveForward(float axis)
	{
		_moveDir.y += axis;
	}

	public virtual void MoveRight(float axis)
	{
		_moveDir.x += axis;
	}

	float _jumpPower = 5.0f;
	public virtual void Jump()
	{
		if (_rigidbody.velocity.y != 0)
			return;

		_rigidbody.velocity = new Vector3(_rigidbody.velocity.x, _jumpPower, _rigidbody.velocity.z);
	}

    bool _bEnableInput = true; // 입력을 받을지
    // 입력받은 방향으로 움직일 수 있는지
    public virtual bool CanMovementInput() 
    {
        return _bEnableInput;
    }

	public bool IsFalling()
	{
		return _rigidbody.velocity.y < 0;
	}

	#endregion

	#region Sync
	Vector3 _syncStartPos;
	Vector3 _syncEndPos;

	Quaternion _syncStartRot;
    Quaternion _syncEndRot;

	float _curSyncLerpTime = 0.0f;
	float _syncPosLerpMultiply = 10;
	float _syncRotLerpMultiply = 10;

    public virtual void Sync(Vector3 pos, Quaternion rot, Vector2 inputDir, bool IsRunnung)
	{
		if (Managers.Network.IsServer)
		{
			transform.position = pos;
			transform.rotation = rot;

            _moveDir = inputDir;
            _moveDir.Normalize();
            _bIsRunning = IsRunnung;
        }
        else
		{
			_curSyncLerpTime = 0.0f;

            _syncStartPos = transform.position;
            _syncStartRot = transform.rotation;

            _syncEndPos = pos;
			_syncEndRot = rot;

            _moveDir = inputDir;
            _moveDir.Normalize();
            _bIsRunning = IsRunnung;
        }
    }
	#endregion

}
