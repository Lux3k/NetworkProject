using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserGoodsData : IUserData
{
    public long Gem { get; set; }
    public long Gold { get; set; }

    public void SetDefaultData()
    {
        Logger.Log($"{GetType()}::SetDefalutData");

        Gem = 0;
        Gold = 0;
    }
    public bool LoadData()
    {
        Logger.Log($"{GetType()}::LoadData");
        bool result = false;
        try
        {
            Gem =long.Parse(PlayerPrefs.GetString("Gem","0"));
            Gold = long.Parse(PlayerPrefs.GetString("Gold","0"));
            result = true;

            Logger.Log($"Gem:{Gem}, Gold:{Gold}");
        }
        catch (System.Exception e)
        {
            Logger.LogError($"{GetType()}::LoadData Exception:{e}");
        }
        return result;
    }

    public bool SaveData()
    {
        Logger.Log($"{GetType()}::SaveData");

        bool result = false;
        try
        {
            PlayerPrefs.SetString("Gem", Gem.ToString());
            PlayerPrefs.SetString("Gold", Gold.ToString());
            result = true;

            Logger.Log($"Gem:{Gem}, Gold:{Gold}");
        }
        catch( System.Exception e)
        {
            Logger.LogError($"{GetType()}::SaveData Exception:{e}");
        }
        return result;
    }


}
