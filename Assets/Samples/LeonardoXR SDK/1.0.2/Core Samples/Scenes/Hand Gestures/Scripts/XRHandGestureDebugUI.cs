using UnityEngine.XR.Hands.Samples.Gestures.DebugTools;
using UnityEngine.XR.Hands.Samples.GestureSample;
using Unity.XR.CoreUtils.Collections;
using UnityEngine.XR.Hands.Analytics;
using UnityEngine.XR.Hands.Gestures;
using System.Collections.Generic;
using UnityEngine.XR.Hands;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace LeonardoXR.Samples.Gestures
{
    /// <summary>
    /// Controls the debug UI for <see cref="XRHandShape"/> that shows the target and tolerances on the UI controlled
    /// by a <see cref="XRAllFingerStatesDebugUI"/>.
    /// </summary>
    public class XRHandGestureDebugUI : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The debug UI that will be used to display the finger states.")]
        XRAllFingerShapesDebugUI m_XRAllFingerShapesDebugUI;

        [SerializeField]
        SerializableDictionary<ScriptableObject, Sprite> m_HandShapeIcons;

        [SerializeField]
        [Tooltip("The hand shape or pose that must be detected for the gesture to be performed.")]
        ScriptableObject m_HandShapeOrPose;

        [SerializeField]
        Image SelectedHandShapeIcon;

        [SerializeField]
        TextMeshProUGUI SelectedHandShapeName;

        [SerializeField]
        XRSelectedHandShapeDebugUI m_XRSelectedHandShapeDebugUI;

        [SerializeField]
        [Tooltip("The component used to calculate how closely the current hand shape matches the target hand shape.")]
        HandShapeCompletenessCalculator m_HandShapeCompletenessCalculator;

        [SerializeField]
        [Tooltip("The progress bar UI that displays the completeness of the hand shape.")]
        Slider m_HandShapeCompletenessProgressBar;

        XRHandShape m_HandShape;

        bool m_HandShapeDetected;

        bool m_HandShapeCompletenessEnabled;

        readonly List<XRFingerShapeDebugBar> k_ReusableBarsToHide = new List<XRFingerShapeDebugBar>();

        readonly List<XRFingerShapeDebugBar> k_Bars = new List<XRFingerShapeDebugBar>();

        static List<XRHandSubsystem> s_SubsystemsReuse = new List<XRHandSubsystem>();

        /// <summary>
        /// The hand shape that will be displayed in the debug UI.
        /// </summary>
        public ScriptableObject handShapeOrPose
        {
            get => m_HandShape;
            set
            {
                var handPose = value as XRHandPose;

                m_HandShape = handPose != null ? handPose.handShape : value as XRHandShape;

                m_HandShapeDetected = m_HandShape != null;
                foreach (var bar in k_Bars)
                    bar.fingerShapeDetected = m_HandShapeDetected;

                if (m_HandShapeDetected)
                {
                    // Hide previously enabled bars
                    foreach (var bar in k_Bars)
                        bar.HideTargetAndTolerance();
                }
            }
        }

        void Awake()
        {
#if UNITY_EDITOR && ENABLE_CLOUD_SERVICES_ANALYTICS
            XRHandAnalyticsData.xrHandCustomGestureDebugActive = true;
#endif
            m_HandShape = (XRHandShape)m_HandShapeOrPose;

            if (m_HandShape == null)
            {
                XRHandPose poseCastTest = (XRHandPose)m_HandShapeOrPose;
                if (poseCastTest != null)
                    m_HandShape = poseCastTest.handShape;
            }

            m_HandShapeDetected = m_HandShape != null;

            if (m_HandShapeDetected)
            {
                SelectedHandShapeIcon.sprite = m_HandShapeIcons[m_HandShape];
                SelectedHandShapeName.text = m_HandShape.name;
                handShapeOrPose = m_HandShape;
                m_XRSelectedHandShapeDebugUI.UpdateSelectedHandShapeTextUI(m_HandShape);
            }

            if (k_Bars.Count == 0)
            {
                foreach (var graph in m_XRAllFingerShapesDebugUI.xrFingerShapeDebugGraphs)
                {
                    foreach (var bar in graph.bars)
                        k_Bars.Add(bar);
                }
            }

            m_HandShapeCompletenessEnabled =
                m_HandShapeCompletenessCalculator != null && m_HandShapeCompletenessProgressBar != null;
        }

        void Update()
        {
            foreach (var bar in k_Bars)
                bar.HideTargetAndTolerance();

            // Track all the bars that have no target and tolerance so they can be hidden
            k_ReusableBarsToHide.Clear();
            foreach (var graph in m_XRAllFingerShapesDebugUI.xrFingerShapeDebugGraphs)
                k_ReusableBarsToHide.AddRange(graph.bars);

            if (m_HandShapeDetected)
            {
                foreach (var condition in m_HandShape.fingerShapeConditions)
                {
                    foreach (var shapeCondition in condition.targets)
                    {
                        if (shapeCondition.shapeType == XRFingerShapeType.Unspecified)
                            continue;
                        var xrFingerShapeDebugGraph = m_XRAllFingerShapesDebugUI.xrFingerShapeDebugGraphs[(int)condition.fingerID];
                        var bar = xrFingerShapeDebugGraph.bars[(int)shapeCondition.shapeType];
                        bar.SetTargetAndTolerances(shapeCondition.desired, shapeCondition.upperTolerance, shapeCondition.lowerTolerance);
                        k_ReusableBarsToHide.Remove(bar);
                    }
                }
            }

            if (m_HandShapeCompletenessEnabled && m_HandShapeDetected)
            {
                if (!TryGetSubsystem(out var subsystem))
                    return;

                var hand = m_XRAllFingerShapesDebugUI.handedness ==
                    Handedness.Left ? subsystem.leftHand : subsystem.rightHand;

                var completenessScore = 0f;
                if (hand.isTracked)
                {
                    m_HandShapeCompletenessCalculator.TryCalculateHandShapeCompletenessScore(
                        hand, m_HandShape, out completenessScore);
                }

                m_HandShapeCompletenessProgressBar.value = completenessScore;
            }
        }

        /// <summary>
        /// Clear the detected handshape reference inorder to stop displaying any corresponding UI
        /// </summary>
        public void ClearDetectedHandShape()
        {
            handShapeOrPose = null;
        }


#if ODIN_INSPECTOR
        [Button("Update Detected Hand Shape")]
#endif
        public void UpdateDetectedHandShape(ScriptableObject handShapeOrPose)
        {
            SelectedHandShapeIcon.sprite = m_HandShapeIcons[handShapeOrPose];
            SelectedHandShapeName.text = handShapeOrPose.name;
            this.handShapeOrPose = handShapeOrPose;
            m_XRSelectedHandShapeDebugUI.UpdateSelectedHandShapeTextUI(handShapeOrPose);
        }

        static bool TryGetSubsystem(out XRHandSubsystem system)
        {
            system = null;

            if (s_SubsystemsReuse.Count == 0)
                SubsystemManager.GetSubsystems(s_SubsystemsReuse);

            if (s_SubsystemsReuse.Count > 0)
            {
                system = s_SubsystemsReuse[0];
                return true;
            }
            return false;
        }
    }
}