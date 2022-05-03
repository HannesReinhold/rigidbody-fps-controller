using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWeaponSocket : MonoBehaviour
{
    public float Amount;
    public float MaxAmount;
    public float Speed = 10;

    private Vector3 InitialPosition;

    void Start()
    {
        InitialPosition = transform.localPosition;
    }

    void Update()
    {
        float mouseX = Mathf.Clamp(-Input.GetAxis("Mouse X") * Amount, - MaxAmount, MaxAmount);
        float mouseY = Mathf.Clamp(-Input.GetAxis("Mouse Y") * Amount, -MaxAmount, MaxAmount);
        Vector3 TargetPosition = new Vector3(mouseX, mouseY, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, InitialPosition + TargetPosition, Time.deltaTime * Speed);
    }

    public void PushBack(Vector3 dir, float strength)
    {
        transform.localPosition -= dir * strength;
    }
}
