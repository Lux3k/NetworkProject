using System.Collections.Generic;
using UnityEngine;

public class UIManager : SingletonBehaviour<UIManager>
{
    [SerializeField]private Transform _uICanvasTransform;
    [SerializeField] private Transform _closedUITransform;
	
    private BaseUI _frontUI;
    private Dictionary<System.Type, GameObject> _openUIPool = new Dictionary<System.Type, GameObject>();
    private Dictionary<System.Type, GameObject> _closedUIPool = new Dictionary<System.Type, GameObject>();

    private BaseUI GetUI<T>(out bool isAlreadyOpen)
    {
        System.Type uiType = typeof(T);

        BaseUI ui = null;
        isAlreadyOpen = false;

        if (_openUIPool.ContainsKey(uiType))
        {
            ui = _openUIPool[uiType].GetComponent<BaseUI>();
            isAlreadyOpen = true;
        }
        else if (_closedUIPool.ContainsKey(uiType))
        {
            ui = _closedUIPool[uiType].GetComponent<BaseUI>();
            _closedUIPool.Remove(uiType);
        }
        else
        {
            var uiObj = Instantiate(Resources.Load($"UI/{uiType}", typeof(GameObject))) as GameObject;
            ui = uiObj.GetComponent<BaseUI>();
        }

        return ui;
    }

    public void OpenUI<T>(BaseUIData uiData)
    {
        System.Type uiType = typeof(T);

        Logger.Log($"{GetType()}::OpenUI({uiType})");

        bool isAlreadyOpen = false;
        var ui = GetUI<T>(out isAlreadyOpen);
        
        if (!ui)
        {
            Logger.LogError($"{uiType} does not exist.");
            return;
        }

        if(isAlreadyOpen)
        {
            Logger.LogError($"{uiType} is already open.");
            return;
        }

        var siblingIdx = _uICanvasTransform.childCount;
        ui.Init(_uICanvasTransform);
        ui.transform.SetSiblingIndex(siblingIdx);
        ui.gameObject.SetActive(true);
        ui.SetInfo(uiData);
        ui.ShowUI();

        _frontUI = ui;
        _openUIPool[uiType] = ui.gameObject;
    }

    public void CloseUI(BaseUI ui)
    {
        System.Type uiType = ui.GetType();

        Logger.Log($"CloseUI UI:{uiType}");

        ui.gameObject.SetActive(false);
        _openUIPool.Remove(uiType);
        _closedUIPool[uiType] = ui.gameObject;
        ui.transform.SetParent(_closedUITransform);

        _frontUI = null;
        var lastChild = _uICanvasTransform.GetChild(_uICanvasTransform.childCount - 1);
        if (lastChild)
        {
            _frontUI = lastChild.gameObject.GetComponent<BaseUI>();
        }
    }

    public BaseUI GetActiveUI<T>()
    {
        var uiType = typeof(T);
        return _openUIPool.ContainsKey(uiType) ? _openUIPool[uiType].GetComponent<BaseUI>() : null;
    }

    public bool ExistsOpenUI()
    {
        return _frontUI != null;
    }

    public BaseUI GetCurrentFrontUI()
    {
        return _frontUI;
    }

    public void CloseCurrFrontUI()
    {
        _frontUI.CloseUI();
    }

    public void CloseAllOpenUI()
    {
        while (_frontUI)
        {
            _frontUI.CloseUI(true);
        }
    }
}
