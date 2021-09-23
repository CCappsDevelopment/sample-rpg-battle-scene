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

    // UI
    private UI _UI = new UI();
    // Utilites
    private Utils _utils = new Utils();

    // Custom delays for Coroutines 
    private readonly WaitForSeconds _battleDelay = new WaitForSeconds(2.0f);
    private readonly WaitForSeconds _animationDelay = new WaitForSeconds(0.5f);

    /// <summary>
    /// UNITY - Start(): Called before the first frame update.
    /// Instantiates and Initialized member variables,
    /// then starts the battle sequence.
    /// </summary>
    public void Start()
    {
        //Initialize GameObjects
        InitGameObjects();

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
        _UI.SetBattlePhaseDisplayText("START");

        // Initialize Units
        InitUnits();

        // Set starting Combat Text.
        string _enemyCount = _utils.CapitalizeFirst(_utils.NumberToWords(_enemyList.Count));
        string _enemyType = _enemyList.First().unitName;
        _UI.SetCombatText($"{_enemyCount} {_enemyType}{(_enemyList.Count != 1 ? "s" : "")} Appeared!");

        // Disable Buttons
        ToggleButtons(false);

        // Delay between battle phases
        yield return _battleDelay;

        // Set Player to their Idle Animations
        foreach (Player player in _playerList)
            player.SetIdle();
        foreach (Enemy_FlyingEye enemy in _enemyList)
            enemy.SetBattleStance();

        // Set Unit Turn Order by Speed
        SetTurnOrder();

        // Get Next Unit turn
        GetNextTurn();
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
    /// PlayerAttack(Unit): Performs current Player's Attack.
    /// Waits for user to select Enemy to Attack.
    /// Then performs the Player's Attack animation, damages
    /// the enemy, then checks to see if the battle is won,
    /// if not it proceeds to the next Turn.
    /// </summary>
    /// /// <param name="_currPlayer">The current Player taking its Turn</param>
    IEnumerator PlayerAttack(Unit _currPlayer)
    {
        // Make Enemies Clickable
        Camera.main.gameObject.AddComponent<ClickObject>();

        // Highlight Enemies on mouseover.
        _UI.HighlightOnMouseover(_enemyList, defaultMaterial, spriteOutlineMaterial);

        // Deselect all Enemies
        foreach (Enemy_FlyingEye enemy in _enemyList)
            enemy.isSelected = false;

        _UI.SetBattlePhaseDisplayText("SELECT ENEMY");
        _UI.SetCombatText($"{_currPlayer.unitName.ToUpper()}: ENEMY SELECT!");

        // Wait for user to select an Enemy by clicking on it.
        yield return new WaitUntil(SelectEnemy);

        _UI.SetBattlePhaseDisplayText(_currPlayer.unitName);

        // Get Enemy that was selected
        Enemy_FlyingEye _chosenEnemy = new Enemy_FlyingEye();
        _chosenEnemy = _enemyList.Find(x => x.isSelected == true);
        _UI.SetCombatText($"{_chosenEnemy.unitName.ToUpper()}: SELECT, prepare thyself.");

        yield return new WaitForSeconds(1.0f);

        // Perform Player's Attack Animation
        _currPlayer.Attack();

        yield return _animationDelay;

        // Deal Damage to the chosen Enemy.
        float _damageTaken = Math.Abs(_currPlayer.attack - _chosenEnemy.defense);
        bool enemyIsDead = _chosenEnemy.TakeDamage(_damageTaken);
        _UI.SetCombatText($"{_chosenEnemy.unitName.ToUpper()} " +
            $"{(enemyIsDead ? "EDURED" : "DIED taking")} " +
            $"{_utils.CapitalizeFirst(_utils.NumberToWords((int)_damageTaken))} " +
            $"damage!");

        // Place Enemy Sprite in BG if dead.
        _UI.SetEnemySortingLayers(_enemyList);

        yield return new WaitForSeconds(1.0f);

        // Set Player's Animation to Idle.
        _currPlayer.SetIdle();

        // If all Enemies are dead Player team wins,
        // otherwise, take next Turn.
        List<Enemy_FlyingEye> _aliveEnemies = _enemyList.FindAll(x => x.isDead == false);
        if (_aliveEnemies.Count != 0)
            GetNextTurn();
        else
        {
            _UI.SetBattlePhaseDisplayText("WON");
            _UI.SetCombatText($"The BATTLE is WON: {_currPlayer.unitName.ToUpper()} LAUDED!");
            yield return new WaitForSeconds(1.0f);

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

        float _damageTaken = Math.Abs(_currEnemy.attack - _chosenPlayer.defense);
        bool playerIsDead = _chosenPlayer.TakeDamage(_damageTaken);
        _UI.SetCombatText($"{_currEnemy.unitName.ToUpper()} " +
            $"ATTACKS: " +
            $"{_chosenPlayer.unitName.ToUpper()} " +
            $"{(playerIsDead ? "KILLED by" : "takes")} " +
            $"{_utils.CapitalizeFirst(_utils.NumberToWords((int)_damageTaken))} " +
            $"DAMAGE!");

        yield return new WaitForSeconds(1.0f);

        // Set Enemy's Animation to Idle.
        _currEnemy.SetBattleStance();

        // If all Players are dead Enemy team wins (Player loss),
        // otherwise, take next Turn.
        _alivePlayers = _playerList.FindAll(x => x.isDead == false);
        if (_alivePlayers.Count != 0)
            GetNextTurn();
        else
        {
            _UI.SetBattlePhaseDisplayText("LOST");
            _UI.SetCombatText($"The BATTLE is LOST: {_chosenPlayer.unitName.ToUpper()} The LAST to fall...");
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
        _healthBarGOList = new List<GameObject>
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
        foreach (var unit in _unitList.Zip(_healthBarGOList, Tuple.Create))
            unit.Item1.healthBar = unit.Item2.GetComponent<HealthBar>();

        // Set all enemies to Entities sorting layer.
        _UI.SetEnemySortingLayers(_enemyList);
    }

    /// <summary>
    /// NextTurn(): Gets next Unit's Turn.
    /// Checks the Unit Type for the next Unit
    /// in the Turn stack, then takes the
    /// matching turn, and updates the Battle State.
    /// If the Turn stack is empty, refill and continue.
    /// </summary>
    private void GetNextTurn()
    {
        // If the Turn stack is not empty, take next Turn.
        if (_nextTurn.Count > 0)
            TakeUnitTurn();
        // Stack is empty, refill it then take next Turn.
        else
        {
            SetTurnOrder();
            GetNextTurn();
        }
        
    }

    /// <summary>
    /// TakeTurn(): Pop next Player Unit off the
    /// Turn stack, and start Player's SelectAction coroutine.
    /// If next Unit is dead, skip to next Unit in stack.
    /// </summary>
    private void TakeUnitTurn()
    {
        // Pop next Player off of the Turn stack
        Unit _currUnit = _nextTurn.Pop();

        // If the Player isn't dead, wait for user to 
        // select Action, otherwise skip to next Unit's Turn.
        if (!_currUnit.isDead)
        {
            _UI.SetBattlePhaseDisplayText(_currUnit.unitName);
            if (_currUnit.unitType == "PLAYER")
                PlayerTurn(_currUnit);
            else if (_currUnit.unitType == "ENEMY")
                EnemyTurn(_currUnit);
            else
                GetNextTurn();
        }
        else GetNextTurn();
    }

    /// <summary>
    /// PlayerTurn(): Pop next Player Unit off the
    /// Turn stack, and start Player's SelectAction coroutine.
    /// If next Unit is dead, skip to next Unit in stack.
    /// </summary>
    private void PlayerTurn(Unit _currPlayer)
    {
        _battleState = BattleState.PLAYER;
        _currPlayer.SetBattleStance();
        _UI.SetCombatText($"{_currPlayer.unitName}'s Turn, ACTION SELECT!");

        ToggleButtons(true);
        StartCoroutine(SelectAction(_currPlayer));
    }

    /// <summary>
    /// EnemyTurn(): Pop next Enemy Unit off the
    /// Turn stack, and start Enemy's Attack coroutine.
    /// If next Unit is dead, skip to next Unit in stack.
    /// </summary>
    private void EnemyTurn(Unit _currEnemy)
    {
        _battleState = BattleState.ENEMY;
        _UI.SetCombatText($"{_currEnemy.unitName}'s Turn, INCOMING ATTACK!");

        ToggleButtons(false);
        StartCoroutine(EnemyAttack(_currEnemy));
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

    private bool SelectEnemy()
    {
        // Check if any Enemies have been selected.
        List<Enemy_FlyingEye> _selectedEnemies = new List<Enemy_FlyingEye>();
        _selectedEnemies = _enemyList.FindAll(x => x.isSelected && !x.isDead);

        // If an Enemy has been selected, trutn true, otherwise false.
        if (_selectedEnemies.Count > 0)
        {
            // Destroy Components that make Enemies Clickable,
            // as well as, makes them highlight on mouseover.
            Destroy(Camera.main.gameObject.GetComponent<ClickObject>());
            foreach (Enemy_FlyingEye enemy in _enemyList)
                Destroy(enemy.GetComponentInParent<HighlightObject>());

            return true;
        }
        return false;
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
}
