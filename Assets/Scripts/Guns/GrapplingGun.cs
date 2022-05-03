using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingGun : MonoBehaviour
{





    [Header("Magazine")]
    [Range(0.001f, 10)] public float ReloadTime;
    public GameObject BulletPrefab;


    [Header("Shooting")]
    [Range(0, 1)] public float Spread;
    [Range(1, 1000)] public float ShootForce;
    [Range(0, 100)] public float UpwardForce;


    [Header("Audio")]
    private AudioSource AudioSource;
    public AudioClip ShootSound;
    public AudioClip ReloadSound;

    [Header("References")]
    public Camera Camera;
    public Transform Player;
    public Transform Muzzle;
    public Animator ShootAnimator;
    public ParticleSystem MuzzleFlash;
    public ParticleSystem ShellEjector;
    public LayerMask Grappleable;


    private LineRenderer lr;
    private SpringJoint joint;
    private float CurrentDistance;

    private bool Shooting;
    private bool ReadyToShoot;
    private bool Pull;

    private Vector3 GrapplePosition;

    public GameObject Hook;




    private void Start()
    {
        AudioSource = GetComponent<AudioSource>();
        ReadyToShoot = true;

        lr = GetComponent<LineRenderer>();
    }


    void Update()
    {
        HandleInput();
        if (Pull) PullHook();
    }

    void LateUpdate()
    {
        DrawRope();
    }



    //========== SHOOTING ==========

    private void ShootGrapple()
    {
        Vector3 directionWithoutSpread = GetShootDirection();
        directionWithoutSpread = directionWithoutSpread.normalized;

        Vector3 spread = Camera.transform.up * Random.Range(-1f, 1f) + Camera.transform.right * Random.Range(-1f, 1f);
        Vector3 directionWithSpread = directionWithoutSpread + spread * Spread * 0.1f;
        //GameObject currentBullet = Instantiate(BulletPrefab, Muzzle.position, Quaternion.identity);
        Hook.GetComponent<Rigidbody>().velocity = new Vector3();
        Hook.transform.position = Muzzle.position;
        Hook.transform.forward = directionWithSpread.normalized;
        Hook.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * ShootForce, ForceMode.Impulse);
        Hook.GetComponent<Rigidbody>().AddForce(Muzzle.transform.up * UpwardForce, ForceMode.Impulse);

        AudioSource.pitch = 1 + Random.Range(-0.1f, 0.1f);
        AudioSource.PlayOneShot(ShootSound, 1);

        ShootAnimator.Play(0);
        MuzzleFlash.Play();
        ShellEjector.Play();



        ReadyToShoot = false;
    }


    private Vector3 GetShootDirection()
    {
        Ray ray = Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Vector3 target_point;
        if (Physics.Raycast(ray, out hit))
            target_point = hit.point;
        else
            target_point = ray.GetPoint(100);

        return target_point - Muzzle.position;
    }

    public void StartGrapple()
    {
        GrapplePosition = Hook.transform.position;
        joint = Player.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = GrapplePosition;

        float distanceFromPoint = Vector3.Distance(Player.position, GrapplePosition);
        CurrentDistance = distanceFromPoint;

        joint.maxDistance = distanceFromPoint * 0.8f;
        joint.minDistance = distanceFromPoint * 0.25f;

        joint.spring = 4.5f;
        joint.damper = 7f;
        joint.massScale = 4.5f;

        lr.positionCount = 2;

        CurrentGrapplePosition = GrapplePosition;
    }


    //========== RELOADING ==========

    private void StopGrapple()
    {
        lr.positionCount = 0;
        Destroy(joint);
        ReadyToShoot = true;
        Hook.GetComponent<GrapplingHook>().ResetHook();
        Hook.transform.position = Muzzle.position;
    }

    Vector3 CurrentGrapplePosition;

    void DrawRope()
    {
        //If not grappling, don't draw rope
        if (!joint) return;

        //CurrentGrapplePosition = Vector3.Lerp(CurrentGrapplePosition, GrapplePosition, Time.deltaTime * 8f);

        lr.SetPosition(0, Muzzle.position);
        lr.SetPosition(1, CurrentGrapplePosition);
    }

    void PullHook()
    {
        if (!joint) return;
        CurrentDistance -= 20f;
        if (CurrentDistance < 1) CurrentDistance = 1;

        joint.maxDistance = CurrentDistance * 0.8f;
        joint.minDistance = CurrentDistance * 0.25f;
    }


    //========== INPUT ==========

    private void HandleInput()
    {
        Shooting = Input.GetButton("Fire1");
        Pull = Input.GetButton("Fire2");

        if (ReadyToShoot && Shooting) ShootGrapple();
        if (!Shooting) StopGrapple();

    }



}
