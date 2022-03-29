using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor;
using UnityEngine;

public class ManyChoiceStringsPopupWindow : PopupWindowContent
{
    private static readonly Vector2 _sizeWindow = new Vector2(250, 200);

    private readonly ReadOnlyCollection<string> _allNames;
    private readonly Action<ReadOnlyCollection<int>> _callbackSetValue;

    private Vector2 _scrollPosition;

    private List<int> _currentChoicedNames = new List<int>();

    public ManyChoiceStringsPopupWindow(ReadOnlyCollection<string> allNames, Action<ReadOnlyCollection<int>> callbackSetValue)
    {
        _allNames = allNames;
        _callbackSetValue = callbackSetValue;
    }

    public override Vector2 GetWindowSize() => _sizeWindow;

    public override void OnGUI(Rect rect)
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        {
            DrawToggleChoiceAll();
            DrawAllToggles();
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawAllToggles()
    {
        for (int i = 0; i < _allNames.Count; ++i)
        {
            Rect positionElelment = EditorGUILayout.GetControlRect();
            EditorGUI.BeginChangeCheck();
            bool changeValue = EditorGUI.ToggleLeft(positionElelment, _allNames[i], _currentChoicedNames.Contains(i));

            if (EditorGUI.EndChangeCheck())
                SetValueFromIndex(i, changeValue);
        }
    }

    private void SetValueFromIndex(int i, bool changeValue)
    {
        if (changeValue)
        {
            int indexInsert = Mathf.Abs(_currentChoicedNames.BinarySearch(i) + 1);
            _currentChoicedNames.Insert(indexInsert, i);
        }
        else
        {
            _currentChoicedNames.Remove(i);
        }
        _callbackSetValue?.Invoke(_currentChoicedNames.AsReadOnly());
    }

    private void DrawToggleChoiceAll()
    {
        Rect positionToggleAll = EditorGUILayout.GetControlRect();
        EditorGUI.BeginChangeCheck();
        bool choiceAll = EditorGUI.ToggleLeft(positionToggleAll, "All", _currentChoicedNames.Count == _allNames.Count);

        if (EditorGUI.EndChangeCheck())
        {
            _currentChoicedNames.Clear();

            if (choiceAll)
                ChoiceAll();
        }
    }

    private void ChoiceAll()
    {
        for (int i = 0; i < _allNames.Count; ++i)
        {
            _currentChoicedNames.Add(i);
        }
        _callbackSetValue?.Invoke(_currentChoicedNames.AsReadOnly());
    }
}