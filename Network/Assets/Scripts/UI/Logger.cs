using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

public class Logger : MonoBehaviour
{
    [Tooltip("경고 색(노란색)")]
    public Color warningColor;
    [Tooltip("에러 색(빨강색)")]
    public Color errorColor;

    [Tooltip("로그창에 출력될 최대 줄 수(안 보이는 것 포함)")]
    private const int maxLineCount = 20;

    [Tooltip("문자열 합치기 위한 스트링 빌더")]
    private StringBuilder sb;

    [Tooltip("로그창에 출력될 모든 문자열 큐")]
    private Queue<string> logLines = new Queue<string>(maxLineCount + 1);

    private TextMeshProUGUI log;
    private TMP_InputField inputField;

    private void Awake()
    {
        Transform child = transform.GetChild(3);
        inputField = child.GetComponent<TMP_InputField>();

        inputField.onSubmit.AddListener((text) =>
        {
            Log(text);

            // 입력 완료되면 비우기
            inputField.text = string.Empty;
            // 포커스 다시 활성화
            inputField.ActivateInputField();
        });

        child = transform.GetChild(0);
        child = child.GetChild(0);
        child = child.GetChild(0);
        log = child.GetComponent<TextMeshProUGUI>();
        sb = new StringBuilder(maxLineCount + 1);
    }

    private void Start()
    {
        log.text = string.Empty;
    }

    /// <summary>
    /// 로거에 문자열을 추가
    /// </summary>
    /// <param name="message"></param>
    public void Log(string message)
    {
        // 강조할 부분들 강조하기
        message = HighlightSubString(message, '[', ']', errorColor);    // [] 사이에 있는 글자는 빨강색(errorColor)으로 출력하기
        message = HighlightSubString(message, '{', '}', warningColor);   // {} 사이에 있는 글자는 빨강색(warningColor)으로 출력하기

        // 입력 내용을 큐에 넣기(한줄 추가)
        logLines.Enqueue(message);

        if (logLines.Count > maxLineCount)
        {
            // 최대 줄 수를 넘어서면 하나 제거
            logLines.Dequeue();
        }

        // 스트링 빌더로 큐 안에 있는 문자열 조합하기
        sb.Clear();

        foreach (string line in logLines)
        {
            sb.AppendLine(line);
        }

        log.text = sb.ToString();
    }

    /// <summary>
    /// 인풋 필드에 포커스를 주는 함수
    /// </summary>
    public void InputFieldFocusOn()
    {
        inputField.ActivateInputField();
    }

    /// <summary>
    /// 지정된 괄호 사이에 있는 글자를 강조
    /// </summary>
    /// <param name="source">원문</param>
    /// <param name="open">여는 괄호</param>
    /// <param name="close">닫는 괄호</param>
    /// <param name="color">강조할 부분의 색</param>
    /// <returns>강조가 완료된 문자열</returns>
    private string HighlightSubString(string source, char open, char close, Color color)
    {
        string result = source;

        // source 문자열 안에 있는 괄호가 쌍으로 잘 맞는지 확인(완전히 맞을 때만 강조 처리)
        if (IsPair(source, open, close))
        {
            // 괄호를 기준으로 문자열을 나누기
            string[] split = source.Split(open, close);
            // 새겡 맞게 16진수 문자열 만들기
            string colorText = ColorUtility.ToHtmlStringRGB(color);
            // IsPair가 실패일 때는 result에 source가 있는게 맞기 때문에 여기서 초기화
            result = string.Empty;

            // 나누어 진 것들을 하나씩 처리
            for (int i = 0; i < split.Length; i++)
            {
                // 나누어진 문자열을 result에 추가
                result += split[i];

                // 마지막 문자열은 제외하고
                if (i != split.Length - 1)
                {
                    if (i % 2 == 0)
                    {
                        // i가 짝수다 == 이 이후에는 괄호가 열렸을 것이다.
                        result += $"<#{colorText}>{open}";     // 괄호가 열렸을 때부터 색상 변경
                    }
                    else
                    {
                        // i가 홀수다 == 이 이후에는 괄호가 닫혔을 것이다.
                        result += $"{close}</color>";        // 생상 변경 정지
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// source에 지정된 괄호가 정확한 조건으로 들어있는지 체크
    /// </summary>
    /// <param name="source">원문</param>
    /// <param name="open">여는 괄호</param>
    /// <param name="close">닫는 괄호</param>
    /// <returns>t : 조건을 모두 만족. f : 불만족</returns>
    private bool IsPair(string source, char open, char close)
    {
        // 정확한 조건 : 열리면 닫혀야 한다. 연속해서 열거나 닫는 것은 금지
        bool result = true;
        // 괄호 갯수
        int count = 0;

        // source의 모든 글자 확인
        for (int i = 0; i < source.Length; i++)
        {
            // 여는 괄호이거나 닫는 괄호일 때
            if (source[i] == open || source[i] == close)
            {
                // 괄호 갯수 증가
                count++;

                if (count % 2 == 1)
                {
                    // count가 홀수면 열려야 한다.
                    if (source[i] != open)
                    {
                        // 열려야 하는 타이밍인데 열리지 않음녀 실패
                        result = false;

                        break;
                    }
                }
                else
                {
                    // count가 짝수면 닫혀야 한다.
                    if (source[i] != close)
                    {
                        // 당혀야 하는 타이밍인데 닫히지 않으면 실패
                        result = false;

                        break;
                    }
                }
            }
        }

        // count는 짝수이어야 열리고 닫힌 쌍이 맞다.
        // count가 0인 경우는 HighlightSubString에서 변경할 필요가 없으니 f
        if (count % 2 != 0 || count == 0)
        {
            result = false;
        }

        return result;
    }
}
