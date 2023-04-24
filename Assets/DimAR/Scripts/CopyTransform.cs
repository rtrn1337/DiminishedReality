using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class CopyTransform : MonoBehaviour
{
    public Transform jointToCopy; 
    private Transform _thisTransform;
    public GameObject arKitSkeleton;
    public Quaternion correctiveRotation; 
    void Start()
    {
       _thisTransform = transform;
       foreach (Transform trans in arKitSkeleton.GetComponentsInChildren<Transform>())
       {
           if (trans.name.Equals(name))
           {
               jointToCopy = trans;
               correctiveRotation = Quaternion.Inverse(jointToCopy.localRotation) * (_thisTransform.localRotation); 
           }
       }
    }
 
    void Update()
    {
        if (jointToCopy) UpdateTransform();
    } 
    private void UpdateTransform()
    { 
        if(name == "Root") _thisTransform.position = jointToCopy.position;
         _thisTransform.rotation = jointToCopy.rotation*Quaternion.AngleAxis(180,Vector3.up);
       // _thisTransform.localRotation = jointToCopy.localRotation * correctiveRotation;
        //_thisTransform.localScale = new Vector3(jointToCopy.localScale.x,jointToCopy.localScale.y,jointToCopy.localScale.z *-1) ;
    }
}
