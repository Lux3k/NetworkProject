using System;
using TMPro;
using UnityEngine.UI;

public enum ConfirmType
{
    OK,
    OK_CANCEL
}

public class ConfirmUIData : BaseUIData
{
    public ConfirmType ConfirmType;
    public string TitleTxt;
    public string DescTxt;
    public string OKBtnTxt;
    public Action OnClickOKBtn;
    public string CancelBtnTxt;
    public Action OnClickCancelBtn;
}

public class ConfirmUI : BaseUI
{
    public TextMeshProUGUI TitleTxt = null;
    public TextMeshProUGUI DescTxt = null;
    public Button OKBtn = null;
    public Button CancelBtn = null;
    public TextMeshProUGUI OKBtnTxt = null;
    public TextMeshProUGUI CancelBtnTxt = null;

    private ConfirmUIData _confirmUIData = null;
    private Action _onClickOKBtn = null;
    private Action _onClickCancelBtn = null;

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);

        _confirmUIData = uiData as ConfirmUIData;

        TitleTxt.text = _confirmUIData.TitleTxt;
        DescTxt.text = _confirmUIData.DescTxt;
        OKBtnTxt.text = _confirmUIData.OKBtnTxt;
        _onClickOKBtn = _confirmUIData.OnClickOKBtn;
        CancelBtnTxt.text = _confirmUIData.CancelBtnTxt;
        _onClickCancelBtn = _confirmUIData.OnClickCancelBtn;

        OKBtn.gameObject.SetActive(true);
        CancelBtn.gameObject.SetActive(_confirmUIData.ConfirmType == ConfirmType.OK_CANCEL);
    }

    public void OnClickOKBtn()
    {
        _onClickOKBtn?.Invoke();
        CloseUI();
    }

    public void OnClickCancelBtn()
    {
        _onClickCancelBtn?.Invoke();
        CloseUI();
    }
}
