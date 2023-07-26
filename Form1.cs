using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Currency_Converter;
using Newtonsoft.Json;
using System.Globalization;
using Newtonsoft.Json.Linq;
using System.Data.Odbc;
using System.Security.Cryptography.X509Certificates;

namespace Currency_Converter
{
    public partial class Form1 : Form
    {
        private const int radius = 10;
        private const string apiKey = "da4eac743cccc395c19babfd";
        private const string apiURL = "https://v6.exchangerate-api.com/v6/da4eac743cccc395c19babfd/latest/USD";
        private DateTime lastRequestDate;
        private CurrencyConversionData apiData;
        private const string fileNameLaunchData = "launch_data.json";
        private const string fileNameCurrencyData = "currency_data.json";
        private readonly List<string> currencies1 = new List<string>
        {
            "USD", "EUR", "UAH", "JPY", "AUD", "CAD", "RUB", "PLN"
        };
        private readonly List<string> country = new List<string>
        {
            "USA", "Europe", "Ukraine", "Japan", "Austria", "Canada", "Russia", "Poland"
        };

        public Form1()
        {
            try
            {
                InitializeComponent();

                Load += Form1_Load;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.Items.AddRange(currencies1.ToArray());
            comboBox2.Items.AddRange(currencies1.ToArray());

            textBox1.TextChanged += textBox1_TextChanged;
            textBox2.TextChanged += textBox2_TextChanged;

            RequestAPI();

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 1;

            CornerRounding(panel1);
            CornerRounding(panel2);

            CheckingForANumber(textBox1);
            CheckingForANumber(textBox2);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Convert1();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            Convert2();
        }

        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == comboBox2.SelectedItem)
            {
                int selectedIndex = comboBox2.SelectedIndex;
                selectedIndex = (selectedIndex + 1) % comboBox2.Items.Count; // Переход к следующему элементу в круговом порядке
                comboBox2.SelectedIndex = selectedIndex;
            }
            textBox3.Text = $"Country: {country[comboBox1.SelectedIndex]}";
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == comboBox2.SelectedItem)
            {
                int selectedIndex = comboBox1.SelectedIndex;
                selectedIndex = (selectedIndex + 1) % comboBox1.Items.Count; // Переход к следующему элементу в круговом порядке
                comboBox1.SelectedIndex = selectedIndex;
            }
            textBox4.Text = $"Country: {country[comboBox2.SelectedIndex]}";
        }

        private void CheckingForANumber(TextBox textBox)
        {
            textBox.KeyPress += (sender, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                {
                    e.Handled = true; // Отменить ввод символа, если это не цифра, точка или управляющий символ
                }

                // Разрешить ввод точки только один раз и только если ее нет в текстовом поле
                if (e.KeyChar == '.' && textBox.Text.Contains("."))
                {
                    e.Handled = true;
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

        private void CornerRounding(Panel panel)
        {
            // Создание региона с закругленными углами
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90); // Левый верхний угол
            path.AddArc(panel.Width - radius, 0, radius, radius, 270, 90); // Правый верхний угол
            path.AddArc(panel.Width - radius, panel.Height - radius, radius, radius, 0, 90); // Правый нижний угол
            path.AddArc(0, panel.Height - radius, radius, radius, 90, 90); // Левый нижний угол
            path.CloseFigure();
            panel.Region = new Region(path);
        }

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

        private void Convert1()
        {
            textBox2.TextChanged -= textBox2_TextChanged;

            if (decimal.TryParse(textBox1.Text, out decimal value1))
            {
                string originalValue1 = textBox1.Text;

                Dictionary<string, decimal> conversionRates = apiData.conversion_rates;
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
        private void Convert2()
        {
            textBox1.TextChanged -= textBox1_TextChanged;

            if (decimal.TryParse(textBox2.Text, out decimal value2))
            {
                string originalValue2 = textBox2.Text;

                Dictionary<string, decimal> conversionRates = apiData.conversion_rates;
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

        private async void RequestAPI()
        {
            if (!File.Exists(fileNameLaunchData) || !File.Exists(fileNameCurrencyData) )
            {
                var launchData = new { LaunchDate = DateTime.Now };
                string jsonLaunchData = JsonConvert.SerializeObject(launchData);
                File.WriteAllText(fileNameLaunchData, jsonLaunchData);

                apiData = await GetCurrencyConversionData(apiKey, apiURL);
                string jsonCurrencyData = JsonConvert.SerializeObject(apiData);
                File.WriteAllText(fileNameCurrencyData, jsonCurrencyData);
            }
            else
            {
                string jsonLaunchData = File.ReadAllText(fileNameLaunchData);
                var launchData = JsonConvert.DeserializeObject<LaunchData>(jsonLaunchData);
                lastRequestDate = launchData.LaunchDate;

                if ((DateTime.Now - lastRequestDate).TotalHours >= 24)
                {
                    apiData = await GetCurrencyConversionData(apiKey, apiURL);
                    lastRequestDate = DateTime.Now;

                    // Сохраняем данные о курсах валют и время последнего запроса
                    SaveCurrencyData();
                }
                else
                {
                    string jsonCurrencyData = File.ReadAllText(fileNameCurrencyData);
                    apiData = JsonConvert.DeserializeObject<CurrencyConversionData>(jsonCurrencyData);
                }
            }
        }
        private void SaveCurrencyData() 
        {
            string jsonCurrencyData = JsonConvert.SerializeObject(apiData);
            var launchData = new LaunchData { LaunchDate = lastRequestDate };
            string jsonLaunchData = JsonConvert.SerializeObject(launchData);
            File.WriteAllText(fileNameLaunchData, jsonLaunchData);
            File.WriteAllText(fileNameCurrencyData, jsonCurrencyData);
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
}