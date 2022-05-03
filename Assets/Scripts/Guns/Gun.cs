using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Gun : MonoBehaviour
{

    public enum FireMode
    {
        Semi,
        Burst,
        Automatic,
    }

    

    [Header("Magazine")]
    [Range(0, 2000)] public int StartAmmo;
    [Range(1, 500)] public int MagazineSize;
    [Range(0, 500)] public int BulletsLeft;
    [Range(0.001f, 10)] public float ReloadTime;
    public GameObject BulletPrefab;


    [Header("Shooting")]
    public FireMode CurrentFireMode;
    [Range(0.001f, 2)] public float FireRate;
    [Range(0.001f, 1)] public float BurstRate;
    [Range(1, 50)] public int BulletsPerShot;
    [Range(1, 10)] public int BulletsPerBurst;
    [Range(0, 1)] public float Spread;
    [Range(1, 1000)] public float ShootForce;
    [Range(0, 100)] public float UpwardForce;


    [Header("Audio")]
    private  AudioSource AudioSource;
    public AudioClip ShootSound;
    public AudioClip ReloadSound;

    [Header("References")]
    public Camera Camera;
    public Transform Muzzle;
    public Animator ShootAnimator;
    public ParticleSystem MuzzleFlash;
    public ParticleSystem ShellEjector;

    private bool Shooting;
    private bool ReadyToShoot;
    private bool Reloading;

    public bool allow_button_hold;
    public bool allow_invoke = true;




    private void Start()
    {
        AudioSource = GetComponent<AudioSource>();
        BulletsLeft = MagazineSize;
        ReadyToShoot = true;
    }


    void Update()
    {
        HandleInput();
    }


    //========== SHOOT MODES ==========

    IEnumerator FullAutomatic(float delayTime)
    {
        Shoot();
        yield return new WaitForSeconds(delayTime);
        if (Shooting && BulletsLeft > 0) StartCoroutine(FullAutomatic(FireRate));
        else ResetShot();
    }

    IEnumerator Burst(int timesLeft, float delayTime)
    {
        Shoot();
        yield return new WaitForSeconds(delayTime);
        if (timesLeft > 1 && BulletsLeft > 0) StartCoroutine(Burst(timesLeft - 1, FireRate));
        else ResetShot();
    }

    private void OneShot()
    {
        Shoot();

        if (allow_invoke)
        {
            Invoke("ResetShot", FireRate);
            allow_invoke = false;
        }
    }


    //========== SHOOTING ==========

    private void PrepareShot()
    {
        ReadyToShoot = false;


        switch (CurrentFireMode)
        {
            case FireMode.Semi:
                OneShot();
                break;
            case FireMode.Burst:
                StartCoroutine(Burst(BulletsPerBurst, BurstRate));
                break;
            case FireMode.Automatic:
                StartCoroutine(FullAutomatic(FireRate));
                break;
        }
    }

    private void Shoot()
    {
        Vector3 directionWithoutSpread = GetShootDirection();
        directionWithoutSpread = directionWithoutSpread.normalized;

        for (int i = 0; i < BulletsPerShot; i++)
        {
            Vector3 spread = Camera.transform.up * Random.Range(-1f, 1f) + Camera.transform.right * Random.Range(-1f, 1f);
            Vector3 directionWithSpread = directionWithoutSpread + spread * Spread * 0.1f;

            GameObject currentBullet = Instantiate(BulletPrefab, Muzzle.position, Quaternion.identity);
            currentBullet.transform.forward = directionWithSpread.normalized;
            currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * ShootForce, ForceMode.Impulse);
            currentBullet.GetComponent<Rigidbody>().AddForce(Muzzle.transform.up * UpwardForce, ForceMode.Impulse);
        }

        BulletsLeft--;

        AudioSource.pitch = 1 + Random.Range(-0.1f, 0.1f);
        AudioSource.PlayOneShot(ShootSound, 1);

        ShootAnimator.Play(0);
        MuzzleFlash.Play();
        ShellEjector.Play();

        Vector3 originalAngles = Camera.transform.eulerAngles;
        Camera.transform.localEulerAngles += new Vector3(-0.2f,0,0);
        StartCoroutine(ResetCamera(originalAngles, 0.03f));



    }

    IEnumerator ResetCamera(Vector3 v, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        Camera.transform.localEulerAngles -= new Vector3(-0.2f, 0, 0);
    }

    private void ResetShot()
    {
        ReadyToShoot = true;
        allow_invoke = true;
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


    //========== RELOADING ==========

    private void Reload()
    {
        Reloading = true;
        Invoke("ReloadFinished", ReloadTime);

        AudioSource.PlayOneShot(ReloadSound, 1);
    }

    private void ReloadFinished()
    {
        BulletsLeft = MagazineSize;
        Reloading = false;
    }


    //========== INPUT ==========

    private void HandleInput()
    {
        if (CurrentFireMode == FireMode.Automatic) Shooting = Input.GetButton("Fire1");
        else Shooting = Input.GetButtonDown("Fire1");

        if (Input.GetKeyDown(KeyCode.R) && BulletsLeft < MagazineSize && !Reloading) Reload();
        if (ReadyToShoot && Shooting && !Reloading && BulletsLeft <= 0) Reload();

        if (ReadyToShoot && Shooting && !Reloading && BulletsLeft > 0) PrepareShot();


        if (Input.GetKeyDown("1")) CurrentFireMode = FireMode.Semi;
        if (Input.GetKeyDown("2")) CurrentFireMode = FireMode.Burst;
        if (Input.GetKeyDown("3")) CurrentFireMode = FireMode.Automatic;
    }



}
