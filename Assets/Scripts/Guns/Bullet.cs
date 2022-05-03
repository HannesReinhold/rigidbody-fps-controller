using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject BulletImpact;

    private void OnCollisionEnter(Collision collision)
    {
        Instantiate(BulletImpact, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
