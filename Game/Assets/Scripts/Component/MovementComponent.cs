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

        _rigidbody = gameObject.GetComponent<Rigidbody>();
        if(_rigidbody == null)
        {
            Debug.Log("Failed to find Rigidbody");
            _rigidbody = gameObject.AddComponent<Rigidbody>();
        }
    }

    void Update()
    {
        _moveDir.Normalize();

        Vector3 velocity = new Vector3(_moveDir.x * _walkMaxSpeed, 0.0f, _moveDir.y * _walkMaxSpeed) * Time.deltaTime;
        gameObject.transform.Translate(velocity);

        _moveDir = new Vector2(0, 0);

        if(Managers.Network.IsServer == false && _owner.IsLocallyControlled() == false && _bSync)
        {
            _curSyncLerpTime += Time.deltaTime;

            Debug.Log(_curSyncLerpTime * _syncLerpMultiply);
            transform.position = Vector3.Lerp(_syncStartPos, _syncEndPos, (_curSyncLerpTime * _syncLerpMultiply));
            transform.eulerAngles = Vector3.Lerp(_syncStartRot, _syncEndRot, (_curSyncLerpTime * _syncLerpMultiply));
        }
    }

    #region Movement
    public Vector3 _velocity { get { return _rigidbody.velocity; }  set { _rigidbody.velocity = value; } }

    float _walkMaxSpeed = 2.5f;
    Vector2 _moveDir = new Vector2(0, 0);

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

    public bool IsFalling()
    {
        return _rigidbody.velocity.y < 0;
    }

    #endregion

    #region Sync
    Vector3 _syncStartPos;
    Vector3 _syncEndPos;

    Vector3 _syncStartRot;
    Vector3 _syncEndRot;

    bool _bSync = false;
    float _curSyncLerpTime = 0.0f;
    const float _syncLerpMultiply = 10;

    public virtual void Sync(Vector3 pos, Vector3 rot)
    {
        if (Managers.Network.IsServer)
        {
            transform.position = pos;
            transform.eulerAngles = rot;
        }
        else
        {
	        _bSync = true;
	        _curSyncLerpTime = 0.0f;
	
	        _syncStartPos = transform.position;
	        _syncStartRot = transform.eulerAngles;
	
	        _syncEndPos = pos;
	        _syncEndRot = rot;
        } 
    }
    #endregion

}
