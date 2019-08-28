using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DianaPuntos : MonoBehaviour
{
    ParticleSystem mySpriteRenderer;

    private void OnEnable()
    {
        mySpriteRenderer = mySpriteRenderer ?? transform.GetChild(0).GetComponent<ParticleSystem>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        collision.transform.GetChild(0).gameObject.SetActive(true);
        mySpriteRenderer.transform.SetParent(null);
        mySpriteRenderer.Play();
        GameManager.Instance.TargetHit();
        Destroy(gameObject);
    }
}
