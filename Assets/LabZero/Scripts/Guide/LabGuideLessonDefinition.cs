using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LabGuideLessonDefinition", menuName = "LabZero/Guide Lesson Definition")]
public class LabGuideLessonDefinition : ScriptableObject
{
    public List<LabGuideStepDefinition> Steps = new();

    public bool ValidateUniqueStepIds(out string duplicateId)
    {
        duplicateId = null;
        var seen = new HashSet<string>();

        foreach (var step in Steps)
        {
            if (step == null || string.IsNullOrWhiteSpace(step.StepId))
            {
                continue;
            }

            if (!seen.Add(step.StepId))
            {
                duplicateId = step.StepId;
                return false;
            }
        }

        return true;
    }
}
