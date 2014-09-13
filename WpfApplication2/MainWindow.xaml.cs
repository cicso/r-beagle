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

namespace WpfApplication2
{
    enum FType { MATCH, DIFF };
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.ComponentModel.BackgroundWorker searchAsync;
        private string optionsFile;
        private Dictionary<string, List<string>> finderResult;
        private FType searchType;
        private int searchTotal = 0;
        private int searchCurrent = 0;
        private int searchMatch = 0;

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
            finderResult = new Dictionary<string, List<string>>();
            searchCurrent = searchTotal = 0;
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

        private void searchAsync_work(object sender, System.ComponentModel.DoWorkEventArgs e)
        {

        }

        private void searchAsync_progress(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
    
        }

        private void searchAsync_complete(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {

        }

        private void searchStart_Click(object sender, RoutedEventArgs e)
        {
            enableControl(false);
            // соxраняем данные для последующего использования
            optionsSave();
        }

        private void searchCancel_Click(object sender, RoutedEventArgs e)
        {
            enableControl(true);
        }
    }
}
