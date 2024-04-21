using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementComponent : MonoBehaviour
{
	ObjectController _owner = null;
	Rigidbody _rigidbody = null;

	void Start()
	{
		_owner = gameObject.GetComponent<ObjectController>();
		if (_owner == null)
		{
			Debug.Log("Failed to find ObjectController");
			Debug.Assert(false);
		}

		_rigidbody = Util.GetOrAddComponent<Rigidbody>(gameObject);
	}

	void Update()
	{
        _curMoveSpeed = _bIsRunning ? _runMaxSpeed : _walkMaxSpeed;

        if (Managers.Network.IsServer) // 서버인 경우
        {
            _rigidbody.useGravity = false;
        }
        else if (_owner.IsLocallyControlled()) // 클라이언트가 빙의한 오브젝트인 경우
        {
            _inputDir.Normalize();
            _lastInputDir = _inputDir;
            _velocity = new Vector3(_inputDir.x * _curMoveSpeed, _velocity.y, _inputDir.y * _curMoveSpeed);
            _inputDir = Vector2.zero;
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
    float _curMoveSpeed = 2.5f; // 현재 이동 속도
	float _walkMaxSpeed = 2.5f; // 걷기 속도
    float _runMaxSpeed = 7.5f; // 뛰기 속도

    // Input
    Vector2 _inputDir = Vector2.zero;
    public Vector2 _lastInputDir = Vector2.zero;

    public virtual void MoveForward(float axis)
	{
		_inputDir.y += axis;
	}

	public virtual void MoveRight(float axis)
	{
		_inputDir.x += axis;
	}

	float _jumpPower = 5.0f;
	public virtual void Jump()
	{
		if (_rigidbody.velocity.y != 0)
			return;

		_rigidbody.velocity = new Vector3(_rigidbody.velocity.x, _jumpPower, _rigidbody.velocity.z);
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

            _inputDir = inputDir;
            _lastInputDir = inputDir;

            _bIsRunning = IsRunnung;
        }
        else
		{
			_curSyncLerpTime = 0.0f;

            _syncStartPos = transform.position;
            _syncStartRot = transform.rotation;

            _syncEndPos = pos;
			_syncEndRot = rot;

            _inputDir = inputDir;
            _lastInputDir = inputDir;

            _bIsRunning = IsRunnung;
        }
    }
	#endregion

}
