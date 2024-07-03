using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using Object = UnityEngine.Object;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.Events;
using UdonSharpEditor;
#endif

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class CanvasSync : UdonSharpBehaviour
{
#if !COMPILER_UDONSHARP && UNITY_EDITOR
    public bool editorSetup = true;
    public bool editorAutoChild = true;
#endif
    [UdonSynced] [HideInInspector] public bool initialized;

    public Toggle[] toggles = new Toggle[0];
    [UdonSynced] [HideInInspector] public bool[] togglesState;

    public TMP_Dropdown[] dropdowns = new TMP_Dropdown[0];
    [UdonSynced] [HideInInspector] public int[] dropdownsState;


    public Slider[] sliders = new Slider[0] ;
    [UdonSynced] [HideInInspector] public float[] slidersState;
    
    public TMP_InputField[] inputFields = new TMP_InputField[0];
    [UdonSynced] [HideInInspector] public string[] inputFieldsState;
    
    public UdonBehaviour[] onValueChanged = new UdonBehaviour[0];


    void Start()
    {
        if (!initialized)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            togglesState = new bool[toggles.Length];
            dropdownsState = new int[dropdowns.Length];
            slidersState = new float[sliders.Length];
            inputFieldsState = new string[inputFields.Length];
            initialized = true;
            _UpdateVariables();
            RequestSerialization();
        }
        _isUpdating = true;
        _ChangeValue(false);
        _isUpdating = false;
    }

    bool _isUpdating;

    public void _OnPressed()
    {
        Debug.Log("Pressed");
        if (_isUpdating) return;
        Networking.SetOwner(Networking.LocalPlayer, gameObject);

        _UpdateVariables();
        OnUpdateEvent();
        RequestSerialization();
    }

    public override void OnDeserialization()
    {
        _isUpdating = true;
        _ChangeValue(false);

        OnUpdateEvent();
        _isUpdating = false;
    }

    void _ChangeValue(bool forced)
    {
        for (int i = 0; i < toggles.Length; i++)
        {
            if (forced) toggles[i].isOn = !togglesState[i];
            if (toggles[i].isOn != togglesState[i] || forced)
            {
                Debug.Log($"Toggle {toggles[i].name} {togglesState[i]}");
                toggles[i].isOn = togglesState[i];
            }
        }

        for (int i = 0; i < dropdowns.Length; i++)
        {
            if (forced) dropdowns[i].value = -1;
            if (dropdowns[i].value != dropdownsState[i] || forced)
            {
                Debug.Log($"Dropdown {dropdowns[i].name} {dropdownsState[i]}");
                dropdowns[i].value = dropdownsState[i];
            }
        }

        for (int i = 0; i < sliders.Length; i++)
        {
            if (forced) sliders[i].value = -1;
            if (!Mathf.Approximately(sliders[i].value, slidersState[i]) || forced)
            {
                Debug.Log($"Slider {sliders[i].name} {slidersState[i]}");
                sliders[i].value = slidersState[i];
            }
        }
        for (int i = 0; i < inputFields.Length; i++)
        {
            if (forced) inputFields[i].text = "";
            if (inputFields[i].text != inputFieldsState[i] || forced)
            {
                Debug.Log($"InputField {inputFields[i].name} {inputFieldsState[i]}");
                inputFields[i].text = inputFieldsState[i];
            }
        }
    }

    void OnUpdateEvent()
    {
        foreach (var u in onValueChanged)
        {
            if (u) u.SendCustomEvent("_onValueChanged");
        }
    }

    void _UpdateVariables()
    {
        for (int i = 0; i < toggles.Length; i++)
        {
            togglesState[i] = toggles[i].isOn;
        }

        for (int i = 0; i < dropdowns.Length; i++)
        {
            dropdownsState[i] = dropdowns[i].value;
        }

        for (int i = 0; i < sliders.Length; i++)
        {
            slidersState[i] = sliders[i].value;
        }
        for (int i = 0; i < inputFields.Length; i++)
        {
            inputFieldsState[i] = inputFields[i].text;
        }
    }
}