using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using APG.Unity.Managers;
using Platformer.Gameplay;
using Platformer.Mechanics;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Platformer.Core.Simulation;
using Random = UnityEngine.Random;

public class Audience : MonoBehaviour
{
    //player
    public GameObject Player;
    public float PlayerRadius;
    private PlayerController playerController;
    public float PlayerSuperJumpForce;
    private Coroutine invertControlsCR,doubleSpeedCR, halfSpeedCR, stopSpeedCR, cantDieCR;

    public GameObject EnemiesParante;
    public GameObject EnemyPrefab;

    public bool canEnableBridge;
    public void SetEnableBridgeFlag(bool val) => canEnableBridge = val;
    public GameObject Bridge;

    private bool isEnding = false, ended = false;

    public TextMeshProUGUI finaltext;
    public GameObject final;

    private void Awake()
    {
        canEnableBridge = false;

        playerController = Player.GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        if(Input.GetKeyDown(KeyCode.F8))
            EnableBridge();

        if(Input.GetKeyDown(KeyCode.F9))
            End(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Player.transform.position, PlayerRadius);
    }

    // Player
    public void PlayerSuperJump()
    {
        if (isEnding)
            return;

        if (playerController.jumpState != PlayerController.JumpState.Grounded)
            return;

        playerController.jumpTakeOffSpeed = PlayerSuperJumpForce;
        playerController.jumpState = PlayerController.JumpState.PrepareToJump;
    }

    public void PlayerDoubleSpeed()
    {
        if (isEnding)
            return;

        if (playerController.maxSpeed > 3)
            return;

        playerController.maxSpeed = 6;

        if(doubleSpeedCR != null)
            StopCoroutine(doubleSpeedCR);
        doubleSpeedCR = StartCoroutine(DoubleSpeedEffet());
    }

    private IEnumerator DoubleSpeedEffet()
    {
        yield return new WaitForSeconds(5);
        playerController.maxSpeed = 3;
    }

    public void PlayerHalfSpeed()
    {
        if (isEnding)
            return;

        if (playerController.maxSpeed < 3)
            return;

        playerController.maxSpeed = 1.5f;
        if (halfSpeedCR != null)
            StopCoroutine(halfSpeedCR);
        halfSpeedCR = StartCoroutine(HalfSpeedEffet());
    }

    private IEnumerator HalfSpeedEffet()
    {
        yield return new WaitForSeconds(3);
        playerController.maxSpeed = 3;
    }

    public void StopPlayer()
    {
        if (isEnding)
            return;

        if (playerController.maxSpeed < 3)
            return;

        playerController.maxSpeed = 0;

        if (stopSpeedCR != null)
            StopCoroutine(stopSpeedCR);
        stopSpeedCR = StartCoroutine(StopSpeedEffet());
    }

    private IEnumerator StopSpeedEffet()
    {
        yield return new WaitForSeconds(3);
        playerController.maxSpeed = 3;
    }

    public void PlayerCantDie()
    {
        if (isEnding)
            return;

        if (!playerController.canBeKilled)
            return;

        playerController.canBeKilled = false;
        playerController.GetComponent<SpriteRenderer>().color = Color.yellow;

        if (cantDieCR != null)
            StopCoroutine(cantDieCR);
        cantDieCR = StartCoroutine(CantDieEffet());
    }
    private IEnumerator CantDieEffet()
    {
        yield return new WaitForSeconds(5);
        playerController.GetComponent<SpriteRenderer>().color = Color.cyan;

        yield return new WaitForSeconds(1);
        playerController.canBeKilled = true;
    }

    public void PlayerInvertControls()
    {
        if (isEnding)
            return;

        if (playerController.invertControlls)
            return;

        playerController.invertControlls = true;
        playerController.GetComponent<SpriteRenderer>().color = Color.red;

        if (invertControlsCR != null)
            StopCoroutine(invertControlsCR);
        invertControlsCR = StartCoroutine(InvertControlsEffect());
    }
    private IEnumerator InvertControlsEffect()
    {
        yield return new WaitForSeconds(5);
        playerController.GetComponent<SpriteRenderer>().color = Color.cyan;

        yield return new WaitForSeconds(0.5f);
        playerController.invertControlls = false;
    }

    public void KillPlayer()
    {
        if (isEnding)
            return;

        Schedule<PlayerDeath>();
    }

    //Enemies

    public void SpawnEnemy(int amount)
    {
        if (isEnding)
            return;

        if (amount <= 0 || amount > 30)
            return;

        var selectPosition = new float[30];
        for (int i = 0; i < amount; i++)
        {
            float xOffset = 0;
            do
            {
                xOffset = Random.Range(2.5f, 5.0f);
                xOffset *= Random.Range(0, 2) == 0 ? -1 : 1;
            } while (selectPosition.Contains(xOffset));

            selectPosition[i] = xOffset;

            var enemy = GameObject.Instantiate(EnemyPrefab,
                new Vector2(Player.transform.position.x + xOffset, Player.transform.position.y + 2.5f),
                Quaternion.identity, EnemiesParante.transform);

            enemy.layer = 6;
            enemy.GetComponent<AnimationController>().enabled = false;
        }
    }

