using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SnakeArcade.Editor
{
    public static class SnakeArcadeChecks
    {
        static void Require(bool condition, string message)
        { if (!condition) throw new InvalidOperationException("Snake Arcade verification: " + message); }

        [MenuItem("Tools/Snake Arcade/Check Game Rules")]
        public static void CheckRules()
        {
            var game = new SnakeGameModel(seed: 42);
            Require(game.State == SnakeState.Ready && !game.Tick(), "must wait for Start");
            game.StartRound();
            Require(!game.QueueDirection(Vector2Int.left), "must reject a reverse direction");
            Require(game.QueueDirection(Vector2Int.up), "must accept a legal turn");
            Require(!game.QueueDirection(Vector2Int.left), "only one direction can be queued per grid step");
            Vector2Int head = game.Body[0]; game.Tick();
            Require(game.Body[0] == head + Vector2Int.up, "queued turn must execute");
            for (int i = 0; i < 30; i++) game.Tick();
            Require(game.State == SnakeState.GameOver, "walls must end a round");
            game.StartRound(); Require(game.State == SnakeState.Playing && game.Score == 0 && game.Body.Count == 4, "restart must reset a round");

            // Walk a shortest collision-free path to a real seeded food spawn.
            var path = PathToFood(game);
            int initialLength = game.Body.Count;
            foreach (var direction in path)
            { game.QueueDirection(direction); Require(game.Tick(), "food path should be legal"); }
            Require(game.Score == 1 && game.Body.Count == initialLength + 1, "eating must grow the snake and award a point");
            Require(!Contains(game.Body, game.Food), "food cannot spawn inside the snake");
            // A four-step loop must collide with the body after growing to length five.
            var heading = game.Direction;
            var perpendicular = new Vector2Int(-heading.y, heading.x);
            var loop = new[] { perpendicular, -heading, -perpendicular, heading };
            var position = game.Body[0]; bool inBounds = true;
            foreach (var d in loop) { position += d; inBounds &= position.x >= 0 && position.x < game.Width && position.y >= 0 && position.y < game.Height; }
            if (!inBounds) loop = new[] { -perpendicular, -heading, perpendicular, heading };
            bool collidedWithBody = false;
            foreach (var d in loop)
            {
                var next = game.Body[0] + d;
                Require(next.x >= 0 && next.x < game.Width && next.y >= 0 && next.y < game.Height, "self-collision test must stay inside the arena");
                game.QueueDirection(d);
                if (!game.Tick()) { collidedWithBody = true; break; }
            }
            Require(collidedWithBody && game.State == SnakeState.GameOver, "body collision must end a round");
            Require(SnakeArcadeJoystick.Cardinal(new Vector2(0.9f, 0.2f)) == Vector2Int.right, "joystick horizontal gate");
            Require(SnakeArcadeJoystick.Cardinal(new Vector2(-0.1f, -0.9f)) == Vector2Int.down, "joystick vertical gate");
            File.WriteAllText("Tools/Integrations/snake-rules-check.json", "{\"success\":true,\"checks\":[\"start_gate\",\"reverse_rejected\",\"one_turn_per_tick\",\"movement\",\"wall_collision\",\"restart\",\"food_growth\",\"food_spawn\",\"body_collision\",\"joystick_mapping\"]}");
            Debug.Log("Snake Arcade rules: all checks passed.");
        }

        [MenuItem("Tools/Snake Arcade/Check Physical Start Button")]
        public static void CheckPhysicalButton()
        {
            Require(EditorApplication.isPlaying, "enter Play mode first");
            var machine = UnityEngine.Object.FindFirstObjectByType<SnakeArcadeMachine>();
            var button = machine.startButton;
            var point = button.transform.TransformPoint(new Vector3(0, button.surfaceHeight, 0));
            Physics.SyncTransforms();
            bool hittable = false;
            foreach (var collider in Physics.OverlapSphere(point, 0.015f, ~0, QueryTriggerInteraction.Ignore))
                if (button.colliders.Contains(collider)) hittable = true;
            Require(hittable, "Start must be detectable by the default non-trigger poke physics query");
            var probe = new GameObject("Temporary poke verification");
            probe.transform.position = point;
            var poke = probe.AddComponent<XRPokeInteractor>();
            var manager = UnityEngine.Object.FindFirstObjectByType<XRInteractionManager>();
            poke.interactionManager = manager; poke.attachTransform = probe.transform;
            int before = button.PressCount;
            try
            {
                manager.HoverEnter((IXRHoverInteractor)poke, (IXRHoverInteractable)button);
                button.ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase.Dynamic);
                Require(button.PressCount == before + 1, "fingertip contact must activate Start");
                button.ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase.Dynamic);
                Require(button.PressCount == before + 1, "holding the finger down must not retrigger Start");
                File.WriteAllText("Tools/Integrations/snake-poke-check.json", "{\"success\":true,\"checks\":[\"poke_physics_hit\",\"fingertip_start\",\"contact_debounce\"]}");
                Debug.Log("Snake Arcade physical Start button: all checks passed.");
            }
            finally
            {
                manager.HoverExit((IXRHoverInteractor)poke, (IXRHoverInteractable)button);
                UnityEngine.Object.Destroy(probe);
            }
        }

        static bool Contains(IReadOnlyList<Vector2Int> body, Vector2Int position)
        { for (int i = 0; i < body.Count; i++) if (body[i] == position) return true; return false; }
        static List<Vector2Int> PathToFood(SnakeGameModel game)
        {
            var queue = new Queue<Vector2Int>(); var previous = new Dictionary<Vector2Int, Vector2Int>();
            var start = game.Body[0]; queue.Enqueue(start); previous[start] = start;
            var directions = new[] { Vector2Int.right, Vector2Int.up, Vector2Int.down, Vector2Int.left };
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == game.Food) break;
                foreach (var d in directions)
                {
                    var next = current + d;
                    if (next.x < 0 || next.x >= game.Width || next.y < 0 || next.y >= game.Height || previous.ContainsKey(next) || Contains(game.Body, next)) continue;
                    previous[next] = current; queue.Enqueue(next);
                }
            }
            Require(previous.ContainsKey(game.Food), "food must be reachable in initial state");
            var result = new List<Vector2Int>(); var cell = game.Food;
            while (cell != start) { var before = previous[cell]; result.Add(cell - before); cell = before; }
            result.Reverse(); return result;
        }

        [MenuItem("Tools/Snake Arcade/Check Live VR Controls")]
        public static void CheckLiveControls()
        {
            Require(EditorApplication.isPlaying, "enter Play mode first");
            var machine = UnityEngine.Object.FindFirstObjectByType<SnakeArcadeMachine>();
            Require(machine != null && machine.Game != null, "machine must initialize in Play");
            var manager = UnityEngine.Object.FindFirstObjectByType<XRInteractionManager>();
            Require(manager != null && machine.playerCamera != null, "XR rig and manager required");
            Require(machine.board.texture != null, "live screen must have a texture");
            var probe = new GameObject("Temporary XR control verification");
            var trigger = probe.AddComponent<SphereCollider>(); trigger.isTrigger = true; trigger.radius = 0.015f;
            var body = probe.AddComponent<Rigidbody>(); body.isKinematic = true; body.useGravity = false;
            var hand = probe.AddComponent<XRDirectInteractor>(); hand.interactionManager = manager;
            var attach = new GameObject("Test grip attach").transform; attach.SetParent(probe.transform, false); hand.attachTransform = attach;
            try
            {
                probe.transform.position = machine.startButton.transform.position;
                hand.StartManualInteraction((IXRSelectInteractable)machine.startButton);
                Require(machine.startButton.PressCount > 0 && machine.Game.State == SnakeState.Playing, "XR selection must activate Start");
                hand.EndManualInteraction();
                probe.transform.position = machine.joystick.transform.position + Vector3.up * 0.14f;
                hand.StartManualInteraction((IXRSelectInteractable)machine.joystick);
                Require(machine.joystick.isSelected, "joystick must be selectable by an XR hand/controller");
                attach.position += Vector3.forward * 0.055f;
                machine.joystick.ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase.Dynamic);
                machine.Game.Tick();
                Require(machine.Game.Direction == Vector2Int.up, "moving the grabbed joystick forward must turn up");
                hand.EndManualInteraction();
                machine.joystick.ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase.Dynamic);
                Require(!machine.joystick.isSelected, "releasing must end joystick selection");
                File.WriteAllText("Tools/Integrations/snake-live-check.json", "{\"success\":true,\"checks\":[\"runtime_screen\",\"xr_start_select\",\"xr_joystick_grab\",\"xr_joystick_movement\",\"xr_joystick_release\"]}");
                Debug.Log("Snake Arcade live XR controls: all checks passed.");
            }
            finally
            {
                if (hand.hasSelection) hand.EndManualInteraction();
                UnityEngine.Object.Destroy(probe);
            }
        }

        [MenuItem("Tools/Snake Arcade/Render Cabinet Preview")]
        public static void RenderPreview()
        {
            var go = new GameObject("Temporary preview camera"); var camera = go.AddComponent<Camera>();
            camera.transform.position = new Vector3(-1.9f, 1.9f, -3.25f);
            camera.transform.LookAt(new Vector3(0, 1.13f, 0)); camera.fieldOfView = 45;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.015f, 0.022f, 0.04f);
            var target = new RenderTexture(1200, 1200, 24); var previous = RenderTexture.active;
            Texture2D image = null;
            try
            {
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                image = new Texture2D(1200, 1200, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, 1200, 1200), 0, 0); image.Apply();
                File.WriteAllBytes("Tools/Integrations/SnakeArcade-Preview.png", image.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous; camera.targetTexture = null; target.Release();
                UnityEngine.Object.DestroyImmediate(target); if (image != null) UnityEngine.Object.DestroyImmediate(image); UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
