using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Currency_Converter
{
    public partial class Form1 : Form
    {
        private List<string> currencies1 = new List<string>
        {
            "USD", "EUR", "UAN", "JPY", "AUD", "CAD", "RUB", "PLN"// и другие валюты...
        };

        private List<string> country = new List<string>
        {
            "United States of America", "Europe", "Ukraine", "Japan", "Austria", "Canada", "Russia", "Poland"
        };

        public Form1()
        {
            InitializeComponent();

            comboBox1.Items.AddRange(currencies1.ToArray());
            comboBox2.Items.AddRange(currencies1.ToArray());
            
            textBox1.KeyPress += textBox1_KeyPress;
            textBox2.KeyPress += textBox2_KeyPress;
            Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Добавление начального элемента в comboBox1
            comboBox1.SelectedIndex = 0;

            // Добавление начального элемента в comboBox2
            comboBox2.SelectedIndex = 0;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            CheckingForANumber(e);
        }
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            CheckingForANumber(e);
        }

        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == comboBox2.SelectedItem)
            {
                int selectedIndex = comboBox2.SelectedIndex;
                selectedIndex = (selectedIndex + 1) % comboBox2.Items.Count; // Переход к следующему элементу в круговом порядке
                comboBox2.SelectedIndex = selectedIndex;
            }
            textBox3.Text = country[comboBox1.SelectedIndex];
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == comboBox2.SelectedItem)
            {
                int selectedIndex = comboBox1.SelectedIndex;
                selectedIndex = (selectedIndex + 1) % comboBox1.Items.Count; // Переход к следующему элементу в круговом порядке
                comboBox1.SelectedIndex = selectedIndex;
            }
            textBox4.Text = country[comboBox2.SelectedIndex];
        }

        private void CheckingForANumber(KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // Отменить ввод символа, если это не цифра или управляющий символ
            }
        }

    }
}
