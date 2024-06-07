﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;

namespace Auto_docker_compose
{
    public partial class Automatizer : Form
    {

        public string dc_path;
        public string dc_folder_path;
        public DirectoryInfo directoryInfo;

        public Automatizer()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
            //textBox3.Text = "node index.js";
            timer1.Start();
        }


        public void button2_Click(object sender, EventArgs e)
        {
            

            dc_folder_path = textBox4.Text;
            dc_path = Path.Combine(dc_folder_path, "docker-compose.yaml");

            directoryInfo = new DirectoryInfo(dc_folder_path);
            label11.Text = directoryInfo.Name.ToString();

            get_volume(directoryInfo);

            string lineToAdd = "services:";

            DialogResult result = MessageBox.Show("Вы уверены, что хотите создать или обновить файл docker-compose.yaml?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            

            if (result == DialogResult.Yes)
            {
                
                try
                {
                    if (textBox4.Text != "")
                    {
                        if (!File.Exists(dc_path))
                        {
                            File.AppendAllText(dc_path, lineToAdd + Environment.NewLine);
                            MessageBox.Show("Файл успешно создан");
                            get_buildlist(comboBox2, dc_folder_path);
                            comboBox2.SelectedIndex = 0;
                        }
                        else if (File.Exists(dc_path))
                        {
                            get_buildlist(comboBox2, dc_folder_path);
                            comboBox2.SelectedIndex = 0;
                        }
                        else
                        {
                            MessageBox.Show("Путь не найден", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                    }
                    else
                    {
                        MessageBox.Show("Путь не найден", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {

                   MessageBox.Show("Произошла ошибка: " + ex.Message);
                }
                
            }
        }

        public void get_buildlist(ComboBox cb1, string folder_path)
        { 
            cb1.Items.Clear();
            if (Directory.Exists(folder_path))
            {
                try
                {
                    string[] directories = Directory.GetDirectories(folder_path);

                    foreach (string directory in directories)
                    {
                        // Проверяем наличие файла Dockerfile в текущей папке
                        if (File.Exists(Path.Combine(directory, "Dockerfile")))
                        {
                            cb1.Items.Add(Path.GetFileName(directory));
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Указанный путь не существует.");
            }
        }

        public string get_ports(string folder_path)
        {
            string dockerfile_path = Path.Combine(folder_path, Path.Combine(comboBox2.SelectedItem.ToString(), "Dockerfile"));
            string str = File.ReadAllText(dockerfile_path);
            string[] splitItems = str.Replace(" ", "").Split(new string[] { "EXPOSE" }, StringSplitOptions.None);
            
            return splitItems[1].Split('#')[0].Replace(" ", "");
        }
        public void get_volume(DirectoryInfo directoryInfo)
        {

            string[] jsons;
            string twoDirectoriesUp = directoryInfo.Parent?.Parent.FullName; //+ $"ПС{directoryInfo.Name.Substring(2)}";
            string jsonFilePath = twoDirectoriesUp + $@"\ПС{directoryInfo.Name.Substring(2)}";
            try
            {
                jsons = Directory.GetFiles(jsonFilePath);
                foreach (var item in jsons)
                {
                    DirectoryInfo di = new DirectoryInfo(item);

                    comboBox3.Items.Add(di.Name);
                }
            }
            catch (Exception)
            {

                MessageBox.Show("Ошибка в пути", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            

            //string[] directories = Directory.GetDirectories(twoDirectoriesUp);
            //string pattern = @".*\\ПС.*";
            //foreach (var item in directories)
            //{
            //    if (Regex.Match(item, pattern).Success)
            //    {
            //        Console.WriteLine(item);
            //    }
            //}



        }

        public List<string> get_values(string folder_path)
        {
            List<string> list = new List<string>();

            string dockerfile_path = Path.Combine(folder_path, Path.Combine(comboBox2.SelectedItem.ToString(), "Dockerfile"));
            string[] lines = File.ReadAllLines(dockerfile_path);
            string pattern = @"^WORKDIR (/usr/src/app/.*)";

            foreach (string line in lines)
            {
                Match match = Regex.Match(line, pattern);
                if (match.Success)
                {
                    Console.WriteLine(match.Groups[1].Value.Replace(" ", ""));
                    
                    list.Add(match.Groups[1].Value.Replace(" ", ""));
                }
            }
            list.RemoveAt(list.Count - 1);
            return list;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool flag = true;
            string ports;
            List<string> strings = new List<string>();

            if (File.Exists(dc_path))
            {
                foreach (var item in File.ReadAllLines(dc_path))
                {
                    if (item.ToString().Replace(" ","") == textBox1.Text + ":")
                    {
                        MessageBox.Show("Используемое название уже существует", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    strings = get_values(dc_folder_path);
                    ports = get_port(comboBox3.SelectedItem.ToString().Replace(" ", "")).ToString();
                    File.AppendAllText(dc_path, $"\n        {textBox1.Text}:");
                    File.AppendAllText(dc_path, $"\n            image: '{textBox2.Text}'");
                    if (comboBox1.SelectedIndex != 0)
                    {
                        File.AppendAllText(dc_path, $"\n            restart: '{comboBox1.SelectedItem}'");
                    }
                    File.AppendAllText(dc_path, $"\n            build: {comboBox2.SelectedItem}/");
                    File.AppendAllText(dc_path, $"\n            command: {textBox3.Text}");
                    File.AppendAllText(dc_path, $"\n            ports:\n             - {ports}:{ports}");
                    File.AppendAllText(dc_path,  "\n            volumes:            ");
                    foreach (var item in strings)
                    {
                        File.AppendAllText(dc_path, $"\n             - /home/sedatec/DockerConfig/ПС{directoryInfo.Name.Substring(2)}:{item}/Configuration");
                    }

                    MessageBox.Show("Файл успешно дополнен");
                }

            }
            else
            {
                MessageBox.Show("Файл не найден", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            get_volume(directoryInfo);
        }

        public int get_port(string file)
        {
            try
            {
                string twoDirectoriesUp = directoryInfo.Parent?.Parent.FullName; //+ $"ПС{directoryInfo.Name.Substring(2)}";
                string jsonFilePath = twoDirectoriesUp + $@"\ПС{directoryInfo.Name.Substring(2)}";
                Console.WriteLine(jsonFilePath);
                string jsonString = File.ReadAllText(jsonFilePath + $@"\{file}");
                JObject jsonObject = JObject.Parse(jsonString);
                int port = (int)jsonObject["Configuration"]["Port"];
                return port;
            }
            catch (Exception)
            {

                MessageBox.Show("Отсутствует порт", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
            

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox3.Text = "node " + comboBox3.SelectedItem.ToString().Replace(" ", "");
            textBox5.Text = get_port(comboBox3.SelectedItem.ToString().Replace(" ", "")).ToString();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            textBox1.Text = string.Empty;
            textBox2.Text = string.Empty;
            textBox3.Text= string.Empty;
            textBox4.Text = string.Empty;
            textBox5.Text = string.Empty;
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (textBox4.Text == "")
            {
                button2.Enabled = false;
            }
            else
            {
                button2.Enabled= true;
            }
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox4.SelectedIndex != 0)
            {
                if (comboBox3.SelectedIndex != -1)
                {
                    Console.WriteLine(comboBox3.SelectedIndex);
                    textBox3.Text = "node " + comboBox4.SelectedItem.ToString() + " " + comboBox3.SelectedItem.ToString();
                }
                
            }
        }
    }
}
