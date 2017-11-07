using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TextMesh))]
public class DynamicInfoScript : MonoBehaviour
{
    private static readonly float _AngleY = 1f;
    private static readonly float _AngleMultiplier = 4f;

    [SerializeField]
    Color textColor;

    [SerializeField]
    float animationTime;

    [SerializeField]
    AnimationCurve alphaOverTime;

    [SerializeField]
    AnimationCurve sizeOverTime;

    [SerializeField]
    AnimationCurve speedOverTime;

    [SerializeField]
    float redirectTime;

    Transform m_Transform;
    TextMesh m_Text;
    Vector3 moveDir;


    void Awake()
    {
        m_Transform = GetComponent<Transform>();
        m_Text = GetComponent<TextMesh>();
    }

    public void Initialize(int numToDisplay, Color color)
    {
        Initialize(numToDisplay.ToString(), color);
    }
    public void Initialize(string textToDisplay, Color color)
    {
        textColor = color;

        m_Text.text = textToDisplay;

        m_Transform.rotation = Camera.main.transform.rotation;

        moveDir.x = Random.Range(-_AngleY * _AngleMultiplier, _AngleY * _AngleMultiplier);
        moveDir.y = Random.Range(-_AngleY, _AngleY);
        moveDir.Normalize();

        StartCoroutine(AnimationRoutine());
    }

    IEnumerator AnimationRoutine()
    {
        float timer = 0f;
        bool hasRedirected = false;

        while (timer < AnimationTime)
        {
            yield return null;

            timer += Time.deltaTime;

            if (!hasRedirected && timer > RedirectTime)
            {
                //moveDir.y += Random.Range(-_Angle2, _Angle2);
                //moveDir.Normalize();

                hasRedirected = true;
            }

            textColor.a = alphaOverTime.Evaluate(timer);
            m_Text.color = textColor;

            m_Transform.localScale = Vector3.one * sizeOverTime.Evaluate(timer);

            m_Transform.position += m_Transform.TransformDirection(moveDir) * speedOverTime.Evaluate(timer) * Time.deltaTime;
        }

        m_Transform.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    float AnimationTime
    {
        get { return animationTime; }
        set { animationTime = Mathf.Clamp(value, 0f, value); }
    }
    float RedirectTime
    {
        get { return redirectTime; }
        set { redirectTime = Mathf.Clamp(value, 0f, value); }
    }


    void OnValidate()
    {
        AnimationTime = AnimationTime;
        RedirectTime = RedirectTime;
    }
}
