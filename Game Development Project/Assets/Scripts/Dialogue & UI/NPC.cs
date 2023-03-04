using UnityEngine;

public class NPC : MonoBehaviour
{
    public Dialogue dialogue = null;

    public void TriggerDialogue()
    {
        FindObjectOfType<DialogueManager>().StartDialogue(dialogue);
    }
}