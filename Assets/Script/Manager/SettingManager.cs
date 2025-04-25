using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    public CarController carController;

    public InputField motorPowerInput;
    public InputField brakePowerInput;
    public InputField idleRPMInput;
    public InputField redLineInput;
    public InputField gearChangeTimeInput;

    public Button saveBtn;
    public Button closeBtn;

    private string filePath;

    private void Start()
    {
        filePath = Application.persistentDataPath + "/carSettings.json";
        saveBtn.onClick.AddListener(SaveSettings);
        closeBtn.onClick.AddListener(() => { gameObject.SetActive(false); Time.timeScale = 1.0f; });
    }

    void SaveSettings()
    {
        carController.motorPower = float.Parse(motorPowerInput.text);
        carController.brakePower = float.Parse(brakePowerInput.text);
        carController.idleRPM = float.Parse(idleRPMInput.text);
        carController.redLine = float.Parse(redLineInput.text);
        carController.ChangeGearTime = float.Parse(gearChangeTimeInput.text);

        CarSettings carSettings = new CarSettings()
        {
            gearChangeTime = carController.ChangeGearTime,
            motorPower = carController.motorPower,
            brakePower = carController.brakePower,
            idleRPM = carController.idleRPM,
            redLine = carController.redLine,
        };

        string json = JsonUtility.ToJson(carSettings, true);
        File.WriteAllText(filePath, json);
    }

    void LoadSettings()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            CarSettings carSettings = JsonUtility.FromJson<CarSettings>(json);

            carController.ChangeGearTime = carSettings.gearChangeTime;
            carController.motorPower = carSettings.motorPower;
            carController.brakePower = carSettings.brakePower;
            carController.idleRPM = carSettings.idleRPM;
            carController.redLine = carSettings.redLine;
        }
    }
}
