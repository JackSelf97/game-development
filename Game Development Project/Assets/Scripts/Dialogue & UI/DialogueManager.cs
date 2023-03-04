using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// https://www.youtube.com/watch?v=_nRzoTzeyxU&t=65s&ab_channel=Brackeys
public class DialogueManager : MonoBehaviour
{
    public Text nameText = null, dialogueText = null;
    public Animator animator;
    private Queue<string> sentences = null;

    // Start is called before the first frame update
    void Start()
    {
        sentences = new Queue<string>();
    }

    public void StartDialogue(Dialogue dialogue)
    {
        animator.SetBool("isOpen", true);

        Debug.Log("Starting conversation with " + dialogue.name);
        nameText.text = dialogue.name;
        sentences.Clear();

        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue(); // if there are sentences left to say
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            // add letter to dialogue.text one by one
            dialogueText.text += letter;
            yield return null; // wait a single frame
        }
    }

    void EndDialogue()
    {
        Debug.Log("End of conversation");
        animator.SetBool("isOpen", false);
    }
}
