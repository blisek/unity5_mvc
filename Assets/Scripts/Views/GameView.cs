using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Views
{
    public class GameView
    {
        #region Constants
        
        private const string ButtonPanelPrefab = "Prefabs/ButtonPanel";
        private const string PlayButtonPrefab = "Prefabs/PlayButton";
        private const string ScoreTextPrefab = "Prefabs/ScoreText";
        private const string TimeTextPrefab = "Prefabs/TimeText";
        private const string OneLifePrefab = "Prefabs/OneLife";
        private const string RestartGamePanelPrefab = "Prefabs/RestartGamePanel";
        private const string ScoreTemplate = "Score: {0}";
        private const string GameOverTemplate = "Game over. Your score: {0}.";
        private static readonly Color RedButtonNormal = Color.red;
        private static readonly Color RedButtonHighlighted = new Color(1f, 0f, 0f, .5f);
        private static readonly Color GreenButtonNormal = Color.green;
        private static readonly Color GreenButtonHighlighted = new Color(0f, 1f, 0f, .5f);

        #endregion

        #region Instance constants
        
        private readonly GameObject _gameView;
        private readonly GameObject _menuView;
        private readonly Action _startGameCallback;
        private readonly Action<int> _buttonPressCallback;
        private readonly Stack<GameObject> _lives;
        private readonly int _livesAmount;

        #endregion

        #region Variables
        
        private bool _isGameViewInitiaded;
        private bool _gameViewInstantiated;
        private bool _menuVisible;
        private Text _timeLeftText;
        private Text _scoreText;
        private Button[] _buttons;

        #endregion

        #region Properties
        
        public bool MenuVisible
        {
            get { return _menuVisible; }
        }

        #endregion

        #region Ctors
        
        public GameView(GameObject menuView, GameObject gameView, Action startGameCallback,
            Action<int> buttonPressCallback, int lives)
        {
            if (menuView == null)
                throw new ArgumentNullException("menuView");

            if (gameView == null)
                throw new ArgumentNullException("gameView");
            
            _isGameViewInitiaded = false;
            _gameView = gameView;
            _menuView = menuView;
            _menuVisible = true;
            _lives = new Stack<GameObject>();
            _startGameCallback = startGameCallback;
            _buttonPressCallback = buttonPressCallback;
            _livesAmount = lives;
            ConstructMenuView();
        }

        #endregion

        #region Public methods
        
        public void ShowRestartPanel(int score, Action restartCallback)
        {
            foreach (var button in _buttons)
                button.enabled = false;

            var restartPanel = UnityEngine.Object.Instantiate(
                Resources.Load(RestartGamePanelPrefab), _gameView.transform) as GameObject;
            restartPanel.GetComponentInChildren<Text>().text = string.Format(GameOverTemplate, score);
            restartPanel.GetComponentInChildren<Button>(false).onClick.AddListener(() =>
            {
                UnityEngine.Object.Destroy(restartPanel);
                restartCallback();
            });
        }

        public void ResetFields()
        {
            foreach (Transform child in _gameView.transform)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }

        public void ToggleMenuGameViews()
        {
            if (_menuVisible)
            {
                _gameView.SetActive(true);
                if (!_gameViewInstantiated)
                    ConstructGameView();
                _menuView.SetActive(false);
            }
            else
            {
                _menuView.SetActive(true);
                _gameView.SetActive(false);
            }

            _menuVisible = !_menuVisible;
        }

        public void MarkButton(int id)
        {
            if (id >= _buttons.Length)
            {
                Debug.LogWarning(string.Format("Trying to mark nonexistent button. Id: {0}", id));
                return;
            }
            
            ChangeButtonColor(id, GreenButtonNormal, GreenButtonHighlighted);
        }

        public void UnmarkButton(int id)
        {
            if (id >= _buttons.Length)
            {
                Debug.LogWarning(string.Format("Trying to unmark nonexistent button. Id: {0}", id));
                return;
            }
            
            ChangeButtonColor(id, RedButtonNormal, RedButtonHighlighted);
        }

        public void UpdateAllUiElements(float timeLeftInSeconds, int points)
        {
            UpdateUiTimeLeft(timeLeftInSeconds);
            UpdateUiPoints(points);
        }

        public void UpdateUiLives(int decreaseBy = 1)
        {
            AssertGameViewInitiated();

            if(decreaseBy <= 0)
                return;

            for(int counter = 0; counter < decreaseBy && _lives.Count > 0; ++counter)
                UnityEngine.Object.Destroy(_lives.Pop());
        }

        public void UpdateUiTimeLeft(float seconds)
        {
            UpdateUiTimeLeft(Mathf.RoundToInt(seconds));
        }

        public void UpdateUiTimeLeft(int seconds)
        {
            AssertGameViewInitiated();
            var secondsLocal = seconds%60;
            var minutes = seconds/60;
            var timeString = string.Format("{0:00}:{1:00}", minutes, secondsLocal);
            _timeLeftText.text = timeString;
        }

        public void UpdateUiPoints(int newValue)
        {
            AssertGameViewInitiated();
            _scoreText.text = string.Format(ScoreTemplate, newValue);
        }

        #endregion

        #region Private methods
        
        private void AssertGameViewInitiated()
        {
            if (!_isGameViewInitiaded)
                throw new InvalidOperationException("Object is not initiated.");
        }

        private void ConstructMenuView()
        {
            var playButton = UnityEngine.Object.Instantiate(Resources.Load(PlayButtonPrefab)) as GameObject;
            var playButtonTransform = playButton.transform;
            playButtonTransform.SetParent(_menuView.transform);
            playButtonTransform.localPosition = Vector2.zero;
            playButton.GetComponent<Button>().onClick.AddListener(() => _startGameCallback());
        }

        private void ConstructGameView()
        {
            var buttonsPanel = UnityEngine.Object.Instantiate(Resources.Load(ButtonPanelPrefab)) as GameObject;
            if (buttonsPanel == null)
                Debug.LogWarning("Buttons panel can't be instantiated");
            var buttonsPanelTransform = buttonsPanel.transform;
            buttonsPanelTransform.SetParent(_gameView.transform);
            buttonsPanelTransform.localPosition = Vector2.zero;
            _buttons = buttonsPanel.GetComponentsInChildren<Button>();
            InitButtons();
            InitGameStatusLabels();
            InitLivesBar(_livesAmount);
            _isGameViewInitiaded = true;
        }

        private void InitButtons()
        {
            for (int i = 0; i < _buttons.Length; ++i)
            {
                var button = _buttons[i];
                ChangeButtonColor(button, RedButtonNormal, RedButtonHighlighted);
                var id = i;
                button.onClick.AddListener(() => _buttonPressCallback(id));
            }
        }

        private void InitLivesBar(int livesAmount)
        {
            var firstLife = UnityEngine.Object.Instantiate(Resources.Load(OneLifePrefab)) as GameObject;
            var firstLifeRectTransform = firstLife.GetComponent<RectTransform>();
            firstLife.transform.SetParent(_gameView.transform);
            firstLifeRectTransform.anchoredPosition = Vector2.zero;
            _lives.Push(firstLife);

            if(livesAmount == 1)
                return;

            var oneLifesWidth = firstLifeRectTransform.rect.width;
            var parent = _gameView.transform;
            var positionVector = new Vector2(-oneLifesWidth, 0);
            for (var counter = 1; counter < livesAmount; ++counter, positionVector.x -= oneLifesWidth)
            {
                var nextLife = UnityEngine.Object.Instantiate(firstLife, parent);
                nextLife.GetComponent<RectTransform>().anchoredPosition = positionVector;
                _lives.Push(nextLife);
            }

        }

        private void InitGameStatusLabels()
        {
            InitScoreTextField();
            InitTimeTextField();
        }

        private void InitTimeTextField()
        {
            var timeTextField = UnityEngine.Object.Instantiate(Resources.Load(TimeTextPrefab)) as GameObject;
            var timeTextRectTransform = timeTextField.GetComponent<RectTransform>();
            timeTextField.transform.SetParent(_gameView.transform);
            //timeTextRectTransform.localPosition = Vector3.zero;
            timeTextRectTransform.anchoredPosition = Vector2.zero;
            _timeLeftText = timeTextField.GetComponent<Text>();
        }

        private void InitScoreTextField()
        {
            var scoreTextField = UnityEngine.Object.Instantiate(Resources.Load(ScoreTextPrefab)) as GameObject;
            var scoreTextRectTransform = scoreTextField.GetComponent<RectTransform>();
            scoreTextField.transform.SetParent(_gameView.transform);
            //scoreTextRectTransform.localPosition = Vector3.zero;
            scoreTextRectTransform.anchoredPosition = Vector2.zero;
            _scoreText = scoreTextField.GetComponent<Text>();
        }

        private void ChangeButtonColor(Button btn, Color normal, Color highlighted)
        {
            var btnColors = btn.colors;
            btnColors.normalColor = normal;
            btnColors.highlightedColor = highlighted;
            btn.colors = btnColors;
        }

        private void ChangeButtonColor(int id, Color normal, Color highlighted)
        {
            var btn = _buttons[id];
            ChangeButtonColor(btn, normal, highlighted);
        }

        #endregion
    }
}
