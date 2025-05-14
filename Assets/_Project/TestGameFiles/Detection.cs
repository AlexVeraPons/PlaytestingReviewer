using System;
using UnityEngine;

namespace PlaytestingReviewer.TestGameFiles
{
    [RequireComponent(typeof(Collider2D))]
    public class Detection : MonoBehaviour
    {
        public Action OnDied;
        public Action OnDodged;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.GetComponent<SpriteRenderer>().color != this.gameObject.GetComponent<SpriteRenderer>().color)
            {
                Destroy(this.gameObject);
                OnDied?.Invoke();
            }
            else
            {
                OnDodged?.Invoke();
            }
        }
    }
}