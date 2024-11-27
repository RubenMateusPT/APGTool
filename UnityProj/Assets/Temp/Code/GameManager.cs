using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI endText;
    public bool gameEnded = true;
    private bool timeSlowActive = false;

    public GameObject doorrequestText;
    public Transform door;
    private bool isdooropen = false;

    private void Awake()
    {
        doorrequestText.SetActive(false);
        gameEnded = true;
        timeSlowActive = false;
        isdooropen = false;
        Time.timeScale = 1;
        endText.gameObject.SetActive(false);
    }

    private void Start()
    {
        gameEnded = false;
    }

    public void EndGame(bool won)
    {
        if (gameEnded)
            return;

        gameEnded = true;

        if (won)
            endText.text = "YOU WIN!";
        else
            endText.text = "GAME OVER!";

        endText.gameObject.SetActive(true);

        FindFirstObjectByType<NetworkManager>().SendScreenshoot(won);

        Time.timeScale = 0;
    }

   

    public void StartTimeSlow()
    {
        if (gameEnded || timeSlowActive)
            return;

        timeSlowActive = true;
        StartCoroutine(TimeSlow());
    }

    private IEnumerator TimeSlow()
    {
        while (Time.timeScale > 0.25f)
        {
            Time.timeScale -= 0.5f * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSecondsRealtime(2);

        while (Time.timeScale < 1.0f)
        {
            Time.timeScale += 2f * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        Time.timeScale = 1;
        timeSlowActive = false;
    }

    public void OpenDoor()
    {
        if (gameEnded || isdooropen)
            return;

        isdooropen = true;
        doorrequestText.SetActive(false);
        StartCoroutine(AnimateDoorOpening());
    }

    private IEnumerator AnimateDoorOpening()
    {
        while (door.eulerAngles.z > 270)
        {
            door.eulerAngles = new Vector3 (0,0, door.eulerAngles.z - 25 * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }

    private void Update()
    {
        if(gameEnded && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(0);
        }
    }
}
