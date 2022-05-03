using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateBarrel : MonoBehaviour
{
    private bool CanRotate = true;

    public float MaxRotationSpeed = 5000;
    private float CurrentRotationSpeed = 0;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            Accelerate();
        }
        else if (CurrentRotationSpeed > 0)
        {
            Decelerate();
        }

        if (CanRotate) transform.Rotate(new Vector3(0,0,1) * Time.deltaTime * CurrentRotationSpeed);
    }

    void Accelerate()
    {
        CurrentRotationSpeed += Time.deltaTime * 10000;
        if (CurrentRotationSpeed > MaxRotationSpeed) CurrentRotationSpeed = MaxRotationSpeed;
    }

    void Decelerate()
    {
        CurrentRotationSpeed -= Time.deltaTime * 4000;
        if (CurrentRotationSpeed < 0) CurrentRotationSpeed = 0;
    }

    public void SetRotate(bool r)
    {
        CanRotate = r;
    }
}
