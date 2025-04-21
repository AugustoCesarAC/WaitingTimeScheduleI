using Newtonsoft.Json;
using Il2CppScheduleOne.GameTime;
using Il2CppScheduleOne.PlayerScripts;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using System.Globalization;

[assembly: MelonInfo(typeof(WaitingTime.WaitingTimeMod), "WaitingTime", "1.0.0", "Gus", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace WaitingTime
{
    public class ModConfig
    {
        public string ActivationKey { get; set; } = "Y";
    }

    public class WaitingTimeMod : MelonMod
    {
        private bool showMenu = false;
        private bool fastForward = false;
        private bool lateTime = false;

        private Texture2D backgroundTexture;

        private float targetTime;  
        private float currentTime;

        private KeyCode activationKey; 
        private ModConfig config; 
        private string configPath; 

        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("Wait Menu Mod loaded!");
            backgroundTexture = new Texture2D(1, 1);
            backgroundTexture.SetPixel(0, 0, Color.black);
            backgroundTexture.Apply();

            configPath = Path.Combine(MelonEnvironment.UserDataDirectory, "WaitingTimeConfig.json");
            LoadConfig();
        }

        public override void OnUpdate()
        {
            if (TimeManager.Instance == null || PlayerMovement.Instance == null || PlayerCamera.Instance == null)
                return;

            if (showMenu && Input.GetKeyDown(KeyCode.Escape))
            {
                showMenu = false;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if(showMenu)
                currentTime = TimeManager.Instance.CurrentTime;

            lateTime = currentTime > 2300 || targetTime < currentTime;

            if (Input.GetKeyDown(activationKey) && Time.timeScale == 1 && !showMenu)
            {
                if (currentTime < 2300)
                    targetTime = currentTime + 100;
                else
                    targetTime = currentTime - 2300;
                showMenu = true;
                Time.timeScale = showMenu ? 0 : 1;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = showMenu ? true : false;
            }

            PlayerMovement.Instance.enabled = !showMenu;
            PlayerCamera.Instance.enabled = !showMenu;
            PlayerManager.Instance.enabled = !showMenu;

            if (fastForward)
            {
                currentTime = TimeManager.Instance.CurrentTime;
                if (currentTime >= targetTime && !lateTime)
                {
                    Time.timeScale = 1;
                    fastForward = false;
                    showMenu = false;
                    Cursor.visible = false;
                    MelonLogger.Msg("Target time reached.");
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }

        public override void OnGUI()
        {
            if (showMenu)
            {
                float width = 600;
                float height = 750;
                float x = (Screen.width - width) / 2;
                float y = (Screen.height - height) / 2;

                GUI.DrawTexture(new Rect(x, y, width, height), backgroundTexture);

                GUIStyle textStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 23,
                    normal = { textColor = Color.green },
                    alignment = TextAnchor.MiddleCenter
                };

                GUI.Label(new Rect(x, y + 35, width, 35), $"Current Time: {FormatTime(currentTime)} | Wait Until: {FormatTime(targetTime)}", textStyle);

                if (GUI.Button(new Rect(x + width / 2 - 50, y + 80, 100, 40), "Add 1 Hour") && Time.timeScale == 0)
                {
                    if (targetTime < 2300)
                        targetTime += 100;
                    else
                        targetTime -= 2300;
                }
                    
                 
                // Botão OK para iniciar fast forward
                if (GUI.Button(new Rect(x + width / 2 - 80, y + 130, 160, 40), "Press to Wait"))
                {
                    Time.timeScale = 10;
                    fastForward = true;
                    MelonLogger.Msg($"Starting fast-forward until {targetTime}.");
                }

                // Botão Cancel
                if (GUI.Button(new Rect(x + width / 2 - 40, y + 180, 80, 30), "Cancel"))
                {
                    showMenu = false;
                    fastForward = false;
                    Time.timeScale = 1;
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }
        private void LoadConfig()
        {
            try
            {
                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    config = JsonConvert.DeserializeObject<ModConfig>(json);
                    MelonLogger.Msg("Configurações carregadas!");
                }
                else
                {
                    config = new ModConfig();
                    string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                    File.WriteAllText(configPath, json);
                    MelonLogger.Msg("Arquivo de configuração criado com valores padrão!");
                }
                if (System.Enum.TryParse(config.ActivationKey, out KeyCode key))
                {
                    activationKey = key;
                    MelonLogger.Msg($"Tecla de ativação definida como: {activationKey}");
                }
                else
                {
                    activationKey = KeyCode.Y;
                    MelonLogger.Error($"Tecla inválida no JSON. Usando padrão: {activationKey}");
                }
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"Erro ao carregar configurações: {e.Message}");
                activationKey = KeyCode.Y;
            }
        }

        public static string FormatTime(float timeValue)
        {
            int timeInt = (int)timeValue;
            int hours = timeInt / 100;
            int minutes = timeInt % 100;

            DateTime dt = new DateTime(1, 1, 1, hours, minutes, 0);
            return dt.ToString("hh:mmtt", CultureInfo.InvariantCulture).ToLower();
        }

    }
}