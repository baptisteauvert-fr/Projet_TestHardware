using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Hands;

namespace HandTrackingMiniJeu
{
    public sealed class FingerGunRange : MonoBehaviour
    {
        public Transform trackingOrigin;
        public Camera playerCamera;
        public FingerGunTarget[] targets;
        public Text totalText,gestureText;
        public LayerMask targetMask;
        public AudioSource speaker;
        public int TotalScore { get; private set; }
        public int Shots { get; private set; }
        public int Hits { get; private set; }
        readonly FingerGunGesture[] gestures={new FingerGunGesture(),new FingerGunGesture()};
        readonly List<XRHandSubsystem> subsystems=new List<XRHandSubsystem>();
        readonly LineRenderer[] beams=new LineRenderer[2];
        readonly float[] beamUntil=new float[2];
        XRHandSubsystem subsystem;
        Material beamMaterial;
        AudioClip shotSound,hitSound;
        float nextSearch;

        void Awake()
        {
            beamMaterial=new Material(Shader.Find("Universal Render Pipeline/Unlit"));beamMaterial.SetColor("_BaseColor",new Color(1f,0.77f,0.3f));
            for(int i=0;i<2;i++)
            {
                var go=new GameObject(i==0?"Left finger shot":"Right finger shot");go.transform.SetParent(transform,false);
                var line=go.AddComponent<LineRenderer>();line.sharedMaterial=beamMaterial;line.positionCount=2;line.startWidth=0.009f;line.endWidth=0.003f;line.useWorldSpace=true;line.enabled=false;beams[i]=line;
            }
            shotSound=Tone(520,0.055f);hitSound=Tone(1100,0.09f);UpdateText();
        }
        void Update()
        {
            if((subsystem==null || !subsystem.running)&&Time.unscaledTime>=nextSearch)
            {nextSearch=Time.unscaledTime+1;SubsystemManager.GetSubsystems(subsystems);subsystem=null;foreach(var s in subsystems)if(s.running){subsystem=s;break;}}
            if(subsystem!=null && subsystem.running)
            {ReadHand(subsystem.leftHand,0);ReadHand(subsystem.rightHand,1);}
            else {gestures[0].Reset();gestures[1].Reset();}
            for(int i=0;i<2;i++)beams[i].enabled=Time.time<beamUntil[i];
            if(gestureText!=null)gestureText.text=gestures[0].Armed||gestures[1].Armed?"ARME / BAISSEZ LE POUCE POUR TIRER":"INDEX + MAJEUR TENDUS / LEVEZ PUIS BAISSEZ LE POUCE";
            // An explicit editor-only fallback lets the target game be checked without pretending to track hands.
#if UNITY_EDITOR
            if(Keyboard.current!=null && Keyboard.current.fKey.wasPressedThisFrame && playerCamera!=null)
                FireRay(new Ray(playerCamera.transform.position,playerCamera.transform.forward),0);
#endif
        }
        void ReadHand(XRHand hand,int side)
        {
            if(!hand.isTracked || !Pose(hand,XRHandJointID.Wrist,out var wrist) || !Pose(hand,XRHandJointID.IndexMetacarpal,out var indexBase)
                || !Pose(hand,XRHandJointID.LittleMetacarpal,out var littleBase) || !Pose(hand,XRHandJointID.ThumbTip,out var thumb)
                || !Pose(hand,XRHandJointID.IndexProximal,out var indexProximal) || !Pose(hand,XRHandJointID.IndexTip,out var indexTip)
                || !Pose(hand,XRHandJointID.MiddleProximal,out var middleProximal) || !Pose(hand,XRHandJointID.MiddleTip,out var middleTip)
                || !Pose(hand,XRHandJointID.RingProximal,out var ringProximal) || !Pose(hand,XRHandJointID.RingTip,out var ringTip)
                || !Pose(hand,XRHandJointID.LittleProximal,out var littleProximal) || !Pose(hand,XRHandJointID.LittleTip,out var littleTip))
            {gestures[side].Reset();return;}
            bool indexExtended=Vector3.Distance(wrist,indexTip)>Vector3.Distance(wrist,indexProximal)*1.45f;
            bool middleExtended=Vector3.Distance(wrist,middleTip)>Vector3.Distance(wrist,middleProximal)*1.45f;
            bool ringFolded=Vector3.Distance(wrist,ringTip)<Vector3.Distance(wrist,ringProximal)*1.55f;
            bool littleFolded=Vector3.Distance(wrist,littleTip)<Vector3.Distance(wrist,littleProximal)*1.55f;
            float palmWidth=Mathf.Max(0.045f,Vector3.Distance(indexProximal,littleProximal));
            float openness=Vector3.Distance(thumb,indexProximal)/palmWidth;
            bool trigger=gestures[side].Update(true,indexExtended&&middleExtended&&ringFolded&&littleFolded,openness,Time.unscaledTime);
            if(!trigger)return;
            Vector3 localDirection=((indexTip-indexProximal).normalized+(middleTip-middleProximal).normalized).normalized;
            Vector3 direction=trackingOrigin!=null?trackingOrigin.TransformDirection(localDirection):localDirection;
            Vector3 origin=HandTrackingMonitor.TrackingToWorld(trackingOrigin,(indexTip+middleTip)*0.5f)+direction*0.05f;
            FireRay(new Ray(origin,direction),side);
        }
        static bool Pose(XRHand hand,XRHandJointID id,out Vector3 point)
        {bool valid=hand.GetJoint(id).TryGetPose(out var pose);point=pose.position;return valid;}
        public int FireRay(Ray ray,int side=0)
        {
            Shots++;int points=0;Vector3 end=ray.origin+ray.direction*20;
            if(Physics.Raycast(ray,out var hit,20,targetMask,QueryTriggerInteraction.Ignore))
            {
                end=hit.point;var target=hit.collider.GetComponentInParent<FingerGunTarget>();
                if(target!=null)points=target.Hit(hit.point);
            }
            if(points>0){Hits++;TotalScore+=points;}
            if(speaker!=null)speaker.PlayOneShot(points>0?hitSound:shotSound,0.28f);
            int index=Mathf.Clamp(side,0,1);beams[index].SetPosition(0,ray.origin);beams[index].SetPosition(1,end);beamUntil[index]=Time.time+0.09f;beams[index].enabled=true;
            UpdateText();return points;
        }
        public void ResetScores(){TotalScore=0;Shots=0;Hits=0;foreach(var target in targets)target.ResetScore();UpdateText();}
        void UpdateText(){if(totalText!=null)totalText.text=$"TIR AUX DOIGTS / SCORE {TotalScore:000}     TOUCHES {Hits}     TIRS {Shots}";}
        static AudioClip Tone(float frequency,float duration)
        {int count=Mathf.CeilToInt(22050*duration);var data=new float[count];for(int i=0;i<count;i++)data[i]=Mathf.Sin(2*Mathf.PI*frequency*i/22050)*0.15f*(1-(float)i/count);var clip=AudioClip.Create("Finger gun tone",count,1,22050,false);clip.SetData(data,0);return clip;}
        void OnDestroy(){if(beamMaterial!=null)Destroy(beamMaterial);if(shotSound!=null)Destroy(shotSound);if(hitSound!=null)Destroy(hitSound);}
    }
}
