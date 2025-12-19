using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MovableObject
{
    protected override void Move()
    {
        transform.position = Vector3.MoveTowards(transform.position, movePoints[0].position, moveSpeed * Time.deltaTime);       
    }

}
