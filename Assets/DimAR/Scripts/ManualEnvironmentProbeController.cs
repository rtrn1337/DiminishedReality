using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace UnityEngine.XR.ARFoundation.Samples
{
    /// <summary>
    /// Example script for demonstrating and playing with manual environment probe placement/removal.
    /// </summary>
    [RequireComponent(typeof(AREnvironmentProbeManager))]
    public class ManualEnvironmentProbeController : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Reference to the AREnvironmentProbe prefab to be placed manually by this controller")]
        AREnvironmentProbe m_ProbePrefab;

        [SerializeField]
        [Tooltip("Reference to the AREnvironmentProbeManager used to query the current platform's subsystem descriptor for supported features")]
        AREnvironmentProbeManager m_ProbeManager;

   
        readonly List<AREnvironmentProbe> m_Probes = new();
        Transform m_OriginCameraTransform;

        /// <summary>
        /// Returns true if manual placement is supported on the platform and our prefab is a valid reference.
        /// </summary>
        public bool canManuallyPlace => m_ProbePrefab != null && m_ProbeManager.descriptor.supportsManualPlacement;

        /// <summary>
        /// Returns true if manual removal is supported on the platform.
        /// </summary>
        public bool canManuallyRemove => m_ProbeManager.descriptor.supportsRemovalOfManual;

        Pose originCameraPose => new(m_OriginCameraTransform.position, m_OriginCameraTransform.rotation);

        private void Start()
        {
            Debug.Log(m_ProbeManager.automaticPlacementRequested);
        }

        AREnvironmentProbe InstantiateProbe(Pose pose)
        {
            var probe = Instantiate(m_ProbePrefab, pose.position, pose.rotation);
            probe.name = $"{probe.name}-{probe.trackableId.ToString()}";
            m_Probes.Add(probe);

            return probe;
        }

        /// <summary>
        /// Places a probe at the given orientation.
        /// </summary>
        /// <param name="pose"><see cref="Pose"/> at which to place the environment probe </param>
        /// <returns>The <see cref="AREnvironmentProbe"/> instantiated at <paramref name="pose"/>, or <see langword="null"/> if manual placement is unsupported.</returns>
        public AREnvironmentProbe PlaceProbeAt(Pose pose) => canManuallyPlace ? InstantiateProbe(pose) : null;

        /// <summary>
        /// Places a probe at the XR Origin Camera's pose.
        /// </summary>
        /// <returns>The <see cref="AREnvironmentProbe"/> instantiated, or <see langword="null"/> if manual placement is unsupported.</returns>
        public AREnvironmentProbe PlaceProbeAtCamera() =>
            canManuallyPlace ? InstantiateProbe(originCameraPose) : null;
 
   
    }
}
