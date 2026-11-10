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
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using Unity.XR.CoreUtils;

namespace HandTrackingMiniJeu.Editor
{
    public static class HandTrackingSceneBuilder
    {
        public const string ScenePath = "Assets/HandTrackingMiniJeu/Scenes/HandTrackingMiniJeu.unity";
        const string ArtPath = "Assets/HandTrackingMiniJeu/Art";
        static Font font;
        static Material navy, white, wood, metal, teal, orange, blue;
        static readonly Color Teal = new Color(0.20f, 0.85f, 0.80f);
        static readonly Color Orange = new Color(1f, 0.65f, 0.28f);
        static readonly Color Blue = new Color(0.38f, 0.59f, 1f);

        [MenuItem("Tools/Hand Tracking MiniJeu/Create Quest 3 Scene")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode first.");
            Directory.CreateDirectory("Assets/HandTrackingMiniJeu/Scenes"); Directory.CreateDirectory(ArtPath);
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var previous = SceneManager.GetSceneAt(i);
                if (!previous.isDirty) continue;
                Directory.CreateDirectory("Assets/HandTrackingMiniJeu/Backups");
                EditorSceneManager.SaveScene(previous, AssetDatabase.GenerateUniqueAssetPath("Assets/HandTrackingMiniJeu/Backups/" + previous.name + ".unity"), true);
            }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                var previous = SceneManager.GetSceneAt(i);
                if (previous != scene && previous.path == ScenePath) EditorSceneManager.CloseScene(previous, true);
            }
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            navy = Mat("Midnight", new Color(0.035f,0.065f,0.105f));
            white = Mat("Porcelain", new Color(0.77f,0.81f,0.82f));
            wood = Mat("Warm table", new Color(0.56f,0.43f,0.31f));
            metal = Mat("Brushed graphite", new Color(0.16f,0.20f,0.24f));
            teal = Mat("Teal", Teal); orange = Mat("Amber", Orange); blue = Mat("Blue", Blue);

            var environment = new GameObject("Hand Lab / Room").transform;
            Box(environment,"Floor",new Vector3(0,-0.07f,0),new Vector3(6,0.12f,6),navy);
            Box(environment,"Back wall",new Vector3(0,1.65f,1.5f),new Vector3(6,3.3f,0.15f),white);
            Box(environment,"Left wall",new Vector3(-3,1.65f,0),new Vector3(0.15f,3.3f,3),white);
            Box(environment,"Right wall",new Vector3(3,1.65f,0),new Vector3(0.15f,3.3f,3),white);
            Box(environment,"Back accent",new Vector3(0,0.17f,1.41f),new Vector3(5.8f,0.03f,0.02f),teal,false);
            for (int i=-4;i<=4;i++) Box(environment,"Floor inlay",new Vector3(i*0.6f,-0.006f,0),new Vector3(0.006f,0.005f,4.7f),metal,false);
            var light = new GameObject("Daylight").AddComponent<Light>(); light.type=LightType.Directional;
            light.transform.rotation=Quaternion.Euler(50,-25,0);light.intensity=1.7f;light.color=new Color(1,0.94f,0.86f);light.shadows=LightShadows.None;
            RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(0.60f,0.65f,0.72f);RenderSettings.fog=false;

