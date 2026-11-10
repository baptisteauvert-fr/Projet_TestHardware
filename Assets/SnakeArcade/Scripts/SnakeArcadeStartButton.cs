using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SnakeArcade
{
    public sealed class SnakeArcadeStartButton : XRSimpleInteractable
    {
        public SnakeArcadeMachine machine;
        public Transform cap;
        public float radius = 0.055f;
        public float surfaceHeight = 0.032f;
        Vector3 capRest;
        float lastPress = -10, depression;
        bool pokeLatched;
        public int PressCount { get; private set; }

        protected override void Awake() { base.Awake(); }
        protected override void OnEnable()
        {
            base.OnEnable(); selectEntered.AddListener(OnSelected);
            if (cap != null) capRest = cap.localPosition;
        }
        protected override void OnDisable() { selectEntered.RemoveListener(OnSelected); base.OnDisable(); }
        void OnSelected(SelectEnterEventArgs args) { Press(); }
        public void Press()
        {
            if (Time.unscaledTime - lastPress < 0.35f) return;
            lastPress = Time.unscaledTime; depression = 1; PressCount++;
            machine.PressStart();
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase phase)
        {
            base.ProcessInteractable(phase);
            if (phase != XRInteractionUpdateOrder.UpdatePhase.Dynamic) return;
            bool touching = false;
            foreach (var interactor in interactorsHovering)
            {
                if (!(interactor is XRPokeInteractor)) continue;
                Vector3 point = transform.InverseTransformPoint(interactor.GetAttachTransform(this).position);
                if (new Vector2(point.x, point.z).magnitude <= radius && point.y <= surfaceHeight + 0.006f && point.y >= -0.035f)
                    touching = true;
            }
            if (touching && !pokeLatched) Press();
            pokeLatched = touching;
            depression = Mathf.MoveTowards(depression, touching ? 1 : 0, Time.deltaTime * 7);
            if (cap != null) cap.localPosition = capRest + Vector3.down * (0.012f * depression);
        }
    }
}
