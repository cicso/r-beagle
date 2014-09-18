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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using System.Xml;
using System.Threading;

namespace resourcer
{
    enum FType { MATCH, DIFF };
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.ComponentModel.BackgroundWorker searchAsync;
        private string optionsFile;
        private Dictionary<string, List<string>> searchResult;
        private FType searchType;
        private int searchTotal = 0;
        private int searchCurrent = 0;
        private int searchMatch = 0;

        // for test
        private List<string> searchDataDirectory;
        private string searchDataStrings;
        private string searchDataMask;
        private bool searchDataRegister;
        private bool searchDataAllString;
        private bool searchDataOneString;
        private bool searchDataMatch;

        public MainWindow()
        {
            InitializeComponent();

            // async search init
            searchAsync = new System.ComponentModel.BackgroundWorker();
            searchAsync.WorkerReportsProgress = true;
            searchAsync.WorkerSupportsCancellation = true;
            searchAsync.DoWork += new System.ComponentModel.DoWorkEventHandler(searchAsync_work);
            searchAsync.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(searchAsync_progress);
            searchAsync.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(searchAsync_complete);

            // настройки
            optionsFile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Soulshanny/Finder.xml");
            optionsLoad();
        }

        // ------------------------------------------------------------
        // Сброс данных
        // ------------------------------------------------------------
        private void clear()
        {
            searchDataDirectory = new List<string>();
            searchResult = new Dictionary<string, List<string>>();
            searchCurrent = searchTotal = searchMatch = 0;

            searchDataDirectory = new List<string>();
            searchDataStrings = "";
        }

        private void enableControl(bool state = false)
        {
            searchCancel.IsEnabled = !state;
            searchStart.IsEnabled = state;
            searchStrings.IsEnabled = state;
            folderAdd.IsEnabled = state;
            folderDel.IsEnabled = state;
            folderList.IsEnabled = state;
            fileType.IsEnabled = state;
            keyAllString.IsEnabled = state;
            keyOneString.IsEnabled = state;
            keyRegister.IsEnabled = state;
            typeDiff.IsEnabled = state;
            typeMatch.IsEnabled = state;
        }

        // ------------------------------------------------------------
        // Сохранение настроек
        // ------------------------------------------------------------
        private void optionsSave()
        {

            string dir = Directory.GetParent(optionsFile).ToString();
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            XmlTextWriter xml = new XmlTextWriter(optionsFile, Encoding.UTF8);
            xml.WriteStartDocument();
            xml.WriteStartElement("options");
            xml.WriteEndElement();
            xml.Close();

            // сохраняем данные
            XmlDocument doc = new XmlDocument();
            doc.Load(optionsFile);

            // маска файлов
            XmlNode mask = doc.CreateElement("filemask");
            XmlAttribute maskvalue = doc.CreateAttribute("value");
            maskvalue.Value = fileType.Text;
            mask.Attributes.Append(maskvalue);
            doc.DocumentElement.AppendChild(mask);

            foreach (ListBoxItem value in folderList.Items)
            {
                XmlNode folder = doc.CreateElement("folder");
                doc.DocumentElement.AppendChild(folder);
                XmlAttribute path = doc.CreateAttribute("value");
                path.Value = value.Content.ToString();
                folder.Attributes.Append(path);
            }

            // добавляем слова для поиска
            XmlNode word = doc.CreateElement("word");
            word.InnerText = searchStrings.Text.Replace("\r\n", "|;|");
            doc.DocumentElement.AppendChild(word);

            optionsParam(doc, "register", keyRegister.IsChecked);
            optionsParam(doc, "all", keyAllString.IsChecked);
            optionsParam(doc, "one", keyOneString.IsChecked);
            optionsParam(doc, "match", typeMatch.IsChecked);

            doc.Save(optionsFile);
        }

        private void optionsParam(XmlDocument doc, string name, bool? value)
        {
            XmlNode param = doc.CreateElement("param");
            XmlAttribute type = doc.CreateAttribute("type");
            XmlAttribute state = doc.CreateAttribute("state");

            type.Value = name;
            state.Value = value.ToString();

            param.Attributes.Append(type);
            param.Attributes.Append(state);

            doc.DocumentElement.AppendChild(param);
        }

