using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShip : MonoBehaviour
{
    [SerializeField] Transform ship;
    [SerializeField] Camera cam;
    [SerializeField] Rigidbody body;
    [SerializeField] ParticleSystem explosion;
    [SerializeField] AudioSource hitSound;
    [SerializeField] AudioSource deathSound;
    [SerializeField] AudioSource shipSound;
    [SerializeField] float horizontalRotation;
    [SerializeField] float verticalRotation;
    [SerializeField] float baseFOV = 70.0f;
    [SerializeField] float hyperFOV = 80.0f;
    [SerializeField] bool godMode = false;

    [Header("Gameplay Variables")]
    [SerializeField] float speed;
    [SerializeField] int maxLives = 3;
    [SerializeField] float immuneTime = 1.0f;
    [SerializeField] float immuneFlashInterval = 0.1f;
    bool hyper = false;
    float desiredFOV = 0.0f;
    float fovStep = 60.0f;
    float speedMod = 1.0f;

    bool immune = false;
    float immuneTimer = 0.0f;
    public int currentLives;

    float immuneFlash = 0;
    MeshRenderer m;

    ParticleSystem ps;
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        InitPlayer();
        var emm = explosion.emission;
        emm.enabled = false;
        m = GetComponent<MeshRenderer>();
    }

    void UpdateMovement()
    {
        Vector3 movement = new Vector3();

        float vert = Input.GetAxis("Vertical");
        float hori = Input.GetAxis("Horizontal");
        movement.x = hori * Time.deltaTime * speed * Game.GameSpeed;
        movement.y = -vert * Time.deltaTime * speed * Game.GameSpeed;
        

        body.MovePosition(transform.position + movement);

        Vector3 desiredRotation = 
            new Vector3(verticalRotation * vert, horizontalRotation * hori, -horizontalRotation * hori);
        Quaternion desired = Quaternion.Euler(desiredRotation);

        ship.rotation = Quaternion.RotateTowards(ship.rotation, desired, 180 * Time.deltaTime * Game.GameSpeed);

    }

    IEnumerator CameraHyperdrive()
    {
        while (cam.fieldOfView != desiredFOV)
        {
            cam.fieldOfView = Mathf.MoveTowards(cam.fieldOfView, desiredFOV, fovStep * Time.deltaTime);
            yield return null;
        }
        if (hyper)
            Game.GameSpeed = 2.0f;
    }

    void UpdatePlatform()
    {
        // sway
        PlatformController.instance.floatValues[0] = PlatformController.instance.MapRange(ship.position.x, -4.5f, 4.5f, -8, 8);
        // heave
        PlatformController.instance.floatValues[2] = PlatformController.instance.MapRange(ship.position.y, -4.5f, 4.5f, -4, 4);
        // pitch
        PlatformController.instance.floatValues[3] = PlatformController.instance.MapRange(Mathf.DeltaAngle(-ship.eulerAngles.x, 0), -verticalRotation, verticalRotation, -5.0f, 5.0f);
        // roll
        PlatformController.instance.floatValues[4] = PlatformController.instance.MapRange(Mathf.DeltaAngle(ship.eulerAngles.z, 0), -horizontalRotation, horizontalRotation, -4, 4);
        // yaw
        PlatformController.instance.floatValues[5] = PlatformController.instance.MapRange(Mathf.DeltaAngle(-ship.eulerAngles.y, 0), -horizontalRotation, horizontalRotation, -4, 4);

        CameraShake shake = cam.GetComponent<CameraShake>();
        if (shake)
        {
            if (shake.GetTrauma > 0)
            {
                Transform camT = cam.GetComponent<Transform>();

                // pitch
                PlatformController.instance.floatValues[3] = PlatformController.instance.MapRange(Mathf.DeltaAngle(camT.eulerAngles.x, 0) * 2.5f, -shake.maxPitch, shake.maxPitch, -5.0f, 5.0f);
                // roll
                PlatformController.instance.floatValues[4] = PlatformController.instance.MapRange(Mathf.DeltaAngle(camT.eulerAngles.z, 0) * 2.5f, -shake.maxRoll, shake.maxRoll, -4, 4);
                // yaw
                PlatformController.instance.floatValues[5] = PlatformController.instance.MapRange(Mathf.DeltaAngle(camT.eulerAngles.y, 0) * 2.5f, -shake.maxYaw, shake.maxYaw, -4, 4);

            }
        }

        PlatformController.instance.SendSerial();
    }

    float sintime = 0;
    float prevSin = 0;

    void Update()
    {
        /*
        if (Game.GameState == GameState.MainMenu)
        {
            sintime += Time.deltaTime * 0.5f;
            if (sintime >= 2.0f)
                sintime = 0;

            Vector3 newPos = body.transform.position;
            float sin = Mathf.Sin(sintime * Mathf.PI) * 1.5f;
            newPos.y = sin - 1.5f;
            Vector3 rot = Vector3.zero;
            rot.x = 15;
            if (sin > prevSin)
            {
                rot.x = -15;
            }
            Quaternion desired = Quaternion.Euler(rot);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, 45 * Time.deltaTime);
            prevSin = sin;
            body.transform.position = newPos;
        }
        */
        if (Game.GameState == GameState.Running)
        {
            if (immuneTimer < immuneTime)
            {
                immuneTimer += Time.deltaTime;
                if (immuneTimer >= immuneTime)
                    immune = false;
            }


            if (immune)
            {
                immuneFlash += Time.deltaTime;
                if (immuneFlash > immuneFlashInterval)
                {
                    immuneFlash = 0;
                    m.enabled = !m.enabled;
                }
            }
            else
            {
                m.enabled = true;
            }

            UpdateMovement();

            if (Input.GetKey(KeyCode.JoystickButton0) && !immune && !hyper)
            {
                hyper = true;
                desiredFOV = hyperFOV;

                StartCoroutine("CameraHyperdrive");
            }
            if (Input.GetKeyUp(KeyCode.JoystickButton0) || immune)
            {
                hyper = false;
                desiredFOV = baseFOV;
                Game.GameSpeed = 1.0f;
                StartCoroutine("CameraHyperdrive");
            }

            if (hyper)
            {
                shipSound.pitch = 2.0f;
            }
            else
            {
                shipSound.pitch = 1.5f;
            }

            
        }
        UpdatePlatform();

        var main = ps.main;
        main.startRotationX = Mathf.Deg2Rad * ship.eulerAngles.x;
        main.startRotationY = Mathf.Deg2Rad * ship.eulerAngles.y;
        main.startRotationZ = Mathf.Deg2Rad * ship.eulerAngles.z;
        main.startSpeed = -5.0f * (Game.GameSpeed > 0 ? Game.GameSpeed : 1);
        var emm = ps.emission;
        emm.rateOverTime = 200.0f;// * Game.GameSpeed;
        /*
        if (currentLives <= 0)
            emm.enabled = false;
        else
            emm.enabled = true;
        */
    }

    IEnumerator Explosion()
    {
        float timer = 0.0f;
        var emm = explosion.emission;
        emm.enabled = true;
        while(timer < 0.5f)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        emm.enabled = false;
    }

    public void TakeDamage()
    {
        if (godMode) return;
        if (immune) return;
        immune = true;
        immuneTimer = 0;
        immuneFlash = 0;
        float trauma = 0.7f;
        currentLives--;

        var shape = explosion.shape;
        var main = explosion.main;
        var emm = explosion.emission;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 7.5f * (maxLives - currentLives);
        main.startSpeed = 15.0f;
        emm.rateOverTime = 250 * (maxLives - currentLives);

        hitSound.Play();

        if (currentLives <= 0)
        {
            emm.rateOverTime = 2000;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            main.startSpeed = 5.0f;

            emm = ps.emission;
            emm.enabled = false;
            main = ps.main;
            main.startSpeed = 0.0f;
            trauma = 0.9f;
            hyper = false;
            desiredFOV = baseFOV;
            Game.GameSpeed = 1.0f;
            StartCoroutine("CameraHyperdrive");
            Game.GameOver();
            //body.gameObject.transform.position = new Vector3(0, -2.5f, -15);
            ship.transform.rotation = Quaternion.Euler(0, 0, 0);
            ship.gameObject.GetComponent<MeshRenderer>().enabled = false;
            immune = false;
            deathSound.Play();
            shipSound.volume = 0;
        }

        StartCoroutine(Explosion());

        CameraShake shake = cam.gameObject.GetComponent<CameraShake>();
        if (shake)
            shake.AddTrauma(trauma);
    }

    public void InitPlayer()
    {
        Debug.Log("Player Init");
        currentLives = maxLives;
        //body.gameObject.transform.position = new Vector3(0, 0, 0);
        ship.transform.rotation = Quaternion.Euler(0, 0, 0);
        ship.gameObject.GetComponent<MeshRenderer>().enabled = true;
        immuneTimer = immuneTime;
        immune = false;
        var emm = ps.emission;
        var main = ps.main;
        main.startSpeed = -5.0f;
        emm.enabled = true;
    }

    public int Lives
    {
        get { return currentLives; }
    }


}
