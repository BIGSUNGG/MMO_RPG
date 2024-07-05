using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterAnimParameter : MonoBehaviour
{
    protected CharacterController _character = null;
    protected Animator _animator;
    protected CharacterMovementComponent _movement;
    protected HealthComponent _health;

    protected virtual void Awake()
    {
        _character = GetComponent<CharacterController>();
        _character._onComboStartEvent.AddListener(OnComboStartEvent);
        _character._onComboEndEvent.AddListener(OnComboEndEvent);

        _animator = GetComponent<Animator>();
        _movement = GetComponent<CharacterMovementComponent>();

        _health = GetComponent<HealthComponent>();
        _health._onTakeDamageEvent.AddListener(OnTakeDamageEvent);
        _health._onDeathEvent.AddListener(OnDeathEvent);
        _health._onRespawnEvent.AddListener(OnRespawnEvent);

    }

    // Start is called before the first frame update
    protected virtual void Start()
    {

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        Vector3 moveDir = new Vector3(_character.Movement.MoveDir.x, 0.0f, _character.Movement.MoveDir.y);
            
        bool bIsMoving = _character.Movement.MoveDir != Vector2.zero; // ĳ���Ͱ� �����̰� �ִ���
        bool bIsRunning = bIsMoving && _movement._bIsRunning; // ĳ���Ͱ� �����̰� �ִ���
        float forwardSpeed = Vector3.Dot(transform.forward, moveDir); // ���� �ӵ� ���ϱ�
        float horizonSpeed = Vector3.Dot(transform.right  , moveDir); // ���� �ӵ� ���ϱ�

        // �ִϸ����Ϳ� �� ����
        _animator.SetBool("IsMoving", bIsMoving);
        _animator.SetBool("IsRunning", bIsRunning);
        _animator.SetFloat("Forward Speed", forwardSpeed);
        _animator.SetFloat("Horizon Speed", horizonSpeed);

        _animator.SetInteger("Current Combo", _character._curCombo);
    }

    protected virtual void OnTakeDamageEvent() // ĳ���Ͱ� ������� �޾��� �� 
    {
        _animator.applyRootMotion = false;
        _animator.SetTrigger("On Take Damage");
    }

    protected virtual void OnDeathEvent() // ĳ���Ͱ� ������� �� 
    {
        _animator.applyRootMotion = false;
        _animator.SetTrigger("On Death");
        _animator.ResetTrigger("On Take Damage");
    }

    protected virtual void OnRespawnEvent() // ĳ���Ͱ� ��Ȱ���� �� 
    {
        _animator.ResetTrigger("On Death");
        _animator.SetTrigger("On Respawn");

    }

    protected virtual void OnComboStartEvent()
    {
        _animator.SetTrigger("On Combo Start");
        _animator.ResetTrigger("On Combo End");
    }

    protected virtual void OnComboEndEvent()
    {
        _animator.SetTrigger("On Combo End");
        _animator.ResetTrigger("On Combo Start");
    }
}
