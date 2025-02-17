using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;
using Unity.VisualScripting;

public class Logger : MonoBehaviour
{
    [Tooltip("�����")]
    public Color warningColor;
    [Tooltip("������")]
    public Color errorColor;

    private const int maxLineCount = 20;

    private StringBuilder sb;

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

    public void Log(string message)
    {
        // "[����] {���}"
        // [] ���̿� �ִ� ���ڴ� ������(errorColor)���� ����ϱ�
        // {} ���̿� �ִ� ���ڴ� �����(warningColor)���� ����ϱ�

        logLines.Enqueue(message);

        if (logLines.Count > maxLineCount)
        {
            logLines.Dequeue();
        }

        sb.Clear();

        foreach (string line in logLines)
        {
            sb.AppendLine(line);
        }

        log.text = sb.ToString();
    }

    public void InputFieldFocusOn()
    {
        inputField.ActivateInputField();
    }
}
