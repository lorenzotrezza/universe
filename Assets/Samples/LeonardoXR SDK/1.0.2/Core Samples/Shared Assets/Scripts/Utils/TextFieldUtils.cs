using UnityEngine;
using UnityEngine.UI;

public class TextFieldUtils : MonoBehaviour
{
    [SerializeField] private Text m_text;

    private void Awake()
    {
        m_text = GetComponent<Text>();
    }

    public void SetText(float value)
    {
        SetText(value.ToString());
    }

    public void SetText(string text)
    {
        m_text.text = text;
    }
}
