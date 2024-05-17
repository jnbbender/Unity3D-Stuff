using System.Collections;
using UnityEngine;

namespace NastyDiaper
{
    public class JumpValidation : MonoBehaviour
    {
        public bool interruptOnMovement = false;
        public float rayDistance;
        public LayerMask collisionLayers;
        public string animClipWhenNotAllowed;
        public int animatorLayer = 0;
        public float movementThreshold = 0.1f;

        Animator animator;
        CapsuleCollider _collider;
        bool doneLookingUp = false;

        Vector3 bodyPositionAtTimeOfLook;

        private void Awake()
        {
            _collider = GetComponent<CapsuleCollider>();
            animator = GetComponent<Animator>();
        }

        public bool CanJump()
        {
            var v = new Vector3(transform.position.x, transform.position.y + _collider.height, transform.position.z);

            if (Physics.Raycast(v, Vector3.up, rayDistance, collisionLayers))
            {
                if (animator && animClipWhenNotAllowed != null)
                {
                    doneLookingUp = false;
                    animator.SetBool("CustomAction", true);

                    if (interruptOnMovement)
                    {
                        bodyPositionAtTimeOfLook = transform.position;
                        StartCoroutine(WatchForMovement());
                    }

                    animator.CrossFadeInFixedTime(animClipWhenNotAllowed, 0.25f, animatorLayer);
                }
                return false;
            }
            return true;
        }

        public void DoneLookingUp()
        {
            doneLookingUp = true;
            animator.SetBool("CustomAction", false);
        }

        // Use this Coroutine to stop the Lookup animation once movement is detected.
        // This is best used when there is no separate "HEAD" animation layer.
        IEnumerator WatchForMovement()
        {
            while (!doneLookingUp)
            {
                yield return null;
                if (!NearEqual(transform.position, bodyPositionAtTimeOfLook))
                {
                    break;
                }
            }
            DoneLookingUp();
        }

        bool NearEqual(Vector3 a, Vector3 b)
        {
            return Vector3.Distance(a, b) <= movementThreshold;
        }
    }
}