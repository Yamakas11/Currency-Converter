using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Currency_Converter
{
    public partial class Form1 : Form
    {
        private const ushort PanelRadius = 10;
        private const ushort HoursBetweenApiRequests = 24;
        private const string ApiKey = "da4eac743cccc395c19babfd";
        private const string ApiURL = "https://v6.exchangerate-api.com/v6/da4eac743cccc395c19babfd/latest/USD";
        private const string LaunchDataFileName = "launch_data.json";
        private const string CurrencyDataFileName = "currency_data.json";
        private const string CountriesAndCodesFilePath = @"C:\app\Currency Converter\jsconfig2.json";
        private const string CountriesAndCurrenciesFilePath = @"C:\app\Currency Converter\jsconfig1.json";
        private DateTime LastRequestTimestamp;
        private CurrencyConversionData ApiData;
        private CountriesAndCodes CountryCodesData;
        private CountriesAndCurrencies CountryAndCurrencyData;

        public Form1()
        {
            InitializeComponent();

            Load += Form1_Load;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // Вызываем методы и настраиваем элементы управления
                InitializeUIElements();

                // Настраиваем обработчики событий для комбобоксов
                InitializeComboBoxEventHandlers();

                // Настраиваем обработчики событий для текстовых полей
                InitializeTextBoxEventHandlers();

                // Запрашиваем данные с API
                RequestCurrencyData();

                // Производим начальные настройки
                InitializeDefaults();

                // Применяем закругление углов к панелям
                ApplyCornerRounding();

                CheckingForANumber(textBox1);
                CheckingForANumber(textBox2);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void InitializeUIElements()
        {
            LoadCountriesAndCurrenciesData();
            AddCurrencyToCombobox(comboBox1);
            AddCurrencyToCombobox(comboBox2);

            AddElementsToCombobox(comboBox3);
            AddElementsToCombobox(comboBox4);
        }

        private void RequestCurrencyData()
        {
            RequestAPI();
        }

        private void InitializeDefaults()
        {
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 1;
        }

        private void ApplyCornerRounding()
        {
            CornerRounding(panel1);
            CornerRounding(panel2);
            CornerRounding(panel3);
            CornerRounding(panel4);
        }

        private void InitializeTextBoxEventHandlers()
        {
            textBox1.TextChanged += textBox1_TextChanged;
            textBox2.TextChanged += textBox2_TextChanged;
        }

        private void InitializeComboBoxEventHandlers()
        {
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            comboBox2.SelectedIndexChanged += comboBox2_SelectedIndexChanged;
        }

        /// <summary>
        /// Initializes data about currencies and countries.
        /// </summary>
        private void LoadCountriesAndCurrenciesData()
        {
            string jsonData = File.ReadAllText(CountriesAndCurrenciesFilePath);
            CountryAndCurrencyData = JsonConvert.DeserializeObject<CountriesAndCurrencies>(jsonData);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Convert1();
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            Convert2();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCountryByCurrency(comboBox1, comboBox3);

            // We change the currency index if the currencies in our comboboxes match.
            UpdateComboBoxIndexToAvoidMatch(comboBox1, comboBox2);
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCountryByCurrency(comboBox2, comboBox4);

            // We change the currency index if the currencies in our comboboxes match.
            UpdateComboBoxIndexToAvoidMatch(comboBox2, comboBox1);
        }

        /// <summary>
        /// Restricts the input of extraneous characters.
        /// </summary>
        /// <param name="textBox"></param>
        private void CheckingForANumber(TextBox textBox)
        {
            textBox.KeyPress += (sender, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != (char)Keys.Back)
                {
                    e.Handled = true; // Отменить ввод символа, если это не цифра, точка или Backspace
                }

                if (e.KeyChar == '.' && textBox.Text.Contains('.'))
                {
                    e.Handled = true;
                }

                if (textBox.Text.Contains('.'))
                {
                    string[] parts = textBox.Text.Split('.');
                    if (parts.Length > 1 && parts[1].Length >= 4 && e.KeyChar != (char)Keys.Back)
                    {
                        e.Handled = true;
                    }
                }
            };
        }

        private void comboBox1_DropDownClosed(object sender, EventArgs e)
        {
            ActiveControl = null;
        }
        private void comboBox2_DropDownClosed(object sender, EventArgs e)
        {
            ActiveControl = null;
        }

        /// <summary>
        /// Create a region with rounded corners.
        /// </summary>
        /// <param name="panel"></param>
        private void CornerRounding(Panel panel)
        {
            // Создание региона с закругленными углами
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(0, 0, PanelRadius, PanelRadius, 180, 90); // Левый верхний угол
            path.AddArc(panel.Width - PanelRadius, 0, PanelRadius, PanelRadius, 270, 90); // Правый верхний угол
            path.AddArc(panel.Width - PanelRadius, panel.Height - PanelRadius, PanelRadius, PanelRadius, 0, 90); // Правый нижний угол
            path.AddArc(0, panel.Height - PanelRadius, PanelRadius, PanelRadius, 90, 90); // Левый нижний угол
            path.CloseFigure();
            panel.Region = new Region(path);
        }

        /// <summary>
        /// Method for retrieving data from the API.
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="apiURL"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<CurrencyConversionData> GetCurrencyConversionData(string apiKey, string apiURL)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                HttpResponseMessage response = await client.GetAsync(apiURL);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();

                    var apiData = JsonConvert.DeserializeObject<CurrencyConversionData>(responseContent);
                    return apiData;
                }
                else
                {
                    throw new Exception("Не удалось получить данные с API. Статусный код: " + response.StatusCode);
                }
            }
        }

        /// <summary>
        /// Currency conversion for combobox1.
        /// </summary>
        private void Convert1()
        {
            textBox2.TextChanged -= textBox2_TextChanged;

            if (decimal.TryParse(textBox1.Text, out decimal value1))
            {
                string originalValue1 = textBox1.Text;

                Dictionary<string, decimal> conversionRates = ApiData.conversion_rates;
                string currency1 = comboBox1.Text;
                string currency2 = comboBox2.Text;

                if (conversionRates.TryGetValue(currency1, out decimal rate1) && conversionRates.TryGetValue(currency2, out decimal rate2))
                {
                    decimal result = (value1 / rate1) * rate2;
                    string resultString = Math.Round(result, 3).ToString("0.000"); // Округляем до тысячных после запятой
                    textBox2.Text = resultString;
                    textBox1.Text = originalValue1;
                }
            }
            else
            {
                textBox2.Text = "";
            }

            textBox2.TextChanged += textBox2_TextChanged;
        }
        /// <summary>
        /// Currency conversion for combobox2
        /// </summary>
        private void Convert2()
        {
            textBox1.TextChanged -= textBox1_TextChanged;

            if (decimal.TryParse(textBox2.Text, out decimal value2))
            {
                string originalValue2 = textBox2.Text;

                Dictionary<string, decimal> conversionRates = ApiData.conversion_rates;
                string currency1 = comboBox1.Text;
                string currency2 = comboBox2.Text;

                if (conversionRates.TryGetValue(currency1, out decimal rate1) && conversionRates.TryGetValue(currency2, out decimal rate2))
                {
                    if (rate2 != 0)
                    {
                        decimal result = (value2 / rate2) * rate1;
                        string resultString = Math.Round(result, 3).ToString("0.000"); // Округляем до тысячных после запятой
                        textBox1.Text = resultString;
                    }
                    else
                    {
                        textBox1.Text = "";
                    }
                }

                textBox2.Text = originalValue2;
            }
            else
            {
                textBox1.Text = "";
            }

            textBox1.TextChanged += textBox1_TextChanged;
        }

        /// <summary>
        /// Request to the API, saving data about the currency and the time of the request. 
        /// Comparison of saved and real time data.
        /// </summary>
        private async void RequestAPI()
        {
            if (!File.Exists(LaunchDataFileName) || !File.Exists(CurrencyDataFileName))
            {
                var launchData = new { LaunchDate = DateTime.Now };
                string jsonLaunchData = JsonConvert.SerializeObject(launchData);
                File.WriteAllText(LaunchDataFileName, jsonLaunchData);

                ApiData = await GetCurrencyConversionData(ApiKey, ApiURL);
                string jsonCurrencyData = JsonConvert.SerializeObject(ApiData);
                File.WriteAllText(CurrencyDataFileName, jsonCurrencyData);
            }
            else
            {
                string jsonLaunchData = File.ReadAllText(LaunchDataFileName);
                var launchData = JsonConvert.DeserializeObject<LaunchData>(jsonLaunchData);
                LastRequestTimestamp = launchData.LaunchDate;

                if ((DateTime.Now - LastRequestTimestamp).TotalHours >= HoursBetweenApiRequests)
                {
                    ApiData = await GetCurrencyConversionData(ApiKey, ApiURL);
                    LastRequestTimestamp = DateTime.Now;

                    // Сохраняем данные о курсах валют и время последнего запроса
                    SaveCurrencyData();
                }
                else
                {
                    string jsonCurrencyData = File.ReadAllText(CurrencyDataFileName);
                    ApiData = JsonConvert.DeserializeObject<CurrencyConversionData>(jsonCurrencyData);
                }
            }
        }
        /// <summary>
        /// We save currency data if the last request to the API was made more than 24 hours.
        /// </summary>
        private void SaveCurrencyData()
        {
            string jsonCurrencyData = JsonConvert.SerializeObject(ApiData);
            var launchData = new LaunchData { LaunchDate = LastRequestTimestamp };
            string jsonLaunchData = JsonConvert.SerializeObject(launchData);
            File.WriteAllText(LaunchDataFileName, jsonLaunchData);
            File.WriteAllText(CurrencyDataFileName, jsonCurrencyData);
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                comboBox3.AutoCompleteSource = AutoCompleteSource.ListItems;
                comboBox3.AutoCompleteMode = AutoCompleteMode.None;

                UpdateCurrencyByCountry(comboBox3, comboBox1);

                string jsonContent = File.ReadAllText(CountriesAndCodesFilePath);

                CountryCodesData = JsonConvert.DeserializeObject<CountriesAndCodes>(jsonContent);

                string selectedCountry = comboBox3.SelectedItem?.ToString(); // Добавляем ?. для безопасного доступа*

                if (!string.IsNullOrEmpty(selectedCountry) && CountryCodesData.countriesAndCodes.TryGetValue(selectedCountry, out string countryCode))
                {
                    string flagURL = $"https://flagsapi.com/{countryCode.ToUpper()}/shiny/48.png";

                    using (WebClient client = new WebClient())
                    {
                        byte[] imageData = client.DownloadData(flagURL);
                        pictureBox1.Image = Image.FromStream(new MemoryStream(imageData));
                    }
                }
                else if (selectedCountry == "Europa")
                {
                    using(WebClient client = new WebClient())
                    {
                        pictureBox1.Image = Properties.Resources.European_Union_Flag;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex}");
            }
        }
        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                comboBox4.AutoCompleteSource = AutoCompleteSource.ListItems;
                comboBox4.AutoCompleteMode = AutoCompleteMode.None;

                UpdateCurrencyByCountry(comboBox4, comboBox2);

                string selectedCountry = comboBox4.SelectedItem?.ToString(); // Добавляем ?. для безопасного доступа*

                if (!string.IsNullOrEmpty(selectedCountry) && CountryCodesData.countriesAndCodes.TryGetValue(selectedCountry, out string countryCode))
                {
                    string flagURL = $"https://flagsapi.com/{countryCode.ToUpper()}/shiny/48.png";

                    using (WebClient client = new WebClient())
                    {
                        byte[] imageData = client.DownloadData(flagURL);
                        pictureBox2.Image = Image.FromStream(new MemoryStream(imageData));
                    }
                }
                else if (selectedCountry == "Europa")
                {
                    pictureBox2.Image = Properties.Resources.European_Union_Flag;
                }
                else
                {
                    // Если выбрано несуществующее или неопределенное значение, можно очистить изображение в PictureBox.
                    pictureBox2.Image = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex}");
            }
        }

        private void AddCurrencyToCombobox(ComboBox combobox)
        {
            string json = File.ReadAllText(CountriesAndCurrenciesFilePath);

            var keyValuePairs = JObject.Parse(json);
            JObject countriesAndCurrencies = keyValuePairs["countriesAndCurrencies"].ToObject<JObject>();

            foreach (var currency in countriesAndCurrencies)
            {
                combobox.Items.Add(currency.Value.ToString());
            }
        }

        private void UpdateCountryByCurrency(ComboBox inputComboBox, ComboBox outputComboBox)
        {
            string selectedCurrency = inputComboBox.SelectedItem?.ToString();

            string selectedCountry = GetCountryByCurrency(selectedCurrency);

            if (!string.IsNullOrEmpty(selectedCountry))
            {
                outputComboBox.SelectedItem = selectedCountry;
            }
        }

        private void UpdateCurrencyByCountry(ComboBox inputComboBox, ComboBox outputComboBox)
        {
            string selectedCountry = inputComboBox.SelectedItem?.ToString();

            // Найдите соответствующую валюту в вашем словаре countriesAndCurrencies
            string selectedCurrency = GetCurrencyByCountry(selectedCountry);

            if (!string.IsNullOrEmpty(selectedCurrency))
            {
                // Обновите выбранную валюту в ComboBox1
                outputComboBox.SelectedItem = selectedCurrency;
            }
        }

        private string GetCountryByCurrency(string currency)
        {
            var pair = CountryAndCurrencyData.countriesAndCurrencies.FirstOrDefault(kv => kv.Value == currency);
            if (!string.IsNullOrEmpty(pair.Key))
            {
                return pair.Key;
            }
            return null;
        }

        private string GetCurrencyByCountry(string country)
        {
            if (CountryAndCurrencyData.countriesAndCurrencies.TryGetValue(country, out string currency))
            {
                return currency;
            }
            return null;
        }

        private void AddElementsToCombobox(ComboBox comboBox)
        {
            KeyValuePair<string, string>[] countriesArray = CountryAndCurrencyData.countriesAndCurrencies.ToArray();

            foreach (var country in countriesArray)
            {
                comboBox.Items.Add(country.Key);
            }
        }

        private void UpdateComboBoxIndexToAvoidMatch(ComboBox firstComboBox,ComboBox secondComboBox)
        {
            if (firstComboBox.SelectedIndex == secondComboBox.SelectedIndex)
            {
                int selectedIndex = secondComboBox.SelectedIndex;
                selectedIndex = (selectedIndex + 1) % secondComboBox.Items.Count;
                secondComboBox.SelectedIndex = selectedIndex;
            }
        }
    }

    public class CurrencyConversionData
    {
        public Dictionary<string, decimal> conversion_rates { get; set; }
    }

    public class LaunchData
    {
        public DateTime LaunchDate { get; set; }
    }

    public class CountriesAndCurrencies
    {
        public Dictionary<string, string> countriesAndCurrencies { get; set; }
    }

    public class CountriesAndCodes
    {
        public Dictionary<string, string> countriesAndCodes { get; set; }
    }
}