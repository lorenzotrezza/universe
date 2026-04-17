using UnityEngine;

public class LabImportedClipLoop : MonoBehaviour
{
    [SerializeField] private AnimationClip[] clips = System.Array.Empty<AnimationClip>();
    [SerializeField] private float playbackSpeed = 1f;

    private float _time;

    private void Update()
    {
        Step(Time.deltaTime);
    }

    public void Step(float deltaTime)
    {
        if (clips == null || clips.Length == 0)
        {
            return;
        }

        var rootLocalPosition = transform.localPosition;
        var rootLocalRotation = transform.localRotation;
        var rootLocalScale = transform.localScale;

        _time += Mathf.Max(0f, playbackSpeed) * deltaTime;

        foreach (var clip in clips)
        {
            if (clip == null || clip.length <= 0f)
            {
                continue;
            }

            clip.SampleAnimation(gameObject, Mathf.Repeat(_time, clip.length));
        }

        transform.localPosition = rootLocalPosition;
        transform.localRotation = rootLocalRotation;
        transform.localScale = rootLocalScale;
    }
}
