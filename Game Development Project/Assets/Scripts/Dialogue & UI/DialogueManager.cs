using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// https://www.youtube.com/watch?v=_nRzoTzeyxU&t=65s&ab_channel=Brackeys
public class DialogueManager : MonoBehaviour
{
    [SerializeField] private PlayerController playerController = null;
    private Queue<string> sentences = null;
    public Text nameText = null, dialogueText = null;
    public Animator animator;
    public Button continueButton = null;
    
    // Start is called before the first frame update
    void Start()
    {
        continueButton = playerController.continueButton;
        sentences = new Queue<string>();
    }

    public void StartDialogue(Dialogue dialogue)
    {
        animator.SetBool("isOpen", true);
        continueButton.Select(); // enable the button for gamepad

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
            playerController.inConversation = false;
            playerController.lockInput = false;
            playerController.ConversationCheck();
            return;
        }

        string sentence = sentences.Dequeue(); // if there are sentences left to say
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        const float beat = 0.01f;
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            // add letter to dialogue.text one by one
            dialogueText.text += letter;
            yield return new WaitForSeconds(beat); // wait before adding a letter
        }
    }

    void EndDialogue()
    {
        Debug.Log("End of conversation");
        EventSystem.current.SetSelectedGameObject(null); // disable the continue button for gamepad
        animator.SetBool("isOpen", false);
    }
}