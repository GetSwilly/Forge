using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(TextMesh))]
[RequireComponent(typeof(FollowTarget))]
public class InteractableUIController : MonoBehaviour {

    TextMesh myText;
    public FollowTarget myFollow;

    void Awake()
    {
        myText = GetComponent<TextMesh>();
        myFollow = GetComponent<FollowTarget>();
    }

    public void Activate(Transform _newFollow, string newText)
    {
        Activate(_newFollow, Vector3.zero, newText);
    }
    public void Activate(Transform _newFollow, Vector3 _offset, string newText)
    {
        if(myFollow == null)
            myFollow = GetComponent<FollowTarget>();

        myFollow.TargetTransform = _newFollow;
        myFollow.TargetOffset = _offset;
        SetText(newText);
    }

    public void SetText(string newText)
    {
        if(myText == null)
            myText = GetComponent<TextMesh>();


        myText.text = newText;
    }
}
