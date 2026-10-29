using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Cibio.City.Editor
{
    public static class HeroAnimations
    {
        public const string Folder = "Assets/Cibio/City/Animations";
        private static readonly Dictionary<string,string> Paths = new Dictionary<string,string>
        {
            {"Hips","Hips"},{"Spine","Hips/Spine"},{"Chest","Hips/Spine/Chest"},{"Neck","Hips/Spine/Chest/Neck"},{"Head","Hips/Spine/Chest/Neck/Head"},
            {"LUpperArm","Hips/Spine/Chest/LUpperArm"},{"LForearm","Hips/Spine/Chest/LUpperArm/LForearm"},{"LHand","Hips/Spine/Chest/LUpperArm/LForearm/LHand"},
            {"RUpperArm","Hips/Spine/Chest/RUpperArm"},{"RForearm","Hips/Spine/Chest/RUpperArm/RForearm"},{"RHand","Hips/Spine/Chest/RUpperArm/RForearm/RHand"},
            {"LUpperLeg","Hips/LUpperLeg"},{"LLowerLeg","Hips/LUpperLeg/LLowerLeg"},{"LFoot","Hips/LUpperLeg/LLowerLeg/LFoot"},
            {"RUpperLeg","Hips/RUpperLeg"},{"RLowerLeg","Hips/RUpperLeg/RLowerLeg"},{"RFoot","Hips/RUpperLeg/RLowerLeg/RFoot"}
        };
        public sealed class Pose
        {
            public float hipY=.96f;
            public readonly Dictionary<string,Vector3> rotations = new Dictionary<string,Vector3>();
            public Pose() { foreach(var bone in Paths.Keys)rotations[bone]=Vector3.zero; }
            public void Set(string bone,float x=0,float y=0,float z=0){rotations[bone]=new Vector3(x,y,z);}
        }

        public static AnimatorController Build()
        {
            var idle=Clip("Idle",2.8f,true,(t)=>PoseFor("Idle",t));
            var walk=Clip("Walk",.70f,true,(t)=>PoseFor("Walk",t));
            var run=Clip("Run",.56f,true,(t)=>PoseFor("Run",t));
            var rise=Clip("Jump",.6f,true,(t)=>PoseFor("Jump",t));
            var fall=Clip("Fall",.6f,true,(t)=>PoseFor("Fall",t));
            var land=Clip("Land",.24f,false,(t)=>PoseFor("Land",t));
            var attack=Clip("Attack",.55f,false,(t)=>PoseFor("Attack",t));
            var death=Clip("Death",.85f,false,(t)=>PoseFor("Death",t));
            string path=Folder+"/Brasileiro.controller";
            if(AssetDatabase.LoadAssetAtPath<AnimatorController>(path))AssetDatabase.DeleteAsset(path);
            var controller=AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("Speed",AnimatorControllerParameterType.Float);
            controller.AddParameter("Vertical",AnimatorControllerParameterType.Float);
            controller.AddParameter("Grounded",AnimatorControllerParameterType.Bool);
            controller.AddParameter("AttackRate",AnimatorControllerParameterType.Float);var parameters=controller.parameters;for(int i=0;i<parameters.Length;i++)if(parameters[i].name=="AttackRate")parameters[i].defaultFloat=1;controller.parameters=parameters;
            controller.AddParameter("Attack",AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Death",AnimatorControllerParameterType.Trigger);
            var sm=controller.layers[0].stateMachine;
            var locomotion=sm.AddState("Locomotion",new Vector3(250,100));
            var locomotionTree=new BlendTree{name="Idle • Walk • Run",blendType=BlendTreeType.Simple1D,blendParameter="Speed",useAutomaticThresholds=false};
            AssetDatabase.AddObjectToAsset(locomotionTree,controller);
            locomotionTree.AddChild(idle,0);locomotionTree.AddChild(walk,2.3f);locomotionTree.AddChild(run,4.6f);locomotion.motion=locomotionTree;
            sm.defaultState=locomotion;
            var airborne=sm.AddState("Airborne",new Vector3(520,100));
            var airTree=new BlendTree{name="Rise • Fall",blendType=BlendTreeType.Simple1D,blendParameter="Vertical",useAutomaticThresholds=false};
            AssetDatabase.AddObjectToAsset(airTree,controller);airTree.AddChild(fall,-1);airTree.AddChild(rise,2);airborne.motion=airTree;
            var landing=sm.AddState("Landing",new Vector3(520,230));landing.motion=land;
            var attacking=sm.AddState("Attack",new Vector3(250,280));attacking.motion=attack;attacking.speedParameter="AttackRate";attacking.speedParameterActive=true;
            var dead=sm.AddState("Defeated",new Vector3(20,280));dead.motion=death;
            Transition(locomotion,airborne,.10f,false).AddCondition(AnimatorConditionMode.IfNot,0,"Grounded");
            Transition(airborne,landing,.07f,false).AddCondition(AnimatorConditionMode.If,0,"Grounded");
            Transition(landing,locomotion,.12f,true);
            Transition(attacking,locomotion,.15f,true);
            var toAttack=sm.AddAnyStateTransition(attacking);toAttack.duration=.09f;toAttack.hasExitTime=false;toAttack.canTransitionToSelf=false;toAttack.AddCondition(AnimatorConditionMode.If,0,"Attack");
            var toDeath=sm.AddAnyStateTransition(dead);toDeath.duration=.12f;toDeath.hasExitTime=false;toDeath.canTransitionToSelf=false;toDeath.AddCondition(AnimatorConditionMode.If,0,"Death");
            EditorUtility.SetDirty(controller);return controller;
        }
        private static AnimatorStateTransition Transition(AnimatorState a,AnimatorState b,float duration,bool exit)
        {var transition=a.AddTransition(b);transition.duration=duration;transition.hasExitTime=exit;transition.exitTime=.9f;return transition;}

        public static Pose PoseFor(string kind,float t)
        {
            var p=new Pose();float wave=Mathf.Sin(t*Mathf.PI*2);
            p.Set("Spine",-1f,0,wave*.5f);p.Set("Chest",2f);p.Set("Neck",-2);
            p.Set("Head",0,wave*1.2f,0);
            p.Set("LUpperArm",-3,0,-4);p.Set("RUpperArm",-3,0,4);
            p.Set("LForearm",-12);p.Set("RForearm",-12);
            p.hipY=.96f+wave*.0035f;
            if(kind=="Walk"||kind=="Run")
            {
                bool sprint=kind=="Run";
                p.hipY=(sprint?.874f:.902f)+Mathf.Cos(t*Mathf.PI*4)*(sprint?.031f:.012f);
                p.Set("Hips",0,Mathf.Sin(t*Mathf.PI*2)*(sprint?6f:4f),Mathf.Sin(t*Mathf.PI*2)*(sprint?2:1.4f));
                p.Set("Spine",sprint?9:3,Mathf.Sin(t*Mathf.PI*2)*(sprint?-7f:-4f));
                p.Set("Chest",sprint?3:1,Mathf.Sin(t*Mathf.PI*2)*(sprint?-8f:-5f));
                p.Set("Neck",sprint?-8:-3);p.Set("Head",0,-Mathf.Sin(t*Mathf.PI*2)*2);
                for(int side=0;side<2;side++)
                {
                    string name=side==0?"L":"R";float phase=Mathf.Repeat(t+side*.5f,1);float angle=phase*Mathf.PI*2;
                    float z,lift;
                    if(sprint)
                    {
                        float stance=.29f;
                        if(phase<stance){z=Mathf.Lerp(.39f,-.39f,phase/stance);lift=0;}
                        else{float swing=(phase-stance)/(1-stance);z=Mathf.Lerp(-.39f,.39f,Mathf.SmoothStep(0,1,swing));lift=Mathf.Sin(swing*Mathf.PI)*.30f;}
                    }
                    else{z=Mathf.Cos(angle)*.34f;lift=Mathf.Max(0,Mathf.Sin(angle))*.12f;}
                    LegIK(p,name,z,.12f+lift);
                    float armSwing=Mathf.Sin(angle)*(sprint?46f:24f);
                    p.Set(name+"UpperArm",armSwing-(sprint?8:2),0,side==0?-5:5);
                    p.Set(name+"Forearm",-(sprint?65:16)-Mathf.Max(0,-armSwing)*.3f);
                    p.Set(name+"Hand",0,side==0?-5:5,0);
                }
            }
            if(kind=="Jump"||kind=="Fall")
            {
                p.hipY=.92f;p.Set("Spine",kind=="Jump"?7:-3);p.Set("Chest",-3);p.Set("Neck",-4);
                p.Set("LUpperLeg",-24);p.Set("LLowerLeg",42);p.Set("LFoot",-12);
                p.Set("RUpperLeg",-38);p.Set("RLowerLeg",62);p.Set("RFoot",-18);
                p.Set("LUpperArm",kind=="Jump"?-45:-26,0,-18);p.Set("RUpperArm",kind=="Jump"?-45:-26,0,18);
                p.Set("LForearm",-32);p.Set("RForearm",-32);
            }
            if(kind=="Land")
            {
                float crouch=Mathf.Sin(t*Mathf.PI);
                p.hipY=.96f-crouch*.13f;
                LegIK(p,"L",.035f,.12f);LegIK(p,"R",-.035f,.12f);
                p.Set("Spine",crouch*10);p.Set("LUpperArm",-12*crouch,0,-5);p.Set("RUpperArm",-12*crouch,0,5);
            }
            if(kind=="Attack")
            {
                float wind=Mathf.Sin(Mathf.Clamp01(t/.38f)*Mathf.PI*.5f);
                float punch=t<.28f?Mathf.SmoothStep(0,1,t/.28f):1-Mathf.SmoothStep(0,1,(t-.28f)/.72f);
                p.hipY=.93f;p.Set("Hips",0,-10+25*punch);p.Set("Spine",4+6*punch,0);p.Set("Chest",0,-20+48*punch);
                p.Set("RUpperArm",-22-68*punch,0,6);p.Set("RForearm",-65+60*punch);
                p.Set("LUpperArm",-35,0,-10);p.Set("LForearm",-70);
                LegIK(p,"L",.08f,.12f);LegIK(p,"R",-.09f,.12f);p.Set("Head",0,-punch*12);
            }
            if(kind=="Death")
            {
                float fall=Mathf.SmoothStep(0,1,Mathf.Clamp01(t/.75f));
                p.hipY=.96f-fall*.71f;p.Set("Hips",0,0,fall*84);
                p.Set("LUpperLeg",-15*fall);p.Set("LLowerLeg",35*fall);p.Set("RUpperLeg",10*fall);
                p.Set("LUpperArm",-25*fall,0,-30*fall);p.Set("RUpperArm",-20*fall,0,20*fall);
            }
            return p;
        }
        private static void LegIK(Pose pose,string side,float forward,float footY)
        {
            const float thigh=.41f,shin=.40f;
            float down=pose.hipY-.03f-footY;
            float d=Mathf.Clamp(Mathf.Sqrt(down*down+forward*forward),.08f,thigh+shin-.001f);
            float direction=Mathf.Atan2(forward,down)*Mathf.Rad2Deg;
            float hip=-direction-Mathf.Acos(Mathf.Clamp((thigh*thigh+d*d-shin*shin)/(2*thigh*d),-1,1))*Mathf.Rad2Deg;
            float knee=180-Mathf.Acos(Mathf.Clamp((thigh*thigh+shin*shin-d*d)/(2*thigh*shin),-1,1))*Mathf.Rad2Deg;
            pose.Set(side+"UpperLeg",hip);pose.Set(side+"LowerLeg",knee);pose.Set(side+"Foot",-hip-knee);
        }
        private static AnimationClip Clip(string name,float duration,bool loop,Func<float,Pose> evaluate)
        {
            var clip=new AnimationClip{name=name,frameRate=60};int samples=Mathf.CeilToInt(duration*60);
            var curves=new Dictionary<string,AnimationCurve[]>();foreach(string bone in Paths.Keys)curves[bone]=new[]{new AnimationCurve(),new AnimationCurve(),new AnimationCurve(),new AnimationCurve()};
            var height=new AnimationCurve();
            for(int i=0;i<=samples;i++)
            {
                float normalized=i/(float)samples,time=normalized*duration;var pose=evaluate(normalized);
                height.AddKey(time,pose.hipY);
                foreach(var bone in Paths.Keys)
                {var q=Quaternion.Euler(pose.rotations[bone]);var c=curves[bone];c[0].AddKey(time,q.x);c[1].AddKey(time,q.y);c[2].AddKey(time,q.z);c[3].AddKey(time,q.w);}
            }
            clip.SetCurve("Hips",typeof(Transform),"m_LocalPosition.y",height);
            string[] axes={"x","y","z","w"};
            foreach(var bone in Paths.Keys)for(int axis=0;axis<4;axis++)clip.SetCurve(Paths[bone],typeof(Transform),"m_LocalRotation."+axes[axis],curves[bone][axis]);
            clip.EnsureQuaternionContinuity();var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=loop;settings.loopBlend=loop;AnimationUtility.SetAnimationClipSettings(clip,settings);
            string path=Folder+"/"+name+".anim";var existing=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if(existing){EditorUtility.CopySerialized(clip,existing);EditorUtility.SetDirty(existing);UnityEngine.Object.DestroyImmediate(clip);return existing;}
            AssetDatabase.CreateAsset(clip,path);return clip;
        }

        public static void ApplyPose(Transform visual,string kind,float time)
        {
            var pose=PoseFor(kind,time);visual.Find("Hips").localPosition=new Vector3(0,pose.hipY,0);
            foreach(var pair in Paths)visual.Find(pair.Value).localRotation=Quaternion.Euler(pose.rotations[pair.Key]);
        }
    }
}
