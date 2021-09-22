using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using Text = UnityEngine.UI.Text;

// Enumerator to keep track of current battle state
public enum BattleState 
{
    START,
    PLAYER,
    ENEMY,
    WON,
    LOST
}

/// <summary>
/// Battle System Class
/// Handles the flow of combat for the battle scene.
/// 
/// </summary>
public class BattleSystem : MonoBehaviour
{
    // ======= PUBLIC VARIABLES ======= //
    // Unit Prefabs to be spawned in
    public GameObject playerPrefab;
    public GameObject rPlayerPrefab;
    public GameObject gPlayerPrefab;
    public GameObject bPlayerPrefab;
    public GameObject enemyPrefab;
    public GameObject rEnemyPrefab;
    public GameObject gEnemyPrefab;
    public GameObject bEnemyPrefab;

    // Transforms for Unit starting positions
    public Transform playerStartPos;
    public Transform rPlayerStartPos;
    public Transform gPlayerStartPos;
    public Transform bPlayerStartPos;
    public Transform enemyStartPos;
    public Transform rEnemyStartPos;
    public Transform gEnemyStartPos;
    public Transform bEnemyStartPos;

    // UI Action Buttons 
    public Button attackButton;
    public Button magicButton;
    public Button defendButton;
    public Button itemButton;

    // Materials for Shaders
    public Material defaultMaterial;
    public Material spriteOutlineMaterial;

    // ======= PRIVATE VARIABLES ======= //
    // Battle state enum
    [SerializeField] private BattleState _battleState;

    // Unit Game Objects
    private GameObject _playerGO;
    private GameObject _playerHealthBarGO;
    private GameObject _rPlayerGO;
    private GameObject _rPlayerHealthBarGO;
    private GameObject _gPlayerGO;
    private GameObject _gPlayerHealthBarGO;
    private GameObject _bPlayerGO;
    private GameObject _bPlayerHealthBarGO;
    private GameObject _enemyGO;
    private GameObject _enemyHealthBarGO;
    private GameObject _rEnemyGO;
    private GameObject _rEnemyHealthBarGO;
    private GameObject _gEnemyGO;
    private GameObject _gEnemyHealthBarGO;
    private GameObject _bEnemyGO;
    private GameObject _bEnemyHealthBarGO;

    // Unit Script Components
    private Player _player;
    private Player _rPlayer;
    private Player _gPlayer;
    private Player _bPlayer;
    private Enemy_FlyingEye _enemy;
    private Enemy_FlyingEye _rEnemy;
    private Enemy_FlyingEye _gEnemy;
    private Enemy_FlyingEye _bEnemy;

    // Unit Lists
    private List<GameObject> _unitGOList;
    private List<GameObject> _healthBarGOList;
    private List<Player> _playerList;
    private List<Enemy_FlyingEye> _enemyList;
    private List<Unit> _unitList;
    private Stack<Unit> _nextTurn;

    // Custom delays for Coroutines 
    private readonly WaitForSeconds _battleDelay = new WaitForSeconds(2.0f);
    private readonly WaitForSeconds _animationDelay = new WaitForSeconds(0.5f);

    /// <summary>
    /// UNITY: Start is called before the first frame update.
    /// Instantiates and Initialized member variables,
    /// then starts the battle sequence.
    /// </summary>
    public void Start()
    {
        //Initialize GameObjects
        InitGameObjects();

        // Set Battle Phase Textbox to START
        SetBattlePhaseDisplayText("START");

        // Setup Battle
        StartCoroutine(SetupBattle());
    }

    // ========================== COROUTINES ========================== //
    /// <summary>
    /// SetupBattle(): Called before each battle sequence.
    /// Starts the battle squence, sets up Units,
    /// stacks the Turn order, then gets next Turn.
    /// </summary>
    IEnumerator SetupBattle()
    {
        // Start Battle Sequence
        _battleState = BattleState.START;
        SetBattlePhaseDisplayText("START");

        // Disable Buttons
        ToggleButtons(false);

        // Initialize Units
        InitUnits();

        yield return _battleDelay;

        // Set Units to their Idle Animations
        foreach (Unit unit in _unitList)
            unit.SetIdle();

        // Set Unit Turn Order by Speed
        SetTurnOrder();

        // Get Next Unit turn
        NextTurn();
    }

