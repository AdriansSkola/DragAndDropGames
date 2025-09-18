using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class TextAnimator : MonoBehaviour
{
    [SerializeField]
    private string _message;

    [SerializeField]
    private float _stringAnimationDuration;

    [SerializeField]
    private TextMeshProUGUI _animatedText;

    [SerializeField]
    private AnimationCurve _sizeCurve;

    [SerializeField]
    private float _sizeScale;

    [SerializeField]
    [Range(0.0001f, 1f)]
    private float _charAnimationDuration;

    private float _timeElapsed;

    private void Start()
    {
        StartCoroutine(RunAnimation(3));
    }

    IEnumerator RunAnimation(float waitForSeconds)
    {
        yield return new WaitForSeconds(waitForSeconds);

        float t = 0;
        while(t <= 1f)
        {
            EvaluateRichText(t);
            t = _timeElapsed / _stringAnimationDuration;
            _timeElapsed += Time.deltaTime;

            yield return null;
        }
    }

    void EvaluateRichText(float t)
    {
        _animatedText.text = "";

        for(int i = 0; i < _message.Length; i++)
        {
            _animatedText.text += EvaluateCharRichText(_message[i], _message.Length, i, t);
        }
    }

    private string EvaluateCharRichText(char c, int sLength, int cPosition, float t)
    {
        float startPoint = ((1 - _charAnimationDuration) / (sLength - 1)) * cPosition;
        float endPoint = startPoint + _charAnimationDuration;

        float subT = t.Map(startPoint, endPoint, 0, 1);

        string sizeStart = $"<size={_sizeCurve.Evaluate(subT) * _sizeScale}>";
        string sizeEnd = "</size>";

        return sizeStart + c + sizeEnd;
    }
}

public static class Extensions
{
    public static float Map(this float value, float fromLow, float fromHigh, float toLow, float toHigh)
    {
        return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
    }
}
