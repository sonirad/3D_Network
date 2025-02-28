using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

public class Logger : MonoBehaviour
{
    [Tooltip("��� ��(�����)")]
    public Color warningColor;
    [Tooltip("���� ��(������)")]
    public Color errorColor;

    [Tooltip("�α�â�� ��µ� �ִ� �� ��(�� ���̴� �� ����)")]
    private const int maxLineCount = 20;

    [Tooltip("���ڿ� ��ġ�� ���� ��Ʈ�� ����")]
    private StringBuilder sb;

    [Tooltip("�α�â�� ��µ� ��� ���ڿ� ť")]
    private Queue<string> logLines = new Queue<string>(maxLineCount + 1);

    #region ������Ʈ

    private TextMeshProUGUI log;
    private TMP_InputField inputField;

    #endregion

    private void Awake()
    {
        Transform child = transform.GetChild(3);
        inputField = child.GetComponent<TMP_InputField>();

        inputField.onSubmit.AddListener((text) =>
        {
            // �Է��� ������ ù��° ���ڰ� '/'��� �ܼ� ����� ó���Ѵ�.
            if (text[0] == '/')
            {
                ConSoleCommand(text);
            }
            else
            {
                if (GameManager.Instance.Player != null)
                {
                    GameManager.Instance.Player.SendChat(text);
                }
                else
                {
                    Log(text);
                }
            }
            
            // �Է� �Ϸ�Ǹ� ����
            inputField.text = string.Empty;
            // ��Ŀ�� �ٽ� Ȱ��ȭ
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
    /// �ΰſ� ���ڿ��� �߰�
    /// </summary>
    /// <param name="message"></param>
    public void Log(string message)
    {
        // ������ �κе� �����ϱ�
        message = HighlightSubString(message, '[', ']', errorColor);    // [] ���̿� �ִ� ���ڴ� ������(errorColor)���� ����ϱ�
        message = HighlightSubString(message, '{', '}', warningColor);   // {} ���̿� �ִ� ���ڴ� ������(warningColor)���� ����ϱ�

        // �Է� ������ ť�� �ֱ�(���� �߰�)
        logLines.Enqueue(message);

        if (logLines.Count > maxLineCount)
        {
            // �ִ� �� ���� �Ѿ�� �ϳ� ����
            logLines.Dequeue();
        }

        // ��Ʈ�� ������ ť �ȿ� �ִ� ���ڿ� �����ϱ�
        sb.Clear();

        foreach (string line in logLines)
        {
            sb.AppendLine(line);
        }

        log.text = sb.ToString();
    }

    /// <summary>
    /// ��ǲ �ʵ忡 ��Ŀ���� �ִ� �Լ�
    /// </summary>
    public void InputFieldFocusOn()
    {
        inputField.ActivateInputField();
    }

    /// <summary>
    /// ������ ��ȣ ���̿� �ִ� ���ڸ� ����
    /// </summary>
    /// <param name="source">����</param>
    /// <param name="open">���� ��ȣ</param>
    /// <param name="close">�ݴ� ��ȣ</param>
    /// <param name="color">������ �κ��� ��</param>
    /// <returns>������ �Ϸ�� ���ڿ�</returns>
    private string HighlightSubString(string source, char open, char close, Color color)
    {
        string result = source;

        // source ���ڿ� �ȿ� �ִ� ��ȣ�� ������ �� �´��� Ȯ��(������ ���� ���� ���� ó��)
        if (IsPair(source, open, close))
        {
            // ��ȣ�� �������� ���ڿ��� ������
            string[] split = source.Split(open, close);
            // ���� �°� 16���� ���ڿ� �����
            string colorText = ColorUtility.ToHtmlStringRGB(color);
            // IsPair�� ������ ���� result�� source�� �ִ°� �±� ������ ���⼭ �ʱ�ȭ
            result = string.Empty;

            // ������ �� �͵��� �ϳ��� ó��
            for (int i = 0; i < split.Length; i++)
            {
                // �������� ���ڿ��� result�� �߰�
                result += split[i];

                // ������ ���ڿ��� �����ϰ�
                if (i != split.Length - 1)
                {
                    if (i % 2 == 0)
                    {
                        // i�� ¦���� == �� ���Ŀ��� ��ȣ�� ������ ���̴�.
                        result += $"<#{colorText}>{open}";     // ��ȣ�� ������ ������ ���� ����
                    }
                    else
                    {
                        // i�� Ȧ���� == �� ���Ŀ��� ��ȣ�� ������ ���̴�.
                        result += $"{close}</color>";        // ���� ���� ����
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// source�� ������ ��ȣ�� ��Ȯ�� �������� ����ִ��� üũ
    /// </summary>
    /// <param name="source">����</param>
    /// <param name="open">���� ��ȣ</param>
    /// <param name="close">�ݴ� ��ȣ</param>
    /// <returns>t : ������ ��� ����. f : �Ҹ���</returns>
    private bool IsPair(string source, char open, char close)
    {
        // ��Ȯ�� ���� : ������ ������ �Ѵ�. �����ؼ� ���ų� �ݴ� ���� ����
        bool result = true;
        // ��ȣ ����
        int count = 0;

        // source�� ��� ���� Ȯ��
        for (int i = 0; i < source.Length; i++)
        {
            // ���� ��ȣ�̰ų� �ݴ� ��ȣ�� ��
            if (source[i] == open || source[i] == close)
            {
                // ��ȣ ���� ����
                count++;

                if (count % 2 == 1)
                {
                    // count�� Ȧ���� ������ �Ѵ�.
                    if (source[i] != open)
                    {
                        // ������ �ϴ� Ÿ�̹��ε� ������ ������ ����
                        result = false;

                        break;
                    }
                }
                else
                {
                    // count�� ¦���� ������ �Ѵ�.
                    if (source[i] != close)
                    {
                        // ������ �ϴ� Ÿ�̹��ε� ������ ������ ����
                        result = false;

                        break;
                    }
                }
            }
        }

        // count�� ¦���̾�� ������ ���� ���� �´�.
        // count�� 0�� ���� HighlightSubString���� ������ �ʿ䰡 ������ f
        if (count % 2 != 0 || count == 0)
        {
            result = false;
        }

        return result;
    }

    /// <summary>
    /// ���߿� �ܼ� ��ɾ� ó��
    /// </summary>
    /// <param name="command">�Է¹��� ��ɾ�</param>
    private void ConSoleCommand(string command)
    {
        int space = command.IndexOf(' ');
        // ù��° ~ space ��ġ �ձ���
        string commandToken = command.Substring(0, space);
        // ���� �ҹ��ڷ� �ٲٱ�
        commandToken = commandToken.ToLower();
        // space ��ġ ���� ĭ ~ ������
        string dataToken = command.Substring(space + 1);
        GameManager gameManager = GameManager.Instance;

        switch (commandToken)
        {
            case "/setname":
                gameManager.UserName = dataToken;

                if (gameManager.PlayerDeco != null)
                {
                    // ������ ���� �ʾ����� ����.
                    gameManager.PlayerDeco.SetName(dataToken);
                }

                break;
            case "/setcolor":
                string[] colorStrings = dataToken.Split(',', ',');
                float[] colorValues = new float[3] { 0, 0, 0 };
                int count = 0;

                foreach (string color in colorStrings)
                {
                    if (color.Length == 0)
                    {
                        continue;
                    }

                    // �� 3���� ó��
                    if (count > 2)
                    {
                        break;
                    }

                    if (!float.TryParse(color, out colorValues[count]))
                    {
                        colorValues[count] = 0;
                    }

                    count++;
                }

                for (int i = 0; i < colorValues.Length; i++)
                {
                    colorValues[i] = Mathf.Clamp01(colorValues[i]);
                }

                Color resultColor = new Color(colorValues[0], colorValues[1], colorValues[2]);

                if (gameManager.PlayerDeco != null)
                {
                    // ������ ���� �ʾ����� ����.
                    gameManager.PlayerDeco.SetColor(resultColor);
                }

                gameManager.UserColor = resultColor;

                break;
        }
    }
}