            var lab = new GameObject("Hand Tracking MiniJeu");
            var game = lab.AddComponent<TabletopSortingGame>();
            var monitor = lab.AddComponent<HandTrackingMonitor>();
            var table = new GameObject("Grab table").transform;
            Box(table,"Rounded worktop",new Vector3(0,0.77f,0.15f),new Vector3(1.95f,0.09f,0.81f),wood);
            Box(table,"Work mat",new Vector3(0,0.818f,0.15f),new Vector3(1.85f,0.006f,0.74f),navy);
            for (int side=-1;side<=1;side+=2)
            {
                Box(table,"Leg",new Vector3(side*0.83f,0.36f,-0.16f),new Vector3(0.07f,0.72f,0.07f),metal);
                Box(table,"Leg",new Vector3(side*0.83f,0.36f,0.46f),new Vector3(0.07f,0.72f,0.07f),metal);
            }
            game.goals = new Transform[3]; var materials=new[]{teal,orange,blue}; var colors=new[]{Teal,Orange,Blue};
            for (int i=0;i<3;i++)
            {
                var goal=new GameObject("Target " + i).transform;goal.position=new Vector3((i-1)*0.59f,0.825f,0.34f);
                game.goals[i]=goal;
                Box(goal,"Tray base",Vector3.zero,new Vector3(0.41f,0.018f,0.30f),materials[i]);
                Box(goal,"Back rim",new Vector3(0,0.035f,0.145f),new Vector3(0.42f,0.07f,0.018f),materials[i]);
                Box(goal,"Left rim",new Vector3(-0.202f,0.035f,0),new Vector3(0.018f,0.07f,0.29f),materials[i]);
                Box(goal,"Right rim",new Vector3(0.202f,0.035f,0),new Vector3(0.018f,0.07f,0.29f),materials[i]);
                Label(goal,"ZONE " + (i+1),new Vector3(0,0.012f,0),new Vector2(0.34f,0.08f),23,new Color(0.03f,0.08f,0.12f),Quaternion.Euler(90,0,0));
            }
            game.pieces=new TabletopPiece[6];
            for (int i=0;i<6;i++) game.pieces[i]=Piece(i,new Vector3(-0.70f+i*0.28f,0.94f,-0.10f),materials[i%3]);
            var reset=new GameObject("RESET / Touch or select");reset.transform.position=new Vector3(0.77f,0.84f,0.14f);
            Box(reset.transform,"Reset cap",new Vector3(0,0.017f,0),new Vector3(0.18f,0.028f,0.085f),white,false);
            var resetCollider=reset.AddComponent<BoxCollider>();resetCollider.center=new Vector3(0,0.015f,0);resetCollider.size=new Vector3(0.20f,0.06f,0.11f);
            var resetBody=reset.AddComponent<Rigidbody>();resetBody.isKinematic=true;resetBody.useGravity=false;
            var button=reset.AddComponent<TabletopResetButton>();button.colliders.Clear();button.colliders.Add(resetCollider);button.game=game;button.cap=reset.transform.GetChild(0);
            Label(reset.transform,"REPLACER",new Vector3(0,0.034f,0),new Vector2(0.17f,0.05f),16,new Color(0.03f,0.08f,0.12f),Quaternion.Euler(90,0,0));

            Box(null,"Tracking screen frame",new Vector3(0,1.68f,1.02f),new Vector3(2.05f,1.23f,0.09f),metal);
            Box(null,"Screen support",new Vector3(0,0.56f,1.07f),new Vector3(0.09f,1.12f,0.09f),metal);
            var screen=Canvas("Hand tracking screen",null,new Vector3(0,1.68f,0.966f),new Vector2(1440,850),0.00135f);
            Panel(screen,"Background",new Vector2(720,425),new Vector2(1440,850),new Color(0.035f,0.065f,0.105f));
            Text(screen,"HAND LAB / QUEST 3",new Vector2(720,795),new Vector2(1350,65),43,Color.white);
            monitor.connectionText=Text(screen,"EN ATTENTE DU QUEST 3 / SUIVI DES MAINS",new Vector2(720,737),new Vector2(1330,40),23,new Color(0.65f,0.75f,0.84f));
            for(int i=0;i<2;i++)
            {
                float x=i==0?365:1075;
                Panel(screen,"Hand card",new Vector2(x,435),new Vector2(675,540),new Color(0.065f,0.105f,0.15f));
                Text(screen,i==0?"MAIN GAUCHE":"MAIN DROITE",new Vector2(x,665),new Vector2(620,45),30,i==0?Teal:Orange);
                var raw=new GameObject("Live skeleton " + i,typeof(RectTransform),typeof(RawImage)).GetComponent<RawImage>();raw.transform.SetParent(screen,false);
                Rect(raw.rectTransform,new Vector2(x,470),new Vector2(620,300));raw.color=Color.white;raw.raycastTarget=false;
                raw.texture=GridPreview();
                Text(screen,"VUE DU DESSUS / X-Z / REPERE JOUEUR",new Vector2(x,295),new Vector2(620,32),17,new Color(0.56f,0.68f,0.77f));
                var readout=Text(screen,"NON SUIVIE\nX  --     Y  --     Z  --\nPINCEMENT  --",new Vector2(x,230),new Vector2(630,105),23,new Color(0.65f,0.75f,0.84f));
                if(i==0){monitor.leftImage=raw;monitor.leftReadout=readout;}else{monitor.rightImage=raw;monitor.rightReadout=readout;}
            }
            game.scoreText=Text(screen,"OBJETS TRIES   0 / 6          SAISIES   0",new Vector2(720,115),new Vector2(1330,55),32,Teal);
            game.hintText=Text(screen,"PINCEZ POUR SAISIR / DEPOSEZ DANS LA ZONE DE MEME COULEUR",new Vector2(720,57),new Vector2(1370,55),22,Color.white);
            game.speaker=lab.AddComponent<AudioSource>();game.speaker.playOnAwake=false;game.speaker.spatialBlend=0.7f;
            game.speaker.minDistance=0.5f;game.speaker.maxDistance=5;

