using UnityEngine;
using UnityEngine.UI;

namespace HandTrackingMiniJeu
{
    public sealed class FingerGunTarget : MonoBehaviour
    {
        public Text hitText;
        public int targetNumber;
        public int Total { get; private set; }
        public int LastAward { get; private set; }
        float popupUntil;
        public int Hit(Vector3 point)
        {
            Vector3 local=transform.InverseTransformPoint(point);
            int points=PointsAt(new Vector2(local.x,local.y).magnitude);
            if(points==0)return 0;
            Total+=points;LastAward=points;popupUntil=Time.time+1.8f;
            if(hitText!=null)hitText.text=$"+{points} POINTS\nTOTAL {Total}";
            return points;
        }
        public static int PointsAt(float radius)=>radius<=0.075f?10:radius<=0.19f?5:radius<=0.35f?2:0;
        void Update(){if(Time.time>popupUntil && hitText!=null)hitText.text=$"CIBLE {targetNumber:00}\nTOTAL {Total}";}
        public void ResetScore(){Total=0;LastAward=0;popupUntil=0;}
    }
}
