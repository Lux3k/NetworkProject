using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ConfirmType
{
    OK,
    OK_CANCEL
}

public class ConfirmUIData : BaseUIData
{
    public ConfirmType ConfirmType;
    public string TitleText;
    public string DescText;
    public string OKButtonText;
    public Action OnClickOKButton;
    public string CancelButtonText;
    public Action OnClickCancelButton;
}

public class ConfirmUI : BaseUI
{
    [SerializeField] private TextMeshProUGUI TitleText = null;
    [SerializeField] private TextMeshProUGUI DescText = null;
    [SerializeField] private Button OKButton = null;
    [SerializeField] private Button CancelButton = null;
    [SerializeField] private TextMeshProUGUI OKButtonText = null;
    [SerializeField] private TextMeshProUGUI CancelButtonText = null;

    private ConfirmUIData _confirmUIData = null;
    private Action _onClickOKButton = null;
    private Action _onClickCancelButton = null;

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);

        _confirmUIData = uiData as ConfirmUIData;

        TitleText.text = _confirmUIData.TitleText;
        DescText.text = _confirmUIData.DescText;
        OKButtonText.text = _confirmUIData.OKButtonText;
        _onClickOKButton = _confirmUIData.OnClickOKButton;
        CancelButtonText.text = _confirmUIData.CancelButtonText;
        _onClickCancelButton = _confirmUIData.OnClickCancelButton;

        OKButton.gameObject.SetActive(true);
        CancelButton.gameObject.SetActive(_confirmUIData.ConfirmType == ConfirmType.OK_CANCEL);
    }

    public void OnClickOKButton()
    {
        _onClickOKButton?.Invoke();
        CloseUI();
    }

    public void OnClickCancelButton()
    {
        _onClickCancelButton?.Invoke();
        CloseUI();
    }
}
