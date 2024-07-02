using System;
using System.Linq;
using TMPro;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VRC.Udon;
using Object = UnityEngine.Object;
[CustomEditor(typeof(CanvasSync))]
public class CanvasSyncEditor : Editor
{
    private bool showDefaultInspector = false;
    private bool listMode = false;
    public override void OnInspectorGUI()
    {
        if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
        CanvasSync canvasSync = (CanvasSync) target;

        showDefaultInspector = EditorGUILayout.Foldout(showDefaultInspector, "Show default inspector");
        if (showDefaultInspector)
            DrawDefaultInspector();
       
        if(GUILayout.Button(listMode? "List View":"Children View")) listMode = !listMode;
        
        AutomaticPanel(canvasSync,listMode);
        EditorGUI.PropertyField(EditorGUILayout.GetControlRect(), serializedObject.FindProperty("onValueChanged"), true);
    }

    private bool automateChild;
    private bool addAllChild;
    private bool removeAllChild;
    
    void AutomaticPanel(CanvasSync canvasSync, bool listView)
    {
        var ub = UdonSharpEditorUtility.GetBackingUdonBehaviour(canvasSync);
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical("Helpbox");
        #region Fields Hell

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
        #endregion
        
        EditorGUILayout.EndVertical();
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
  
    public static void OnBuild()
    {
        var canvasSync = GameObject.FindObjectsOfType<CanvasSync>();

        foreach (var v in canvasSync)
        {
            EditorUtility.SetDirty(v);
            var ub = UdonSharpEditorUtility.GetBackingUdonBehaviour(v);

                v.toggles = v.GetComponentsInChildren<Toggle>();
                foreach (var t in v.toggles)
                {
                    AddListener(ub, t.onValueChanged, "_OnPressed",t);
                }

                v.dropdowns = v.GetComponentsInChildren<TMP_Dropdown>();
                foreach (var t in v.dropdowns)
                {
                    AddListener(ub, t.onValueChanged, "_OnPressed",t);
                }

                v.sliders = v.GetComponentsInChildren<Slider>();
                foreach (var t in v.sliders)
                {
                    AddListener(ub, t.onValueChanged, "_OnPressed",t);
                }
            
        }
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