    /// <summary>
    /// SelectAction(Unit): Gets next Player action
    /// by waiting for the user to select one of the
    /// Action Buttons located on UI. Then calls each
    /// Actions' respective handler.
    /// </summary>
    /// <param name="_currPlayer">The current Player taking its Turn</param>
    IEnumerator SelectAction(Unit _currPlayer)
    {
        // Add UI Action Buttons to waitlist.
        var waitForButton = new WaitForUIButtons(
            attackButton,
            magicButton,
            defendButton,
            itemButton
        );

        // Wait for one of the UI Buttons to be pressed.
        yield return waitForButton.Reset();

        // Handle respective Actions.
        if (waitForButton.PressedButton == attackButton)
            OnAttackButton(_currPlayer);
        else if (waitForButton.PressedButton == magicButton)
            OnMagicButton(_currPlayer);
        else if (waitForButton.PressedButton == defendButton)
            OnDefendButton(_currPlayer);
        else if (waitForButton.PressedButton == itemButton)
            OnItemButton(_currPlayer);
    }

    /// <summary>
    /// PlayerAttck(Unit): Performs current Player's Attack.
    /// Waits for user to select Enemy to Attack.
    /// Then performs the Player's Attack animation, damages
    /// the enemy, then checks to see if the battle is won,
    /// if not it proceeds to the next Turn.
    /// </summary>
    /// /// <param name="_currPlayer">The current Player taking its Turn</param>
    IEnumerator PlayerAttack(Unit _currPlayer)
    {
        // Perform Player's Attack Animation
        _currPlayer.Attack();

        yield return _animationDelay;

        // TODO: Choose Enemy by mouseover rather than random selection.
        List<Enemy_FlyingEye> _aliveEnemies = new List<Enemy_FlyingEye>();
        _aliveEnemies = _enemyList.FindAll(x => x.isDead == false);
        var random = new System.Random();
        Enemy_FlyingEye _chosenEnemy = _aliveEnemies[random.Next(_aliveEnemies.Count)];
        bool playerIsDead = _chosenEnemy.TakeDamage(Math.Abs(_currPlayer.attack - _chosenEnemy.defense));

        yield return new WaitForSeconds(1.0f);

        // Set Player's Animation to Idle.
        _currPlayer.SetIdle();

        // If all Enemies are dead Player team wins,
        // otherwise, take next Turn.
        _aliveEnemies = _enemyList.FindAll(x => x.isDead == false);
        if (_aliveEnemies.Count != 0)
            NextTurn();
        else
        {
            SetBattlePhaseDisplayText("WON");
            StartCoroutine(BattleEnd("PLAYER"));
        }
    }

    /// <summary>
    /// EnemyAttack(Unit): Performs current Enemy's Attack.
    /// Selects a Player unit at random to Attack
    /// Then performs the Enemies's Attack animation, damages
    /// the chosen Player, then checks to see if the battle is lost,
    /// if not it proceeds to the next Turn.
    /// </summary>
    /// /// <param name="_currPlayer">The current Player taking its Turn</param>
    IEnumerator EnemyAttack(Unit _currEnemy)
    {
        // Perform Player's Attack Animation
        _currEnemy.Attack();

        yield return _animationDelay;

        // Choose random, alive Player and deal damage to them.
        List<Player> _alivePlayers = new List<Player>();
        _alivePlayers = _playerList.FindAll(x => x.isDead == false);
        var random = new System.Random();
        Player _chosenPlayer = _alivePlayers[random.Next(_alivePlayers.Count)];
        bool playerIsDead = _chosenPlayer.TakeDamage(Math.Abs(_currEnemy.attack - _chosenPlayer.defense));

        yield return _animationDelay;

        // Set Enemy's Animation to Idle.
        _currEnemy.SetIdle();

        // If all Players are dead Enemy team wins (Player loss),
        // otherwise, take next Turn.
        _alivePlayers = _playerList.FindAll(x => x.isDead == false);
        if (_alivePlayers.Count != 0)
            NextTurn();
        else
        {
            SetBattlePhaseDisplayText("LOST");
            StartCoroutine(BattleEnd("ENEMY"));
        }
    }

