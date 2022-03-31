using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptedDoorAnimator : MonoBehaviour
{
    public bool closed = true;
    public float angleAmount;
    public float speed;

    float startingDoorPosition;
    bool interacting = false;

    Quaternion moveTowards;

    private void Awake()
    {
        startingDoorPosition = transform.localRotation.eulerAngles.y;
    }

    public void Interact()
    {
        if (closed)
        {
            Open();
            closed = false;
        }
        else
        {
            Close();
            closed = true;
        }
    }

    void Open()
    {
        moveTowards = Quaternion.Euler(new Vector3(transform.localRotation.x, angleAmount, transform.localRotation.z));
        interacting = true;
    }

    void Close()
    {
        moveTowards = Quaternion.Euler(new Vector3(transform.localRotation.x, startingDoorPosition, transform.localRotation.z));
        interacting = true;
    }

    private void Update()
    {
        if (!interacting)
            return;

        transform.localRotation = Quaternion.Lerp(transform.localRotation, moveTowards, speed * Time.deltaTime);
        if (Quaternion.Angle(transform.localRotation, moveTowards) < 0.1f)
            interacting = false;
    }
}