        // ------------------------------------------------------------
        // Загрузка настроек
        // ------------------------------------------------------------
        private void optionsLoad()
        {
            // нет файла
            if (!File.Exists(optionsFile))
                return;

            // читаем настройка
            XmlReader xml = XmlReader.Create(optionsFile);
            xml.MoveToContent();
            while (xml.Read())
            {
                if (xml.NodeType == XmlNodeType.Element)
                {
                    switch (xml.Name)
                    {
                        case "filemask":
                            fileType.Text = xml.GetAttribute("value");
                            break;
                        case "folder":
                            ListBoxItem item = new ListBoxItem();
                            item.Content = xml.GetAttribute("value");
                            item.ToolTip = item.Content;
                            folderList.Items.Add(item);
                            break;
                        case "param":
                            if (xml.GetAttribute("type") == "register")
                                keyRegister.IsChecked = Convert.ToBoolean(xml.GetAttribute("state"));
                            if (xml.GetAttribute("type") == "all")
                                keyAllString.IsChecked = Convert.ToBoolean(xml.GetAttribute("state"));
                            if (xml.GetAttribute("type") == "one")
                                keyOneString.IsChecked = Convert.ToBoolean(xml.GetAttribute("state"));
                            if (xml.GetAttribute("type") == "match")
                            {
                                typeMatch.IsChecked = Convert.ToBoolean(xml.GetAttribute("state"));
                                typeDiff.IsChecked = !Convert.ToBoolean(xml.GetAttribute("state"));
                            }
                            break;
                    }
                }
                if (xml.NodeType == XmlNodeType.Text)
                    searchStrings.Text = xml.Value.Replace("|;|", "\r\n");
            }
            xml.Close();
        }

        // ------------------------------------------------------------
        // Добавление найденых файлов
        // ------------------------------------------------------------
        private void finderAddMatch(string key, string file)
        {
            // добавляем переданый ключ если его нет в списке
            if (!searchResult.ContainsKey(key))
                searchResult.Add(key, new List<string>());
            // добавляем файл и увеличиваем счетчик
            searchResult[key].Add(file);
            searchMatch++;
        }
        
        private void searchAsync_work(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            System.ComponentModel.BackgroundWorker worker = (System.ComponentModel.BackgroundWorker)sender;
            List<string> list = new List<string>();

            // ищем в указаных папках все файлы с подходящим типом
            // найденые файлы записываем в массив
            try
            {
                for (int i = 0; i < searchDataDirectory.Count; i++)
                {
                    /*
                     * Метод основаный на запросе нужных данных из другого потока
                     * 
                    
                    // получаем маску
                    string mask = "*.*";
                    fileType.Dispatcher.Invoke((ThreadStart)delegate { mask = "*." + fileType.Text; });

                    // получаем данные папок
                    string path = "";
                    folderList.Items.Dispatcher.Invoke((ThreadStart)delegate { path = ((ListBoxItem)folderList.Items[i]).Content.ToString(); }); */

                    var files = Directory.EnumerateFiles(searchDataDirectory[i], "*." + searchDataMask, SearchOption.AllDirectories);
                    foreach (string file in files)
                    {
                        // Invoke(new delegateNoParam(updateCount));
                        worker.ReportProgress(0, "update_count");

                        // прерывание выполнения
                        if (worker.CancellationPending)
                        {
                            e.Cancel = true;
                            break;
                        }
                    }
                    list.AddRange(files);
                }
            }
            catch (Exception error)
            {
                System.Windows.Forms.MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK);
            }

            // изменяем прогресс бар и инфу о поиске
            worker.ReportProgress(0, "change_bar");

