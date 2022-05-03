using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicGun : MonoBehaviour
{
    public Transform Muzzle;
    public GameObject Bullet;

    public AudioClip ChargeSound;
    public AudioClip ShootSound;
    public AudioClip StopSound;
    private AudioSource audioSource;

    public MoveWeaponSocket MoveWeapon; 


    public float FireRate = 0.1f;
    public float ShootForce = 100;

    private float charge = 0;

    private bool CanShoot = true;
    private bool Shooting;
    private bool AlreadyPlayingSound = false;

    public ParticleSystem MuzzleFlash;


    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
    }
    private void Update()
    {
        HandleInput();
        HandleSound();

        if (Shooting)
        {
            Charge();
        }
        else
        {
            Stop();
        }

        if (Shooting && CanShoot) Shoot();
    }

    void HandleInput()
    {
        Shooting = Input.GetButton("Fire1");
    }

    void HandleSound()
    {
        if (charge < 1 && Input.GetButtonDown("Fire1"))
        {
            audioSource.Stop();
            audioSource.clip = ChargeSound;
            audioSource.time = charge * ShootSound.length;
            audioSource.loop = false;
            audioSource.Play();
            CanShoot = false;
        }else if (charge == 1 && Shooting && !AlreadyPlayingSound)
        {
            audioSource.Stop();
            audioSource.clip = ShootSound;
            audioSource.loop = true;
            audioSource.Play();
            AlreadyPlayingSound = true;
            CanShoot = true;
        }
        else if(Input.GetButtonUp("Fire1"))
        {
            audioSource.Stop();
            AlreadyPlayingSound = false;
            audioSource.clip = StopSound;
            audioSource.time = (1-charge) * ShootSound.length;
            audioSource.loop = false;
            audioSource.Play();
        }
    }

    void Charge()
    {
        charge += Time.deltaTime * 1.5f;
        if (charge >= 1) charge = 1;
    }

    void Stop()
    {
        charge -= Time.deltaTime;
        if (charge < 0) charge = 0;
    }


    void Shoot()
    {
        GameObject current_bullet = Instantiate(Bullet, Muzzle.position, Quaternion.identity);
        Vector3 rand = Muzzle.up * Random.Range(-1f,1f)+ Muzzle.right * Random.Range(-1f, 1f);
        rand = rand * 0.01f;
        Vector3 dir = Muzzle.forward + rand;
        current_bullet.transform.forward = Muzzle.forward;
        current_bullet.GetComponent<Rigidbody>().AddForce((Muzzle.forward.normalized + rand) * ShootForce, ForceMode.Impulse);
        
        CanShoot = false;

        MuzzleFlash.Play();

        MoveWeapon.PushBack(Muzzle.forward.normalized, 0.01f);
        Invoke("ResetShot", FireRate);
    }

    void ResetShot()
    {
        CanShoot = true;
    }
}
