using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreditsMenu : MonoBehaviour
{
    public List<Member> members;
    private Member member;
    [SerializeField] private Image imageMember;
    [SerializeField] private TextMeshProUGUI nameMember;
    [SerializeField] private TextMeshProUGUI roleMember;
    [SerializeField] private TextMeshProUGUI descriptionMember;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Animator animator;
    private float fadeDuration = 0.5f;

    [Serializable]
    public class Member
    {
        public Sprite image;
        public string memberName;
        public string memberRole;
        [TextArea(3, 6)] public string memberDescription;
    }

    private void OnEnable()
    {
        canvasGroup.alpha = 0f;
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;

        if (members.Count > 0)
        {
            member = members[0];
            FadeAndSetMember();
        }
    }

    public void SetMember(Member member)
    {
        this.member = member;
        animator.SetTrigger("Next");
        StartCoroutine(WaitForAnimation(2));
    }

    private IEnumerator WaitForAnimation( float time)
    {
        yield return new WaitForSecondsRealtime(time);
        FadeAndSetMember();
    }


    public void FadeAndSetMember()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;



        imageMember.sprite = member.image;
        nameMember.text = member.memberName;
        roleMember.text = member.memberRole;
        descriptionMember.text = member.memberDescription;

        canvasGroup.DOFade(1, fadeDuration).SetUpdate(true).OnComplete(() =>
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        });
    }

    public void NextMember()
    {
        Debug.Log("Next Member: " + member.memberName);

        int currentIndex = members.IndexOf(member);
        int nextIndex = (currentIndex + 1) % members.Count;

        canvasGroup.DOFade(0, fadeDuration).SetUpdate(true).OnComplete(() =>
        {
            SetMember(members[nextIndex]);
        });
    }
}
