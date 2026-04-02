using System.Text;
using UnityEngine;

public static class PromptBuilder
{
    #region Dialogue Prompt
    public static string BuildDialoguePrompt(NpcInformation npc)
    {
        StringBuilder sb = new StringBuilder();
        string npcName = npc.player.ToString();

        TextAsset _role = Resources.Load<TextAsset>($"Common/role_dialogue");
        TextAsset _situation = Resources.Load<TextAsset>("Common/situation");
        TextAsset _progress = Resources.Load<TextAsset>("Common/progress");
        // Debug.Log(npcName);
        TextAsset _persona = Resources.Load<TextAsset>($"{npcName}/persona");
        // TextAsset _memory_status = Resources.Load<TextAsset>("Data/Example");
        // TextAsset _log = Resources.Load<TextAsset>("Data/Example"); 얘는 user_input 사이드?
        string _conversation_rules = Resources.Load<TextAsset>("Common/conversation_rules").text;
        _conversation_rules.Replace("$NPCNAME$", npcName);

        sb.Append(_role.text);
        sb.Append(_situation.text);
        sb.Append(_progress.text);
        sb.Append(_persona.text);
        sb.Append(_conversation_rules);

        return sb.ToString();
    }
    #endregion

    #region Simulate Prompt
    public static string BuildSimulatePrompt(Player A, Player B)
    {
        StringBuilder sb = new StringBuilder();
        string npcNameA = A.ToString();
        string npcNameB = B.ToString();

        TextAsset _role = Resources.Load<TextAsset>($"Common/role_simulate");
        TextAsset _situation = Resources.Load<TextAsset>("Common/situation");
        TextAsset _progress = Resources.Load<TextAsset>("Common/progress");

        // 이전 대화 Summary
        TextAsset _summary = Resources.Load<TextAsset>($"{npcNameA}/summary/{npcNameB}");

        // Debug.Log(npcName);
        TextAsset _personaA = Resources.Load<TextAsset>($"{npcNameA}/persona");
        TextAsset _opinionAB = Resources.Load<TextAsset>($"{npcNameA}/opinion/{npcNameB}");

        // TextAsset _memory_statusA = Resources.Load<TextAsset>("Data/Example");
        TextAsset _personaB = Resources.Load<TextAsset>($"{npcNameB}/persona");
        TextAsset _opinionBA = Resources.Load<TextAsset>($"{npcNameB}/opinion/{npcNameA}");

        // TextAsset _memory_statusB = Resources.Load<TextAsset>("Data/Example");
        TextAsset _simulate_rules = Resources.Load<TextAsset>("Common/simulate_rules");

        sb.Append(_role.text);
        sb.Append(_situation.text);
        sb.Append(_progress.text);

        sb.Append("### **Summary of previous conversations between A and B**\n\n");
        sb.Append(_summary.text);
        sb.Append("\n");

        sb.Append("### **Persona A**\n\n");
        sb.Append(_personaA.text);
        sb.Append("### **Opinion of Persona A on Perona B**\n");
        sb.Append(_opinionAB.text);
        sb.Append("\n");

        sb.Append("### **Persona B**\n\n");
        sb.Append(_personaB.text);
        sb.Append("### **Opinion of Persona B on Perona A**\n");
        sb.Append(_opinionBA.text);
        sb.Append("\n");

        sb.Append(_simulate_rules.text);

        return sb.ToString();
    }
    #endregion

    #region Summarize Prompt
    public static string BuildSummarizePrompt(NpcInformation npc)
    {
        StringBuilder sb = new StringBuilder();
        string npcName = npc.player.ToString();

        string _role = Resources.Load<TextAsset>($"Common/role_summarize").text;
        _role = _role.Replace("$NPCNAME$", npcName);

        TextAsset _situation = Resources.Load<TextAsset>("Common/situation");
        TextAsset _progress = Resources.Load<TextAsset>("Common/progress");
        // Debug.Log(npcName);
        TextAsset _persona_short = Resources.Load<TextAsset>($"{npcName}/persona_short");
        // TextAsset _memory_status = Resources.Load<TextAsset>("Data/Example");
        TextAsset _log = Resources.Load<TextAsset>("Data/Example");

        string _summarize_rules = Resources.Load<TextAsset>("Common/summarize_rules").text;
        _summarize_rules = _summarize_rules.Replace("$NPCNAME$", npcName);

        sb.Append(_role);
        sb.Append(_situation.text);
        sb.Append(_progress.text);
        sb.Append("### NPC Persona\n\n");
        sb.Append(_persona_short.text);
        sb.Append("### **Log**\n\n");
        sb.Append(_log.text);
        sb.Append(_summarize_rules);

        return sb.ToString();
    }
    #endregion
}
