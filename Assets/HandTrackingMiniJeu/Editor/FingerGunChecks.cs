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
        [MenuItem("Tools/Hand Tracking MiniJeu/Check Cannon Feedback")]
        public static void CheckFeedback()
        {
            var gesture=new FingerGunGesture();
            gesture.Update(true,true,0.95f,0);gesture.Update(true,true,0.95f,0.10f);
            Require(gesture.Armed && gesture.Update(true,true,0.68f,0.2f),"A natural partial thumb lowering must fire.");
            Require(!gesture.Update(true,true,0.68f,1) && !gesture.Update(true,true,0.68f,1.2f) && !gesture.Armed,"Keeping the thumb down must not rearm.");
            gesture.Update(true,true,1.0f,1.5f);gesture.Update(true,true,1.0f,1.6f);
            Require(gesture.Update(true,true,0.60f,1.8f),"Raising then lowering the thumb must fire again.");
            float raised=FingerGunRange.ThumbLift(new Vector3(0.06f,0.06f,-0.03f),Vector3.zero,Vector3.forward,0.065f);
            float lowered=FingerGunRange.ThumbLift(new Vector3(0.018f,0.012f,-0.14f),Vector3.zero,Vector3.forward,0.065f);
            Require(raised>1 && lowered<0.5f,"Thumb lift must ignore distance along the pointing fingers.");
            var joints=new Vector3[26];var valid=new bool[26];var first=new Vector2Int[26];var second=new Vector2Int[26];
            for(int i=0;i<26;i++){joints[i]=new Vector3((i%5-2)*0.023f,0,i*0.007f);valid[i]=true;}
            joints[0]=Vector3.zero;joints[6]=new Vector3(0.04f,0,0.06f);joints[12]=new Vector3(0,0,0.10f);joints[21]=new Vector3(-0.04f,0,0.06f);
            HandTrackingMonitor.ProjectHand(joints,valid,first);
            var rotation=Quaternion.Euler(73,145,28);for(int i=0;i<26;i++)joints[i]=rotation*joints[i]+new Vector3(2,1.6f,-4);
            HandTrackingMonitor.ProjectHand(joints,valid,second);
            int minX=255,maxX=0,minY=255,maxY=0;
            for(int i=0;i<26;i++)
            {
                Require(Vector2Int.Distance(first[i],second[i])<=2,"Turning or moving the hand must preserve the centered image.");
                Require(second[i].x>=20 && second[i].x<=235 && second[i].y>=20 && second[i].y<=235,"All hand joints must remain inside the frame.");
                minX=Mathf.Min(minX,second[i].x);maxX=Mathf.Max(maxX,second[i].x);minY=Mathf.Min(minY,second[i].y);maxY=Mathf.Max(maxY,second[i].y);
            }
            Require(Mathf.Abs((minX+maxX)*0.5f-127.5f)<1 && Mathf.Abs((minY+maxY)*0.5f-127.5f)<1,"The hand bounds must be centered in both axes.");
            File.WriteAllText("Tools/Integrations/hand-feedback-check.json","{\"success\":true,\"checks\":[\"partial_thumb_trigger\",\"no_held_thumb_rearm\",\"repeat_after_raise\",\"perpendicular_thumb_measure\",\"rotation_and_translation\",\"all_joints_in_frame\",\"centered_hand_bounds\"]}");
            Debug.Log("Cannon trigger and centered hand display checks passed.");
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
