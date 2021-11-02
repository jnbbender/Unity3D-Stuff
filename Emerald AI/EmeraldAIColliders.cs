using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class EmeraldAIColliders : MonoBehaviour
{
    [Tag]
    public string collidersOnTag;
    public CapsuleCollider[] colliders;

    private bool _enabled;
    private bool _isTrigger;

    public bool enabled
    {
        get { return _enabled; }
        set { _enabled = value; SetEnabled(); }
    }

    public bool isTrigger
    {
        get { return _isTrigger; }
        set { _isTrigger = value; SetTrigger(); }
    }

    void Awake()
    {
        if (colliders.Length < 1)
        {
            CapsuleCollider[] colds = GetComponentsInChildren<CapsuleCollider>();
            List<CapsuleCollider> bodyParts = new List<CapsuleCollider>();

            for (int i =0; i < colds.Length; ++i)
            {
                if (colds[i].CompareTag(collidersOnTag))
                {
                    bodyParts.Add(colds[i]);
                }
            }

            colliders = bodyParts.ToArray();
        }
    }

    void SetEnabled()
    {
        foreach (CapsuleCollider collider in colliders)
            collider.enabled = _enabled;
    }

    void SetTrigger()
    {
        foreach (CapsuleCollider collider in colliders)
            collider.isTrigger = _isTrigger;
    }
}
