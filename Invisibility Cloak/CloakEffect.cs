using UnityEngine;
using UnityEngine.Rendering;
using System;

public class CloakEffect : MonoBehaviour
{
    public Material invisibleMaterial;

    public AudioClip cloakAudio;
    public AudioClip uncloakAudio;

    public AudioSource audioSource;

    private bool cloaked = false;
    private Renderer render;
    private Material[] originalMaterials;
    private ShadowCastingMode originalShadowMode;

    private void Awake()
    {
        if (render == null)
            render = GetComponent<Renderer>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        originalMaterials = render.materials;
        originalShadowMode = render.shadowCastingMode;
    }

    void Update() 
    {
        if (Input.GetButtonDown("Cloak"))
        {
            if (cloaked)
                Uncloak();
            else
                Invisible();
            cloaked = !cloaked;
        }
    }

    void Invisible()
    {
        Material[] tmpMat = render.materials;

        for(int i = 0; i < tmpMat.Length; ++i)
        {
            tmpMat[i] = invisibleMaterial;
        }
        render.materials = tmpMat;
        render.shadowCastingMode = ShadowCastingMode.Off;
        if (audioSource != null)
        {
            audioSource.clip = cloakAudio;
            audioSource.Play();
        }
    }

    void Uncloak()
    {
        Material[] tmpMat = render.materials;

        for (int i = 0; i < tmpMat.Length; ++i)
        {
            tmpMat[i] = originalMaterials[i];
        }
        render.materials = tmpMat;
        render.shadowCastingMode = originalShadowMode;
        if (audioSource != null)
        {
            audioSource.clip = uncloakAudio;
            audioSource.Play();
        }
    }
}