using UnityEngine;

namespace HandTrackingMiniJeu
{
    // Hysteresis and a short arming hold prevent thumb jitter or tracking loss from firing.
    public sealed class FingerGunGesture
    {
        public bool Armed { get; private set; }
        float raisedSince=-1, lastShot=-10;
        public bool Update(bool tracked,bool fingersPointing,float thumbOpenness,float time)
        {
            if(!tracked || !fingersPointing){Reset();return false;}
            if(thumbOpenness>=0.90f)
            {
                if(raisedSince<0)raisedSince=time;
                if(time-raisedSince>=0.08f && time-lastShot>=0.28f)Armed=true;
            }
            else if(!Armed)raisedSince=-1;
            if(Armed && thumbOpenness<=0.62f)
            {Armed=false;raisedSince=-1;lastShot=time;return true;}
            return false;
        }
        public void Reset(){Armed=false;raisedSince=-1;}
    }
}