    /// <summary>
    /// BattleEnd(string): Ends (then restarts) Battle Sequence.
    /// Sets Battle State to either WON or LOSS, then restarts
    /// the Battle Sequence.
    /// </summary>
    /// <param name="_winner">String representing winning team.</param>
    IEnumerator BattleEnd(string _winner)
    {
        if (_winner.ToUpper() == "PLAYER")
        {
            foreach (Unit player in _playerList)
                player.SetIdle();
            _battleState = BattleState.WON;
        }
        else
        {
            foreach (Unit enemy in _enemyList)
                enemy.SetIdle();
            _battleState = BattleState.LOST;
        }

        yield return _battleDelay;

        foreach (Unit player in _playerList)
            player.Recover();
        foreach (Unit enemy in _enemyList)
            enemy.Recover();

        StartCoroutine(SetupBattle());
    }

    // ======================== PRIVATE METHODS ======================== //
    /// <summary>
    /// InitGameObjects(): Instantiates GameObjects for each Unit,
    /// and Initializes the GameObjects for each Units HealthBar.
    /// </summary>
    private void InitGameObjects()
    {
        // Instantiate Unit GameObjectes
        _playerGO = Instantiate(playerPrefab, playerStartPos);
        _rPlayerGO = Instantiate(rPlayerPrefab, rPlayerStartPos);
        _gPlayerGO = Instantiate(gPlayerPrefab, gPlayerStartPos);
        _bPlayerGO = Instantiate(bPlayerPrefab, bPlayerStartPos);
        _enemyGO = Instantiate(enemyPrefab, enemyStartPos);
        _rEnemyGO = Instantiate(rEnemyPrefab, rEnemyStartPos);
        _gEnemyGO = Instantiate(gEnemyPrefab, gEnemyStartPos);
        _bEnemyGO = Instantiate(bEnemyPrefab, bEnemyStartPos);
        _unitGOList = new List<GameObject>
        {
            _playerGO,
            _rPlayerGO,
            _gPlayerGO,
            _bPlayerGO,
            _enemyGO,
            _rEnemyGO,
            _gEnemyGO,
            _bEnemyGO
        };

        // Get Health Bar GOs in Hierarchy
        _playerHealthBarGO = GameObject.Find("PlayerHealthBar");
        _rPlayerHealthBarGO = GameObject.Find("R_PlayerHealthBar");
        _gPlayerHealthBarGO = GameObject.Find("G_PlayerHealthBar");
        _bPlayerHealthBarGO = GameObject.Find("B_PlayerHealthBar");
        _enemyHealthBarGO = GameObject.Find("EnemyHealthBar");
        _rEnemyHealthBarGO = GameObject.Find("R_EnemyHealthBar");
        _gEnemyHealthBarGO = GameObject.Find("G_EnemyHealthBar");
        _bEnemyHealthBarGO = GameObject.Find("B_EnemyHealthBar");
        _unitGOList = new List<GameObject>
        {
            _playerHealthBarGO,
            _rPlayerHealthBarGO,
            _gPlayerHealthBarGO,
            _bPlayerHealthBarGO,
            _enemyHealthBarGO,
            _rEnemyHealthBarGO,
            _gEnemyHealthBarGO,
            _bEnemyHealthBarGO
        };
    }

