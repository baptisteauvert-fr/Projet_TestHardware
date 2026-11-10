using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using UnityEngine.InputSystem;

namespace HandTrackingMiniJeu
{
    public sealed class HandTrackingMonitor : MonoBehaviour
    {
        public Transform trackingOrigin;
        public Camera playerCamera;
        public RawImage leftImage, rightImage;
        public Text leftReadout, rightReadout, connectionText;
        public Transform[] desktopViews;
        public bool LeftTracked { get; private set; }
        public bool RightTracked { get; private set; }
        readonly List<XRHandSubsystem> subsystems = new List<XRHandSubsystem>();
        readonly Vector3[] jointPositions = new Vector3[26];
        readonly bool[] jointValid = new bool[26];
        readonly Vector2Int[] projectedJoints = new Vector2Int[26];
        readonly Texture2D[] textures = new Texture2D[2];
        readonly Color32[][] pixels = new Color32[2][];
        XRHandSubsystem subsystem;
        float nextSearch, nextDisplay;
        int desktopView;
        const int Size = 256;
        static readonly int[][] Fingers = {
            new[] {0, 2, 3, 4, 5}, new[] {0, 6, 7, 8, 9, 10},
            new[] {0, 11, 12, 13, 14, 15}, new[] {0, 16, 17, 18, 19, 20},
            new[] {0, 21, 22, 23, 24, 25}
        };

        void Awake()
        {
            for (int i = 0; i < 2; i++)
            {
                textures[i] = new Texture2D(Size, Size, TextureFormat.RGBA32, false) { name = "Live hand position " + i, filterMode = FilterMode.Bilinear };
                pixels[i] = new Color32[Size * Size];
            }
            leftImage.texture = textures[0]; rightImage.texture = textures[1];
            DisplaySample(0, false, Vector3.zero, -1, null, null);
            DisplaySample(1, false, Vector3.zero, -1, null, null);
        }

        void Update()
        {
            if(!XRSettings.isDeviceActive && Keyboard.current!=null && Keyboard.current.tabKey.wasPressedThisFrame && desktopViews!=null && desktopViews.Length>0)
                desktopView=(desktopView+1)%desktopViews.Length;
            if ((subsystem == null || !subsystem.running) && Time.unscaledTime >= nextSearch)
            {
                nextSearch = Time.unscaledTime + 1;
                SubsystemManager.GetSubsystems(subsystems);
                subsystem = null;
                foreach (var candidate in subsystems) if (candidate.running) { subsystem = candidate; break; }
            }
            if (Time.unscaledTime < nextDisplay) return;
            nextDisplay = Time.unscaledTime + 0.075f;
            if (subsystem == null || !subsystem.running)
            {
                DisplaySample(0, false, Vector3.zero, -1, null, null);
                DisplaySample(1, false, Vector3.zero, -1, null, null);
                connectionText.text = "EN ATTENTE DU QUEST 3 / SUIVI DES MAINS";
                return;
            }
            ReadHand(0, subsystem.leftHand); ReadHand(1, subsystem.rightHand);
            connectionText.text = LeftTracked || RightTracked ? "OPENXR CONNECTE / COORDONNEES EN METRES" : "MAINS HORS CHAMP / PRESENTEZ VOS MAINS";
        }

