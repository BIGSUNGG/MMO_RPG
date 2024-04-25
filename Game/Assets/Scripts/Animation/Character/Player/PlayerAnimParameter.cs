using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimParameter : CharacterAnimParameter
{
    private PlayerMovementComponent _playerMovement;

    protected override void Awake()
    {
        base.Awake();

        _playerMovement = GetComponent<PlayerMovementComponent>();
        _playerMovement._onDodgeStartEvent.AddListener(OnDodgeStartEvent);
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected virtual void OnDodgeStartEvent() // 플레이어가 구르기 시작했을 때 호출
    {
        _animator.SetTrigger("OnDodge");
    }
}
