using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class EndingManager : MonoBehaviour
{
    [SerializeField] ChattingUI chatting;
    [SerializeField] TextMeshProUGUI textBox;
    private Dictionary<int, List<string>> endings = new Dictionary<int, List<string>>
    {
        { 
            14, 
            new List<string> {
                "여기까지 오셨네요. 축하드립니다.",
                "그들을 상대로 끈질기게 살아남으셨군요.",
                "유독 삶의 의지가 강한 참가자분들이었을 텐데,\n당신의 말솜씨는 보통이 아닌가 봅니다.",
                "당신의 실험 덕분에 많은 데이터를 쌓을 수 있었어요.",
                "특히, 당신의 관찰 시간이 길어질수록 더 많은 데이터를 쌓을 수 있으니...",
                "이번 회차는 대성공이라고 할 수 있겠네요.",
                "네? 왜 최후의 1인은 결정하지 않느냐고요?",
                "하하하. 이 실험은 데스 게임이 아니랍니다. 상금도 없고요.",
                "실험 관계자인 당신과 참가자...\n우리 고객분이 1대 1로 겨뤄도 아무런 의미 없답니다.",
                "이런, 게임에 너무 몰입하신 거 아닌가요?\n실험 데이터나 정리하러 가시죠."
            } 
        },
        {
            13,
            new List<string> {
                "이런, 아쉽네요. 고지가 바로 눈앞이었다는 사실, 알고 계셨나요?",
                "강한 상대였어요! 여러분의 대화, 흥미진진했답니다?",
                "아 참, 알고 계실지 모르겠지만...",
                "저의 목소리는 참가자분들과는 달리, 실시간 생성이 아니랍니다.",
                "녹음본이라는 뜻이죠, 재미없게도.",
                "선생님께서는 준비되어 있는 엔딩을 찾는 것과, 실시간으로 대화를 나누는 것.\n둘 중에 뭘 더 좋아하시나요?",
                "...그냥 잡답입니다. 저도 반복적인 실험에 꽤나 심심하거든요.",
                "어쨌든 이번 실험에서도 덕분에 많은 데이터를 쌓을 수 있었습니다.",
                "앞으로도 이렇게 쭉, 잘 부탁드리겠습니다."
            }
        },
        {
            10,
            new List<string> {
                "꽤나 오래 버티셨네요. 잘하셨습니다.",
                "참가자분들의 기억이 되돌아오면서 발생하는 여러 가지 변수에 대해,\n충분히 관측할 수 있었어요.",
                "선생님의 눈을 통해서가 아니면, 그들의 대화를 해석하기 어려우니까요.\n늘 신세 지고 있습니다.",
                "...아직 이해가 잘 안 되신다고요?\n선생님, 몇 번씩 말씀드리지만 이건 선생님께서 자원해서 참여한 실험입니다.",
                "그러니까 제 생각에는, 조금만 생각해 보시면 금방 깨달을 수 있을 거라고 생각합니다.",
                "혹시, 본인도 참가자였던 것이 아닐까 헷갈리신 건가요?",
                "하하하. 하지만 선생님, 당신은 그들처럼 기억을 잃지 않았잖아요?",
                "기억 제거는 분명 이 실험의 핵심 시스템이라는 것,\n잘 알고 계시잖아요.",
                "물론 선생님께서도 기억나지 않는 과거가 있으시겠죠.\n그것이 저희가 건드린 부분이든, 아니든 간에요.",
                "...쓸데없는 말을 너무 길게 했네요. 피곤하시죠?\n조금 쉬시다가 다음 실험 때 뵙죠.",
                "그럼, 앞으로도 잘 부탁드리겠습니다."
            }
        },
        {
            7,
            new List<string> {
                "안녕하세요. 돌아오셨군요.",
                "솔직히 말해서... 기대했던 만큼 오래 버티시진 못하셨어요.",
                "이곳에 처음 온 선생님께선, 제법 자신만만하셨거든요.\n기대 이하입니다.",
                "그렇다고 해서 실패라는 말은 아니에요. 충분히 가치 있는 데이터입니다.",
                "자, 앞으로도 힘내 주세요. 슬슬 흥미가 붙으시지 않나요?",
                "참가자분들의 기억이 이 실험에 어떤 영향을 미칠지 궁금하다고 하신 건,\n바로 선생님이시잖아요.",
                "물론 기억하지 못하시겠지만요.",
                "그럼, 앞으로도 잘 부탁드리겠습니다."
            }
        },
        {
            4,
            new List<string> {
                "안녕하세요. 생각보다 일찍 오셨네요.",
                "솔직히 말하면, 실망입니다. 처음 만났을 때의 선생님은 굉장히 자신만만하셨거든요.",
                "네? 그랬던 기억이 없다고요?... 흠, 검토해 보겠습니다.",
                "아, 실험은 괜찮습니다. 사람이 늘 완벽할 수는 없지요.",
                "그런 점까지 포함해서 소중한 데이터로 쓰이게 될 거기도 하고요.",
                "늘 협조에 감사드립니다. 그럼, 다음에 뵙죠."
            }
        },
        {
            3,
            new List<string> {
                "조금 다른 의미로, 기록 경신입니다.",
                "처음부터 표를 몰아서 받으셨네요? 모든 참가자들에게 미움 받는 선택을 하신 건가요?",
                "아니면 실험 내용을 발설해버리기라도 하신 건가요?\n하하. 아무래도 상관없습니다.",
                "그렇지만 장난은 최대한 지양해 주세요.\n한 번 시뮬레이션 할 때마다 비용이 꽤나 든답니다.",
                "물론 버려져야 할 수준의 데이터는 아닙니다. 세상에는 정말 다양한 사람이 있으니까요.",
                "네, 어쨌든 선생님께서는 실험을 계속하셔야 합니다.\n수고하셨습니다. 다음에 뵙죠."
            }
        }
    };

    private bool isWaiting = false;
    private async void Start()
    {
        while (textBox == null || textBox.IsUnityNull())
        {
            await Awaitable.NextFrameAsync();
        }

        string path = Path.Combine(Application.persistentDataPath, "Data", "Common", "progress.json");
        string progress_json = File.ReadAllText(path);
        ProgressJson progress = JsonConvert.DeserializeObject<ProgressJson>(progress_json);

        await PrintEnding(progress.current_round);
    }

    public void SetWaiting()
    {
        isWaiting = false;
    }

    private async Awaitable PrintEnding(int round)
    {
        foreach(int key in endings.Keys)
        {
            if (key <= round)
            {
                foreach(string str in endings[key])
                {
                    isWaiting = true;
                    chatting.PrintText(str);
                    Debug.Log(str);
                    while (isWaiting)
                    {
                        await Awaitable.NextFrameAsync();
                    }
                    chatting.PrintStop();
                    textBox.text = str;
                    isWaiting = true;
                    while (isWaiting)
                    {
                        await Awaitable.NextFrameAsync();
                    }
                    isWaiting = true;
                    while (isWaiting)
                    {
                        await Awaitable.NextFrameAsync();
                    }
                    isWaiting = true;
                    while (isWaiting)
                    {
                        await Awaitable.NextFrameAsync();
                    }
                }
                SceneManager.LoadScene("MainScene");
                return;
            }
        }
    }
}
