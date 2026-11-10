using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using Unity.XR.CoreUtils;

namespace SnakeArcade.Editor
{
    public static class SnakeArcadeSceneBuilder
    {
        public const string ScenePath = "Assets/SnakeArcade/Scenes/SnakeArcadeVR.unity";
        const string ArtPath = "Assets/SnakeArcade/Art";
        static readonly Color Cyan = new Color(0.10f, 0.86f, 0.92f);
        static readonly Color Coral = new Color(1f, 0.27f, 0.42f);
        static Font font;
        static Material shell, dark, metal, cyan, coral, white, floor;

        [MenuItem("Tools/Snake Arcade/Create VR Scene")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode before creating the scene.");
            Directory.CreateDirectory("Assets/SnakeArcade/Scenes"); Directory.CreateDirectory(ArtPath);
            // Preserve unsaved work in any currently open scene before opening the new scene.
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var previous = SceneManager.GetSceneAt(i);
                if (!previous.isDirty) continue;
                Directory.CreateDirectory("Assets/SnakeArcade/Backups");
                string backup = AssetDatabase.GenerateUniqueAssetPath("Assets/SnakeArcade/Backups/" + previous.name + "-BeforeArcade.unity");
                EditorSceneManager.SaveScene(previous, backup, true);
            }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            // Unity cannot save a second loaded scene over an already loaded scene asset.
            for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                var loaded = SceneManager.GetSceneAt(i);
                if (loaded != scene && loaded.path == ScenePath) EditorSceneManager.CloseScene(loaded, true);
            }
            scene.name = "SnakeArcadeVR";
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            shell = Material("Midnight enamel", new Color(0.065f, 0.09f, 0.16f), false, 0.3f);
            dark = Material("Obsidian bezel", new Color(0.009f, 0.015f, 0.025f));
            metal = Material("Brushed metal", new Color(0.25f, 0.32f, 0.39f), false, 0.55f);
            cyan = Material("Ion cyan", Cyan, true);
            coral = Material("Hot coral", Coral, true);
            white = Material("Warm lettering", new Color(0.88f, 0.96f, 0.92f), true);
            floor = Material("Room charcoal", new Color(0.045f, 0.055f, 0.08f));
            BuildRoom();

            var cabinet = new GameObject("SNAKE / Arcade Cabinet");
            var machine = cabinet.AddComponent<SnakeArcadeMachine>();
            Box(cabinet.transform, "Lower cabinet", new Vector3(0, 0.50f, 0.04f), new Vector3(0.84f, 1, 0.65f), shell);
            Box(cabinet.transform, "Front door", new Vector3(0, 0.53f, -0.292f), new Vector3(0.68f, 0.69f, 0.025f), dark);
            Box(cabinet.transform, "Door cyan seam", new Vector3(0, 0.88f, -0.308f), new Vector3(0.69f, 0.012f, 0.012f), cyan, false);
            Box(cabinet.transform, "Kick plate", new Vector3(0, 0.10f, -0.313f), new Vector3(0.73f, 0.11f, 0.03f), metal);
            Box(cabinet.transform, "Coin slot plate", new Vector3(0.18f, 0.70f, -0.315f), new Vector3(0.15f, 0.19f, 0.025f), metal);
            Box(cabinet.transform, "Coin slot", new Vector3(0.18f, 0.725f, -0.331f), new Vector3(0.065f, 0.012f, 0.008f), dark, false);
            Label(cabinet.transform, "FREE PLAY", new Vector3(0, 0.44f, -0.313f), new Vector2(0.56f, 0.15f), 43, Cyan);
            Label(cabinet.transform, "NO COINS. JUST SKILL.", new Vector3(0, 0.32f, -0.313f), new Vector2(0.60f, 0.08f), 18, new Color(0.5f, 0.65f, 0.7f));
            Box(cabinet.transform, "Screen backing", new Vector3(0, 1.49f, 0.14f), new Vector3(0.88f, 0.82f, 0.53f), shell);
            Box(cabinet.transform, "Screen bezel", new Vector3(0, 1.49f, -0.137f), new Vector3(0.76f, 0.80f, 0.033f), dark);
            Box(cabinet.transform, "Screen cyan underline", new Vector3(0, 1.095f, -0.16f), new Vector3(0.74f, 0.012f, 0.012f), cyan, false);
            for (int side = -1; side <= 1; side += 2)
            {
                SidePanel(cabinet.transform, side);
                Box(cabinet.transform, "Vertical light piping", new Vector3(side * 0.461f, 0.54f, -0.33f), new Vector3(0.014f, 0.83f, 0.014f), side < 0 ? cyan : coral, false);
                Box(cabinet.transform, "Marquee edge", new Vector3(side * 0.43f, 1.99f, -0.41f), new Vector3(0.018f, 0.20f, 0.018f), cyan, false);
            }
            Box(cabinet.transform, "Marquee hood", new Vector3(0, 1.99f, 0.02f), new Vector3(0.91f, 0.24f, 0.78f), shell);
            Box(cabinet.transform, "Marquee face", new Vector3(0, 1.99f, -0.384f), new Vector3(0.83f, 0.19f, 0.018f), dark);
            Label(cabinet.transform, "S N A K E", new Vector3(0, 2.012f, -0.398f), new Vector2(0.78f, 0.14f), 65, Cyan);
            Label(cabinet.transform, "IMMERSIVE ARCADE / 01", new Vector3(0, 1.93f, -0.398f), new Vector2(0.75f, 0.05f), 17, Color.white);
            for (int i = -6; i <= 6; i++)
                Box(cabinet.transform, "Speaker grille", new Vector3(i * 0.025f, 1.865f, -0.157f), new Vector3(0.01f, 0.025f, 0.006f), metal, false);

