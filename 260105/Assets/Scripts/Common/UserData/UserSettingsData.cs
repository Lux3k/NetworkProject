
    using UnityEngine;

    public class UserSettingsData : IUserData
    {
        public bool Sound { get; set; }

        public void SetDefaultData()
        {
            Logger.Log($"{GetType()}::SetDefalutData");

            Sound = true;
        }
        public bool LoadData()
        {

            Logger.Log($"{GetType()}::LoadData");
            bool result = false;
            try
            {
                Sound = PlayerPrefs.GetInt("Sound")==1?true:false;
                result = true;

                Logger.Log($"Sound:{Sound}");
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
                PlayerPrefs.SetInt("Sound", Sound ? 1 : 0);
                result = true;

                Logger.Log($"Sound:{Sound}");
            }
            catch (System.Exception e)
            {
                Logger.LogError($"{GetType()}::SaveData Exception:{e}");
            }
            return result;
        }

  
    }   
