using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace resourcer
{
    /// <summary>
    /// Логика взаимодействия для ResultWindow.xaml
    /// </summary>
    public partial class ResultWindow : Window
    {
        public ResultWindow()
        {
            InitializeComponent();
        }

        private List<string> copyList;

        public void Result(string filter, Dictionary<string, List<string>> list)
        {
            int count = 0;
            int match = 0;
            copyList = new List<string>();

            // заголовок
            result.Text = "Результаты поиска по *." + filter + " файлам:\r\n";

            foreach (KeyValuePair<string, List<string>> line in list)
            {
                // вывод заголовка
                result.AppendText("\r\n\t" + line.Key + " [найдено " + line.Value.Count + "]:\r\n");
                try
                {
                    // вывод файлов
                    foreach (string file in line.Value)
                    {
                        // добавляем файл
                        result.AppendText("\t\t" + file + "\r\n");
                        // если нет в списке для копирования, то добавляем
                        if (!copyList.Contains(file))
                        {
                            copyList.Add(file);
                            count++;
                        }
                        match++;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message + "\n" + e.StackTrace, "Result Loop Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // статус

            //statusInfo.Text = "Всего совпадений: " + match;

            // если файлов нет, блокируем кнопки чтобы не сохраняли пустой список
            if (count == 0)
            {
                //listSave.Enabled = false;
                //listCopy.Enabled = false;
            }
        }

        /*private void listCopy_click(object sender, EventArgs e)
        {
            if (choicePath.ShowDialog() != DialogResult.OK)
                return;

            string selectPath = choicePath.SelectedPath;
            string currentPath;

            try
            {
                foreach (string file in copyList)
                {
                    // собераем путь к файлу
                    currentPath = selectPath + "\\" + file.Replace(Path.GetPathRoot(file), "");
                    // создаем папку
                    Directory.CreateDirectory(Directory.GetParent(currentPath).ToString());
                    // копируем файл
                    File.Copy(file, currentPath);
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, "Copy Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void listSave_click(object sender, EventArgs e)
        {
            if (saveDialog.ShowDialog() == DialogResult.OK)
                File.WriteAllText(saveDialog.FileName, resultText.Text, Encoding.UTF8);
        }*/
    }
}
