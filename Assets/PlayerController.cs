using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.AI;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private Quaternion rotation;
    private new Collider collider;
    private GameObject sparks;
    private ParticleSystem sparkParticles;
    private bool tryingJump = false;
    private Vector3 vel;
    private Vector3 surface;
    private bool canDash;
    private TrailRenderer trail;
    private const float dashSpeed = 256f;
    private AudioSource dashAudio;
    private AudioSource pushAudio;
    public Material screenMaterial;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        rotation = Camera.main.transform.rotation;
        sparks = GameObject.Find("Sparks");
        sparkParticles = sparks.GetComponent<ParticleSystem>();
        surface = TouchingSurface();
        tryingJump = false;
        canDash = true;
        trail = GetComponentInChildren<TrailRenderer>();
        var sources = GetComponents<AudioSource>();
        dashAudio = sources[0];
        pushAudio = sources[1];
    }

    Vector3 TouchingSurface()
    {
        float e = 0.2f;
        if (Physics.Raycast(rb.transform.position, Vector3.left, collider.bounds.extents.x + e)) return Vector3.left;
        if (Physics.Raycast(rb.transform.position, Vector3.right, collider.bounds.extents.x + e)) return Vector3.right;
        if (Physics.Raycast(rb.transform.position, Vector3.forward, collider.bounds.extents.z + e)) return Vector3.forward;
        if (Physics.Raycast(rb.transform.position, Vector3.back, collider.bounds.extents.z + e)) return Vector3.back;
        if (Physics.Raycast(rb.transform.position, Vector3.up, collider.bounds.extents.y + e)) return Vector3.up;
        if (Physics.Raycast(rb.transform.position, Vector3.down, collider.bounds.extents.y + e)) return Vector3.down;
        return Vector3.zero;
    }

    void Jump()
    {
        tryingJump = true;
        if (surface == Vector3.down)
        {
            rb.AddForce(Vector3.up * 3000f);
            tryingJump = false;
        }
    }

    void UpdateWallJump()
    {
        if (isMidAir())
            return;
        if (tryingJump && surface != Vector3.down && surface != Vector3.up)
        {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * 0.2f, rb.velocity.z);
            rb.AddForce((Vector3.up * 2f - surface).normalized * 5000f);
            tryingJump = false;
        }
    }

    bool isMidAir()
    {
        return surface == Vector3.zero;
    }

    void UpdateRoll()
    {
        rotation = Camera.main.transform.rotation;
        vel = rotation * new Vector3(
            (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f),
            0f,
            (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f)
        ).normalized * 5f;
        if (isMidAir())
            rb.velocity = new Vector3(rb.velocity.x * 0.997f, rb.velocity.y, rb.velocity.z * 0.997f);
        rb.AddForce(vel * 5f);
        float d = -(vel.normalized.x * rb.velocity.normalized.x + vel.normalized.z * rb.velocity.normalized.z);
        d = Mathf.Max(0f, d) * 0.1f + (isMidAir() ? 0.2f : 0.0f);
        Vector2 vel2 = new Vector2(rb.velocity.x, rb.velocity.z);
        rb.velocity = new Vector3(math.lerp(rb.velocity.x, vel.normalized.x * vel2.magnitude, d), rb.velocity.y, math.lerp(rb.velocity.z, vel.normalized.z * vel2.magnitude, d));
        trail.emitting = Mathf.Sqrt(rb.velocity.magnitude) > 10f;
    }

    void UpdateSparks()
    {
        // Calculate the spark direction opposite to the velocity
        Vector3 sparkDirection = -rb.velocity.normalized;

        if (Vector3.Dot(vel, rb.velocity) < 0.0f && !isMidAir())
        {
            // Emit sparks
            sparks.transform.position = transform.position + surface * 0.5f;
            sparks.transform.rotation = Quaternion.LookRotation(-sparkDirection, transform.up);
            sparkParticles.Emit(1);
        }
    }

    void Dash()
    {
        if (!canDash)
            return;
        dashAudio.Play();
        CameraController.Shake(2f, 2f, 10);
        rb.AddForce(new Vector3(vel.x, 0f, vel.z).normalized * dashSpeed * dashSpeed);
        StartCoroutine(CooldownDash());
    }

    void Push()
    {
        pushAudio.Play();
        CameraController.Shake(2f, 5f, 10);
        const float explosionRadius = 64f;
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, 1 << 22);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].TryGetComponent(out Rigidbody rb))
                rb.AddExplosionForce(1024f, transform.position, explosionRadius);
        }
    }

    IEnumerator CooldownDash()
    {
        canDash = false;
        yield return new WaitForSeconds(0.07f);
        rb.velocity /= dashSpeed * dashSpeed;
        yield return new WaitForSeconds(1f);
        canDash = true;
    }

    void Update()
    {
        surface = TouchingSurface();
        UpdateRoll();

        if (Input.GetKeyDown(KeyCode.Space))
            Jump();

        if (Input.GetKeyDown(KeyCode.LeftShift))
            Dash();

        if (Input.GetKeyDown(KeyCode.Q))
            Push();

        UpdateWallJump();

        UpdateSparks();

        float speed = Mathf.Clamp01(Mathf.Sqrt(rb.velocity.magnitude) * 0.17f - 1f);
        screenMaterial.SetFloat("_SpeedLines", speed);

        rb.velocity *= 0.999f;
        rb.AddForce(Vector3.down * 20f); // Gravity
    }
}
