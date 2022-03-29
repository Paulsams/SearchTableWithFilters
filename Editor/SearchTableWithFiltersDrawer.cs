using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class SearchTableWithFiltersDrawer
{
    public interface ISortingElementForSearch
    {
        public string Name { get; }
    }

    public class FilterParameters
    {
        public delegate bool FilterCallback(int index, int indexFilter);

        public readonly ReadOnlyCollection<string> Filters;
        public readonly FilterCallback Callback;

        public FilterParameters(string[] filters, FilterCallback callback)
        {
            Filters = Array.AsReadOnly(filters);
            Callback = callback;
        }
    }

    public struct SortingElement
    {
        public readonly int Index;
        public readonly ISortingElementForSearch Element;

        public SortingElement(int index, ISortingElementForSearch element)
        {
            Index = index;
            Element = element;
        }
    }

    private const string _fieldNameForFilters = "Filter Categories";

    private readonly FilterParameters _filterParameters;
    private readonly ReadOnlyCollection<ISortingElementForSearch> _elementsOrderDefault;

    private Box _containerForItemDatas;
    private VisualElement _filterElementsContainer;
    private ToolbarPopupSearchField _toolbarSearchField;

    private ReadOnlyCollection<ISortingElementForSearch> _sortingElements;
    private List<SortingElement> _currentSortingsElements;

    private List<SortingElement> _currentAvailiableElementForSearch;

    public SearchTableWithFiltersDrawer(ISortingElementForSearch[] sortingElements, FilterParameters filterParameters)
    {
        _filterParameters = filterParameters;
        _elementsOrderDefault = Array.AsReadOnly(sortingElements);

        _sortingElements = Sorting(_elementsOrderDefault);

        _currentSortingsElements = GetListFromAllSortingElements();
        _currentAvailiableElementForSearch = new List<SortingElement>(_currentSortingsElements);
    }

    private List<SortingElement> GetListFromAllSortingElements() => _sortingElements.Select((sortingElement, index) => new SortingElement(index, sortingElement)).ToList();

    public VisualElement CreateInspectorGUI(Func<int, VisualElement> funcCreateElement)
    {
        VisualElement container = new VisualElement();
        _filterElementsContainer = CreateFilterElementsContainer(container);

        _containerForItemDatas = new Box();
        CreateSearchField();

        CreateElements(funcCreateElement);
        container.Add(_containerForItemDatas);

        CreateFiltersChoice();

        return container;
    }

    private void CreateSearchField()
    {
        _toolbarSearchField = new ToolbarPopupSearchField();
        _toolbarSearchField.RegisterValueChangedCallback((changeEvent) => SortingForSearchName(changeEvent.newValue));
        _toolbarSearchField.style.width = 0;
        _toolbarSearchField.style.flexGrow = 1;
        _filterElementsContainer.Add(_toolbarSearchField);
    }

    private void SortingForSearchName(string searchName)
    {
        _currentSortingsElements.Clear();
        for (int i = 0; i < _currentAvailiableElementForSearch.Count; ++i)
        {
            var sortingElement = _currentAvailiableElementForSearch[i];
            bool isFind = sortingElement.Element.Name.IndexOf(searchName, StringComparison.CurrentCultureIgnoreCase) != -1;

            if (isFind)
                _currentSortingsElements.Add(sortingElement);

            _containerForItemDatas[sortingElement.Index].style.display = isFind ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private VisualElement CreateFilterElementsContainer(VisualElement container)
    {
        VisualElement filterElementsContainer = new VisualElement();
        filterElementsContainer.style.flexDirection = FlexDirection.Row;
        container.Add(filterElementsContainer);
        return filterElementsContainer;
    }

    private void CreateElements(Func<int, VisualElement> funcCreateElement)
    {
        for (int i = 0; i < _sortingElements.Count; ++i)
        {
            VisualElement sortingElement = funcCreateElement.Invoke(_elementsOrderDefault.IndexOf(_sortingElements[i]));
            _containerForItemDatas.Add(sortingElement);
        }
    }

    private void CreateFiltersChoice()
    {
        ManyChoiceStringsPopupWindow choicesCategoriesPopupWindow = new ManyChoiceStringsPopupWindow(_filterParameters.Filters, SetFilters);

        Button button = new Button();
        button.text = _fieldNameForFilters;
        button.clickable = new Clickable(() =>
        {
            UnityEditor.PopupWindow.Show(button.worldBound, choicesCategoriesPopupWindow);
        });
        button.style.width = 120f;
        _filterElementsContainer.Add(button);
    }

    private ReadOnlyCollection<ISortingElementForSearch> Sorting(IList<ISortingElementForSearch> sortingElements) =>
        Array.AsReadOnly(sortingElements.OrderBy((sortingElement) => sortingElement.Name).ToArray());

    private void SetFilters(ReadOnlyCollection<int> indexesFilters)
    {
        if (indexesFilters.Count == 0)
        {
            _currentAvailiableElementForSearch = GetListFromAllSortingElements();
            SortingForSearchName(_toolbarSearchField.value);
            return;
        }

        ApplyFiltersAndSearch(indexesFilters);
    }

    private void ApplyFiltersAndSearch(ReadOnlyCollection<int> indexesFilters)
    {
        _currentAvailiableElementForSearch.Clear();
        for (int i = 0; i < _sortingElements.Count; ++i)
        {
            bool isFind = false;
            for (int j = 0; j < indexesFilters.Count; ++j)
            {
                if (_filterParameters.Callback.Invoke(i, indexesFilters[j]))
                {
                    _currentAvailiableElementForSearch.Add(new SortingElement(i, _sortingElements[i]));
                    isFind = true;
                    break;
                }
            }
            UnityEngine.Debug.Log(isFind);
            _containerForItemDatas[i].style.display = isFind ? DisplayStyle.Flex : DisplayStyle.None;
        }

        SortingForSearchName(_toolbarSearchField.value);
    }
}