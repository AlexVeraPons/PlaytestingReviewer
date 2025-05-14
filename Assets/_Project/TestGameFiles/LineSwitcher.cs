using System;
using System.Collections.Generic;
using UnityEngine;
using PlaytestingReviewer.TestGameFiles;

public class LineSwitcher : MonoBehaviour
{
    public Action OnRight;
    public Action OnLeft;
    public Action OnChangeColor;

    [SerializeField] private List<GameObject> lines;
    [SerializeField] private AvailableColors _colors;
    [SerializeField] private GameObject _gameObjectToControl;

    private List<Color> possibleColors => _colors.colorsList;
    private SpriteRenderer _renderer;
    private int currentLine = 0;
    private int currentColor = 0;

    private void Awake()
    {
        if (_gameObjectToControl == null)
        {
            Debug.LogError($"{nameof(LineSwitcher)}: GameObject to control is not assigned.");
            enabled = false;
            return;
        }

        _renderer = _gameObjectToControl.GetComponent<SpriteRenderer>();
        if (_renderer == null)
        {
            Debug.LogError($"{nameof(LineSwitcher)}: No SpriteRenderer found on {_gameObjectToControl.name}.");
            enabled = false;
            return;
        }

        if (_colors == null || _colors.colorsList == null || _colors.colorsList.Count == 0)
        {
            Debug.LogError($"{nameof(LineSwitcher)}: No colors available in AvailableColors.");
            enabled = false;
            return;
        }

        if (lines == null || lines.Count == 0)
        {
            Debug.LogError($"{nameof(LineSwitcher)}: Lines list is null or empty.");
            enabled = false;
            return;
        }

        // Initialize color and position
        _renderer.color = possibleColors[currentColor];
        Transform lineTransform = lines[currentLine].transform;
        transform.SetParent(lineTransform, false);
        transform.position = lineTransform.position;
    }

    private void Update()
    {
        if (!enabled) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            GoLeft();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            GoRight();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            NextColor();
        }
    }

    private void GoRight()
    {
        if (lines == null || lines.Count == 0) return;

        currentLine = (currentLine - 1 + lines.Count) % lines.Count;
        Transform targetLine = lines[currentLine]?.transform;
        if (targetLine != null)
        {
            transform.SetParent(targetLine, false);
            transform.position = targetLine.position;
        }

        OnRight?.Invoke();
    }

    private void GoLeft()
    {
        if (lines == null || lines.Count == 0) return;

        currentLine = (currentLine + 1) % lines.Count;
        Transform targetLine = lines[currentLine]?.transform;
        if (targetLine != null)
        {
            transform.SetParent(targetLine, false);
            transform.position = targetLine.position;
        }

        OnLeft?.Invoke();
    }

    private void NextColor()
    {
        if (possibleColors == null || possibleColors.Count == 0) return;

        currentColor = (currentColor + 1) % possibleColors.Count;
        _renderer.color = possibleColors[currentColor];

        OnChangeColor?.Invoke();
    }
}