        void ReadHand(int index, XRHand hand)
        {
            if (!hand.isTracked) { DisplaySample(index, false, Vector3.zero, -1, null, null); return; }
            for (int i = 0; i < 26; i++)
            {
                var id = (XRHandJointID)(i + (int)XRHandJointID.BeginMarker);
                jointValid[i] = hand.GetJoint(id).TryGetPose(out var pose);
                jointPositions[i] = pose.position;
            }
            float pinch = -1;
            if (hand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out var thumb) && hand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out var finger))
                pinch = Mathf.InverseLerp(0.065f, 0.018f, Vector3.Distance(thumb.position, finger.position));
            DisplaySample(index, true, hand.rootPose.position, pinch, jointPositions, jointValid);
        }

        // Samples are displayed locally only. No hand data is saved or transmitted.
        public void DisplaySample(int handIndex, bool tracked, Vector3 trackingPosition, float pinch, Vector3[] joints, bool[] valid)
        {
            if (handIndex == 0) LeftTracked = tracked; else RightTracked = tracked;
            var text = handIndex == 0 ? leftReadout : rightReadout;
            Color32 ink = handIndex == 0 ? new Color32(52, 218, 209, 255) : new Color32(255, 177, 92, 255);
            Color32 dark = new Color32(12, 24, 37, 255), grid = new Color32(25, 44, 60, 255);
            var target = pixels[handIndex];
            for (int y = 0; y < Size; y++) for (int x = 0; x < Size; x++) target[y * Size + x] = (x % 32 == 0 || y % 32 == 0) ? grid : dark;
            Line(target, new Vector2Int(128, 0), new Vector2Int(128, 255), new Color32(50, 77, 95, 255));
            if (!tracked)
            {
                text.text = "NON SUIVIE\nX  --     Y  --     Z  --\nPINCEMENT  --";
                text.color = new Color(0.6f, 0.67f, 0.73f);
            }
            else
            {
                Vector3 world = TrackingToWorld(trackingOrigin, trackingPosition);
                string pinchText = pinch < 0 ? "--" : Mathf.RoundToInt(pinch * 100) + " %";
                text.text = $"SUIVIE\nX {world.x:+0.00;-0.00;0.00}    Y {world.y:0.00}    Z {world.z:+0.00;-0.00;0.00}\nPINCEMENT  {pinchText}";
                text.color = ink;
                if (joints != null && valid != null)
                {
                    ProjectHand(joints, valid, projectedJoints);
                    foreach (var finger in Fingers)
                        for (int j = 1; j < finger.Length; j++)
                            if (valid[finger[j - 1]] && valid[finger[j]])
                                Line(target, projectedJoints[finger[j - 1]], projectedJoints[finger[j]], ink);
                    for (int i = 0; i < Mathf.Min(26, joints.Length); i++) if (valid[i]) Dot(target, projectedJoints[i], 3, ink);
                    if(valid[0]) Dot(target, projectedJoints[0], 5, new Color32(240,250,255,255));
                }
                else Dot(target, new Vector2Int(128,128), 5, new Color32(240,250,255,255));
            }
            textures[handIndex].SetPixels32(target); textures[handIndex].Apply(false);
        }

        public static Vector3 TrackingToWorld(Transform origin, Vector3 local) => origin != null ? origin.TransformPoint(local) : local;
        public static Vector2Int Project(Vector3 trackingPosition)
            => new Vector2Int(Mathf.RoundToInt((trackingPosition.x / 1.5f + 0.5f) * 255), Mathf.RoundToInt(trackingPosition.z / 1.4f * 255));
        // Fit the complete skeleton in its own palm plane. World XYZ remains in the readout.
        public static void ProjectHand(Vector3[] joints,bool[] valid,Vector2Int[] output)
        {
            Vector3 up=Vector3.forward,right=Vector3.right;
            if(valid[0] && valid[6] && valid[12] && valid[21])
            {
                var axis=joints[12]-joints[0];
                var width=Vector3.ProjectOnPlane(joints[6]-joints[21],axis.normalized);
                if(axis.sqrMagnitude>0.00001f && width.sqrMagnitude>0.00001f){up=axis.normalized;right=width.normalized;}
            }
            Vector2 min=new Vector2(float.MaxValue,float.MaxValue),max=new Vector2(float.MinValue,float.MinValue);
            int count=Mathf.Min(26,Mathf.Min(joints.Length,valid.Length));bool any=false;
            for(int i=0;i<count;i++)if(valid[i])
            {
                var p=new Vector2(Vector3.Dot(joints[i],right),Vector3.Dot(joints[i],up));
                min=Vector2.Min(min,p);max=Vector2.Max(max,p);any=true;
            }
            var center=any?(min+max)*0.5f:Vector2.zero;
            float scale=208f/Mathf.Max(0.12f,max.x-min.x,max.y-min.y);
            for(int i=0;i<count;i++)if(valid[i])
            {
                var p=new Vector2(Vector3.Dot(joints[i],right),Vector3.Dot(joints[i],up));
                output[i]=new Vector2Int(Mathf.RoundToInt(127.5f+(p.x-center.x)*scale),Mathf.RoundToInt(127.5f+(p.y-center.y)*scale));
            }
        }
        static void Dot(Color32[] target, Vector2Int point, int radius, Color32 color)
        {
            for (int y = -radius; y <= radius; y++) for (int x = -radius; x <= radius; x++)
                if (x*x + y*y <= radius*radius) Put(target, point.x + x, point.y + y, color);
        }
        static void Line(Color32[] target, Vector2Int a, Vector2Int b, Color32 color)
        {
            // Clamp before drawing, so a distant/unusual pose cannot cause an unbounded loop.
            a.x = Mathf.Clamp(a.x, -32, 287); a.y = Mathf.Clamp(a.y, -32, 287);
            b.x = Mathf.Clamp(b.x, -32, 287); b.y = Mathf.Clamp(b.y, -32, 287);
            int steps = Mathf.Max(Mathf.Abs(b.x-a.x), Mathf.Abs(b.y-a.y));
            for (int i = 0; i <= steps; i++) { float t = steps == 0 ? 0 : (float)i/steps; Put(target, Mathf.RoundToInt(Mathf.Lerp(a.x,b.x,t)), Mathf.RoundToInt(Mathf.Lerp(a.y,b.y,t)), color); }
        }
        static void Put(Color32[] target, int x, int y, Color32 color) { if (x >= 0 && y >= 0 && x < Size && y < Size) target[y * Size + x] = color; }
        void LateUpdate()
        {
            if (!XRSettings.isDeviceActive && playerCamera != null)
            {
                if(desktopViews!=null && desktopViews.Length>0)
                    playerCamera.transform.SetPositionAndRotation(desktopViews[desktopView].position,desktopViews[desktopView].rotation);
                else playerCamera.transform.SetPositionAndRotation(new Vector3(0, 1.62f, -0.95f), Quaternion.Euler(9, 0, 0));
            }
        }
        void OnDestroy() { foreach (var texture in textures) if (texture != null) Destroy(texture); }
    }
}
