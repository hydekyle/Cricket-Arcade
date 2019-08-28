using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallOut : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(collision.gameObject, 0.2f);
        GameManager.Instance.BallOut();
    }
}
