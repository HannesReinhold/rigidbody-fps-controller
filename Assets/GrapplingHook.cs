using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{

    private Rigidbody rb;
    public GrapplingGun grapplingGun;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & grapplingGun.Grappleable) == 0) return;
        rb.isKinematic = true;
        grapplingGun.StartGrapple();
    }

    public void ResetHook()
    {
        rb.isKinematic = false;
    }

}
