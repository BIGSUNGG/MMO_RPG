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
        _animator = GetComponent<Animator>();
        _movement = GetComponent<CharacterMovementComponent>();

        _health = GetComponent<HealthComponent>();
        _health._onTakeDamageEvent.AddListener(OnTakeDamageEvent);
        _health._onDeathEvent.AddListener(OnDeathEvent);
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        Vector3 moveDir = new Vector3(_character._inputDir.x, 0.0f, _character._inputDir.y);
        if(_character.IsLocallyControlled() == false)
            Debug.Log($"{gameObject.name} {moveDir}");

        bool bIsMoving = _character._inputDir != Vector2.zero; // ĳ���Ͱ� �����̰� �ִ���
        bool bIsRunning = bIsMoving && _movement._bIsRunning; // ĳ���Ͱ� �����̰� �ִ���
        float forwardSpeed = Vector3.Dot(transform.forward, moveDir); // ���� �ӵ� ���ϱ�
        float horizonSpeed = Vector3.Dot(transform.right  , moveDir); // ���� �ӵ� ���ϱ�

        // �ִϸ����Ϳ� �� ����
        _animator.SetBool("IsMoving", bIsMoving);
        _animator.SetBool("IsRunning", bIsRunning);
        _animator.SetFloat("Forward Speed", forwardSpeed);
        _animator.SetFloat("Horizon Speed", horizonSpeed);


    }

    protected virtual void OnTakeDamageEvent() // ĳ���Ͱ� ������� �޾��� �� 
    {
        _animator.SetTrigger("On Take Damage");
    }

    protected virtual void OnDeathEvent() // ĳ���Ͱ� ������� �� 
    {
        _animator.SetTrigger("On Death");
        _animator.ResetTrigger("On Take Damage");
    }
}
