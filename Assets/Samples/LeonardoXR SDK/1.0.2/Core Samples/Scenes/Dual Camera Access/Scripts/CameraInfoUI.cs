using UnityEngine.UI;
using UnityEngine;

namespace Youbiquo.LeonardoXR.Samples.DualCamera
{
    public class CameraInfoUI : MonoBehaviour
    {
        public Text[] ResolutionTexts;
        public Text[] FocalLengthTexts;
        public Text[] PrincipalPointTexts;
        public Text FramerateText;
        public Toggle DepthSensorSupportedToggle;
    }
}