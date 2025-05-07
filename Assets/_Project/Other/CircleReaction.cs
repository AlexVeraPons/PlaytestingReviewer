using System;                                 // already present
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;            // NEW – required for scene loading
using Random = UnityEngine.Random;            

namespace PlaytestingReviewer.Other
{
    public class CircleReaction : MonoBehaviour
    {
        public Action OnColorChangeAction;
        public Action OnLoadAction;

        public float moveSpeed = 5f;

        [SerializeField] private SpriteRenderer _spriteRenderer;
        public Color _currentColor;

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            Move();
            ChangeColor();
            CheckSceneAdvance();              // NEW
        }

        /* -------------------------------------------------
         * Existing features
         * -------------------------------------------------*/
        private void Move()
        {
            Vector2 moveDirection = Vector2.zero;

            if (Input.GetKeyDown(KeyCode.W))
                moveDirection += Vector2.up;
            if (Input.GetKeyDown(KeyCode.S))
                moveDirection += Vector2.down;
            if (Input.GetKeyDown(KeyCode.A))
                moveDirection += Vector2.left;
            if (Input.GetKeyDown(KeyCode.D))
                moveDirection += Vector2.right;

            transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
        }

        private void ChangeColor()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _currentColor = new Color(Random.value, Random.value, Random.value);
                _spriteRenderer.color = _currentColor;
                Debug.Log("Space pressed – colour changed");
                OnColorChangeAction?.Invoke();
            }
        }

        /* -------------------------------------------------
         * NEW feature: advance to the next scene with P
         * -------------------------------------------------*/
        private void CheckSceneAdvance()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                int i = SceneManager.GetActiveScene().buildIndex;
                int next = i + 1;

                // If you've reached the last scene, you can loop or ignore.
                // Here we wrap to 0 so the game keeps running in editor/standalone.
                if (next >= SceneManager.sceneCountInBuildSettings)
                    next = 0;

                OnLoadAction?.Invoke();
                SceneManager.LoadScene(next);
            }
        }
    }
}
