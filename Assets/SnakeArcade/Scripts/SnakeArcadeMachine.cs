using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;

namespace SnakeArcade
{
    public sealed class SnakeArcadeMachine : MonoBehaviour
    {
        public RawImage board;
        public Text scoreText, statusText, overlayText;
        public AudioSource speaker;
        public Camera playerCamera;
        public SnakeArcadeJoystick joystick;
        public SnakeArcadeStartButton startButton;
        public SnakeGameModel Game { get; private set; }
        public int BestScore { get; private set; }
        Texture2D texture;
        Color32[] pixels;
        float elapsed, countdown;
        AudioClip startSound, eatSound, endSound, turnSound;
        const int CellPixels = 12;
        const string BestKey = "SnakeArcade.BestScore";

        void Awake()
        {
            BestScore = PlayerPrefs.GetInt(BestKey, 0);
            Game = new SnakeGameModel(seed: System.Environment.TickCount);
            texture = new Texture2D(240, 240, TextureFormat.RGBA32, false) { name = "Snake live screen", filterMode = FilterMode.Point };
            pixels = new Color32[texture.width * texture.height];
            board.texture = texture;
            startSound = Tone(660, 0.13f); eatSound = Tone(880, 0.08f);
            endSound = Tone(150, 0.35f); turnSound = Tone(260, 0.025f);
            RefreshScreen();
        }

        public void PressStart()
        {
            // Ignore accidental presses against the button while a round is in progress.
            if (Game == null || Game.State == SnakeState.Playing) return;
            Game.StartRound(); elapsed = 0; countdown = 1.5f;
            Play(startSound); RefreshScreen();
        }

        public bool SetDirection(Vector2Int direction)
        {
            bool changed = Game != null && Game.QueueDirection(direction);
            if (changed) Play(turnSound, 0.16f);
            return changed;
        }

        void Update()
        {
            var keys = Keyboard.current;
            if (keys != null)
            {
                if (keys.spaceKey.wasPressedThisFrame || keys.enterKey.wasPressedThisFrame) startButton.Press();
                if (keys.upArrowKey.isPressed || keys.wKey.isPressed) DesktopDirection(Vector2Int.up);
                else if (keys.downArrowKey.isPressed || keys.sKey.isPressed) DesktopDirection(Vector2Int.down);
                else if (keys.leftArrowKey.isPressed || keys.aKey.isPressed) DesktopDirection(Vector2Int.left);
                else if (keys.rightArrowKey.isPressed || keys.dKey.isPressed) DesktopDirection(Vector2Int.right);
            }
            if (Game.State != SnakeState.Playing) return;
            if (countdown > 0)
            {
                countdown -= Time.deltaTime;
                overlayText.text = countdown > 0.75f ? "PRET ?" : "GO !";
                if (countdown <= 0) overlayText.gameObject.SetActive(false);
                return;
            }
            elapsed += Time.deltaTime;
            // Never advance a whole backlog after a pause or a long headset frame.
            if (elapsed < Game.StepInterval) return;
            elapsed %= Game.StepInterval;
            int previousScore = Game.Score;
            Game.Tick();
            if (Game.Score != previousScore) Play(eatSound);
            if (Game.State == SnakeState.GameOver || Game.State == SnakeState.Won)
            {
                Play(endSound);
                if (Game.Score > BestScore)
                {
                    BestScore = Game.Score;
                    PlayerPrefs.SetInt(BestKey, BestScore); PlayerPrefs.Save();
                }
            }
            RefreshScreen();
        }

        void DesktopDirection(Vector2Int direction)
        {
            SetDirection(direction);
            if (joystick != null) joystick.PreviewDirection(direction);
        }

        void LateUpdate()
        {
            // The same scene can be checked with a keyboard without an attached headset.
            if (!XRSettings.isDeviceActive && playerCamera != null)
                playerCamera.transform.SetPositionAndRotation(new Vector3(0, 1.58f, -1.03f), Quaternion.Euler(7, 0, 0));
        }

        void OnApplicationPause(bool paused) { elapsed = 0; }

        public void RefreshScreen()
        {
            if (Game == null || texture == null) return;
            var dark = new Color32(8, 19, 24, 255);
            var grid = new Color32(13, 31, 36, 255);
            for (int y = 0; y < 240; y++)
                for (int x = 0; x < 240; x++) pixels[y * 240 + x] = (x % CellPixels == 0 || y % CellPixels == 0) ? grid : dark;
            DrawCell(Game.Food, new Color32(255, 111, 100, 255), 2);
            for (int i = Game.Body.Count - 1; i >= 0; i--)
                DrawCell(Game.Body[i], i == 0 ? new Color32(224, 255, 164, 255) : new Color32(124, 232, 124, 255), 1);
            Vector2Int head = Game.Body[0];
            int hx = head.x * CellPixels, hy = head.y * CellPixels;
            // Two small eyes remain readable on the cabinet screen.
            if (Game.Direction.x != 0)
            {
                int ex = hx + (Game.Direction.x > 0 ? 8 : 3);
                Pixel(ex, hy + 4, dark); Pixel(ex, hy + 8, dark);
            }
            else
            {
                int ey = hy + (Game.Direction.y > 0 ? 8 : 3);
                Pixel(hx + 4, ey, dark); Pixel(hx + 8, ey, dark);
            }
            texture.SetPixels32(pixels); texture.Apply(false);
            scoreText.text = $"SCORE  {Game.Score:000}                 RECORD  {BestScore:000}";
            bool playing = Game.State == SnakeState.Playing;
            overlayText.gameObject.SetActive(!playing || countdown > 0);
            overlayText.text = Game.State == SnakeState.Ready ? "S N A K E\n\nAPPUYEZ SUR START" :
                Game.State == SnakeState.Won ? "BRAVO !\n\nSTART POUR REJOUER" :
                Game.State == SnakeState.GameOver ? $"GAME OVER\nSCORE  {Game.Score:000}\n\nSTART POUR REJOUER" : "PRET ?";
            statusText.text = playing ? "JOYSTICK  /  4 DIRECTIONS" : "SAISIR LE JOYSTICK  +  START";
        }

        void DrawCell(Vector2Int cell, Color32 color, int inset)
        {
            for (int y = inset; y < CellPixels - inset; y++)
                for (int x = inset; x < CellPixels - inset; x++) Pixel(cell.x * CellPixels + x, cell.y * CellPixels + y, color);
        }
        void Pixel(int x, int y, Color32 color) { pixels[y * 240 + x] = color; }
        void Play(AudioClip clip, float volume = 0.35f) { if (speaker != null && clip != null) speaker.PlayOneShot(clip, volume); }
        static AudioClip Tone(float hz, float duration)
        {
            const int rate = 22050;
            float[] data = new float[Mathf.CeilToInt(rate * duration)];
            for (int i = 0; i < data.Length; i++)
                data[i] = (Mathf.Sin(2 * Mathf.PI * hz * i / rate) > 0 ? 0.12f : -0.12f) * (1f - (float)i / data.Length);
            var clip = AudioClip.Create("Arcade tone", data.Length, 1, rate, false); clip.SetData(data, 0); return clip;
        }
        void OnDestroy()
        {
            if (texture != null) Destroy(texture);
            foreach (var clip in new[] { startSound, eatSound, endSound, turnSound }) if (clip != null) Destroy(clip);
        }
    }
}
