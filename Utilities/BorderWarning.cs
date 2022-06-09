using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using UnityEngine.Events;

// Requires a Post-Processing Volume somewhere in the scene

public class BorderWarning : MonoBehaviour
{
    public float minSaturation;
    public float saturationDuration;

    public Text countDownText;
    public int timeToCount;

    public bool outOfBoundsOnExit;

    private PostProcessVolume ppVolume;

    // For color saturation handling
    private ColorGrading colorGrading;
    private float originalSaturation;

    private float saturateTimeElapsed = 0f;

    // For timer handling
    private float timeTillDeath;
    private WaitForSeconds waitTimer;
    
    private GameObject player;
    private Canvas canvas;
    
    void Start()
    {
        GetVolumeAndColorGrading();

        timeTillDeath = timeToCount;
        countDownText.text = timeTillDeath.ToString();
        waitTimer = new WaitForSeconds(1f);
        canvas = GetComponentInChildren<Canvas>();
        canvas.enabled = false;
    }

    void GetVolumeAndColorGrading()
    {
        if (ppVolume == null)
        {
            GameObject ppGo = GameObject.FindGameObjectWithTag("PostProcess");
            if (ppGo != null)
            {
                ppVolume = ppGo.GetComponent<PostProcessVolume>();
            }
            else
            {
                Debug.LogError("No Post Processing Volume GameObject Found.  Add one.");
            }
        }
        if (ppVolume != null)
        {
            if (ppVolume.profile.TryGetSettings(out colorGrading))
            {
                originalSaturation = colorGrading.saturation;
            }
            else
            {
                Debug.LogWarning("No ColorGrading found in Post Processing Volume");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (outOfBoundsOnExit)
                InBounds(other);
            else
                OutOfBounds(other);
        }
    }

    void OutOfBounds(Collider other)
    {
        if (ppVolume == null)
        {
            Debug.LogError("Can't do much without a PPV object!");
            return;
        }

        player = other.gameObject;
        // Let's make sure we get the root object
        /*
        while (player.transform.parent != null)
            player = player.transform.parent.gameObject;
        */
        Debug.Log("Got Player " + player.name);
        timeTillDeath = timeToCount;

        canvas.enabled = true;
        StopAllCoroutines();
        StartCoroutine(Saturate());
        StartCoroutine(Alert());
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (outOfBoundsOnExit)
                OutOfBounds(other);
            else
                InBounds(other);
        }
    }

    void InBounds(Collider other)
    {
        timeTillDeath = timeToCount;

        player = null;
        StopAllCoroutines();
        StartCoroutine(Desaturate());
        canvas.enabled = false;

    }
    
    IEnumerator Alert()
    {
        while (timeTillDeath >= 0)
        {
            countDownText.text = timeTillDeath.ToString();
            timeTillDeath--;
            yield return waitTimer;
        }
        // If we get here the coroutine was not stopped
        Killed();

    }

    void Killed()
    {
        canvas.enabled = false;
        /*
        if (player != null)
            PlayerManager.Instance.KillPlayer(player);
        */
    }

    IEnumerator Saturate()
    {
        saturateTimeElapsed = 0f;
        while (colorGrading.saturation.value != minSaturation)
        {
            if (saturateTimeElapsed < saturationDuration)
            {
                colorGrading.saturation.value = Mathf.Lerp(colorGrading.saturation.value, minSaturation, saturateTimeElapsed / saturationDuration);
                saturateTimeElapsed += Time.deltaTime;
            }
            else
            {
                colorGrading.saturation.value = minSaturation;
            }
            yield return null;
        }
    }

    IEnumerator Desaturate()
    {
        saturateTimeElapsed = 0f;
        while (colorGrading.saturation.value != originalSaturation)
        {
            if (saturateTimeElapsed < saturationDuration)
            {
                colorGrading.saturation.value = Mathf.Lerp(colorGrading.saturation.value, originalSaturation, saturateTimeElapsed / saturationDuration);
                saturateTimeElapsed += Time.deltaTime;
            }
            else
            {
                colorGrading.saturation.value = originalSaturation;
            }
            yield return null;
        }
    }
}