    public void KillAllEnemies()
    {
        if (isEnding)
            return;

        var enemies = Physics2D.OverlapCircleAll(Player.transform.position, PlayerRadius, LayerMask.GetMask("Enemy"));

        if (enemies.Length == 0)
            return;

        foreach (var enemy in enemies)
        {
            Schedule<EnemyDeath>().enemy = enemy.GetComponent<EnemyController>();
        }
        
    }

    public void KillEnemy(int amount)
    {
        if (isEnding)
            return;

        var enemies = Physics2D.OverlapCircleAll(Player.transform.position, PlayerRadius, LayerMask.GetMask("Enemy"));
        
        if (enemies.Length == 0)
            return;

        if (amount >= enemies.Length)
            amount = enemies.Length;

        if (amount == enemies.Length)
        {
            foreach (var enemy in enemies)
            {
                Schedule<EnemyDeath>().enemy = enemy.GetComponent<EnemyController>();
            }
        }
    }

    //World
    public void EnableBridge()
    {
        if (isEnding)
            return;

        if (!canEnableBridge)
            return;

        if (Bridge.activeSelf)
            return;

        Bridge.SetActive(true);
    }


    private string[] helpCodes =
    {
        "I really like the player"
    };

    private string helpCode = String.Empty;

    private string[] denyCodes =
    {
        "I want the player to fail"
    };
    private string denyCode = String.Empty;

    public void DoEnding()
    {
        if (isEnding)
            return;

        isEnding = true;

        helpCode = helpCodes[Random.Range(0, helpCodes.Length)];
        denyCode = denyCodes[Random.Range(0, denyCodes.Length)];

        FindFirstObjectByType<APGManager>().SendRequest("End", $"Audience, its time for the final challenge!\n" +
                                                               $"You can either help or kill the player!\n" +
                                                               $"The first to write the correct code will decide the player fate!\n\n" +
                                                               $"HELP: \"/command end , {helpCode}\"\n" +
                                                               $"KILL: \"/command end , {denyCode}\"\n");
    }


    private string[] wrongCode =
    {
        "Uh Oh. Misspelled Code!",
        "Bad Code!"
    };
    public void Endgame(string keycode)
    {
        if(ended)
            return;

        if(!isEnding)
            return;

        bool isHelpCode = false;
        bool isDenyCode = false;
        string code = keycode;
        code = code.TrimStart();
        code = code.TrimEnd();


        foreach (var help in helpCodes)
        {
            if (help.ToUpperInvariant() == code.ToUpperInvariant())
            {
                End(true);
                return;
            }
        }

        foreach (var deny in denyCodes)
        {
            if (deny.ToUpperInvariant() == code.ToUpperInvariant())
            {
                End(false);
                return;
            }
        }
            //nothing happens
            finaltext.text = wrongCode[Random.Range(0,wrongCode.Length)];
        
    }

    public void End(bool playerwon)
    {
        if (ended)
            return;

        ended = true;

        finaltext.text = "The Audience has \"DECIDED\"";

        StopAllCoroutines();

        if (playerwon)
            StartCoroutine(OnGoodEnd());
        else
            StartCoroutine(OnBadEnd());
    }

    private IEnumerator OnGoodEnd()
    {
        yield return new WaitForSeconds(3);

        finaltext.text = "You have been";
        for (int i = 0; i < 6; i++)
        {
            finaltext.text += ".";
            yield return new WaitForSeconds(1);
            if (i == 3)
            {
                finaltext.text = "You have been";
                yield return new WaitForSeconds(1);
            }
        }

        FindFirstObjectByType<APGManager>().SendScreenShoot(string.Empty);
        finaltext.text = "You have been\nSAFED!";

        yield return new WaitForSeconds(3);

        final.SetActive(false);
    }

    private IEnumerator OnBadEnd()
    {
        yield return new WaitForSeconds(3);

        finaltext.text = "You have been";
        for (int i = 0; i < 6; i++)
        {
            finaltext.text += ".";
            yield return new WaitForSeconds(1);
            if (i == 3)
            {
                finaltext.text = "You have been";
                yield return new WaitForSeconds(1);
            }
        }

        FindFirstObjectByType<APGManager>().SendScreenShoot(string.Empty);
        finaltext.text = "You have been\nDEMISED!";

        yield return new WaitForSeconds(3);

        finaltext.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.25f);

        /*for (float i = -5; i < 5; i++)
        {
            var enemy = GameObject.Instantiate(EnemyPrefab,
                new Vector2(Player.transform.position.x + (i/10), Player.transform.position.y + 2.5f),
                Quaternion.identity, EnemiesParante.transform);

            enemy.layer = 6;
            enemy.GetComponent<AnimationController>().enabled = false;
        }*/

        Schedule<PlayerDeath>();

        yield return new WaitForSeconds(0.25f);

        FindFirstObjectByType<APGManager>().SendScreenShoot(string.Empty);
        final.SetActive(false);
    }
}
