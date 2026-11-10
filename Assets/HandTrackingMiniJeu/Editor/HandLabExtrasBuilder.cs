using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SnakeArcade;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace HandTrackingMiniJeu.Editor
{
    public static class HandLabExtrasBuilder
    {
        static Font font;
        [MenuItem("Tools/Hand Tracking MiniJeu/Add Arcade and Finger Targets")]
        public static void AddExtras()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play mode first.");
            var monitor=UnityEngine.Object.FindAnyObjectByType<HandTrackingMonitor>();
            if(monitor==null)throw new InvalidOperationException("Open the HandTrackingMiniJeu scene first.");
            if(UnityEngine.Object.FindAnyObjectByType<FingerGunRange>()!=null)return;
            var destination=monitor.gameObject.scene;SceneManager.SetActiveScene(destination);
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var source=EditorSceneManager.OpenScene("Assets/SnakeArcade/Scenes/SnakeArcadeVR.unity",OpenSceneMode.Additive);
            var sourceMachine=source.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<SnakeArcadeMachine>(true)).FirstOrDefault();
            if(sourceMachine==null)throw new InvalidOperationException("Snake arcade cabinet not found in the source scene.");
            SceneManager.SetActiveScene(destination);
            var cabinet=UnityEngine.Object.Instantiate(sourceMachine.gameObject);cabinet.name="SNAKE / Arcade Cabinet";
            cabinet.transform.SetPositionAndRotation(new Vector3(1.95f,0,-0.05f),Quaternion.Euler(0,75,0));
            cabinet.GetComponent<SnakeArcadeMachine>().playerCamera=null;
            EditorSceneManager.CloseScene(source,true);
            SceneManager.SetActiveScene(destination);
            var interactionManager=UnityEngine.Object.FindObjectsByType<XRInteractionManager>(FindObjectsSortMode.None).First(m=>m.gameObject.scene==destination);
            foreach(var interactable in cabinet.GetComponentsInChildren<XRBaseInteractable>(true))interactable.interactionManager=interactionManager;
            var navy=AssetDatabase.LoadAssetAtPath<Material>("Assets/HandTrackingMiniJeu/Art/Midnight.mat");
            var metal=AssetDatabase.LoadAssetAtPath<Material>("Assets/HandTrackingMiniJeu/Art/Brushed graphite.mat");
            var amber=AssetDatabase.LoadAssetAtPath<Material>("Assets/HandTrackingMiniJeu/Art/Amber.mat");
            var white=AssetDatabase.LoadAssetAtPath<Material>("Assets/HandTrackingMiniJeu/Art/Porcelain.mat");
            var teal=AssetDatabase.LoadAssetAtPath<Material>("Assets/HandTrackingMiniJeu/Art/Teal.mat");
            var root=new GameObject("Finger gun / Far targets");
            var range=root.AddComponent<FingerGunRange>();range.trackingOrigin=monitor.trackingOrigin;range.playerCamera=monitor.playerCamera;
            range.speaker=root.AddComponent<AudioSource>();range.speaker.playOnAwake=false;range.speaker.spatialBlend=0.25f;
            int layer=TargetLayer();range.targetMask=1<<layer;range.targets=new FingerGunTarget[3];
            for(int i=0;i<3;i++)
            {
                var target=new GameObject("Finger target " + (i+1));target.transform.SetParent(root.transform,false);
                target.transform.position=new Vector3((i-1)*1.25f,1.50f,-4.75f);target.layer=layer;
                var collider=target.AddComponent<BoxCollider>();collider.size=new Vector3(0.70f,0.70f,0.08f);
                Disc(target.transform,"Outer ring / 2 points",0.70f,0,amber);
                Disc(target.transform,"Middle ring / 5 points",0.38f,0.023f,white);
                Disc(target.transform,"Bullseye / 10 points",0.15f,0.045f,teal);
                Cube(root.transform,"Target stand",new Vector3((i-1)*1.25f,0.65f,-4.80f),new Vector3(0.055f,1.3f,0.055f),metal);
                Cube(root.transform,"Target foot",new Vector3((i-1)*1.25f,0.055f,-4.80f),new Vector3(0.55f,0.11f,0.42f),metal);
                var score=target.AddComponent<FingerGunTarget>();score.targetNumber=i+1;range.targets[i]=score;
                score.hitText=Label(root.transform,"CIBLE "+(i+1).ToString("00")+"\nTOTAL 0",new Vector3((i-1)*1.25f,2.13f,-4.68f),new Vector2(1.1f,0.35f),42,Color.white);
            }
            Cube(root.transform,"Range floor",new Vector3(0,-0.07f,-4.6f),new Vector3(6,0.12f,3.2f),navy);
            Cube(root.transform,"Range back wall",new Vector3(0,1.6f,-5.7f),new Vector3(6,3.2f,0.12f),metal);
            range.totalText=Label(root.transform,"TIR AUX DOIGTS / SCORE 000",new Vector3(0,2.88f,-5.62f),new Vector2(4.8f,0.26f),55,new Color(0.2f,0.85f,0.8f));
            range.gestureText=Label(root.transform,"INDEX + MAJEUR TENDUS / LEVEZ PUIS BAISSEZ LE POUCE",new Vector3(0,2.57f,-5.62f),new Vector2(5.2f,0.23f),30,Color.white);
            Label(root.transform,"10 AU CENTRE / 5 AU MILIEU / 2 SUR LE BORD",new Vector3(0,0.36f,-5.62f),new Vector2(4.4f,0.25f),38,Color.white);
            var game=UnityEngine.Object.FindAnyObjectByType<TabletopSortingGame>();game.shootingRange=range;
            monitor.desktopViews=new Transform[3];
            monitor.desktopViews[0]=View("Desktop / Table",new Vector3(0,1.62f,-0.95f),Quaternion.Euler(9,0,0));
            monitor.desktopViews[1]=View("Desktop / Snake",cabinet.transform.TransformPoint(new Vector3(0,1.58f,-1.03f)),cabinet.transform.rotation*Quaternion.Euler(7,0,0));
            monitor.desktopViews[2]=View("Desktop / Finger targets",new Vector3(0,1.62f,-0.65f),Quaternion.Euler(0,180,0));
            EditorSceneManager.MarkSceneDirty(destination);if(!string.IsNullOrEmpty(destination.path))EditorSceneManager.SaveScene(destination);AssetDatabase.SaveAssets();
            Debug.Log("Snake arcade and three finger-gun targets added to the hand-tracking scene.");
        }
        static Transform View(string name,Vector3 position,Quaternion rotation)
        {var view=new GameObject(name).transform;view.SetPositionAndRotation(position,rotation);return view;}
        static void Disc(Transform parent,string name,float diameter,float depth,Material material)
        {var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=new Vector3(0,0,depth);go.transform.localRotation=Quaternion.Euler(90,0,0);go.transform.localScale=new Vector3(diameter,0.012f,diameter);go.GetComponent<Renderer>().sharedMaterial=material;UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());}
        static void Cube(Transform parent,string name,Vector3 position,Vector3 scale,Material material)
        {var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.position=position;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;}
        static Text Label(Transform parent,string text,Vector3 position,Vector2 size,int fontSize,Color color)
        {
            var go=new GameObject("Range label",typeof(RectTransform),typeof(Canvas));go.transform.SetParent(parent,false);go.transform.position=position;go.transform.rotation=Quaternion.Euler(0,180,0);go.transform.localScale=Vector3.one*0.001f;
            var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;((RectTransform)go.transform).sizeDelta=size*1000;
            var child=new GameObject("Score text",typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));child.transform.SetParent(go.transform,false);
            var label=child.GetComponent<Text>();label.font=font;label.text=text;label.fontSize=fontSize;label.color=color;label.alignment=TextAnchor.MiddleCenter;label.raycastTarget=false;((RectTransform)child.transform).sizeDelta=size*1000;return label;
        }
        static int TargetLayer()
        {
            int existing=LayerMask.NameToLayer("HandLabTargets");if(existing>=0)return existing;
            var asset=AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0];var serialized=new SerializedObject(asset);var layers=serialized.FindProperty("layers");
            for(int i=8;i<32;i++)if(string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue))
            {layers.GetArrayElementAtIndex(i).stringValue="HandLabTargets";serialized.ApplyModifiedProperties();return i;}
            throw new InvalidOperationException("No free physics layer for the target range.");
        }
    }
}
