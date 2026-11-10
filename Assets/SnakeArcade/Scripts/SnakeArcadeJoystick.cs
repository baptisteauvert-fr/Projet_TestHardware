using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SnakeArcade
{
    public sealed class SnakeArcadeJoystick : XRBaseInteractable
    {
        public SnakeArcadeMachine machine;
        public Transform lever;
        public float travel = 0.055f;
        public float deadZone = 0.22f;
        IXRSelectInteractor hand;
        Vector3 grabPoint;
        Vector2 value;
        float previewUntil;
        public Vector2 Value => value;

        protected override void OnEnable()
        {
            base.OnEnable(); selectEntered.AddListener(Grab); selectExited.AddListener(Release);
        }
        protected override void OnDisable()
        {
            selectEntered.RemoveListener(Grab); selectExited.RemoveListener(Release);
            hand = null; base.OnDisable();
        }
        void Grab(SelectEnterEventArgs args)
        {
            hand = args.interactorObject;
            grabPoint = transform.InverseTransformPoint(hand.GetAttachTransform(this).position);
        }
        void Release(SelectExitEventArgs args) { hand = null; value = Vector2.zero; }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase phase)
        {
            base.ProcessInteractable(phase);
            if (phase != XRInteractionUpdateOrder.UpdatePhase.Dynamic) return;
            if (hand != null)
            {
                Vector3 delta = transform.InverseTransformPoint(hand.GetAttachTransform(this).position) - grabPoint;
                value = Vector2.ClampMagnitude(new Vector2(delta.x, delta.z) / travel, 1);
                if (value.magnitude >= deadZone) machine.SetDirection(Cardinal(value));
            }
            else if (Time.unscaledTime > previewUntil) value = Vector2.MoveTowards(value, Vector2.zero, Time.deltaTime * 9);
            lever.localRotation = Quaternion.Euler(value.y * 24, 0, -value.x * 24);
        }
        public void PreviewDirection(Vector2Int direction)
        { if (hand == null) { value = direction; previewUntil = Time.unscaledTime + 0.12f; } }
        public static Vector2Int Cardinal(Vector2 input)
        {
            return Mathf.Abs(input.x) > Mathf.Abs(input.y)
                ? (input.x > 0 ? Vector2Int.right : Vector2Int.left)
                : (input.y > 0 ? Vector2Int.up : Vector2Int.down);
        }
    }
}
