using System;
using System.Collections;
using UnityEngine;

public class Player : Unit
{
    public void Start()
    {
        ResetHealth();

        _animator = GetComponent<Animator>();
    }
}
