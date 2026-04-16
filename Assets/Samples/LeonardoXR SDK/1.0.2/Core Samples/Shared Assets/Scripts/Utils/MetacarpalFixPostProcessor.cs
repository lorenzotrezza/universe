using UnityEngine.XR.Hands.Processing;
using Unity.XR.CoreUtils.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Hands;
using Unity.XR.CoreUtils;
using Unity.Collections;
using System;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace UnityEngine.XR.Interaction.Toolkit.Samples.Hands.Extensions
{
    public class MetacarpalFixPostProcessor : MonoBehaviour, IXRHandProcessor
    {
        [Serializable]
        public class JointFix
        {
            public float Distance;
            public Vector3 Spread;

            public JointFix(float forwardPush, Vector3 spread)
            {
                Distance = forwardPush;
                Spread = spread;
            }
        }

        [SerializeField] private bool m_metacarpalJointsFixForSpaces;
        [SerializeField] private float m_spreadFactor;
        [SerializeField] private float m_distanceFactor;
        [SerializeField] private float m_thresholdDistance;

#if ODIN_INSPECTOR
        [DictionaryDrawerSettings(KeyLabel = "Joint ID", ValueLabel = "Joint Fix", DisplayMode = DictionaryDisplayOptions.Foldout)]
#endif
        [SerializeField] private SerializableDictionary<XRHandJointID, JointFix> m_jointFixes;


#if ODIN_INSPECTOR
        [FoldoutGroup("Debug"), Sirenix.OdinInspector.ReadOnly] 
#endif
        [SerializeField] private float m_currentDistance;

#if ODIN_INSPECTOR
        [FoldoutGroup("Debug/Live Joints"), Sirenix.OdinInspector.ReadOnly]
#endif
        [SerializeField] protected Pose m_wristJointLivePose;

#if ODIN_INSPECTOR
        [FoldoutGroup("Debug/Live Joints"), Sirenix.OdinInspector.ReadOnly] 
#endif
        [SerializeField] protected SerializableDictionary<XRHandJointID, Pose> m_leftHandMetacarpalJointLivePoses;

#if ODIN_INSPECTOR
        [FoldoutGroup("Debug/Live Joints"), Sirenix.OdinInspector.ReadOnly] 
#endif
        [SerializeField] protected SerializableDictionary<XRHandJointID, Pose> m_rightHandMetacarpalJointLivePoses;

        /// <inheritdoc />
        public int callbackOrder => -1;

        bool m_WasLeftHandTrackedLastFrame;
        bool m_WasRightHandTrackedLastFrame;

        XRHandSubsystem m_Subsystem;
        static readonly List<XRHandSubsystem> s_SubsystemsReuse = new List<XRHandSubsystem>();

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        void OnDisable()
        {
            if (m_Subsystem != null)
            {
                m_Subsystem.UnregisterProcessor(this);
                m_Subsystem = null;
            }
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        void Update()
        {
            if (m_Subsystem != null && m_Subsystem.running)
                return;

            SubsystemManager.GetSubsystems(s_SubsystemsReuse);
            var foundRunningHandSubsystem = false;
            for (var i = 0; i < s_SubsystemsReuse.Count; ++i)
            {
                var handSubsystem = s_SubsystemsReuse[i];
                if (handSubsystem.running)
                {
                    m_Subsystem?.UnregisterProcessor(this);
                    m_Subsystem = handSubsystem;
                    foundRunningHandSubsystem = true;
                    break;
                }
            }

            if (!foundRunningHandSubsystem)
                return;

            m_WasLeftHandTrackedLastFrame = false;
            m_WasRightHandTrackedLastFrame = false;
            m_Subsystem.RegisterProcessor(this);
        }

        /// <inheritdoc />
        public void ProcessJoints(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags successFlags, XRHandSubsystem.UpdateType updateType)
        {
            if (!m_metacarpalJointsFixForSpaces) return;

            var leftHand = subsystem.leftHand;
            if (leftHand.isTracked)
            {
                var leftHandPose = leftHand.rootPose;
                if (!m_WasLeftHandTrackedLastFrame)
                {

                }
                else
                {
                    FixMetacarpalJointLivePoses(leftHand);
                    subsystem.SetCorrespondingHand(leftHand);
                }
            }

            m_WasLeftHandTrackedLastFrame = leftHand.isTracked;

            var rightHand = subsystem.rightHand;
            if (rightHand.isTracked)
            {
                var rightHandPose = rightHand.rootPose;
                if (!m_WasRightHandTrackedLastFrame)
                {

                }
                else
                {
                    FixMetacarpalJointLivePoses(rightHand);
                    subsystem.SetCorrespondingHand(rightHand);
                }
            }

            m_WasRightHandTrackedLastFrame = rightHand.isTracked;
        }

        private void FixMetacarpalJointLivePoses(XRHand hand)
        {
            foreach (XRHandJointID jointID in m_jointFixes.Keys)
            {
                XRHandJoint joint = hand.GetJoint(jointID);
                XRHandJoint nextJoint = hand.GetJoint(jointID+1);
                NativeArray<XRHandJoint> handJoints = hand.GetRawJointArray();

                joint.TryGetPose(out Pose jointPose);
                nextJoint.TryGetPose(out Pose nextJointPose);
                hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out Pose wristJointPose);

                m_wristJointLivePose = wristJointPose;

                if (Mathf.Abs((m_distanceFactor * m_jointFixes[jointID].Distance) - (m_currentDistance = (jointPose.position - wristJointPose.position).magnitude)) > m_thresholdDistance)
                {
#if UNITY_EDITOR
                    Dictionary<XRHandJointID, Pose> jointLivePoses = hand.handedness == Handedness.Left ? m_leftHandMetacarpalJointLivePoses : m_rightHandMetacarpalJointLivePoses;

                    if (jointLivePoses.ContainsKey(jointID) && Vector3.Distance(jointPose.position, jointLivePoses[jointID].position) < 0.001f)
                        continue;
#endif
                    FixJointPose(wristJointPose, jointPose, nextJointPose, jointID, hand, out jointPose);

                    joint.SetPose(jointPose);

                    if (hand.handedness == Handedness.Left)
                        m_leftHandMetacarpalJointLivePoses[jointID] = jointPose;
                    else
                        m_rightHandMetacarpalJointLivePoses[jointID] = jointPose;

                    handJoints[jointID.ToIndex()] = joint;
                }
            }
        }

        private void FixJointPose(in Pose wristJointPose, in Pose jointPose, in Pose nextJointPose, XRHandJointID jointID, XRHand hand, out Pose fixedJointPose)
        {
            fixedJointPose = new(wristJointPose.position
                        + ((nextJointPose.position - wristJointPose.position) * m_distanceFactor * m_jointFixes[jointID].Distance)
                        + (Quaternion.LookRotation(nextJointPose.position - wristJointPose.position, wristJointPose.up) * (m_spreadFactor * m_jointFixes[jointID].Spread.Multiply((hand.handedness == Handedness.Left ? Vector3.one : new Vector3(-1f, 1f, 1f))))), jointPose.rotation);
        }


        public void ToggleFix()
        {
            m_metacarpalJointsFixForSpaces = !m_metacarpalJointsFixForSpaces;
        }

        public void SetDistanceFactor(float distanceFactor)
        {
            m_distanceFactor = distanceFactor;
        }

        public void SetSpreadFactor(float spreadFactor)
        {
            m_spreadFactor = spreadFactor;
        }

        public void SetThresholdDistance(float distance)
        {
            m_thresholdDistance = distance;
        }
    }
}