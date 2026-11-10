using UnityEngine;

namespace HandTrackingMiniJeu
{
    // A local, procedural arm cannon: no colliders or interaction components can obstruct grabbing.
    public sealed class HandCannonVisual
    {
        readonly GameObject root, model, flash;
        readonly Material shell, metal, energy;
        readonly LineRenderer sight, reticle;
        bool visible;
        float flashUntil;
        public bool Visible => visible;
        public Vector3 MuzzlePosition => root.transform.TransformPoint(new Vector3(0,0,0.19f));
        public Vector3 Direction => root.transform.forward;

        public HandCannonVisual(Transform parent,int side)
        {
            shell=Material("Universal Render Pipeline/Lit",new Color(0.95f,0.39f,0.09f));
            metal=Material("Universal Render Pipeline/Lit",new Color(0.08f,0.19f,0.17f));
            energy=Material("Universal Render Pipeline/Unlit",Color.cyan);
            root=new GameObject(side==0?"Left hand / Energy cannon":"Right hand / Energy cannon");root.transform.SetParent(parent,false);
            model=new GameObject("Arm cannon shell");model.transform.SetParent(root.transform,false);
            Cylinder("Arm cuff",0.083f,0.06f,-0.055f,shell);
            Cylinder("Orange armor",0.072f,0.14f,0.025f,shell);
            Cylinder("Green barrel",0.060f,0.075f,0.125f,metal);
            Cylinder("Muzzle rim",0.066f,0.018f,0.169f,shell);
            Cylinder("Energy aperture",0.040f,0.004f,0.181f,energy);
            for(int i=0;i<4;i++)
            {
                var rib=Shape(PrimitiveType.Cube,"Energy rail",energy);
                float angle=i*Mathf.PI*0.5f;rib.transform.localPosition=new Vector3(Mathf.Cos(angle)*0.036f,Mathf.Sin(angle)*0.036f,0.045f);
                rib.transform.localScale=new Vector3(0.009f,0.009f,0.09f);
            }
            flash=Shape(PrimitiveType.Sphere,"Muzzle flash",energy);flash.transform.localPosition=new Vector3(0,0,0.21f);flash.transform.localScale=Vector3.one*0.075f;flash.SetActive(false);
            sight=Line("Cannon aiming beam",parent,0.0025f);reticle=Line("Cannon target reticle",parent,0.0035f);reticle.loop=true;
            Hide();
        }
        static Material Material(string shader,Color color)
        {
            var material=new Material(Shader.Find(shader));material.SetColor("_BaseColor",color);
            if(material.HasProperty("_Metallic")){material.SetFloat("_Metallic",0.55f);material.SetFloat("_Smoothness",0.65f);}return material;
        }
        GameObject Shape(PrimitiveType type,string name,Material material)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(model.transform,false);
            var collider=go.GetComponent<Collider>();collider.enabled=false;Object.Destroy(collider);
            go.GetComponent<Renderer>().sharedMaterial=material;return go;
        }
        void Cylinder(string name,float diameter,float length,float z,Material material)
        {
            var go=Shape(PrimitiveType.Cylinder,name,material);go.transform.localPosition=new Vector3(0,0,z);
            go.transform.localRotation=Quaternion.Euler(90,0,0);go.transform.localScale=new Vector3(diameter,length*0.5f,diameter);
        }
        LineRenderer Line(string name,Transform parent,float width)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);var line=go.AddComponent<LineRenderer>();
            line.sharedMaterial=energy;line.startWidth=width;line.endWidth=width;line.useWorldSpace=true;return line;
        }
        public void Show(Vector3 position,Vector3 direction,Vector3 up,bool armed,LayerMask targets)
        {
            var rotation=Quaternion.LookRotation(direction,up);
            float blend=visible?1-Mathf.Exp(-24*Time.unscaledDeltaTime):1;
            root.transform.position=Vector3.Lerp(root.transform.position,position,blend);
            root.transform.rotation=Quaternion.Slerp(root.transform.rotation,rotation,blend);
            visible=true;root.SetActive(true);
            Color color=armed?new Color(0.2f,1,0.45f):new Color(1,0.65f,0.12f);energy.SetColor("_BaseColor",color);
            var ray=new Ray(MuzzlePosition,Direction);Vector3 end=ray.GetPoint(12);
            bool hit=Physics.Raycast(ray,out var impact,20,targets,QueryTriggerInteraction.Ignore);
            if(hit)end=impact.point;
            sight.enabled=true;sight.positionCount=2;sight.SetPosition(0,ray.origin);sight.SetPosition(1,end);
            reticle.enabled=hit;
            if(hit)
            {
                reticle.positionCount=32;var axis=Vector3.Cross(impact.normal,Vector3.up).normalized;
                if(axis.sqrMagnitude<0.01f)axis=Vector3.right;
                var second=Vector3.Cross(impact.normal,axis);
                for(int i=0;i<32;i++){float angle=i*Mathf.PI*2/32;reticle.SetPosition(i,end+impact.normal*0.007f+0.033f*(Mathf.Cos(angle)*axis+Mathf.Sin(angle)*second));}
            }
            flash.SetActive(Time.time<flashUntil);
            model.transform.localPosition=Vector3.back*(Time.time<flashUntil?0.014f:0);
        }
        public void Fire(){flashUntil=Time.time+0.10f;flash.SetActive(true);}
        public void Hide(){visible=false;root.SetActive(false);sight.enabled=false;reticle.enabled=false;}
        public void Dispose(){Object.Destroy(root);Object.Destroy(sight.gameObject);Object.Destroy(reticle.gameObject);Object.Destroy(shell);Object.Destroy(metal);Object.Destroy(energy);}
    }
}
