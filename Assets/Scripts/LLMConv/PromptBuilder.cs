using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class PromptBuilder
{
    private static string ReadFileProgress()
    {
        string path = Path.Combine(Application.persistentDataPath, "Data", "Common", "progress.json");
        string _progress = "";
        if (!File.Exists(path))
        {
            // 파일이 저장될 폴더 경로만 추출
            string directoryPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            _progress = Resources.Load<TextAsset>("Common/progress").text;
            File.WriteAllText(path, _progress);
        }
        else
        {
            _progress = ReadFileToText(path);
        }
        
        return _progress;
    }

    private static string ReadPrevConv(string npcName)
    {
        List<string> players = new List<string> { "USER", "AFURE", "CHENYI", "VIREO", "SIGRID" };
        string result = "";
        string prevConvPlayer = "";
        string summary = "";
        string opinion = "";
        foreach (string player in players)
        {
            if (npcName.Equals(player)) continue;

            string path1 = Path.Combine(Application.persistentDataPath, "Data", npcName, "summary", $"{player}.txt");
            // Debug.Log("summary 확인: " + path1);
            string _summary = ReadFileToText(path1);
            if (!string.IsNullOrEmpty(_summary))
            {
                prevConvPlayer += $"{player}, ";
                summary += $"**Summary of Conversation with {player}**\n{_summary}\n\n";
            }

            string path2 = Path.Combine(Application.persistentDataPath, "Data", npcName, "opinion", $"{player}.txt");
            // Debug.Log("opinion 확인: " + path2);
            string _opinion = ReadFileToText(path2);
            if (!string.IsNullOrEmpty(_opinion))
            {
                opinion += $"**Opinion about {player}**\n{_opinion}\n\n";
            }
        }

        result = $"\n\n**Previous Conversation Partner**: {prevConvPlayer}\n\n{summary}{opinion}";
        return result;
    }

    #region Dialogue Prompt
    public static string BuildDialoguePrompt(NpcInformation npc, int round)
    {
        StringBuilder sb = new StringBuilder();
        string npcName = npc.player.ToString();

        TextAsset _role = Resources.Load<TextAsset>("Common/role_dialogue");
        TextAsset _situation = Resources.Load<TextAsset>("Common/situation");

        string _progress = ReadFileProgress();
        _progress = _progress.Replace("$DEFAULT$", "USER");

        // Debug.Log(npcName);
        TextAsset _persona = Resources.Load<TextAsset>($"{npcName}/persona");

        string _memory_common = "";
        for (int i= 1; i <= round; i++)
        {
            _memory_common += Resources.Load<TextAsset>($"Common/memory/round_{round}").text + "\n\n";
        }
        string _memory_npc = Resources.Load<TextAsset>($"{npcName}/memory/round_{round}").text;

        string _memory = $"\n### Common Memory\n\n{_memory_common}\n\n### {npcName}'s Personal Memory\n\n{_memory_npc}\n\n";

        // TextAsset _memory_status = Resources.Load<TextAsset>("Data/Example");
        // TextAsset _log = Resources.Load<TextAsset>("Data/Example"); 얘는 user_input 사이드?
        string _prevConv = ReadPrevConv(npcName);
        /// Log: local은 자동저장인데, OpenAI는 아님
        /// 데모는 local이니까 일단나중에 (어차피 OpenAI로 할거면 서버에다 넣을거)
        string _conversation_rules = Resources.Load<TextAsset>("Common/conversation_rules").text;
        _conversation_rules = _conversation_rules.Replace("$NPCNAME$", npcName);

        sb.Append(_role.text);
        sb.Append(_situation.text);
        sb.Append(_progress);

        sb.Append(_persona.text);
        sb.Append(_memory);
        sb.Append(_prevConv);

        sb.Append(_conversation_rules);

        return sb.ToString();
    }
    #endregion

    #region Simulate Prompt
    public static string BuildSimulatePrompt(Player A, Player B, int round)
    {
        StringBuilder sb = new StringBuilder();
        string npcNameA = A.ToString();
        string npcNameB = B.ToString();

        TextAsset _role = Resources.Load<TextAsset>($"Common/role_simulate");
        TextAsset _situation = Resources.Load<TextAsset>("Common/situation");
        string _progress = ReadFileProgress();
        _progress = _progress.Replace("$DEFAULT$", $"{npcNameA}, {npcNameB}");

        // 이전 대화 Summary
        string _sPath = Path.Combine(Application.persistentDataPath, "Data", $"{npcNameA}", "summary", $"{npcNameB}.txt");
        string _summary = ReadFileToText(_sPath);

        // Debug.Log(npcName);
        TextAsset _personaA = Resources.Load<TextAsset>($"{npcNameA}/persona");
        string _oPathAB = Path.Combine(Application.persistentDataPath, "Data", $"{npcNameA}","opinion", $"{npcNameB}.txt");
        string _opinionAB = ReadFileToText(_oPathAB);
        // TextAsset _memory_statusA = Resources.Load<TextAsset>("Data/Example");

        TextAsset _personaB = Resources.Load<TextAsset>($"{npcNameB}/persona");
        string _oPathBA = Path.Combine(Application.persistentDataPath, "Data", $"{npcNameB}", "opinion", $"{npcNameA}.txt");
        string _opinionBA = ReadFileToText(_oPathBA);
        // TextAsset _memory_statusB = Resources.Load<TextAsset>("Data/Example");

        TextAsset _simulate_rules = Resources.Load<TextAsset>("Common/simulate_rules");

        string _memory_common = Resources.Load<TextAsset>($"Common/memory/round_{round}").text;
        string _memoryC = $"\n### Common Memory\n\n{_memory_common}\n\n";

        string _memory_npcA = Resources.Load<TextAsset>($"{npcNameA}/memory/round_{round}").text;
        string _memory_npcB = Resources.Load<TextAsset>($"{npcNameB}/memory/round_{round}").text;

        string _memoryA = $"\n### {npcNameA}'s Personal Memory\n\n{_memory_npcA}\n\n";
        string _memoryB = $"\n### {npcNameB}'s Personal Memory\n\n{_memory_npcB}\n\n";

        sb.Append(_role.text);
        sb.Append(_situation.text);
        sb.Append(_progress);

        sb.Append(_memoryC);

        sb.Append("\n### **Summary of previous conversations between A and B**\n\n");
        sb.Append(_summary);

        sb.Append("\n### **Persona A**\n");
        sb.Append(_personaA.text);
        sb.Append(_memoryA);
        sb.Append("\n### **Opinion of Persona A on Perona B**\n");
        sb.Append(_opinionAB);

        sb.Append("\n### **Persona B**\n");
        sb.Append(_personaB.text);
        sb.Append(_memoryB);
        sb.Append("\n### **Opinion of Persona B on Perona A**\n");
        sb.Append(_opinionBA);
        sb.Append("\n");
        sb.Append(_simulate_rules.text);

        return sb.ToString();
    }
    #endregion

    #region Summarize Prompt
    public static string BuildSummarizePrompt(NpcInformation npc, int round)
    {
        StringBuilder sb = new StringBuilder();
        string npcName = npc.player.ToString();

        string _role = Resources.Load<TextAsset>($"Common/role_summarize").text;
        _role = _role.Replace("$NPCNAME$", npcName);

        TextAsset _situation = Resources.Load<TextAsset>("Common/situation");
        string _progress = ReadFileProgress();
        _progress = _progress.Replace("$DEFAULT$", $"USER");

        // Debug.Log(npcName);
        TextAsset _persona_short = Resources.Load<TextAsset>($"{npcName}/persona_short");
        // TextAsset _memory_status = Resources.Load<TextAsset>("Data/Example");
        // TextAsset _log = Resources.Load<TextAsset>("Data/Example");

        string path = Path.Combine(Application.persistentDataPath, "Data", "Common", "log.txt");
        string _log = ReadFileToText(path);

        string _summarize_rules = Resources.Load<TextAsset>("Common/summarize_rules").text;
        _summarize_rules = _summarize_rules.Replace("$NPCNAME$", npcName);

        string _memory_common = Resources.Load<TextAsset>($"Common/memory/round_{round}").text;
        string _memory_npc = Resources.Load<TextAsset>($"{npcName}/memory/round_{round}").text;

        string _memory = $"\n### Common Memory\n\n{_memory_common}\n\n### {npcName}'s Personal Memory\n\n{_memory_npc}\n\n";

        sb.Append(_role);
        sb.Append(_situation.text);
        sb.Append(_progress);
        sb.Append("\n### NPC Persona\n\n");
        sb.Append(_persona_short.text);
        sb.Append(_memory);
        sb.Append("\n### **Log**\n\n");
        sb.Append(_log);
        sb.Append(_summarize_rules);

        return sb.ToString();
    }
    #endregion

    #region Vote Prompt
    public static string BuildVotePrompt(List<string> candidates)
    {
        StringBuilder sb = new StringBuilder();
        string path;

        string _role = Resources.Load<TextAsset>($"Common/role_vote").text;

        TextAsset _situation = Resources.Load<TextAsset>("Common/situation");
        string _progress = ReadFileProgress();
        _progress = _progress.Replace("$DEFAULT$", $"-");
        // Debug.Log(npcName);

        List<Player> player = new List<Player> { Player.AFURE, Player.CHENYI, Player.SIGRID, Player.VIREO, Player.USER };
        string[] playerFeature = new string[5];

        for(int i = 0; i < 4; i++)
        {
            string fullStr = $"**{player[i].ToString()}**\n";
            fullStr += Resources.Load<TextAsset>($"{player[i].ToString()}/persona_short").text + "\n\n";
            for (int j = i + 1; j < 5; j++)
            {
                path = Path.Combine(Application.persistentDataPath, "Data", $"{player[i].ToString()}", "summary", $"{player[j].ToString()}.txt");
                fullStr += $"**Summary of {player[i].ToString()} & {player[j].ToString()} 's conversation**\n" + ReadFileToText(path) + "\n\n";
                path = Path.Combine(Application.persistentDataPath, "Data", $"{player[i].ToString()}", "opinion", $"{player[j].ToString()}.txt");
                fullStr += $"**{player[i].ToString()}'s opinion to {player[j].ToString()}**\n" + ReadFileToText(path) + "\n\n";
            }
            playerFeature[i] = fullStr;
        }
        string _allNpc = string.Join("\n\n", playerFeature);

        // TextAsset _memory_status = Resources.Load<TextAsset>("Data/Example");
        string _vote_rules = Resources.Load<TextAsset>("Common/vote_rules").text;

        sb.Append(_role);
        sb.Append(_situation.text);
        sb.Append(_progress);
        sb.Append("\n\n");
        sb.Append(_allNpc);
        sb.Append(_vote_rules);

        string result = sb.ToString();
        string candidateList = "";
        foreach(string cand in candidates)
        {
            candidateList += $"{cand}, ";
        }
        string candidateOr = "";
        foreach (string cand in candidates)
        {
            candidateOr += $"{cand}|";
        }
        candidateList.Substring(0, candidateList.Length - 2);
        // Debug.Log(candidateList);

        candidateOr.Substring(0, candidateOr.Length - 1);
        // Debug.Log(candidateOr);

        result = result.Replace("$CANDIDATELIST$", candidateList);
        result = result.Replace("$CANDIDATEOR$", candidateOr);

        return result;
    }
    #endregion

    public static string ReadFileToText(string fullPath)
    {
        string result = "";

        try
        {
            // 파일이 저장될 폴더 경로만 추출
            string directoryPath = Path.GetDirectoryName(fullPath);

            // 폴더가 없으면 생성
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                // Debug.Log($"폴더 생성됨: {directoryPath}");

                // 파일 쓰기
                File.WriteAllText(fullPath, "");
            }

            // 2. 파일이 존재하는지 확인
            if (File.Exists(fullPath))
            {
                // 파일이 있으면 읽기
                result = File.ReadAllText(fullPath);
            }
            else
            {
                // 파일이 없으면 빈 파일 생성 (또는 기본값 저장)
                File.WriteAllText(fullPath, "");
                // Debug.Log($"빈 파일 생성됨: {fullPath}");
            }
        }
        catch (System.Exception e)
        {
            // Debug.LogError($"파일 읽기 실패: {e.Message}");
        }

        return result;
    }
}
