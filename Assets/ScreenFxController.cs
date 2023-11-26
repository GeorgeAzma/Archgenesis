using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenFxController : MonoBehaviour
{
    private Volume volume;
    private ChromaticAberration chromaticAbberation;
    private float chromaAbberVal = 0.0f;
    private Rigidbody playerRb;
    void Start()
    {
        volume = GetComponent<Volume>();
        volume.profile.TryGet(out chromaticAbberation);
        chromaticAbberation.active = true;
        playerRb = GameObject.Find("Player").GetComponent<Rigidbody>();
    }

    void Update()
    {
        float speed = Mathf.Clamp01(Mathf.Sqrt(playerRb.velocity.magnitude) * 0.1f - 1f);
        chromaAbberVal = Mathf.Max(chromaAbberVal, speed);
        chromaAbberVal += Mathf.Sign(speed - chromaAbberVal) * 0.003f;
        chromaticAbberation.intensity.Override(chromaAbberVal);
    }
}
