using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using SnakeArcade;

namespace HandTrackingMiniJeu.Editor
{
    public static class FingerGunChecks
    {
        static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException("Finger gun check: "+message);}
        [MenuItem("Tools/Hand Tracking MiniJeu/Check Finger Gun Rules")]
        public static void CheckRules()
        {
            var gesture=new FingerGunGesture();
            Require(!gesture.Update(true,true,0.4f,0),"Thumb down alone must not fire.");
            Require(!gesture.Update(true,true,1.2f,1) && !gesture.Armed,"Raising thumb begins arming hold.");
            Require(!gesture.Update(true,true,1.2f,1.1f) && gesture.Armed,"Raised thumb arms after hold.");
            Require(gesture.Update(true,true,0.4f,1.2f),"Lowering thumb fires once.");
            Require(!gesture.Update(true,true,0.4f,1.3f),"Holding thumb down cannot repeat fire.");
            gesture.Update(true,true,1.2f,2);gesture.Update(true,true,1.2f,2.1f);
            Require(!gesture.Update(false,true,0.4f,2.2f) && !gesture.Armed,"Tracking loss disarms without firing.");
            Require(!gesture.Update(true,true,0.4f,2.3f),"Reacquisition with thumb down cannot fire.");
            gesture.Update(true,true,1.2f,3);gesture.Update(true,true,1.2f,3.1f);
            Require(!gesture.Update(true,false,0.4f,3.2f),"Invalid finger pose must not fire.");
            Require(FingerGunTarget.PointsAt(0)==10 && FingerGunTarget.PointsAt(0.14f)==5 && FingerGunTarget.PointsAt(0.27f)==2 && FingerGunTarget.PointsAt(0.5f)==0,"Target ring scoring.");
            var range=UnityEngine.Object.FindAnyObjectByType<FingerGunRange>();Require(range!=null && range.targets.Length==3,"Three targets must be in the same scene.");
            Require(UnityEngine.Object.FindAnyObjectByType<SnakeArcadeMachine>()!=null,"Snake arcade must be in the same scene.");
            File.WriteAllText("Tools/Integrations/hand-gun-rules-check.json","{\"success\":true,\"checks\":[\"no_shot_without_arming\",\"stable_thumb_arming\",\"thumb_lowering_fires\",\"one_shot_per_gesture\",\"tracking_loss_disarms\",\"reacquisition_safe\",\"wrong_pose_safe\",\"ring_scoring\",\"three_targets\",\"snake_arcade_present\"]}");
            Debug.Log("Finger gun gesture checks passed.");
        }
        [MenuItem("Tools/Hand Tracking MiniJeu/Check Target Hits")]
        public static void CheckHits()
        {
            Require(EditorApplication.isPlaying,"Enter Play mode first.");
            var arcade=UnityEngine.Object.FindAnyObjectByType<SnakeArcadeMachine>();
            Require(arcade!=null && arcade.Game!=null && arcade.joystick.machine==arcade && arcade.startButton.machine==arcade,"Cloned arcade references must point to its own machine.");
            arcade.startButton.Press();Require(arcade.Game.State==SnakeState.Playing,"Snake must start in the combined scene.");
            var range=UnityEngine.Object.FindAnyObjectByType<FingerGunRange>();range.ResetScores();Physics.SyncTransforms();
            var target=range.targets[0];var center=target.transform.position;
            Require(range.FireRay(new Ray(center+Vector3.forward*2,Vector3.back))==10,"Center ray awards ten points.");
            Require(target.hitText.text.Contains("+10"),"Hit score must appear above target.");
            Require(range.FireRay(new Ray(center+new Vector3(0.14f,0,2),Vector3.back))==5,"Middle ray awards five points.");
            Require(range.FireRay(new Ray(center+new Vector3(0.27f,0,2),Vector3.back))==2,"Outer ray awards two points.");
            Require(range.FireRay(new Ray(center+new Vector3(0,1.4f,2),Vector3.back))==0,"Miss awards no points.");
            Require(range.TotalScore==17 && range.Shots==4 && range.Hits==3,"Global score and hit counters.");
            range.ResetScores();Require(range.TotalScore==0 && target.Total==0,"Reset clears target and global score.");
            File.WriteAllText("Tools/Integrations/hand-gun-live-check.json","{\"success\":true,\"checks\":[\"arcade_references\",\"arcade_starts\",\"center_hit\",\"score_popup\",\"middle_hit\",\"outer_hit\",\"miss\",\"total_and_hit_count\",\"reset_scores\"]}");
            Debug.Log("Finger gun target hit checks passed.");
        }
    }
}
