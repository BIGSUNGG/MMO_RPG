using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpBar : Effect
{
    CharacterController _character;
    protected GameObject _camera;
    public GameObject _canvas;
    public GameObject _forgeGround;

    protected override void Start()
    {
        base.Start();

        _character = GetComponentInParent<CharacterController>();
        if (_character == null)
            Debug.LogWarning("CharacterController is null");

        _camera = GameObject.Find("Main Camera");
        if (_camera == null)
            Debug.LogWarning("Camera is null");

        if (_canvas == null)
            Debug.LogWarning("ForgeGround is null");

        if (_forgeGround == null)
            Debug.LogWarning("ForgeGround is null");
    }

    protected override void Update()
    {
        base.Update();

        _canvas.transform.eulerAngles = new Vector3(90.0f, 0.0f, 180.0f);
        _canvas.transform.position = new Vector3(0.0f, 2.0f, 0.3f) + _character.transform.position;

        if (_character)
            _forgeGround.transform.localScale = new Vector3(((float)_character.Health._curHp / (float)_character.Health._maxHp) * 10, 1.0f, 0.0f);
    }
}
