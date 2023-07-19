using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System;
using System.IO;


public class CurrencyApiClient
{
    private const string API_KEY = "da4eac743cccc395c19babfd";
    private const string API_ENDPOINT = "https://v6.exchangerate-api.com/v6/da4eac743cccc395c19babfd/latest/USD";
    private const string DATA_FILE = "currency_data.json";
    private const int UPDATE_INTERVAL_HOURS = 24;

    private CurrencyData currencyData;

    public double GetExchangeRate(string fromCurrency, string toCurrency)
    {
        UpdateCurrencyDataIfNeeded();

        // Получить обменный курс из сохраненных данных
        if (currencyData.Rates.TryGetValue(toCurrency, out var rates))
        {
            if (rates.TryGetValue(fromCurrency, out var exchangeRate))
            {
                return exchangeRate;
            }
        }

        // Если обменный курс не найден, вернуть значение по умолчанию
        return 0;
    }

    private bool CheckLastUpdateTime()
    {
        // Проверить время последнего обновления из сохраненных данных
        if (currencyData == null || DateTime.Now - currencyData.LastUpdated > TimeSpan.FromHours(UPDATE_INTERVAL_HOURS))
        {
            return true;
        }
        return false;
    }

    private void FetchCurrencyData()
    {
        using (var client = new WebClient())
        {
            // Сделать запрос к API для получения данных о курсах валют
            string response = client.DownloadString(API_ENDPOINT + "?access_key=" + API_KEY);

            // Десериализовать полученные данные
            currencyData = JsonConvert.DeserializeObject<CurrencyData>(response);
        }
    }

    private void SaveCurrencyData()
    {
        // Сохранить данные в локальный файл
        string jsonData = JsonConvert.SerializeObject(currencyData);
        File.WriteAllText(DATA_FILE, jsonData);
    }

    private void LoadCurrencyData()
    {
        // Загрузить данные из локального файла
        if (File.Exists(DATA_FILE))
        {
            string jsonData = File.ReadAllText(DATA_FILE);
            currencyData = JsonConvert.DeserializeObject<CurrencyData>(jsonData);
        }
    }

    private void UpdateCurrencyDataIfNeeded()
    {
        // Проверить время последнего обновления
        if (CheckLastUpdateTime())
        {
            // Если прошло более 24 часов или данные отсутствуют, выполнить запрос и обновить данные
            FetchCurrencyData();
            currencyData.LastUpdated = DateTime.Now;
            SaveCurrencyData();
        }
    }


    public class CurrencyData
    {
        public DateTime LastUpdated { get; set; }
        public Dictionary<string, Dictionary<string, double>> Rates { get; set; }
    }
}