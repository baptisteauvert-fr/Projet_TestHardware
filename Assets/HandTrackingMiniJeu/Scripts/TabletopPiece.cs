using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace HandTrackingMiniJeu
{
    [RequireComponent(typeof(Rigidbody), typeof(XRGrabInteractable))]
    public sealed class TabletopPiece : MonoBehaviour
    {
        public int colorIndex;
        public int pieceIndex;
        public Rigidbody Body { get; private set; }
        public XRGrabInteractable Grab { get; private set; }
        Vector3 home;
        Quaternion homeRotation;
        public bool Held => Grab != null && Grab.isSelected;
        void Awake() { Body = GetComponent<Rigidbody>(); Grab = GetComponent<XRGrabInteractable>(); home = transform.position; homeRotation = transform.rotation; }
        public bool ResetHome()
        {
            if (Held) return false;
            Body.linearVelocity = Vector3.zero; Body.angularVelocity = Vector3.zero;
            Body.position = home; Body.rotation = homeRotation;
            transform.SetPositionAndRotation(home, homeRotation); Body.WakeUp();
            return true;
        }
    }
}
