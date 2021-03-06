﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ModbusSimulator.models;

namespace ModbusSimulator
{
    public partial class ModbusSimulator : Form
    {
        public NodesRunner NodesRunner;
       
        public byte AddNodeIp1 { get; set; }
        public byte AddNodeIp2 { get; set; }
        public byte AddNodeIp3 { get; set; }
        public byte AddNodeIp4 { get; set; }
        private System.Windows.Forms.Timer t;


        public ModbusSimulator()
        {
            NodesRunner = new NodesRunner();
            AddNodeIp1 = 127;
            AddNodeIp2 = 1;
            AddNodeIp3 = 1;
            AddNodeIp4 = 2;
            InitializeComponent();
            t = new System.Windows.Forms.Timer();
            t.Interval = 400;
            t.Tick += new EventHandler(UpdateValues);
            BindControls();
        
            t.Start();
        }

        void UpdateValues(object sender, EventArgs e)
        {
            if (!(listBox2.SelectedItem is RegisterValue reg)) return;
            var node = NodesRunner.Nodes.FirstOrDefault(n => n.Id == reg.NodeConfigId);
            if (node != null)
            {
                var regval = node.ActiveRegisters.FirstOrDefault(ar => ar.RegisterValueId == reg.RegisterValueId);
                reg.Value = regval.Value;
                if (checkedListBox1 != null && reg.RegisterType == RegisterType.Coil)
                {
                    checkedListBox1.SetItemChecked(0, Convert.ToBoolean(reg.Value));
                }
            }
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
                case "0x - Coil": return RegisterType.Coil;
                case "1x - Discrete Input": return RegisterType.DiscreteInput;
                case "3x - Input Register": return RegisterType.InputRegister;
                case "4x - Holding Register": return RegisterType.HoldingRegister;
                default: return RegisterType.Coil;
            }
        }

        private void AddNodeButton_Click(object sender, EventArgs e)
        {
            var ip = new byte[] { AddNodeIp1, AddNodeIp2, AddNodeIp3, AddNodeIp4 };
            var registerType = GetRegisterType(comboBoxRegType.SelectedItem.ToString());
            var registerNumber = Convert.ToInt16(textBoxRegNumber.Text);
            var name = textBoxName.Text;
            NodesRunner.AddNodeSimulation(
               ip,
               registerType,
               registerNumber,
               name);

            UpdateNodeList();
            UpdateRegisterList();
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
            listBox1.DisplayMember = nameof(Node.Ip);
            listBox1.ValueMember = nameof(Node.Id);

            UpdateRegisterList();
            //listBox2.ValueMember = nameof(Node.ActiveRegisters);
        }

        private void UpdateRegisterList()
        {
            if (!(listBox1.SelectedItem is Node node)) return;

            listBox2.DataSource = null;
            listBox2.DataSource = node.ActiveRegisters;
            listBox2.DisplayMember = nameof(RegisterValue.Name);
            listBox2.ValueMember = nameof(RegisterValue.RegisterNumber);
        }

        private void UpdateRegCheckBox(RegisterValue reg)
        {
            BitArray b = new BitArray(new int[] { reg.Value });
            for (int i = 0; i < 15; i++)
            {
                var isChecked = Convert.ToBoolean(b[i]);
                checkedListBox1.SetItemChecked(i, isChecked);
            }
        }
        private void UpdateRegValueText(RegisterValue reg)
        {
            // Update input textbox
            maskedTextBox1.DataBindings.Clear();
            maskedTextBox1.DataBindings.Add(new Binding("Text", reg, nameof(RegisterValue.Value)));
        }

        //private UpdateRegValue(RegisterValue reg)
        //{}

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateRegisterList();
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(listBox2.SelectedItem is RegisterValue reg)) return;
            UpdateRegCheckBox(reg);
            UpdateRegValueText(reg);
        }

        private void maskedTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (!(listBox2.SelectedItem is RegisterValue reg)) return;
            reg.Value = Convert.ToUInt16(maskedTextBox1.Text);
            UpdateRegCheckBox(reg);
            NodesRunner.UpdateNodeValue(reg.NodeConfigId, reg.RegisterValueId, reg.Value);
        }

        private void checkedListBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!(listBox1.SelectedItem is Node node) || !(listBox2.SelectedItem is RegisterValue reg)) return;

            BitArray bitArray = new BitArray(16);

            for (int i = 0; i < 16; i++)
            {
                bitArray[i] = checkedListBox1.GetItemChecked(i);
            }

            var value = GetUShortFromBitArray(bitArray);
            reg.Value = Convert.ToUInt16(value);

            UpdateRegValueText(reg);
            NodesRunner.UpdateNodeValue(reg.NodeConfigId, reg.RegisterValueId, reg.Value);
        }

        private ushort GetUShortFromBitArray(BitArray bitArray)
        {

            if (bitArray.Length > 16)
                throw new ArgumentException("Argument length shall be at most 16 bits.");

            int[] array = new int[1];
            bitArray.CopyTo(array, 0);
            return Convert.ToUInt16(array[0]);

        }

        // Delete register
        private void button1_Click(object sender, EventArgs e)
        {
            if (!(listBox2.SelectedItem is RegisterValue reg)) return;
            NodesRunner.RemoveNodeRegiser(reg);
            UpdateRegisterList();
        }
    }
}