            Box(cabinet.transform, "Control deck", new Vector3(0, 1.045f, -0.275f), new Vector3(0.88f, 0.09f, 0.43f), dark);
            Box(cabinet.transform, "Deck front cyan trim", new Vector3(0, 1.035f, -0.496f), new Vector3(0.88f, 0.018f, 0.018f), cyan, false);
            BuildScreen(cabinet.transform, machine);
            BuildControls(cabinet.transform, machine);
            machine.speaker = cabinet.AddComponent<AudioSource>();
            machine.speaker.playOnAwake = false; machine.speaker.spatialBlend = 0.85f;
            machine.speaker.minDistance = 0.3f; machine.speaker.maxDistance = 5;

            var rigPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Samples/XR Interaction Toolkit/3.5.1/Hands Interaction Demo/Prefabs/XR Origin Hands (XR Rig).prefab");
            if (rigPrefab == null) throw new InvalidOperationException("The XR Hands rig prefab is missing.");
            var rig = (GameObject)PrefabUtility.InstantiatePrefab(rigPrefab, scene);
            rig.name = "Player / Quest + OpenXR Hands and Controllers";
            rig.transform.SetPositionAndRotation(new Vector3(0, 0, -0.9f), Quaternion.identity);
            var origin = rig.GetComponentInChildren<XROrigin>(true);
            if (origin == null) throw new InvalidOperationException("XR Origin not found.");
            origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
            machine.playerCamera = origin.Camera;
            machine.playerCamera.nearClipPlane = 0.04f;
            machine.playerCamera.farClipPlane = 30;
            machine.playerCamera.clearFlags = CameraClearFlags.SolidColor;
            machine.playerCamera.backgroundColor = new Color(0.015f, 0.022f, 0.04f);
            foreach (var provider in rig.GetComponentsInChildren<LocomotionProvider>(true)) provider.enabled = false;
            if (UnityEngine.Object.FindObjectsByType<XRInteractionManager>(FindObjectsSortMode.None).All(m => m.gameObject.scene != scene))
                new GameObject("XR Interaction Manager").AddComponent<XRInteractionManager>();
            if (rig.GetComponentInChildren<InputActionManager>(true) == null)
            {
                var input = rig.AddComponent<InputActionManager>();
                input.actionAssets.Add(AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets/XRI Default Input Actions.inputactions"));
            }
            Label(null, "S N A K E  /  V R", new Vector3(-1.55f, 1.9f, 1.66f), new Vector2(1.9f, 0.30f), 65, Cyan);
            Label(null, "01  SAISIR LE JOYSTICK\n02  APPUYER SUR START\n03  MANGER. GRANDIR. SURVIVRE.", new Vector3(-1.55f, 1.40f, 1.66f), new Vector2(1.85f, 0.6f), 26, new Color(0.68f, 0.77f, 0.83f));
            Label(null, "CONTROLEURS : GRIP POUR SAISIR\nMAINS : PINCER POUR SAISIR\nSTART : TOUCHER OU SELECTIONNER", new Vector3(1.63f, 1.45f, 1.66f), new Vector2(1.8f, 0.55f), 23, new Color(0.68f, 0.77f, 0.83f));
            Label(null, "QUATRE DIRECTIONS.\nUNE SEULE VIE.", new Vector3(1.63f, 1.95f, 1.66f), new Vector2(1.8f, 0.35f), 36, Coral);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.40f, 0.45f, 0.58f);
            RenderSettings.fog = false;
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new InvalidOperationException("Could not save the arcade scene.");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) }
                .Concat(EditorBuildSettings.scenes.Where(s => s.path != ScenePath)).ToArray();
            AssetDatabase.SaveAssets();
            // Opening the saved scene isolates it from the original template during Play.
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Selection.activeGameObject = UnityEngine.Object.FindFirstObjectByType<SnakeArcadeMachine>().gameObject;
            SceneView.lastActiveSceneView?.LookAt(new Vector3(0, 1.1f, 0), Quaternion.Euler(8, 0, 0), 3.4f);
            File.WriteAllText("Tools/Integrations/snake-scene-built.json", "{\"scene\":\"" + ScenePath + "\",\"success\":true}");
            Debug.Log("Snake Arcade VR created: " + ScenePath);
        }

        static void BuildRoom()
        {
            var room = new GameObject("Neon room").transform;
            Box(room, "Floor", new Vector3(0, -0.065f, 0), new Vector3(6, 0.12f, 6), floor);
            Box(room, "Back wall", new Vector3(0, 1.6f, 1.78f), new Vector3(6, 3.2f, 0.15f), shell);
            Box(room, "Left wall", new Vector3(-3, 1.6f, 0), new Vector3(0.15f, 3.2f, 3.5f), floor);
            Box(room, "Right wall", new Vector3(3, 1.6f, 0), new Vector3(0.15f, 3.2f, 3.5f), floor);
            for (int i = -5; i <= 5; i++)
            {
                Box(room, "Floor grid X", new Vector3(i * 0.55f, -0.003f, -0.3f), new Vector3(0.007f, 0.005f, 5.2f), metal, false);
                Box(room, "Floor grid Z", new Vector3(0, -0.003f, i * 0.5f), new Vector3(5.8f, 0.005f, 0.007f), metal, false);
            }
            Box(room, "Cyan wall line", new Vector3(0, 0.12f, 1.69f), new Vector3(5.8f, 0.025f, 0.01f), cyan, false);
            Box(room, "Coral wall line", new Vector3(0, 2.75f, 1.69f), new Vector3(5.8f, 0.025f, 0.01f), coral, false);
            for (int side = -1; side <= 1; side += 2)
                Box(room, "Wall neon bar", new Vector3(side * 2.75f, 1.45f, 1.68f), new Vector3(0.02f, 2.6f, 0.01f), side < 0 ? cyan : coral, false);
            var key = new GameObject("Soft overhead light").AddComponent<Light>();
            key.type = LightType.Directional; key.intensity = 1.5f; key.color = new Color(0.83f, 0.9f, 1);
            key.transform.rotation = Quaternion.Euler(48, -35, 0); key.shadows = LightShadows.None;
            var accent = new GameObject("Cabinet warm light").AddComponent<Light>();
            accent.type = LightType.Point; accent.color = new Color(1, 0.55f, 0.65f); accent.intensity = 1.2f; accent.range = 3;
            accent.transform.position = new Vector3(0.8f, 2.1f, -1.1f); accent.shadows = LightShadows.None;
        }

        static void BuildScreen(Transform parent, SnakeArcadeMachine machine)
        {
            var canvas = Surface("CRT / World Space Screen", parent, new Vector3(0, 1.49f, -0.158f), new Vector2(800, 920), 0.0008f);
            TextUI("Screen heading", canvas, "S N A K E", new Vector2(400, 870), new Vector2(740, 60), 38, Cyan);
            machine.scoreText = TextUI("Score", canvas, "SCORE  000                 RECORD  000", new Vector2(400, 815), new Vector2(750, 40), 23, Color.white);
            var image = new GameObject("Pixel Board", typeof(RectTransform), typeof(RawImage)).GetComponent<RawImage>();
            image.transform.SetParent(canvas, false); Rect(image.rectTransform, new Vector2(400, 425), new Vector2(720, 720));
            var preview = new Texture2D(240, 240, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "Snake attract screen" };
            var colors = new Color32[240 * 240];
            for (int y = 0; y < 240; y++) for (int x = 0; x < 240; x++)
                colors[y * 240 + x] = (x % 12 == 0 || y % 12 == 0) ? new Color32(13, 31, 36, 255) : new Color32(8, 19, 24, 255);
            for (int x = 85; x < 131; x++) for (int y = 121; y < 131; y++) colors[y * 240 + x] = new Color32(124, 232, 124, 255);
            for (int x = 157; x < 165; x++) for (int y = 157; y < 165; y++) colors[y * 240 + x] = new Color32(255, 111, 100, 255);
            preview.SetPixels32(colors); preview.Apply();
            string previewPath = ArtPath + "/SnakeAttract.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(previewPath);
            if (existing == null) { AssetDatabase.CreateAsset(preview, previewPath); existing = preview; }
            else UnityEngine.Object.DestroyImmediate(preview);
            image.texture = existing; image.raycastTarget = false; machine.board = image;
            machine.overlayText = TextUI("Game status", canvas, "S N A K E\n\nAPPUYEZ SUR START", new Vector2(400, 430), new Vector2(700, 450), 42, new Color(0.93f, 1f, 0.85f));
            machine.statusText = TextUI("Screen footer", canvas, "SAISIR LE JOYSTICK  +  START", new Vector2(400, 35), new Vector2(750, 45), 22, Cyan);
        }

        static void BuildControls(Transform parent, SnakeArcadeMachine machine)
        {
            var stick = new GameObject("Joystick / Grab and tilt"); stick.transform.SetParent(parent, false);
            stick.transform.localPosition = new Vector3(-0.22f, 1.095f, -0.29f);
            Shape(PrimitiveType.Cylinder, stick.transform, "Joystick socket", Vector3.zero, new Vector3(0.14f, 0.012f, 0.14f), metal, false);
            var pivot = new GameObject("Lever pivot").transform; pivot.SetParent(stick.transform, false);
            Shape(PrimitiveType.Cylinder, pivot, "Steel shaft", new Vector3(0, 0.065f, 0), new Vector3(0.022f, 0.065f, 0.022f), metal, false);
            var ball = Shape(PrimitiveType.Sphere, pivot, "Grab ball", new Vector3(0, 0.14f, 0), Vector3.one * 0.085f, coral, true);
            var rigid = stick.AddComponent<Rigidbody>(); rigid.isKinematic = true; rigid.useGravity = false;
            var joystick = stick.AddComponent<SnakeArcadeJoystick>();
            joystick.colliders.Clear(); joystick.colliders.Add(ball.GetComponent<Collider>());
            joystick.machine = machine; joystick.lever = pivot; machine.joystick = joystick;
            var button = new GameObject("START / Poke or select"); button.transform.SetParent(parent, false);
            button.transform.localPosition = new Vector3(0.22f, 1.095f, -0.29f);
            Shape(PrimitiveType.Cylinder, button.transform, "Button surround", Vector3.zero, new Vector3(0.14f, 0.012f, 0.14f), metal, false);
            var cap = Shape(PrimitiveType.Cylinder, button.transform, "Illuminated cap", new Vector3(0, 0.018f, 0), new Vector3(0.108f, 0.014f, 0.108f), cyan, false);
            // Poke and ray interactors ignore trigger colliders by default.
            var trigger = button.AddComponent<BoxCollider>(); trigger.center = new Vector3(0, 0.010f, 0); trigger.size = new Vector3(0.115f, 0.055f, 0.115f); trigger.isTrigger = false;
            var buttonRigid = button.AddComponent<Rigidbody>(); buttonRigid.isKinematic = true; buttonRigid.useGravity = false;
            var start = button.AddComponent<SnakeArcadeStartButton>();
            start.colliders.Clear(); start.colliders.Add(trigger); start.machine = machine; start.cap = cap.transform; machine.startButton = start;
            // Cap's OnEnable runs before its fields are assigned in the editor, but runs with all references in Play.
            Label(parent, "JOYSTICK", new Vector3(-0.22f, 1.097f, -0.445f), new Vector2(0.28f, 0.055f), 23, Color.white, Quaternion.Euler(90, 0, 0));
            Label(parent, "START", new Vector3(0.22f, 1.097f, -0.445f), new Vector2(0.25f, 0.055f), 25, Cyan, Quaternion.Euler(90, 0, 0));
            Label(parent, "+", new Vector3(-0.22f, 1.098f, -0.17f), new Vector2(0.1f, 0.06f), 35, Cyan, Quaternion.Euler(90, 0, 0));
        }

        static void SidePanel(Transform parent, int side)
        {
            var profile = new[] { new Vector2(0, -0.34f), new Vector2(0.96f, -0.34f), new Vector2(1.02f, -0.49f), new Vector2(1.13f, -0.49f), new Vector2(1.17f, -0.13f), new Vector2(1.84f, -0.13f), new Vector2(1.88f, -0.39f), new Vector2(2.12f, -0.39f), new Vector2(2.12f, 0.42f), new Vector2(0, 0.42f) };
            var vertices = new Vector3[profile.Length * 2]; var triangles = new System.Collections.Generic.List<int>();
            for (int i = 0; i < profile.Length; i++)
            { vertices[i] = new Vector3(side * 0.43f, profile[i].x, profile[i].y); vertices[i + profile.Length] = new Vector3(side * 0.47f, profile[i].x, profile[i].y); }
            // Polygon is intentionally triangulated around the convex rear edge in horizontal bands.
            int[] face = { 0,1,9, 1,8,9, 1,2,3, 1,3,4, 1,4,8, 4,5,8, 5,6,7, 5,7,8 };
            for (int i = 0; i < face.Length; i += 3)
            {
                triangles.Add(face[i]); triangles.Add(face[i + 2]); triangles.Add(face[i + 1]);
                triangles.Add(face[i] + profile.Length); triangles.Add(face[i + 1] + profile.Length); triangles.Add(face[i + 2] + profile.Length);
            }
            for (int i = 0; i < profile.Length; i++)
            { int j = (i + 1) % profile.Length, n = profile.Length; triangles.AddRange(new[] {i,j,j+n,i,j+n,i+n}); }
            var mesh = new Mesh { name = side < 0 ? "Left cabinet profile" : "Right cabinet profile" };
            mesh.vertices = vertices; mesh.triangles = triangles.ToArray(); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            string path = ArtPath + (side < 0 ? "/LeftPanel.asset" : "/RightPanel.asset");
            var saved = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (saved == null) { AssetDatabase.CreateAsset(mesh, path); saved = mesh; } else UnityEngine.Object.DestroyImmediate(mesh);
            var panel = new GameObject(side < 0 ? "Left sculpted side panel" : "Right sculpted side panel"); panel.transform.SetParent(parent, false);
            panel.AddComponent<MeshFilter>().sharedMesh = saved; panel.AddComponent<MeshRenderer>().sharedMaterial = shell;
            panel.AddComponent<MeshCollider>().sharedMesh = saved;
        }

        static Material Material(string name, Color color, bool unlit = false, float smooth = 0.12f)
        {
            string path = ArtPath + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find(unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit")) { name = name };
            material.SetColor("_BaseColor", color); material.SetFloat("_Smoothness", smooth);
            // Double-sided panels avoid winding differences in the procedural side profiles.
            material.SetFloat("_Cull", 0);
            AssetDatabase.CreateAsset(material, path); return material;
        }
        static GameObject Box(Transform parent, string name, Vector3 position, Vector3 size, Material material, bool collider = true)
            => Shape(PrimitiveType.Cube, parent, name, position, size, material, collider);
        static GameObject Shape(PrimitiveType type, Transform parent, string name, Vector3 position, Vector3 size, Material material, bool collider)
        {
            var obj = GameObject.CreatePrimitive(type); obj.name = name; obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position; obj.transform.localScale = size; obj.GetComponent<Renderer>().sharedMaterial = material;
            if (!collider) UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
            return obj;
        }
        static RectTransform Surface(string name, Transform parent, Vector3 position, Vector2 size, float scale)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas)); go.transform.SetParent(parent, false);
            go.transform.localPosition = position; go.transform.localScale = Vector3.one * scale;
            var canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace;
            var rect = (RectTransform)go.transform; rect.sizeDelta = size; return rect;
        }
        static Text TextUI(string name, Transform parent, string text, Vector2 position, Vector2 size, int fontSize, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)); go.transform.SetParent(parent, false);
            var ui = go.GetComponent<Text>(); ui.font = font; ui.text = text; ui.fontSize = fontSize; ui.color = color;
            ui.alignment = TextAnchor.MiddleCenter; ui.raycastTarget = false; ui.horizontalOverflow = HorizontalWrapMode.Wrap;
            Rect(ui.rectTransform, position, size); return ui;
        }
        static void Rect(RectTransform rect, Vector2 position, Vector2 size)
        { rect.anchorMin = rect.anchorMax = Vector2.zero; rect.pivot = Vector2.one * 0.5f; rect.anchoredPosition = position; rect.sizeDelta = size; }
        static void Label(Transform parent, string text, Vector3 position, Vector2 size, int fontSize, Color color, Quaternion? rotation = null)
        {
            var surface = Surface("Lettering / " + text.Replace('\n', ' '), parent, position, size * 1000, 0.001f);
            surface.localRotation = rotation ?? Quaternion.identity;
            TextUI("Label", surface, text, size * 500, size * 1000, fontSize, color);
        }
    }
}