    /// <summary>
    /// InitUnits(): Initializes Unit Script Components,
    /// adds them to _unitList, then attaches a HealthBar
    /// GameObject to the Unit.
    /// </summary>
    private void InitUnits()
    {
        // Get Player Scripts
        _player = _playerGO.GetComponent<Player>();
        _rPlayer = _rPlayerGO.GetComponent<Player>();
        _gPlayer = _gPlayerGO.GetComponent<Player>();
        _bPlayer = _bPlayerGO.GetComponent<Player>();
        _playerList = new List<Player>
        {
            _player,
            _rPlayer,
            _gPlayer,
            _bPlayer
        };

        // Get Enemy Scripts
        _enemy = _enemyGO.GetComponent<Enemy_FlyingEye>();
        _rEnemy = _rEnemyGO.GetComponent<Enemy_FlyingEye>();
        _gEnemy = _gEnemyGO.GetComponent<Enemy_FlyingEye>();
        _bEnemy = _bEnemyGO.GetComponent<Enemy_FlyingEye>();
        _enemyList = new List<Enemy_FlyingEye>
        {
            _enemy,
            _rEnemy,
            _gEnemy,
            _bEnemy
        };

        // Create Unit List
        _unitList = new List<Unit>();
        _unitList.AddRange(_playerList);
        _unitList.AddRange(_enemyList);

        // Assign Unit Health Bars
        foreach (var unit in _unitList.Zip(_unitGOList, Tuple.Create))
            unit.Item1.healthBar = unit.Item2.GetComponent<HealthBar>();
    }

    /// <summary>
    /// NextTurn(): Gets next Unit's Turn.
    /// Checks the Unit Type for the next Unit
    /// in the Turn stack, then takes the
    /// matching turn, and updates the Battle State.
    /// If the Turn stack is empty, refill and continue.
    /// </summary>
    private void NextTurn()
    {
        // If the Turn stack is not empty, take next Turn.
        if (_nextTurn.Count > 0)
        {
            // If next Unit is a Player, take Player Turn.
            if (_nextTurn.Peek().unitType.ToUpper() == "PLAYER")
            {
                _battleState = BattleState.PLAYER;
                ToggleButtons(true);
                PlayerTurn();
            }
            // If next Unit is a Enemy, take Enemy Turn.
            else
            {
                _battleState = BattleState.ENEMY;
                ToggleButtons(false);
                EnemyTurn();
            }
        }
        // Stack is empty, refill it then take next Turn.
        else
        {
            SetTurnOrder();
            NextTurn();
        }
        
    }

    /// <summary>
    /// PlayerTurn(): Pop next Player Unit off the
    /// Turn stack, and start Player's SelectAction coroutine.
    /// If next Unit is dead, skip to next Unit in stack.
    /// </summary>
    private void PlayerTurn()
    {
        // Pop next Player off of the Turn stack
        Unit _currPlayer = _nextTurn.Pop();

        // If the Player isn't dead, wait for user to 
        // select Action, otherwise skip to next Unit's Turn.
        if (!_currPlayer.isDead)
        {
            SetBattlePhaseDisplayText(_currPlayer.unitName);
            _currPlayer.SetBattleStance();
            StartCoroutine(SelectAction(_currPlayer));
        }
        else NextTurn();
    }

    /// <summary>
    /// EnemyTurn(): Pop next Enemy Unit off the
    /// Turn stack, and start Enemy's Attack coroutine.
    /// If next Unit is dead, skip to next Unit in stack.
    /// </summary>
    private void EnemyTurn()
    {
        // Pop next Enemy off of the Turn stack
        Unit _currEnemy = _nextTurn.Pop();

        // If the Enemy isn't dead, take Enemy's Attack,
        // otherwise skip to next Unit's Turn.
        if (!_currEnemy.isDead)
        {
            _currEnemy.SetBattleStance();
            SetBattlePhaseDisplayText(_currEnemy.unitName);
            StartCoroutine(EnemyAttack(_currEnemy));
        }
        else NextTurn();
    }

    /// <summary>
    /// SetTurnOrder(): Places all Units in a Stack
    /// that is ordered by speed, with the highest
    /// speed value place on top of the Stack. This
    /// is used to indicate Turn order.
    /// </summary>
    private void SetTurnOrder()
    {
        // Empty Turn Stack.
        _nextTurn = new Stack<Unit>();

        // Order Unit List by speed value, and push it
        // onto the Turn Stack, with highest speed on top.
        List<Unit> _turnOrder = _unitList.OrderBy(x => x.speed).ToList();
        foreach (Unit unit in _turnOrder)
            _nextTurn.Push(unit);
    }

