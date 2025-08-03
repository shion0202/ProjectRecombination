using UnityEditor;
using UnityEngine;

namespace _Project.Scripts.VisualScripting.Editor
{
    public class VisualScriptingEditor
    {
        // 우 클릭 메뉴에 등록하려면 GameObject/ 로 경로가 시작되어야 한다.
        [MenuItem("GameObject/VisualScripting/Input/Trigger", false, 10)]
        private static void CreateTriggerAsset(MenuCommand menuCommand)
        {
            // Trigger 스크립트를 컴포넌트로 등록한 GameObject를 생성합니다.
            GameObject go = new GameObject("NewTrigger");
            go.AddComponent<Trigger>();
            go.AddComponent<BoxCollider>();
            
            // BoxCollider를 Trigger로 설정합니다.
            go.GetComponent<BoxCollider>().isTrigger = true;
        
            // 부모 객체가 선택되어 있을 때 해당 객체의 자식으로 추가합니다.
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        
            // Undo 등록 및 선택
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
        
        [MenuItem("GameObject/VisualScripting/Input/TriggerEnter", false, 10)]
        private static void CreateTriggerEnterAsset(MenuCommand menuCommand)
        {
            // Trigger 스크립트를 컴포넌트로 등록한 GameObject를 생성합니다.
            GameObject go = new GameObject("NewTriggerEnter");
            go.AddComponent<TriggerEnter>();
            go.AddComponent<BoxCollider>();
            
            // BoxCollider를 Trigger로 설정합니다.
            go.GetComponent<BoxCollider>().isTrigger = true;
        
            // 부모 객체가 선택되어 있을 때 해당 객체의 자식으로 추가합니다.
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        
            // Undo 등록 및 선택
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
        
        [MenuItem("GameObject/VisualScripting/Input/TriggerExit", false, 10)]
        private static void CreateTriggerExitAsset(MenuCommand menuCommand)
        {
            // Trigger 스크립트를 컴포넌트로 등록한 GameObject를 생성합니다.
            GameObject go = new GameObject("NewTriggerEnter");
            go.AddComponent<TriggerExit>();
            go.AddComponent<BoxCollider>();
            
            // BoxCollider를 Trigger로 설정합니다.
            go.GetComponent<BoxCollider>().isTrigger = true;
        
            // 부모 객체가 선택되어 있을 때 해당 객체의 자식으로 추가합니다.
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        
            // Undo 등록 및 선택
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
        
        [MenuItem("GameObject/VisualScripting/Input/Timer", false, 10)]
        private static void CreateTimerAsset(MenuCommand menuCommand)
        {
            // Trigger 스크립트를 컴포넌트로 등록한 GameObject를 생성합니다.
            GameObject go = new GameObject("NewTimer");
            go.AddComponent<Timer>();
        
            // 부모 객체가 선택되어 있을 때 해당 객체의 자식으로 추가합니다.
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        
            // Undo 등록 및 선택
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
        
        [MenuItem("GameObject/VisualScripting/Process/IsDestroy", false, 10)]
        private static void CreateIsDestroyAsset(MenuCommand menuCommand)
        {
            // Trigger 스크립트를 컴포넌트로 등록한 GameObject를 생성합니다.
            GameObject go = new GameObject("NewIsDestroy");
            go.AddComponent<IsDestroy>();
        
            // 부모 객체가 선택되어 있을 때 해당 객체의 자식으로 추가합니다.
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        
            // Undo 등록 및 선택
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
        
        [MenuItem("GameObject/VisualScripting/Process/LogicalOperation", false, 10)]
        private static void CreateLogicalOperationAsset(MenuCommand menuCommand)
        {
            // Logic 스크립트를 컴포넌트로 등록한 GameObject를 생성합니다.
            GameObject go = new GameObject("NewLogic");
            go.AddComponent<Logic>();
        
            // 부모 객체가 선택되어 있을 때 해당 객체의 자식으로 추가합니다.
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        
            // Undo 등록 및 선택
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        [MenuItem("GameObject/VisualScripting/Process/Conditional", false, 10)]
        private static void CreateConditionalAsset(MenuCommand menuCommand)
        {
            // Conditional 스크립트를 컴포넌트로 등록한 GameObject를 생성
            GameObject go = new GameObject("NewConditional");
            go.AddComponent<Conditional>();
            // 부모 객체가 선택되어 있을 때 해당 객체의 자식으로 추가합니다.
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            // Undo 등록 및 선택
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        [MenuItem("GameObject/VisualScripting/Output/Activate", false, 10)]
        private static void CreateActivateAsset(MenuCommand menuCommand)
        {
            // Activate 스크립트를 컴포넌트로 등록한 GameObject를 생성합니다.
            GameObject go = new GameObject("NewActivate");
            go.AddComponent<Activate>();

            // 부모 객체가 선택되어 있을 때 해당 객체의 자식으로 추가합니다.
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);

            // Undo 등록 및 선택
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        [MenuItem("GameObject/VisualScripting/Output/Deactivate", false, 10)]
        private static void CreateDeactivateAsset(MenuCommand menuCommand)
        {
            // Deactivate 스크립트를 컴포넌트로 등록한 GameObject를
            GameObject go = new GameObject("NewDeactivate");
            go.AddComponent<Deactivate>();
            // 부모 객체가 선택되어 있을 때 해당 객체의 자식으로 추가합니다.
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            // Undo 등록 및 선택
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        [MenuItem("GameObject/VisualScripting/Output/ToggleActivate", false, 10)]
        private static void CreateToggleActivateAsset(MenuCommand menuCommand)
        {
            // ToggleActivate 스크립트를 컴포넌트로 등록한 GameObject를 생성합니다.
            GameObject go = new GameObject("NewToggleActivate");
            go.AddComponent<ToggleActivate>();

            // 부모 객체가 선택되어 있을 때 해당 객체의 자식으로 추가합니다.
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);

            // Undo 등록 및 선택
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
        
        [MenuItem("GameObject/VisualScripting/Output/Cutscene", false, 10)]
        private static void CreateCutsceneAsset(MenuCommand menuCommand)
        {
            // Cutscene 스크립트를 컴포넌트로 등록한 GameObject를 생성합니다.
            GameObject go = new GameObject("NewCutscene");
            go.AddComponent<CameraControl>();

            // 부모 객체가 선택되어 있을 때 해당 객체의 자식으로 추가합니다.
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);

            // Undo 등록 및 선택
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        [MenuItem("GameObject/VisualScripting/Output/Instance", false, 10)]
        private static void CreateInstanceAsset(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("NewInstance");
            go.AddComponent<Instance>();
            // 부모 객체가 선택되어 있을 때 해당 객체의 자식으로 추가합니다.
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            // Undo 등록 및 선택
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
        
        [MenuItem("GameObject/VisualScripting/Output/Move", false, 10)]
        private static void CreateMoveObjectAsset(MenuCommand menuCommand)
        {
            // MoveGameObject 스크립트를 컴포넌트로 등록한 GameObject를 생성합니다.
            GameObject go = new GameObject("NewMove");
            go.AddComponent<MoveObject>();

            // 부모 객체가 선택되어 있을 때 해당 객체의 자식으로 추가합니다.
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);

            // Undo 등록 및 선택
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
    }
}
