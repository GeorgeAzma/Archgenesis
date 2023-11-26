using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private GameObject player;
    private Rigidbody player_rb;
    private Vector3 init_pos;
    private float init_fov;
    private Quaternion rotation;
    public static CameraController Instance;
    private GameObject temp;

    void Start()
    {
        player = GameObject.Find("Player");
        player_rb = player.GetComponent<Rigidbody>();
        init_pos = transform.position;
        init_fov = Camera.main.fieldOfView;
        Cursor.lockState = CursorLockMode.Locked;
        temp = new GameObject();
    }

    void Update()
    {
        Instance = this;
        transform.position = player.transform.position;
        rotation.eulerAngles += new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
        transform.position += rotation * init_pos;
        transform.LookAt(player.transform.position);
        transform.rotation *= temp.transform.rotation;
        

        float front = Vector3.Dot(transform.rotation * Vector3.forward, player_rb.velocity.normalized);
        front += Mathf.Max(0f, 2f * front);
        float new_fov = Mathf.Sqrt(player_rb.velocity.magnitude) * front + init_fov;
        float delta_fov = new_fov - Camera.main.fieldOfView;
        Camera.main.fieldOfView += 0.3f * math.tanh(delta_fov);
    }

    private void OnShake(float duration = 1f, float strength = 1f, int vibrato = 10)
    {
        temp.transform.DOShakeRotation(duration, strength, vibrato, randomnessMode: ShakeRandomnessMode.Harmonic).OnUpdate(() =>
        {
            Update();
            temp.transform.rotation = Quaternion.identity;
        });
    }
    public static void Shake(float duration = 1f, float strength = 1f, int vibrato = 10) => Instance.OnShake(duration, strength, vibrato);
}
