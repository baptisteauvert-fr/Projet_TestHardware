using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace HandTrackingMiniJeu
{
    public sealed class TabletopResetButton : XRSimpleInteractable
    {
        public TabletopSortingGame game;
        public Transform cap;
        Vector3 rest;
        bool touched;
        float lastPress = -10;
        public int PressCount { get; private set; }
        protected override void OnEnable() { base.OnEnable(); selectEntered.AddListener(OnSelect); if (cap != null) rest = cap.localPosition; }
        protected override void OnDisable() { selectEntered.RemoveListener(OnSelect); base.OnDisable(); }
        void OnSelect(SelectEnterEventArgs args) => Press();
        public void Press()
        {
            if (Time.unscaledTime - lastPress < 0.4f) return;
            lastPress = Time.unscaledTime; PressCount++; game.ResetObjects();
        }
        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase phase)
        {
            base.ProcessInteractable(phase); if (phase != XRInteractionUpdateOrder.UpdatePhase.Dynamic) return;
            bool contact = false;
            foreach (var interactor in interactorsHovering)
            {
                if (!(interactor is XRPokeInteractor)) continue;
                var point = transform.InverseTransformPoint(interactor.GetAttachTransform(this).position);
                if (Mathf.Abs(point.x) < 0.10f && Mathf.Abs(point.z) < 0.055f && point.y < 0.04f && point.y > -0.025f) contact = true;
            }
            if (contact && !touched) Press(); touched = contact;
            if (cap != null) cap.localPosition = Vector3.Lerp(cap.localPosition, rest + Vector3.down * (contact ? 0.012f : 0), Time.deltaTime * 15);
        }
    }
}
