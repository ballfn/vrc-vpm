using System;
using System.Linq;
using TMPro;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.Udon;
using Object = UnityEngine.Object;
[CustomEditor(typeof(CanvasSync))]
public class CanvasSyncEditor : Editor
{
    private bool showAdvanced = false;
    private bool showDefaultInspector = false;
    private bool listMode = false;

    public override void OnInspectorGUI()
    {
        if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
        CanvasSync canvasSync = (CanvasSync) target;
        if(canvasSync.editorSetup)
        {
            EditorGUILayout.LabelField("Canvas Sync",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This script will sync the state of UI elements across all clients, currently supports Toggles, Dropdowns, Sliders, VRCUrlInputField, and TMP_InputFields", MessageType.None);
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Would you like to automatically add all children of this object to the sync list?", MessageType.Info);
            if (GUILayout.Button("Yes, automatically add all children"))
            {
                canvasSync.editorSetup = false;
                canvasSync.editorAutoChild = true;
            }
            if (GUILayout.Button("No, I will add them manually"))
            {
                canvasSync.editorSetup = false;
                canvasSync.editorAutoChild = false;
            }
            
        }
        else
        {
            if (GUILayout.Button(listMode ? "List View" : "Children View")) listMode = !listMode;
            
                    AutomaticPanel(canvasSync, listMode);
            
                    showAdvanced = EditorGUILayout.Foldout(showAdvanced, "Advanced Settings");
                    if (showAdvanced)
                    {
                        EditorGUILayout.BeginVertical("Helpbox");
                        EditorGUILayout.HelpBox
                            ("call _onValueChanged on UdonBehaviours when value changed", MessageType.Info);
                        SerializedProperty onValueChangedProp = serializedObject.FindProperty("onValueChanged");
                        float propertyHeight = EditorGUI.GetPropertyHeight(onValueChangedProp, true);
                        Rect propertyRect = EditorGUILayout.GetControlRect(true, propertyHeight);
                        EditorGUI.PropertyField(propertyRect, onValueChangedProp, true);
            
                        showDefaultInspector = EditorGUILayout.Foldout(showDefaultInspector, "Show default inspector");
                        if (showDefaultInspector)
                            DrawDefaultInspector();
                        EditorGUILayout.EndVertical();
                    }
        }
        
    }

    private bool automateChild;
    private bool addAllChild;
    private bool removeAllChild;
    
    void AutomaticPanel(CanvasSync canvasSync, bool listView)
    {
        var ub = UdonSharpEditorUtility.GetBackingUdonBehaviour(canvasSync);
        EditorGUILayout.Space();
        
        #region Fields Hell

        #region Child View
        
        if (!listView)
        {
            string auto = canvasSync.editorAutoChild ? "Enabled" : "Disabled";
            GUI.backgroundColor = canvasSync.editorAutoChild ? Color.green : Color.red;
            if(GUILayout.Button($"Auto add all children: {auto}"))
            {
                canvasSync.editorAutoChild = !canvasSync.editorAutoChild;
            }
            GUI.backgroundColor = Color.white;

            if (!canvasSync.editorAutoChild)
            {
                automateChild = EditorGUILayout.Foldout(automateChild, "Automate");
                if (automateChild)
                {
                    EditorGUILayout.BeginHorizontal();

                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("Active All"))
                    {
                        addAllChild = true;
                    }
                
                    GUI.backgroundColor =  Color.red;
                    if (GUILayout.Button("Remove All"))
                    {
                        removeAllChild = true;
                    }
                    GUI.backgroundColor = Color.white;
                

                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                addAllChild = true;
            }
        }

        #endregion

        #region Toggle

        
        EditorGUILayout.BeginVertical("Helpbox");
        EditorGUILayout.LabelField("Toggles", EditorStyles.boldLabel);
        var t = listView ? canvasSync.toggles: canvasSync.GetComponentsInChildren<Toggle>();
        for (int i = 0; i < t.Length; i++)
        {
            DrawObjectField(t[i], ub, t[i]?.onValueChanged, "_OnPressed", ref canvasSync.toggles, listView);

        }
        if(t.Length == 0) EditorGUILayout.LabelField("No Toggles found");
        if(listView )
        {
            EditorGUILayout.Space();
            var addT = (Toggle)EditorGUILayout.ObjectField("Add", null, typeof(Toggle), true);
            if (addT)
            {
                if (!canvasSync.toggles.Contains(addT)) ArrayUtility.Add(ref canvasSync.toggles, addT);
            }
        }
        EditorGUILayout.EndVertical();
        #endregion
        #region Dropdown
        EditorGUILayout.BeginVertical("Helpbox");
        EditorGUILayout.LabelField("Dropdowns", EditorStyles.boldLabel);
        var d = listView ? canvasSync.dropdowns: canvasSync.GetComponentsInChildren<TMP_Dropdown>();
        for (int i = 0; i < d.Length; i++)
        {
            DrawObjectField(d[i],ub,d[i]?.onValueChanged,"_OnPressed",ref canvasSync.dropdowns, listView);
        }
        if(d.Length == 0) EditorGUILayout.LabelField("No Dropdowns found");
        if(listView )
        {
            EditorGUILayout.Space();
            var addD = (TMP_Dropdown)EditorGUILayout.ObjectField("Add", null, typeof(TMP_Dropdown), true);
            if (addD)
            {
                if (!canvasSync.dropdowns.Contains(addD)) ArrayUtility.Add(ref canvasSync.dropdowns, addD);
            }
        }
        EditorGUILayout.EndVertical();
        #endregion
        #region Slider
        EditorGUILayout.BeginVertical("Helpbox");
        EditorGUILayout.LabelField("Sliders", EditorStyles.boldLabel);
        var s = listView ? canvasSync.sliders: canvasSync.GetComponentsInChildren<Slider>();
        for (int i = 0; i < s.Length; i++)
        {
            DrawObjectField(s[i],ub,s[i]?.onValueChanged,"_OnPressed",ref canvasSync.sliders, listView);
        }
        if(s.Length == 0) EditorGUILayout.LabelField("No Sliders found");
        if(listView )
        {
            EditorGUILayout.Space();
            var addS = (Slider)EditorGUILayout.ObjectField("Add", null, typeof(Slider), true);
            if (addS)
            {
                if (!canvasSync.sliders.Contains(addS)) ArrayUtility.Add(ref canvasSync.sliders, addS);
            }
        }
        EditorGUILayout.EndVertical();
        #endregion
        #region InputField
        EditorGUILayout.BeginVertical("Helpbox");
        EditorGUILayout.LabelField("InputFields", EditorStyles.boldLabel);
        var ip = listView ? canvasSync.inputFields: canvasSync.GetComponentsInChildren<TMP_InputField>();
        for (int j = 0; j < ip.Length; j++)
        {
            DrawObjectField(ip[j],ub,ip[j]?.onValueChanged,"_OnPressed",ref canvasSync.inputFields, listView);
        }
        if(ip.Length == 0) EditorGUILayout.LabelField("No InputFields found");
        if(listView )
        {
            EditorGUILayout.Space();
            var addIp = (TMP_InputField)EditorGUILayout.ObjectField("Add", null, typeof(TMP_InputField), true);
            if (addIp)
            {
                if (!canvasSync.inputFields.Contains(addIp)) ArrayUtility.Add(ref canvasSync.inputFields, addIp);
            }
        }
        
        EditorGUILayout.EndVertical();
        #endregion
        #region UrlField
        EditorGUILayout.BeginVertical("Helpbox");
        EditorGUILayout.LabelField("VRCUrlInputFields", EditorStyles.boldLabel);
        var uf = listView ? canvasSync.urlFields : canvasSync.GetComponentsInChildren<VRCUrlInputField>();
        for (int i = 0; i < uf.Length; i++)
        {
            DrawObjectField(uf[i], ub, uf[i]?.onValueChanged, "_OnPressed", ref canvasSync.urlFields, listView);
        }
        if (uf.Length == 0) EditorGUILayout.LabelField("No VRCUrlInputField found");
        if (listView)
        {
            EditorGUILayout.Space();
            var addUf = (VRCUrlInputField)EditorGUILayout.ObjectField("Add", null, typeof(VRCUrlInputField), true);
            if (addUf)
            {
                if (!canvasSync.urlFields.Contains(addUf)) ArrayUtility.Add(ref canvasSync.urlFields, addUf);
            }
        }
        EditorGUILayout.EndVertical();
        
        #endregion
        #endregion
        
        
        addAllChild = false;
        removeAllChild = false;
        
    }
    
    bool DrawObjectField<T>(T obj,UdonBehaviour ub,UnityEventBase eventBase, string methodName,ref T[] source,bool listView) where T : Object
    {
        EditorGUILayout.BeginHorizontal();
        obj = (T) EditorGUILayout.ObjectField("", obj, typeof(T), true);
        /*if (!obj)
        {
            EditorGUILayout.EndHorizontal();
            return false;
        }*/
        bool isListenerAdded =eventBase!=null && IsListenerAdded(ub, eventBase);
        bool button=false;
        if (addAllChild&&!isListenerAdded)
        {
            AddListener(ub, eventBase, methodName,obj);
            if(!source.Contains(obj)) ArrayUtility.Add(ref source, obj);
        }
        if (removeAllChild&&isListenerAdded)
        {
            RemoveListener(ub, eventBase, methodName,obj);
            if(source.Contains(obj)) ArrayUtility.Remove(ref source, obj);
        }
        if (!listView)
        {
            GUI.backgroundColor = isListenerAdded ? Color.green : Color.red;
            button =GUILayout.Button(isListenerAdded?"Active":"Disabled",GUILayout.Width(63));
        }
        else
        {
            button = GUILayout.Button("-",GUILayout.Width(20));
            if (obj == null) button = true;
        }
        if (button)
        {
                if (isListenerAdded||listView)
                {
                    RemoveListener(ub, eventBase, methodName,obj);
                    if(source.Contains(obj)) ArrayUtility.Remove(ref source, obj);
                }
                else
                {
                    AddListener(ub, eventBase, methodName,obj);
                    if(!source.Contains(obj)) ArrayUtility.Add(ref source, obj);
                }   
        }

        
        if(obj&&listView&&!isListenerAdded) AddListener(ub, eventBase, methodName,obj);
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();
        return isListenerAdded;
    }
    public static void AddListener(UdonBehaviour ub, UnityEventBase eventBase, string methodName, Object target)
    {
        if(eventBase==null) return;
        if (IsListenerAdded(ub, eventBase))
        {
            Debug.LogWarning("Listener already added");
            return;
        }
        EditorUtility.SetDirty(target);
        var targetinfo = UnityEvent.GetValidMethodInfo(ub, "SendCustomEvent", new Type[] {typeof(string)});
        UnityAction<string> action =
            Delegate.CreateDelegate(typeof(UnityAction<string>), ub, targetinfo, false) as UnityAction<string>;
        UnityEventTools.RemovePersistentListener(eventBase, action);
        UnityEventTools.AddStringPersistentListener(eventBase, action, methodName);       
        PrefabUtility.RecordPrefabInstancePropertyModifications(target);
    }
    public static void RemoveListener(UdonBehaviour ub, UnityEventBase eventBase, string methodName, Object target)
    {
        if(eventBase==null) return;
        EditorUtility.SetDirty(target);
        var targetinfo = UnityEvent.GetValidMethodInfo(ub, "SendCustomEvent", new Type[] {typeof(string)});
        UnityAction<string> action =
            Delegate.CreateDelegate(typeof(UnityAction<string>), ub, targetinfo, false) as UnityAction<string>;
        UnityEventTools.RemovePersistentListener(eventBase, action);
        PrefabUtility.RecordPrefabInstancePropertyModifications(target);
    }
public static bool IsListenerAdded(UdonBehaviour ub, UnityEventBase eventBase)
{
    for (int i = 0; i < eventBase.GetPersistentEventCount(); i++)
    {
        if (eventBase.GetPersistentTarget(i) == ub)
        {
            return true;
        }
    }

    return false;
}
}
