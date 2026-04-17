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

    public static LabGuideLessonDefinition CreateDefaultRuntimeLesson()
    {
        var lesson = CreateInstance<LabGuideLessonDefinition>();
        lesson.Steps = new List<LabGuideStepDefinition>
        {
            Step("guide_intro", "Segui la guida e resta nell'area sicura.", "Ti orienta prima di entrare tra macchine e passaggi.", "guide_intro", "area_sicura_intro", LabGuideTargetKind.Area, 20f),
            Step("raggiungi_area_controllo", "Raggiungi il punto di controllo prima di operare.", "Da qui verifichi l'ambiente senza esporti al traffico interno.", "raggiungi_area_controllo", "punto_controllo", LabGuideTargetKind.Area, 30f),
            Step("interagisci_postazione_dpi", "Controlla la postazione DPI prima di entrare.", "Casco, occhiali e guanti riducono i rischi vicino ai macchinari.", "interagisci_postazione_dpi", "postazione_dpi", LabGuideTargetKind.Object, 35f),
            Step("metti_in_sicurezza_passaggio", "Rimuovi l'ostacolo dal passaggio operativo.", "Un corridoio libero evita inciampi e collisioni durante la movimentazione.", "metti_in_sicurezza_passaggio", "passaggio_operativo", LabGuideTargetKind.Area, 35f),
            Step("chiusura_guidata", "Conferma il percorso sicuro completato.", "Chiudere il controllo fissa le azioni corrette prima della libera esplorazione.", "chiusura_guidata", "uscita_sicura", LabGuideTargetKind.Area, 30f),
        };
        return lesson;
    }

    private static LabGuideStepDefinition Step(
        string stepId,
        string objective,
        string reason,
        string signal,
        string targetId,
        LabGuideTargetKind targetKind,
        float expectedDuration)
    {
        return new LabGuideStepDefinition
        {
            StepId = stepId,
            ObjectiveItalian = objective,
            SafetyReasonItalian = reason,
            RequiredSignalId = signal,
            TargetId = targetId,
            TargetKind = targetKind,
            HintDelaySeconds = 20f,
            WarningDelaySeconds = 45f,
            ExpectedDurationSeconds = expectedDuration,
        };
    }
}
