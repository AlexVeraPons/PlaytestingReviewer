using System;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace PlaytestingReviewer.Other
{
    public class CircleReaction : MonoBehaviour
    {
        public Action OnColorChangeAction;

        public float moveSpeed = 5f;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        public Color _currentColor;
        private int randomInt = 20;

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            Move();
            ChangeColor();
        }

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
                _spriteRenderer.color =  new Color(Random.value, Random.value, Random.value);
                Debug.Log("space pressed");
                OnColorChangeAction?.Invoke();
            }
        }
    }
}