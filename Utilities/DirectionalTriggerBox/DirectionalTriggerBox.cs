using NaughtyAttributes;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace NastyDiaper
{
    [Serializable]
    public enum DIRECTION_TYPE
    {
        Front,
        Back,
        Left,
        Right,
    }

    [Serializable]
    public class EventListener
    {
        public DIRECTION_TYPE direction;
        public UnityEvent doEvent;
    }

    [RequireComponent(typeof(BoxCollider))]

    public class DirectionalTriggerBox : MonoBehaviour
    {
        [Tag]
        public string collisionTag;
        [Space(5)]

        public bool onEnterFrom;
        [VInspector.ShowIf("onEnterFrom")]
        public EventListener extrance;
        [VInspector.EndIf]

        [Space(10)]

        public bool onExitFrom;
        [VInspector.ShowIf("onExitFrom")]
        public EventListener exit;
        [VInspector.EndIf]

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsValidCollider(other)) { return; }

            if (onEnterFrom && GetDirection(other) == extrance.direction)
            {
                Debug.Log("Would Enter Event Since directions match");
                extrance.doEvent?.Invoke();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsValidCollider(other)) { return; }

            if (onExitFrom && GetDirection(other) == exit.direction)
            {
                Debug.Log("Would Exit Event Since directions match");
                exit.doEvent?.Invoke();
            }
        }

        private bool IsValidCollider(Collider other)
        {
            // Avoid repeated LayerMask.NameToLayer calls and null/empty checks
            return other != null
                && other.CompareTag(collisionTag);
        }

        private DIRECTION_TYPE GetDirection(Collider other)
        {
            Vector3 localDirection = transform.InverseTransformPoint(other.bounds.center).normalized;
            if (Mathf.Abs(localDirection.x) > Mathf.Abs(localDirection.z))
                return localDirection.x > 0 ? DIRECTION_TYPE.Right : DIRECTION_TYPE.Left;
            else
                return localDirection.z > 0 ? DIRECTION_TYPE.Front : DIRECTION_TYPE.Back;
        }


#if UNITY_EDITOR
        // Colors for each face
        private static readonly Color FrontColor = new Color(0.3f, 1, 0.3f, 0.3f);   // Green
        private static readonly Color BackColor = new Color(1, 0.3f, 0.3f, 0.3f);    // Red
        private static readonly Color LeftColor = new Color(0.3f, 0.3f, 1, 0.3f);    // Blue
        private static readonly Color RightColor = new Color(1, 1, 0.3f, 0.3f);      // Yellow

        private const float FaceThickness = 0.02f; // Not zero!

        private void OnDrawGizmos()
        {
            var box = GetComponent<BoxCollider>();
            if (box == null) return;

            Vector3 c = box.center;
            Vector3 s = box.size;

            // Draw faces and labels
            DrawColoredFaceWithLabel(
                c + new Vector3(0, 0, s.z / 2 + FaceThickness / 2),
                new Vector3(s.x, s.y, FaceThickness),
                FrontColor,
                "Front"
            );
            DrawColoredFaceWithLabel(
                c + new Vector3(0, 0, -s.z / 2 - FaceThickness / 2),
                new Vector3(s.x, s.y, FaceThickness),
                BackColor,
                "Back"
            );
            DrawColoredFaceWithLabel(
                c + new Vector3(-s.x / 2 - FaceThickness / 2, 0, 0),
                new Vector3(FaceThickness, s.y, s.z),
                LeftColor,
                "Left"
            );
            DrawColoredFaceWithLabel(
                c + new Vector3(s.x / 2 + FaceThickness / 2, 0, 0),
                new Vector3(FaceThickness, s.y, s.z),
                RightColor,
                "Right"
            );
        }

        private void DrawColoredFaceWithLabel(Vector3 localCenter, Vector3 faceSize, Color color, string label)
        {
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = color;
            Gizmos.DrawCube(localCenter, faceSize);
            Gizmos.matrix = oldMatrix;

            // Draw the text label at the face center (in world coordinates)
            Vector3 worldCenter = transform.TransformPoint(localCenter);
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState() { textColor = Color.black }
            };
            Handles.Label(worldCenter, label, style);
        }
#endif
    }
}
