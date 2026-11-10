using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace HandTrackingMiniJeu.Editor
{
    public static class HandTrackingChecks
    {
        [Serializable] class Report { public bool success; public bool running; public string error; public string[] checks; }
        static void Require(bool condition,string message) { if(!condition)throw new InvalidOperationException(message); }
        [MenuItem("Tools/Hand Tracking MiniJeu/Check Scene")]
        public static void CheckScene()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<TabletopSortingGame>();
            var monitor=UnityEngine.Object.FindFirstObjectByType<HandTrackingMonitor>();
            Require(game!=null && monitor!=null,"MiniJeu scene must be open.");
            Require(game.pieces.Length==6 && game.goals.Length==3,"Six objects and three targets required.");
            foreach(var piece in game.pieces)
            {
                Require(piece.GetComponent<Rigidbody>()!=null,"Object needs physics.");
                var grab=piece.GetComponent<XRGrabInteractable>();Require(grab!=null && grab.colliders.Count>0,"Object must support XR grab.");
                foreach(var collider in grab.colliders)Require(!collider.isTrigger,"Grab colliders must be solid.");
            }
            Require(monitor.trackingOrigin!=null && monitor.playerCamera!=null && monitor.leftReadout!=null && monitor.rightReadout!=null,"Tracking screen must be fully wired.");
            Require(OVRProjectConfig.CachedProjectConfig.handTrackingSupport==OVRProjectConfig.HandTrackingSupport.ControllersAndHands,"Meta hand tracking permission configuration.");
            Require(TabletopSortingGame.InsideGoal(new Vector3(0,0.08f,0)),"Center belongs to target.");
            Require(!TabletopSortingGame.InsideGoal(new Vector3(0.4f,0.08f,0)),"Off-target is rejected.");
            Require(!TabletopSortingGame.InsideGoal(new Vector3(0,-1,0)),"Under-table position is rejected.");
            var point=HandTrackingMonitor.Project(new Vector3(0,1.2f,0.7f));Require(Mathf.Abs(point.x-128)<=1 && Mathf.Abs(point.y-128)<=1,"Screen projection must use tracking-space X/Z.");
            var report=new Report{success=true,checks=new[]{"six_grabbable_objects","physics_and_solid_colliders","three_targets","tracking_ui_references","meta_hand_permission","goal_center","goal_bounds","goal_height","position_projection"}};
            File.WriteAllText("Tools/Integrations/hand-scene-check.json",JsonUtility.ToJson(report,true));
            Debug.Log("Hand Tracking MiniJeu scene checks passed.");
        }

        [MenuItem("Tools/Hand Tracking MiniJeu/Check Live Interactions")]
        public static async void CheckLive()
        {
            const string reportPath="Tools/Integrations/hand-live-check.json";
            var checks=new List<string>();GameObject probe=null;XRDirectInteractor hand=null;
            var report=new Report{running=true};File.WriteAllText(reportPath,JsonUtility.ToJson(report));
            try
            {
                Require(EditorApplication.isPlaying,"Play mode is required.");
                var game=UnityEngine.Object.FindFirstObjectByType<TabletopSortingGame>();
                var monitor=UnityEngine.Object.FindFirstObjectByType<HandTrackingMonitor>();
                Require(game!=null && monitor!=null && monitor.leftImage.texture!=null,"Runtime screen initialized.");checks.Add("runtime_screen");
                var local=new Vector3(0.1f,1.1f,0.4f);var expected=monitor.trackingOrigin.TransformPoint(local);
                Require(Vector3.Distance(HandTrackingMonitor.TrackingToWorld(monitor.trackingOrigin,local),expected)<0.0001f,"World coordinate conversion.");checks.Add("world_coordinates");
                monitor.DisplaySample(0,true,local,0.8f,null,null);Require(monitor.LeftTracked && monitor.leftReadout.text.Contains("80 %"),"Live sample and pinch percentage displayed.");checks.Add("position_and_pinch_display");
                monitor.DisplaySample(0,false,Vector3.zero,-1,null,null);Require(!monitor.LeftTracked && monitor.leftReadout.text.Contains("NON SUIVIE") && monitor.leftReadout.text.Contains("--"),"Tracking loss must clear stale positions.");checks.Add("tracking_loss_clears_coordinates");
                var manager=UnityEngine.Object.FindFirstObjectByType<XRInteractionManager>();Require(manager!=null,"XR manager exists.");
                probe=new GameObject("Temporary hand interaction check");var sphere=probe.AddComponent<SphereCollider>();sphere.isTrigger=true;sphere.radius=0.012f;
                var rigid=probe.AddComponent<Rigidbody>();rigid.isKinematic=true;rigid.useGravity=false;
                hand=probe.AddComponent<XRDirectInteractor>();hand.interactionManager=manager;
                var attach=new GameObject("Grip").transform;attach.SetParent(probe.transform,false);hand.attachTransform=attach;
                var first=game.pieces[0];var home=first.transform.position;probe.transform.position=home;
                hand.StartManualInteraction((IXRSelectInteractable)first.Grab);Require(first.Held,"Hand can grab a table object.");checks.Add("xr_grab");
                await Task.Delay(120);probe.transform.position+=new Vector3(0.12f,0.15f,0.05f);await Task.Delay(250);
                Require(Vector3.Distance(first.Grab.GetTargetPose().position,home)>0.08f,"Object target follows the hand.");checks.Add("grab_target_follows_hand");
                hand.EndManualInteraction();Require(!first.Held && first.Body.useGravity,"Release restores gravity.");checks.Add("xr_release_and_gravity");
                game.ResetObjects();await Task.Delay(120);
                for(int i=0;i<game.pieces.Length;i++)
                {
                    var piece=game.pieces[i];var target=game.goals[piece.colorIndex].position+new Vector3(i<3?-0.085f:0.085f,0.10f,0);
                    piece.Body.position=target;piece.transform.position=target;piece.Body.linearVelocity=Vector3.zero;piece.Body.angularVelocity=Vector3.zero;
                }
                Physics.SyncTransforms();game.Evaluate(0.5f);Require(game.Score==6,"All matching objects must score.");checks.Add("matching_color_scoring");
                first.Body.position=game.goals[1].position+Vector3.up*0.1f;first.transform.position=first.Body.position;game.Evaluate(0.5f);
                Require(game.Score==5,"Wrong color must not score.");checks.Add("wrong_color_rejected");
                game.ResetObjects();first.Body.position=new Vector3(0,-2,0);first.transform.position=first.Body.position;game.Evaluate(0.5f);
                Require(first.transform.position.y>0.8f,"Fallen objects return to the table.");checks.Add("fallen_object_recovery");
                var button=UnityEngine.Object.FindFirstObjectByType<TabletopResetButton>();probe.transform.position=button.transform.position;
                hand.StartManualInteraction((IXRSelectInteractable)button);hand.EndManualInteraction();Require(button.PressCount>0 && game.Score==0,"XR Reset works.");checks.Add("xr_reset");
                report.success=true;report.checks=checks.ToArray();Debug.Log("Hand Tracking MiniJeu live checks passed.");
                game.ResetObjects();
            }
            catch(Exception error){report.error=error.Message;report.checks=checks.ToArray();Debug.LogException(error);}
            finally
            {
                if(hand!=null && hand.hasSelection)hand.EndManualInteraction();
                if(probe!=null)UnityEngine.Object.Destroy(probe);
                report.running=false;File.WriteAllText(reportPath,JsonUtility.ToJson(report,true));
            }
        }

        [MenuItem("Tools/Hand Tracking MiniJeu/Render Preview")]
        public static void RenderPreview()
        {
            var monitor=UnityEngine.Object.FindFirstObjectByType<HandTrackingMonitor>();
            var renderers=monitor.trackingOrigin.GetComponentsInChildren<Renderer>(true);var enabled=new bool[renderers.Length];
            for(int i=0;i<renderers.Length;i++){enabled[i]=renderers[i].enabled;renderers[i].enabled=false;}
            var go=new GameObject("Temporary lab preview camera");var camera=go.AddComponent<Camera>();camera.transform.position=new Vector3(-1.4f,1.95f,-2.65f);
            camera.transform.LookAt(new Vector3(0,1.22f,0.30f));camera.fieldOfView=48;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(0.04f,0.06f,0.09f);
            var target=new RenderTexture(1400,1050,24);var previous=RenderTexture.active;Texture2D image=null;
            try{camera.targetTexture=target;camera.Render();RenderTexture.active=target;image=new Texture2D(1400,1050,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1400,1050),0,0);image.Apply();File.WriteAllBytes("Tools/Integrations/HandTrackingMiniJeu-Preview.png",image.EncodeToPNG());}
            finally{RenderTexture.active=previous;camera.targetTexture=null;target.Release();UnityEngine.Object.DestroyImmediate(target);if(image!=null)UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(go);for(int i=0;i<renderers.Length;i++)renderers[i].enabled=enabled[i];}
        }
    }
}
