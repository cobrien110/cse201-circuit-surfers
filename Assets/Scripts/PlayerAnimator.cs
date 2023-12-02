using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = this.gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setMoving(bool isTrue)
    {
        animator.SetBool("isMoving", isTrue);
    }

    public void setGrinding(bool isTrue)
    {
        animator.SetBool("isGrinding", isTrue);
    }

    public void triggerJump()
    {
        animator.SetTrigger("jumpTrigger");
    }

    public IEnumerator resetJumpTrigger()
    {
        yield return new WaitForSeconds(0.05f);
        animator.ResetTrigger("jumpTrigger");
    }
}
