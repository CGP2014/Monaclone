using UnityEngine;

public class LevelExit : MonoBehaviour
{
    public bool returnToBar = true;
    public string nextLevel;
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.gameObject.CompareTag("Player")) return;
        
        if(nextLevel != string.Empty)
            PlayerPrefs.SetString("CurrentLevel", nextLevel);
        
        if(returnToBar)
            LevelManager.ChangeScene("Bar Scene");
        else
            LevelManager.Instance.ChangeScene();
    }
}
