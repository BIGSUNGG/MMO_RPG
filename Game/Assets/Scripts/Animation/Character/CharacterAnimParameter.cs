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

        bool bIsMoving = _character._inputDir != Vector2.zero; // 캐릭터가 움직이고 있는지
        bool bIsRunning = bIsMoving && _movement._bIsRunning; // 캐릭터가 움직이고 있는지
        float forwardSpeed = Vector3.Dot(transform.forward, moveDir); // 정면 속도 구하기
        float horizonSpeed = Vector3.Dot(transform.right  , moveDir); // 수평 속도 구하기

        // 애니메이터에 값 적용
        _animator.SetBool("IsMoving", bIsMoving);
        _animator.SetBool("IsRunning", bIsRunning);
        _animator.SetFloat("Forward Speed", forwardSpeed);
        _animator.SetFloat("Horizon Speed", horizonSpeed);


    }

    protected virtual void OnTakeDamageEvent() // 캐릭터가 대미지를 받았을 때 
    {
        _animator.SetTrigger("On Take Damage");
    }

    protected virtual void OnDeathEvent() // 캐릭터가 사망했을 때 
    {
        _animator.SetTrigger("On Death");
        _animator.ResetTrigger("On Take Damage");
    }
}
