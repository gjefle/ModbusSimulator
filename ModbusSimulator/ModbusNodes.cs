using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModbusSimulator
{
    public partial class ModbusSimulator : Form
    {
        public NodesRunner NodesRunner;
        public byte AddNodeIp1 { get; set; }
        public byte AddNodeIp2 { get; set; }
        public byte AddNodeIp3 { get; set; }
        public byte AddNodeIp4 { get; set; }

        public ModbusSimulator()
        {
            NodesRunner = new NodesRunner();
            AddNodeIp1 = 127;
            AddNodeIp2 = 1;
            AddNodeIp3 = 1;
            AddNodeIp4 = 2;
            InitializeComponent();

            BindControls();

        }



        protected void BindControls()
        {
          
            UpdateNodeList();
            // Bind ip boxes
            textBoxIp1.DataBindings.Add(new Binding("Text", this, nameof(AddNodeIp1)));
            textBoxIp2.DataBindings.Add(new Binding("Text", this, nameof(AddNodeIp2)));
            textBoxIp3.DataBindings.Add(new Binding("Text", this, nameof(AddNodeIp3)));
            textBoxIp4.DataBindings.Add(new Binding("Text", this, nameof(AddNodeIp4)));



        }

        private RegisterType GetRegisterType(string selection)
        {
            switch (selection)
            {
                case "0x": return RegisterType.Coil;
                case "1x": return RegisterType.DiscreteInput;
                case "3x": return RegisterType.InputRegister;
                case "4x": return RegisterType.HoldingRegister;
                default: return RegisterType.Coil;
            }
        }

        private void AddNodeButton_Click(object sender, EventArgs e)
        {
            var ip = new byte[] { AddNodeIp1, AddNodeIp2, AddNodeIp3, AddNodeIp4 };
            var registerType = GetRegisterType(comboBoxRegType.SelectedText);
            var registerNumber = Convert.ToInt16(textBoxRegNumber.Text);
            NodesRunner.AddNodeSimulation(
               ip,
               registerType,
               registerNumber,
               AddNodeName.Text);

            UpdateNodeList();
            UpdateRegisterList();

            AddNodeName.Text = "";
        }

  

        private void buttonDeleteNode_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem is Node node)
            {
                NodesRunner.RemoveNodeSimulation(node.Id);
                UpdateNodeList();
            }
        }

        private void UpdateNodeList()
        {
            listBox1.DataSource = null;
            listBox1.DataSource = NodesRunner.Nodes;
            listBox1.DisplayMember = nameof(Node.Name);
            listBox1.ValueMember = nameof(Node.Id);
        }

        private void UpdateRegisterList()
        {
            
        }
    }
}