    /// <summary>
    /// ToggleButtons(bool): Set UI Action Buttons'
    /// interactivity to the passed in boolean value.
    /// Buttons are disabled on Enemy Units' Turn.
    /// </summary>
    /// <param name="_buttonsActive">bool determining whether
    /// or not Action buttons should be interactable.</param>
    private void ToggleButtons(bool _buttonsActive)
    {
        // Get Action Buttons
        List<Button> _actionButtons = new List<Button>
        {
            attackButton,
            magicButton,
            defendButton,
            itemButton
        };

        // Set action buttons according to _buttonsActive
        foreach (Button b in _actionButtons)
            b.interactable = _buttonsActive;
    }

    /// <summary>
    /// OnAttackButton(Unit): Handles when Attack
    /// Action Button is pressed, starts current
    /// Player's Attack coroutine.
    /// </summary>
    /// <param name="_currPlayer">The current Player taking its Turn.</param>
    private void OnAttackButton(Unit _currPlayer)
    {
        // If not a Player's turn, do nothing.
        if (_battleState != BattleState.PLAYER)
            return;

        // Start Player's Attack.
        StartCoroutine(PlayerAttack(_currPlayer));
    }

    /// <summary>
    /// OnMagicButton(Unit): Handles when Magic
    /// Action Button is pressed, starts current
    /// Player's Magic Attack coroutine.
    /// </summary>
    /// <param name="_currPlayer">The current Player taking its Turn.</param>
    private void OnMagicButton(Unit _currPlayer)
    {
        // If not a Player's turn, do nothing.
        if (_battleState != BattleState.PLAYER)
            return;

        // TODO: Implement Player's Magic Attack coroutine.
        Debug.Log("MAGIC BUTTON PRESSED");
        StartCoroutine(SelectAction(_currPlayer));
        //StartCoroutine(PlayerAction("MAGIC"));
    }

    /// <summary>
    /// OnDefendButton(Unit): Handles when Defend
    /// Action Button is pressed, starts current
    /// Player's Defend coroutine.
    /// </summary>
    /// <param name="_currPlayer">The current Player taking its Turn.</param>
    private void OnDefendButton(Unit _currPlayer)
    {
        // If not a Player's turn, do nothing.
        if (_battleState != BattleState.PLAYER)
            return;

        // TODO: Implement Player's Defend coroutine.
        Debug.Log("DEFEND BUTTON PRESSED");
        StartCoroutine(SelectAction(_currPlayer));
        //StartCoroutine(PlayerAction("DEFEND"));
    }

    /// <summary>
    /// OnItemButton(Unit): Handles when Item
    /// Action Button is pressed, opens the
    /// User's Inventory.
    /// </summary>
    /// <param name="_currPlayer">The current Player taking its Turn.</param>
    private void OnItemButton(Unit _currPlayer)
    {
        // If not a Player's turn, do nothing.
        if (_battleState != BattleState.PLAYER)
            return;

        // TODO: Implement Invetory selection.
        Debug.Log("ITEM BUTTON PRESSED");
        StartCoroutine(SelectAction(_currPlayer));
        //StartCoroutine(OpenInventory());
    }

    /// <summary>
    /// SetBattlePhaseDisplayText(string): Sets the text
    /// for the Battle Phase display on the UI.
    /// </summary>
    /// <param name="_battleText">String representing the current Battle State.</param>
    private void SetBattlePhaseDisplayText(string _battleText)
    {
        // Set UI Battle Phase Display Text.
        GameObject _battlePhaseDisplayGO = GameObject.Find("BattlePhaseDisplay");
        Text _battlePhaseText = _battlePhaseDisplayGO.GetComponentInChildren<Text>();
        _battlePhaseText.text = _battleText;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    private void RestoreDefaultMaterial(GameObject _unit)
    {
        _unit.GetComponent<SpriteRenderer>().material = defaultMaterial;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    private void SetTurnIndicationOutline(GameObject _unit)
    {
        _unit.GetComponent<SpriteRenderer>().material = spriteOutlineMaterial;
    }
}
