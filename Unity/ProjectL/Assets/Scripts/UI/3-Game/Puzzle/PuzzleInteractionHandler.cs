using UnityEngine;

public class PuzzleInteractionHandler : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"{gameObject.name}: enter: {collision.gameObject.name}");
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log($"{gameObject.name}: exit: {collision.gameObject.name}");
    }
}
