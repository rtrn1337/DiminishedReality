using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfViewCopier : MonoBehaviour
{
   Camera _mainCamera, _maskCamera;
    void Start()
    {
        _mainCamera = Camera.main;
        _maskCamera = GetComponent<Camera>();
    }
 
    void Update()
    {
        _maskCamera.fieldOfView = _mainCamera.fieldOfView;
    }
}