            var rigPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Samples/XR Interaction Toolkit/3.5.1/Hands Interaction Demo/Prefabs/XR Origin Hands (XR Rig).prefab");
            if(rigPrefab==null)throw new InvalidOperationException("XR Hands rig prefab missing.");
            var rig=(GameObject)PrefabUtility.InstantiatePrefab(rigPrefab,scene);rig.name="Quest 3 / Hands + controllers";
            rig.transform.position=new Vector3(0,0,-0.65f);
            var origin=rig.GetComponentInChildren<XROrigin>(true);origin.RequestedTrackingOriginMode=XROrigin.TrackingOriginMode.Floor;
            origin.Camera.nearClipPlane=0.04f;origin.Camera.farClipPlane=25;origin.Camera.clearFlags=CameraClearFlags.SolidColor;origin.Camera.backgroundColor=new Color(0.04f,0.06f,0.09f);
            monitor.trackingOrigin=origin.transform;monitor.playerCamera=origin.Camera;
            foreach(var provider in rig.GetComponentsInChildren<LocomotionProvider>(true))provider.enabled=false;
            if(UnityEngine.Object.FindObjectsByType<XRInteractionManager>(FindObjectsSortMode.None).All(m=>m.gameObject.scene!=scene))new GameObject("XR Interaction Manager").AddComponent<XRInteractionManager>();
            if(rig.GetComponentInChildren<InputActionManager>(true)==null)
            {var input=rig.AddComponent<InputActionManager>();input.actionAssets.Add(AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets/XRI Default Input Actions.inputactions"));}
            Label(null,"PRENEZ LE TEMPS\nDE PRENDRE EN MAIN.",new Vector3(-1.9f,1.75f,1.405f),new Vector2(1.35f,0.60f),38,new Color(0.10f,0.17f,0.23f));
            Label(null,"01  MONTREZ VOS MAINS\n02  PINCEZ UN OBJET\n03  TRIEZ PAR COULEUR",new Vector3(1.9f,1.7f,1.405f),new Vector2(1.35f,0.60f),25,new Color(0.10f,0.17f,0.23f));
            Label(table,"ZONE DE MANIPULATION / LES OBJETS TOMBES REVIENNENT SUR LA TABLE",new Vector3(0,0.724f,-0.263f),new Vector2(1.65f,0.06f),17,Color.white);

            ConfigureQuest();
            HandLabExtrasBuilder.AddExtras();
            if(!EditorSceneManager.SaveScene(scene,ScenePath))throw new InvalidOperationException("Could not save the new scene.");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)}.Concat(EditorBuildSettings.scenes.Where(s=>s.path!=ScenePath)).ToArray();
            AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            Selection.activeGameObject=UnityEngine.Object.FindFirstObjectByType<HandTrackingMonitor>().gameObject;
            SceneView.lastActiveSceneView?.LookAt(new Vector3(0,1.15f,0.25f),Quaternion.Euler(12,0,0),3.2f);
            File.WriteAllText("Tools/Integrations/hand-scene-built.json","{\"success\":true,\"scene\":\""+ScenePath+"\",\"objects\":6}");
            Debug.Log("Quest 3 Hand Tracking MiniJeu scene created.");
        }

        static void ConfigureQuest()
        {
            var config=OVRProjectConfig.CachedProjectConfig;config.handTrackingSupport=OVRProjectConfig.HandTrackingSupport.ControllersAndHands;
            OVRProjectConfig.CommitProjectConfig(config);
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel34;
            PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android,"com.baptisteauvert.handtrackingminijeu");
        }
        static TabletopPiece Piece(int index,Vector3 position,Material material)
        {
            var go=new GameObject("Grab object " + (index+1));go.transform.position=position;
            int shape=index%3;var type=shape==0?PrimitiveType.Cube:shape==1?PrimitiveType.Sphere:PrimitiveType.Cylinder;
            var mesh=GameObject.CreatePrimitive(type);mesh.name="Visual";mesh.transform.SetParent(go.transform,false);
            mesh.transform.localScale=shape==2?new Vector3(0.11f,0.065f,0.11f):Vector3.one*0.12f;
            mesh.GetComponent<Renderer>().sharedMaterial=material;UnityEngine.Object.DestroyImmediate(mesh.GetComponent<Collider>());
            Collider collider;
            if(shape==1){var c=go.AddComponent<SphereCollider>();c.radius=0.06f;collider=c;}
            else{var c=go.AddComponent<BoxCollider>();c.size=shape==2?new Vector3(0.11f,0.13f,0.11f):Vector3.one*0.12f;collider=c;}
            var body=go.AddComponent<Rigidbody>();body.mass=0.15f;body.interpolation=RigidbodyInterpolation.Interpolate;body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
            var grab=go.AddComponent<XRGrabInteractable>();grab.colliders.Clear();grab.colliders.Add(collider);
            grab.movementType=XRBaseInteractable.MovementType.VelocityTracking;grab.useDynamicAttach=true;grab.matchAttachPosition=true;grab.matchAttachRotation=true;
            grab.smoothPosition=true;grab.smoothRotation=true;grab.attachEaseInTime=0.08f;grab.throwOnDetach=true;
            var piece=go.AddComponent<TabletopPiece>();piece.pieceIndex=index;piece.colorIndex=index%3;return piece;
        }
        static Texture2D GridPreview()
        {
            string path=ArtPath+"/TrackingGrid.asset";var existing=AssetDatabase.LoadAssetAtPath<Texture2D>(path);if(existing!=null)return existing;
            var texture=new Texture2D(256,256,TextureFormat.RGBA32,false){name="Tracking grid",filterMode=FilterMode.Bilinear};var pixels=new Color32[65536];
            for(int y=0;y<256;y++)for(int x=0;x<256;x++)pixels[y*256+x]=(x%32==0||y%32==0)?new Color32(25,44,60,255):new Color32(12,24,37,255);
            texture.SetPixels32(pixels);texture.Apply();AssetDatabase.CreateAsset(texture,path);return texture;
        }
        static Material Mat(string name,Color color)
        {string path=ArtPath+"/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(mat!=null)return mat;mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};mat.SetColor("_BaseColor",color);mat.SetFloat("_Smoothness",0.25f);AssetDatabase.CreateAsset(mat,path);return mat;}
        static GameObject Box(Transform parent,string name,Vector3 position,Vector3 scale,Material material,bool collider=true)
        {var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;if(!collider)UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());return go;}
        static RectTransform Canvas(string name,Transform parent,Vector3 position,Vector2 size,float scale)
        {var go=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.Canvas));go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=Vector3.one*scale;go.GetComponent<UnityEngine.Canvas>().renderMode=RenderMode.WorldSpace;var rect=(RectTransform)go.transform;rect.sizeDelta=size;return rect;}
        static void Rect(RectTransform rect,Vector2 position,Vector2 size){rect.anchorMin=rect.anchorMax=Vector2.zero;rect.pivot=Vector2.one*0.5f;rect.anchoredPosition=position;rect.sizeDelta=size;}
        static Text Text(Transform parent,string value,Vector2 position,Vector2 size,int fontSize,Color color)
        {var go=new GameObject(value.Replace('\n',' '),typeof(RectTransform),typeof(CanvasRenderer),typeof(UnityEngine.UI.Text));go.transform.SetParent(parent,false);var text=go.GetComponent<UnityEngine.UI.Text>();text.font=font;text.text=value;text.fontSize=fontSize;text.color=color;text.alignment=TextAnchor.MiddleCenter;text.raycastTarget=false;Rect(text.rectTransform,position,size);return text;}
        static void Panel(Transform parent,string name,Vector2 position,Vector2 size,Color color)
        {var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);var image=go.GetComponent<Image>();image.color=color;image.raycastTarget=false;Rect(image.rectTransform,position,size);}
        static void Label(Transform parent,string text,Vector3 position,Vector2 size,int fontSize,Color color,Quaternion? rotation=null)
        {var canvas=Canvas(text,parent,position,size*1000,0.001f);canvas.localRotation=rotation??Quaternion.identity;Text(canvas,text,size*500,size*1000,fontSize,color);}
    }
}
