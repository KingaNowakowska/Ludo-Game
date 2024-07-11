using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Schema;

namespace ludo_game
{
    public partial class Message_ComboBox : Form
    {
        public string SelectedValue { get; private set; }
        public List<string> options;

        public Message_ComboBox(List<string> options)
        {
            InitializeComponent();
            this.options = options;

            foreach (var color in options)
            {
                comboBox1.Items.Add(color);
            }

            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;

            // Optionally, set the ComboBox to select the first item by default
            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Assign the selected value to the property
            SelectedValue = comboBox1.SelectedItem.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}

