using NaughtyAttributes;
using System;
using System.Collections.Generic;
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
    public enum FONTCOLOR_TYPE
    {
        Black,
        White
    }

    [Serializable]
    public class EventListener
    {
        public DIRECTION_TYPE direction;
        public UnityEvent doEvent;
    }

    [RequireComponent(typeof(BoxCollider))]
    public class DirectionalCollisionTrigger : MonoBehaviour
    {
        [Tag]
        public string collisionTag;
        public FONTCOLOR_TYPE labelColor;

        [Space(5)]

        public bool onEnterFrom;
        [ShowIf(nameof(onEnterFrom))]
        public List<EventListener> entranceEvents = new();

        [Space(10)]

        public bool onExitFrom;
        [ShowIf(nameof(onExitFrom))]
        public List<EventListener> exitEvents = new();

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsValidCollider(other)) return;

            if (onEnterFrom)
            {
                var direction = GetDirection(other);
                foreach (var listener in entranceEvents)
                {
                    if (listener.direction == direction)
                    {
                        listener.doEvent?.Invoke();
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsValidCollider(other)) return;

            if (onExitFrom)
            {
                var direction = GetDirection(other);
                foreach (var listener in exitEvents)
                {
                    if (listener.direction == direction)
                    {
                        listener.doEvent?.Invoke();
                    }
                }
            }
        }

        private bool IsValidCollider(Collider other)
        {
            return other != null && other.CompareTag(collisionTag);
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
        private static readonly Color FrontColor = new(0.3f, 1, 0.3f, 0.3f);
        private static readonly Color BackColor = new(1, 0.3f, 0.3f, 0.3f);
        private static readonly Color LeftColor = new(0.3f, 0.3f, 1, 0.3f);
        private static readonly Color RightColor = new(1, 1, 0.3f, 0.3f);

        private const float FaceThickness = 0.02f;

        private void OnDrawGizmos()
        {
            var box = GetComponent<BoxCollider>();
            if (box == null) return;

            Vector3 c = box.center;
            Vector3 s = box.size;

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

            Vector3 worldCenter = transform.TransformPoint(localCenter);
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState()
                {
                    textColor = labelColor == FONTCOLOR_TYPE.Black ? Color.black : Color.white
                }
            };
            Handles.Label(worldCenter, label, style);
        }
#endif
    }
}
