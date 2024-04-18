using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimParameter : MonoBehaviour
{
    private Animator _animator;
    private MovementComponent _movement;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _movement = GetComponent<MovementComponent>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 inputDir = new Vector3(_movement._lastInputDir.x, 0.0f, _movement._lastInputDir.y);

        bool bIsMoving = _movement._lastInputDir != Vector2.zero; // 캐릭터가 움직이고 있는지
        float forwardSpeed = Vector3.Dot(transform.forward, inputDir); // 정면 속도 구하기
        float horizonSpeed = Vector3.Dot(transform.right  , inputDir); // 수평 속도 구하기

        // 애니메이터에 값 적용
        _animator.SetBool("IsMoving", bIsMoving);
        _animator.SetFloat("Forward Speed", forwardSpeed);
        _animator.SetFloat("Horizon Speed", horizonSpeed);

        Debug.Log($"{bIsMoving}, {forwardSpeed}, {horizonSpeed}");
    }
}
