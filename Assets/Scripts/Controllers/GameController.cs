using Assets.Scripts.Models;
using Assets.Scripts.Views;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class GameController : MonoBehaviour
    {
        #region Constants
        
        private const int ButtonCount = 4;

        #endregion

        #region Editor settings
        
        [SerializeField] private GameObject menuView;
        [SerializeField] private GameObject gameView;
        public int lives;
        public int timeLimit;
        public int pointsAwardedForGoodAnswer;

        #endregion

        #region Variables

        private GameModel _model;
        private GameView _view;
        private bool _gameStarted;
        private System.Random _randomEngine;
        private int _goodAnswerId;

        #endregion

        #region Unity callbacks
        
        void Start ()
        {
            ResetVariables();
            menuView.SetActive(true);
        }
        
        void Update () {
            if(!_gameStarted)
                return;


            DecreaseTime();
            if (_model.TimeLeft <= 0)
            {
                _gameStarted = false;
                _view.ShowRestartPanel(_model.Points, RestartGame);
                Debug.Log("Time elapsed");
            }
        }

        #endregion

        #region EventSystem callbacks

        private void StartGame()
        {
            if (_gameStarted)
                return;

            _view.ToggleMenuGameViews();
            _view.UpdateAllUiElements(_model.TimeLeft, _model.Points);
            NextTour();
            _gameStarted = true;
        }

        private void RestartGame()
        {
            _view.ToggleMenuGameViews();
            ResetVariables();
        }

        private void OnAnswerButtonClick(int buttonId)
        {
            if (!_gameStarted)
                return;

            if (buttonId != _goodAnswerId)
            {
                DecreaseLife();
            }
            else
            {
                IncreasePoints();
            }

            NextTour();
        }

        #endregion

        #region Private methods

        private void NextTour()
        {
            var nextId = _randomEngine.Next(ButtonCount);
            if(_goodAnswerId >= 0)
                _view.UnmarkButton(_goodAnswerId);
            _view.MarkButton(nextId);
            _goodAnswerId = nextId;
        }

        private void ResetVariables()
        {
            _goodAnswerId = -1;
            if(_view != null)
                _view.ResetFields();
            InitModel();
            InitView();
            _randomEngine = new System.Random();
        }

        private void InitModel()
        {
            _model = new GameModel
            {
                Lives = lives,
                Points = 0,
                TimeLeft = timeLimit
            };
        }

        private void InitView()
        {
            _view = new GameView(menuView, gameView, StartGame, OnAnswerButtonClick, lives);
        }

        private void DecreaseTime()
        {
            _model.TimeLeft -= Time.deltaTime;
            _view.UpdateUiTimeLeft(Mathf.RoundToInt(_model.TimeLeft));
        }

        private void DecreaseLife()
        {
            if (--_model.Lives <= 0)
            {
                _gameStarted = false;
                _view.ShowRestartPanel(_model.Points, RestartGame);
            }
            _view.UpdateUiLives();
        }

        private void IncreasePoints()
        {
            _model.Points += pointsAwardedForGoodAnswer;
            _view.UpdateUiPoints(_model.Points);
        }

        #endregion
    }
}
