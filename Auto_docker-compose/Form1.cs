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
using System.Threading;
using System.Security.Policy;
using System.Diagnostics;

namespace Auto_docker_compose
{
    public partial class Automatizer : Form
    {

        public string dc_path;
        public string dc_folder_path;
        public DirectoryInfo directoryInfo;
        private bool isMouseOverPanel;

        public Automatizer()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;
            
            //textBox3.Text = "node index.js";
            timer1.Start();
            timer2.Start();
            toolTip1.AutoPopDelay = 1000000000; // Время показа подсказки
            toolTip1.InitialDelay = 100; // Задержка перед первым показом
            toolTip1.ReshowDelay = 200; // Задержка перед повторным показом
            toolTip1.SetToolTip(textBox1, "Название сегмента");
            toolTip1.SetToolTip(textBox2, "Параметр image: Вводится без кавычек");
            toolTip1.SetToolTip(textBox3, "Параметр command: Заполняется автоматически, можно дополнить доп. командой");
            toolTip1.SetToolTip(textBox4, "Полный путь до папки в которой нужно создать docker-compose файл или выбрать уже существующий.");
            toolTip1.SetToolTip(textBox5, "Параметр command: Заполняется автоматически из конфигурационного файла");
            //toolTip1.SetToolTip(textBox6, "Данные из .yaml файла");
            toolTip1.SetToolTip(comboBox1,"Параметр restart: опциональный. По умолчанию - none");
            toolTip1.SetToolTip(comboBox2,"Выбор параметра build");
            toolTip1.SetToolTip(comboBox3,"Выбор конфигурационного файла");
            toolTip1.SetToolTip(comboBox3,"Выбор доп. найстроки команды");
            toolTip1.SetToolTip(label11,  "Выбранное устройство");
            toolTip1.SetToolTip(button1, "Добавить запись в файл");
            toolTip1.SetToolTip(button2, "Выбрать/Обновить/Создать файл по выбранному пути");
            toolTip1.SetToolTip(button3, "Очистить поля");
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
                            //textBox6.Text = File.ReadAllText(dc_path);
                            get_buildlist(comboBox2, dc_folder_path);
                            comboBox2.SelectedIndex = 0;
                        }
                        else if (File.Exists(dc_path))
                        {
                            get_buildlist(comboBox2, dc_folder_path);
                            //textBox6.Text = File.ReadAllText(dc_path);

                            comboBox2.SelectedIndex = 0;
                        }
                        else
                        {
                            MessageBox.Show("Путь не найден", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            label11.Text = string.Empty;
                        }

                    }
                    else
                    {
                        MessageBox.Show("Путь не найден", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        label11.Text = string.Empty;
                    }
                }
                catch (Exception ex)
                {

                   MessageBox.Show("Произошла ошибка: " + ex.Message);
                   label11.Text = string.Empty;
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

                MessageBox.Show($"Отсутвует путь до необходимых json файлов конфигурации. По пути, в папке '{twoDirectoriesUp}' отсутствуют нужные директории", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); ; ; ; ;
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
                    //Console.WriteLine(match.Groups[1].Value.Replace(" ", ""));
                    
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
                    //textBox6.Text = File.ReadAllText(dc_path);
                }

            }
            else
            {
                MessageBox.Show("Файл не найден", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        public string get_port(string file)
        {
            try
            {
                string port = "0000"; 
                string twoDirectoriesUp = directoryInfo.Parent?.Parent.FullName; //+ $"ПС{directoryInfo.Name.Substring(2)}";
                string jsonFilePath = twoDirectoriesUp + $@"\ПС{directoryInfo.Name.Substring(2)}";
                //Console.WriteLine(jsonFilePath);
                string jsonString = File.ReadAllText(jsonFilePath + $@"\{file}");
                JObject jsonObject = JObject.Parse(jsonString);

                //port = Convert.ToString(jsonObject["Configuration"]["Port"]);

                if (jsonObject.TryGetValue("Configuration", out JToken configToken))
                {
                    JObject configObject = configToken as JObject;
                    if (configObject != null && configObject.TryGetValue("Port", out JToken portToken))
                    {
                        port = portToken.ToString();
                    }
                }

                return port;
            }
            catch (Exception)
            {

                MessageBox.Show("Отсутствует порт", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "0000";
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
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
                panel2.Enabled = false;
            }
            else
            {
                panel2.Enabled = true;
                button1.Enabled = true;
                button2.Enabled= true;
                button3.Enabled = true;
                button4.Enabled= true;
                button5.Enabled = true;
            }
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox4.SelectedIndex != 0)
            {
                if (comboBox3.SelectedIndex != -1)
                {
                    //Console.WriteLine(comboBox3.SelectedIndex);
                    textBox3.Text = "node " + comboBox4.SelectedItem.ToString() + " " + comboBox3.SelectedItem.ToString();
                }
                
            }
        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            if (textBox4.Text == "")
            {
                Point cursorPosition = Cursor.Position;
                Point panelPosition = panel2.PointToScreen(Point.Empty);
                Rectangle panelRectangle = new Rectangle(panelPosition, panel2.Size);

                if (panelRectangle.Contains(cursorPosition))
                {
                    if (!isMouseOverPanel)
                    {
                        isMouseOverPanel = true;
                        // Подсветка текстового поля
                        textBox4.BackColor = Color.Red;
                        textBox4.BorderStyle = BorderStyle.FixedSingle;
                    }
                }
                else
                {
                    if (isMouseOverPanel)
                    {
                        isMouseOverPanel = false;
                        // Возврат текстового поля к исходному состоянию
                        textBox4.BackColor = SystemColors.Window;
                        textBox4.BorderStyle = BorderStyle.Fixed3D;
                    }
                }
            }
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Очистка файла перед выполнением основной логики
            try
            {
                File.WriteAllText(dc_path, string.Empty);
                Console.WriteLine("Файл успешно очищен.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при очистке файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string lineToAdd = "services:";
            File.AppendAllText(dc_path, lineToAdd + Environment.NewLine);
            for (int i = 0; i < comboBox3.Items.Count; i++)
            {
                bool flag = true;
                string ports = "0000";
                List<string> strings = new List<string>();
                string pattern1;
                string buildtoappend = "";
                string name = "";

                if (File.Exists(dc_path))
                {
                    strings = get_values(dc_folder_path);
                    ports = get_port(comboBox3.Items[i].ToString().Replace(" ", "")).ToString();
                    name = comboBox3.Items[i].ToString().ToLower().Replace("configuration", "").Replace(".json", "");
                    string item = $@"{name}";
                    for (int j = 0; j < comboBox2.Items.Count; j++)
                    {
                        pattern1 = $@".*{comboBox2.Items[j].ToString().ToLower()}.*";
                        
                        

                        //Console.WriteLine(item + " - " + pattern1.Substring(0, pattern1.Length - 1));
                        if (Regex.Match(item, pattern1).Success && !Regex.Match(item, @".*rt.*").Success)
                        {
                            //Console.WriteLine(item + " - " + pattern1);
                            buildtoappend = $"\n            build: {comboBox2.Items[j]}/";
                            flag = true;
                            break;
                        }
                        else if (Regex.Match(item, @".*realtime.*").Success)
                        {

                            //Console.WriteLine(item +" - realtime");
                            buildtoappend = $"\n            build: RT/";
                            flag = true;
                            break;
                        }
                        else if (Regex.Match(item, @".*gs.*").Success)
                        {

                            //Console.WriteLine(item + " - GS");
                            buildtoappend = $"\n            build: GS/";
                            flag = true;
                            break;
                        }
                        else if (Regex.Match(item, @".*arch.*v1").Success)
                        {
                            //Console.WriteLine(item + " - ReadArch");
                            buildtoappend = $"\n            build: ReadArch/";
                            break;
                        }
                        else if (Regex.Match(item, @".*arch.*v2").Success)
                        {
                            //Console.WriteLine(item + " - ReadArchG");
                            buildtoappend = $"\n            build: ReadArchG/";
                            break;
                        }
                        else if (Regex.Match(item, @".*segment.*").Success)
                        {
                            //Console.WriteLine(item + " - " + pattern1);
                            buildtoappend = $"\n            build: Segments/";
                            flag = true;
                            break;
                        }
                        else
                        {
                            flag = false;
                            buildtoappend = "";
                        }

                    }
                    //Console.WriteLine(buildtoappend);
                    if (flag)
                    {
                       
                        if (ports != "0000")
                        {
                            File.AppendAllText(dc_path, $"\n        {name}:");
                            if (Regex.Match(name, @".*disp.*").Success || Regex.Match(name, @".*gal.*").Success || Regex.Match(name, @".*map.*").Success)
                            {
                                File.AppendAllText(dc_path, $"\n            image: '{name.Replace($"{label11.Text.Substring(2)}", "")}:v1.1.0'");
                            }
                            else if (Regex.Match(name, @".*realtime.*").Success || Regex.Match(name, @".*arch.*").Success)
                            {
                                File.AppendAllText(dc_path, $"\n            image: '{name.Replace($"{label11.Text.Substring(2)}", "")}:v1.0.5'");
                            }
                            else if (Regex.Match(name, @"gs").Success)
                            {
                                File.AppendAllText(dc_path, $"\n            image: '{name.Replace($"{label11.Text.Substring(2)}", "")}:v1.0.8'");
                            }
                            else if (Regex.Match(name, @"autorization").Success)
                            {
                                File.AppendAllText(dc_path, $"\n            image: '{name.Replace($"{label11.Text.Substring(2)}", "")}:v1.0.3'");
                            }
                            else if (Regex.Match(name, @"segment").Success)
                            {
                                File.AppendAllText(dc_path, $"\n            image: '{name.Replace($"{label11.Text.Substring(2)}", "")}:v1.0.7'");
                            }
                            File.AppendAllText(dc_path, buildtoappend);
                            if (Regex.Match(item, @".*segment.*").Success)
                            {
                                //Console.WriteLine(Regex.Match(item, @".*segment.*").Success);
                                File.AppendAllText(dc_path, $"\n            command: node appSegments.js {comboBox3.Items[i]}");
                            }
                            else if (Regex.Match(item, @".*autorization.*").Success)
                            {
                                Console.WriteLine(Regex.Match(item, @".*autorization.*").Success);
                                File.AppendAllText(dc_path, $"\n            command: node authServer.js");
                            }
                            else if (Regex.Match(item, @".*disp.*").Success && !Regex.Match(item, @".*gs.*").Success)
                            {
                                //Console.WriteLine(Regex.Match(item, @".*disp.*").Success);
                                File.AppendAllText(dc_path, $"\n            command: node app.js {comboBox3.Items[i]}");
                            }
                            else if (Regex.Match(item, @".*realtime.*").Success)
                            {
                                //Console.WriteLine(Regex.Match(item, @".*realtime.*").Success);
                                File.AppendAllText(dc_path, $"\n            command: node server.js {comboBox3.Items[i]}");
                            }
                            else if (Regex.Match(item, @".*gs.*").Success)
                            {
                                
                                //Console.WriteLine(Regex.Match(item, @".*gs.*").Success);
                                File.AppendAllText(dc_path, $"\n            command: node appGS.js {comboBox3.Items[i]}");
                            }
                            else
                            {
                                File.AppendAllText(dc_path, $"\n            command: node app.js {comboBox3.Items[i]}");
                            }
                            
                            File.AppendAllText(dc_path, $"\n            ports:\n             - {ports}:{ports}");
                            File.AppendAllText(dc_path, "\n            volumes:            ");
                            foreach (var item1 in strings)
                            {
                                File.AppendAllText(dc_path, $"\n             - /home/sedatec/DockerConfig/ПС{directoryInfo.Name.Substring(2)}:{item1}/Configuration");
                            }
                        }
                    }
                    
                    

                    

                }
                else
                {
                    MessageBox.Show("Файл не найден", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            //textBox6.Text = File.ReadAllText(dc_path);
            MessageBox.Show("Файл успешно дополнен");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (File.Exists(dc_path))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(dc_path);
                Process.Start(dc_path);
            }
            else
            {
                MessageBox.Show($"Файла по пути: {dc_path} не существует");
            }

        }
    }
}
