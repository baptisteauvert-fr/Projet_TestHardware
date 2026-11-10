using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace HandTrackingMiniJeu
{
    public sealed class TabletopSortingGame : MonoBehaviour
    {
        public TabletopPiece[] pieces;
        public Transform[] goals;
        public Text scoreText, hintText;
        public AudioSource speaker;
        public FingerGunRange shootingRange;
        public int Score { get; private set; }
        public int GrabCount { get; private set; }
        readonly float[] settledTime = new float[6];
        readonly bool[] wasHeld = new bool[6];
        AudioClip completeSound;
        bool won;
        void Awake()
        {
            var data = new float[11025];
            for (int i = 0; i < data.Length; i++) data[i] = Mathf.Sin(2 * Mathf.PI * (i < 5500 ? 660 : 880) * i / 22050) * 0.12f * (1 - (float)i / data.Length);
            completeSound = AudioClip.Create("Hand lab complete", data.Length, 1, 22050, false); completeSound.SetData(data, 0);
        }
        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) ResetObjects();
            Evaluate(Time.deltaTime);
        }
        public void Evaluate(float deltaTime)
        {
            Score = 0;
            for (int i = 0; i < pieces.Length; i++)
            {
                var piece = pieces[i]; bool held = piece.Held;
                if (held && !wasHeld[i]) GrabCount++;
                wasHeld[i] = held;
                if (!held && (piece.transform.position.y < 0.25f || piece.transform.position.sqrMagnitude > 25)) piece.ResetHome();
                Vector3 relative = goals[piece.colorIndex].InverseTransformPoint(piece.transform.position);
                bool inGoal = InsideGoal(relative) && !held && piece.Body.linearVelocity.sqrMagnitude < 0.16f;
                settledTime[i] = inGoal ? settledTime[i] + Mathf.Max(0, deltaTime) : 0;
                if (settledTime[i] >= 0.35f) Score++;
            }
            scoreText.text = $"OBJETS TRIES   {Score} / {pieces.Length}          SAISIES   {GrabCount}";
            bool complete = Score == pieces.Length;
            if (complete && !won && speaker != null) speaker.PlayOneShot(completeSound, 0.4f);
            won = complete;
            hintText.text = complete ? "BRAVO !  LES SIX OBJETS SONT A LEUR PLACE" : "PINCEZ POUR SAISIR / DEPOSEZ DANS LA ZONE DE MEME COULEUR";
        }
        public static bool InsideGoal(Vector3 point) => Mathf.Abs(point.x) <= 0.18f && Mathf.Abs(point.z) <= 0.13f && point.y >= 0 && point.y <= 0.23f;
        public void ResetObjects()
        {
            foreach (var piece in pieces) piece.ResetHome();
            for (int i = 0; i < settledTime.Length; i++) settledTime[i] = 0;
            Score = 0; GrabCount = 0; won = false;
            if(shootingRange!=null)shootingRange.ResetScores();
        }
        void OnDestroy() { if (completeSound != null) Destroy(completeSound); }
    }
}
