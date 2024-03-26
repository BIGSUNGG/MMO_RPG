using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementComponent : MonoBehaviour
{
    Rigidbody _rigidbody = null;

    void Start()
    {
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

        Vector3 getVel = new Vector3(_moveDir.x * _walkMaxSpeed, _rigidbody.velocity.y, _moveDir.y * _walkMaxSpeed);
        _rigidbody.velocity = getVel;

        _moveDir = new Vector2(0, 0);
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
}
