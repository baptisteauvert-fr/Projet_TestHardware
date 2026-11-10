using UnityEngine;

namespace HandTrackingMiniJeu
{
    // Hysteresis and a short arming hold prevent thumb jitter or tracking loss from firing.
    public sealed class FingerGunGesture
    {
        public bool Armed { get; private set; }
        float raisedSince=-1, lastShot=-10, peak, rearmLevel=0.80f;
        public bool Update(bool tracked,bool fingersPointing,float thumbOpenness,float time)
        {
            if(!tracked || !fingersPointing){Reset();return false;}
            if(thumbOpenness>=rearmLevel)
            {
                if(raisedSince<0){raisedSince=time;peak=thumbOpenness;}
                peak=Mathf.Max(peak,thumbOpenness);
                if(time-raisedSince>=0.08f && time-lastShot>=0.28f)Armed=true;
            }
            else if(!Armed)raisedSince=-1;
            // Adapt the trigger to the raised thumb instead of requiring a nearly closed fist.
            float triggerLevel=Mathf.Max(0.50f,Mathf.Min(peak-0.18f,peak*0.78f));
            if(Armed && thumbOpenness<=triggerLevel)
            {Armed=false;raisedSince=-1;lastShot=time;rearmLevel=Mathf.Clamp(peak*0.82f,0.80f,1.05f);return true;}
            return false;
        }
        public void Reset(){Armed=false;raisedSince=-1;peak=0;rearmLevel=0.80f;}
    }
}
