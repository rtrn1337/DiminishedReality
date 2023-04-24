using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITextureQualitySetter : MonoBehaviour
{
    private Dropdown _dropdown;
    private void Start()
    {
        _dropdown = GetComponent<Dropdown>();
        if (PlayerPrefs.HasKey("texturequality"))
        {
            _dropdown.value = _dropdown.options.FindIndex(option => option.text == PlayerPrefs.GetInt("texturequality").ToString());
        }
        else
        {  
            SafeValueToPlayerPrefs();
        }
    } 

    public void SafeValueToPlayerPrefs()
    {
        PlayerPrefs.SetInt("texturequality", int.Parse(_dropdown.options[_dropdown.value].text));
        PlayerPrefs.Save();
    }
    
}
