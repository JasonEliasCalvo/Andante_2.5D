using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableObject : MonoBehaviour
{
    [SerializeField] protected Transform[] movePoints;
    [SerializeField] protected float moveSpeed;

    protected int nextPlatform = 1;
    protected bool platformOrder = true;
    protected bool canMove = false;

    private void Update()
    {
        if (canMove)
        {
            Move();
        }
    }

    protected virtual void Move()
    {

    }

    public void StartMoving() => canMove = true;
    public void StopMoving() => canMove = false;
}