            // данные для поиска по файлам
            try
            {
                string find_data = searchDataRegister ? searchDataStrings : searchDataStrings.ToLower();
                string[] words = find_data.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                bool next = false;

                float step = 100 / (float)(words.Length * list.Count);
                float count = 0;
                int percent = 0;

                // перебераем все файлы
                foreach (string file in list)
                {
                    // 
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }

                    // нет файла - пропускаем
                    if (!File.Exists(file)) continue;

                    // читаем файл и сбрасываем регист если нужно
                    string file_text = File.ReadAllText(file);
                    if (!searchDataRegister)
                        file_text = file_text.ToLower();

                    // если не выбрано "все строки"
                    if (!searchDataAllString)
                    {
                        // перебираем слова для поиска
                        foreach (string word in words)
                        {
                            // 
                            if (worker.CancellationPending)
                            {
                                e.Cancel = true;
                                break;
                            }
                            // записываем файл если он подходит
                            if ((searchType == FType.MATCH && file_text.IndexOf(word) >= 0) ||
                                (searchType == FType.DIFF && file_text.IndexOf(word) == -1))
                            {
                                this.Dispatcher.Invoke(new Action(() => finderAddMatch(word, file)));
                                // больше не проверяем слова если включено "одна строка"
                                if (searchDataOneString)
                                    break;
                            }
                            // обновляем счетчик
                            if (!searchDataOneString)
                            {
                                count++;
                                float p = count * step;
                                if ((int)p > percent)
                                {
                                    percent = (int)p;
                                    worker.ReportProgress(percent);
                                }
                            }

                        }
                        // обновляем счетчик
                        if (searchDataOneString)
                        {
                            count += words.Length;
                            float p = count * step;
                            if ((int)p > percent)
                            {
                                percent = (int)p;
                                worker.ReportProgress(percent);
                            }
                        }
                    }
                    // если выбрано "все строки"
                    else
                    {
                        next = true;
                        // перебираем слова
                        foreach (string word in words)
                        {
                            if (worker.CancellationPending)
                            {
                                e.Cancel = true;
                                break;
                            }
                            // переодим к следующему файлу если не совпала строка
                            if ((searchType == FType.DIFF && file_text.IndexOf(word) >= 0) ||
                                (searchType == FType.MATCH && file_text.IndexOf(word) == -1))
                            {
                                next = false;
                                break;
                            }
                        }
                        // записываем файл если совпали все строка
                        if (next)
                        {
                            this.Dispatcher.Invoke(new Action(() => finderAddMatch(
                                searchType == FType.MATCH ? "Совпадения" : "Не совпадения",
                                file)));
                        }
                        // обновляем счетчик
                        count += words.Length;
                        float p = count * step;
                        if ((int)p > percent)
                        {
                            percent = (int)p;
                            worker.ReportProgress(percent);
                        }
                    }
                    worker.ReportProgress(0, "update_checked");
                }
            }
            catch (Exception error)
            {
                System.Windows.Forms.MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK);
            }
        }

        private void searchAsync_progress(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            switch ((string)e.UserState)
            {
                case "update_count":
                    searchTotal++;
                    progressInfo.Text = string.Format("Файлов найдено: {0}", searchTotal);
                    break;
                case "change_bar":
                    progressBar.IsIndeterminate = false;
                    progressBar.Value = 0;
                    progressInfo.Text = string.Format("Проверено 0 из {0}", searchTotal);
                    break;
                case "update_checked":
                    searchCurrent++;
                    // показываем каждый 11 файл
                    if (searchCurrent % 11 == 0)
                        progressInfo.Text = string.Format("Осталось проверить: {0}, совпадений: {1}", searchTotal - searchCurrent, searchMatch);
                    break;
                default:
                    progressBar.Value = e.ProgressPercentage;
                    break;
            }
        }

        private void searchAsync_complete(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            // скрываем прогресс бар
            progressBar.Visibility = System.Windows.Visibility.Hidden;
            progressBar.Value = 0;

            // востанавливаем управление
            enableControl(true);

            // выводим сообщение если прервалось из за ошибки
            if (e.Error != null)
            {
                System.Windows.Forms.MessageBox.Show(e.Error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // выводим сообщение если вручную остановили поиск
            else if (e.Cancelled)
            {
                System.Windows.Forms.MessageBox.Show("Поиск отменен", "Инфо", MessageBoxButtons.OK);
                //progressInfo.Text = lang.searchStop;
                return;
            }

            // обновляем инфо о поиске
            //progressInfo.Text = string.Format(lang.searchDone, finderCurrent, finderMatch);

            // новая форма для вывода найденых файлов
            ResultWindow win = new ResultWindow();
            win.Result(fileType.Text, searchResult);
            win.Show();
        }

        private void searchStart_Click(object sender, RoutedEventArgs e)
        {
            // проверка данных: папки, слова для поиска и маска файлов
            if (folderList.Items.Count < 1)
            {
                System.Windows.Forms.MessageBox.Show("Выберите папку для поиска", "Инфо", MessageBoxButtons.OK);
                return;
            }
            if (searchStrings.Text.Length < 3)
            {
                System.Windows.Forms.MessageBox.Show("Строка поиска меньше 3 символов", "Инфо", MessageBoxButtons.OK);
                return;
            }
            if (fileType.Text.Length < 1)
            {
                System.Windows.Forms.MessageBox.Show("Укажите тип файла", "Инфо", MessageBoxButtons.OK);
                return;
            }

            // сбрасываем данные и блокируем управление
            clear();
            enableControl(false);


            // отображаем прогресс бар
            progressBar.IsIndeterminate = true;
            progressBar.Value = 0;
            progressBar.Visibility = System.Windows.Visibility.Visible;

            progressInfo.Text = "0";

            // соxраняем данные для последующего использования
            optionsSave();

            // все нужные данные пишем в переменную
            foreach (ListBoxItem item in folderList.Items.Cast<ListBoxItem>())
                searchDataDirectory.Add(item.Content.ToString());

            searchDataMask = fileType.Text;
            searchDataStrings = searchStrings.Text;
            searchDataAllString = (bool)keyAllString.IsChecked;
            searchDataOneString = (bool)keyOneString.IsChecked;
            searchDataRegister = (bool)keyRegister.IsChecked;
            searchDataMatch = (bool)typeMatch.IsChecked;

            // запускаем отдельный поток с поиском
            searchAsync.RunWorkerAsync();
        }

        private void searchCancel_Click(object sender, RoutedEventArgs e)
        {
            searchAsync.CancelAsync();
        }

        private void typeMatch_Checked(object sender, RoutedEventArgs e)
        {
            searchType = FType.MATCH;
        }

        private void typeDiff_Checked(object sender, RoutedEventArgs e)
        {
            searchType = FType.DIFF;
        }

        private void folderAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = dialog.SelectedPath;
                item.ToolTip = item.Content;
                folderList.Items.Add(item);
            }
        }

        private void folderDel_Click(object sender, RoutedEventArgs e)
        {
            var selecter = new List<ListBoxItem>(folderList.SelectedItems.Cast<ListBoxItem>());
            foreach (ListBoxItem item in selecter)
                folderList.Items.Remove(item);
        }
    }
}
