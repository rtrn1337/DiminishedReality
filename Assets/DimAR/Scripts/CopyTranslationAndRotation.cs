using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyTranslationAndRotation : MonoBehaviour
{
    public Transform transformToCopyFrom;

    // Update is called once per frame
    void Update()
    {
        this.transform.localPosition = transformToCopyFrom.transform.localPosition;
        this.transform.rotation = transformToCopyFrom.transform.rotation;
    }
}
