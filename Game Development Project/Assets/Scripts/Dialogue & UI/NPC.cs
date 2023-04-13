using UnityEngine;

public class NPC : MonoBehaviour
{
    public bool dialogueStarted = false;
    public Dialogue dialogue = null;
    

    public void TriggerDialogue()
    {
        FindObjectOfType<DialogueManager>().StartDialogue(dialogue);
    }
}