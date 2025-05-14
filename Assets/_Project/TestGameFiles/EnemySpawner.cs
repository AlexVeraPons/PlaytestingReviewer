using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PlaytestingReviewer.TestGameFiles
{
    public class EnemySpawner : MonoBehaviour
    {
        public Action OnEnemySpawned;
        
        [SerializeField] private AvailableColors _availableColors;
        [SerializeField]private GameObject enemy;
        [SerializeField]private List<GameObject> lines;
        
        [SerializeField] private float _timeToSpawn;
        private void Start()
        {
            StartCoroutine(SpawnCoroutine());
        }

        private IEnumerator SpawnCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_timeToSpawn);
                SpawnEnemy();
            }
        }

        private void SpawnEnemy()
        {
            int randomPosition = Random.Range(0, lines.Count);
            GameObject e = Instantiate(enemy,lines[randomPosition].transform.position,Quaternion.identity);
            int randomColor = Random.Range(0, _availableColors.colorsList.Count + 1);
            if (randomColor != _availableColors.colorsList.Count)
            {
                e.GetComponent<SpriteRenderer>().color = _availableColors.colorsList[randomColor];
            }
            
            OnEnemySpawned?.Invoke();
        }
    }
}