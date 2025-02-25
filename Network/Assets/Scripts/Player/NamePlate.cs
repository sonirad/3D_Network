using UnityEngine;
using TMPro;

public class NamePlate : MonoBehaviour
{
    private TextMeshPro naemText;

    private void Awake()
    {
        naemText = GetComponentInChildren<TextMeshPro>();
    }

    private void LateUpdate()
    {
        transform.rotation = Camera.main.transform.rotation;
    }

    public void SetName(string name)
    {
        naemText.text = name;
    }
}
