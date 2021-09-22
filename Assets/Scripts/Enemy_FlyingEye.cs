using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_FlyingEye : Unit
{
    // Position Vectors
    private Vector3 _originalPosition;
    private Vector3 _currentPosition;

    // Ground GameObject
    private GameObject _ground;
    private Transform _groundTransform;

    public void Start()
    {
        ResetHealth();

        _animator = GetComponent<Animator>();

        _ground = GameObject.Find("Ground");
        _groundTransform = _ground.GetComponent<Transform>();
        _currentPosition = new Vector3(
            transform.position.x, 
            transform.position.y, 
            transform.position.z
        );
        _originalPosition = _currentPosition;
    }

    public override void Die()
    {
        _animator.SetTrigger("Death");

        // Move down to touch ground
        this.transform.position = Vector3.Lerp(
            _currentPosition,
            new Vector3(_currentPosition.x, _groundTransform.position.y, 0),
            1f);

        isDead = true;
    }

    public override void Recover()
    {
        ResetHealth();

        if (isDead)
        {
            _animator.SetTrigger("Recover");
            // Move up to original position
            this.transform.position = Vector3.Lerp(
                _currentPosition,
                _originalPosition,
                1f);
        }
        isDead = false;
    }
}
