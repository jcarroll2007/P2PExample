using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace P2PClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ClientViewModel viewModel;
        public MainWindow()
        {
            InitializeComponent();
            viewModel = new ClientViewModel();
            this.DataContext = viewModel;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Add the tab if it doesnt exist
            AddTab((sender as ListBox).SelectedItem.ToString());

            //Switch to that tab
            for(int i = 0; i < ConversationTabControl.Items.Count; i++)
                if (((TabItem) ConversationTabControl.Items[i]).Header == (sender as ListBox).SelectedItem.ToString())
                    ConversationTabControl.SelectedIndex = i;
        }

        private void AddTab(string title)
        {
            if (!TabExists(title))
            {
                TabItem newTab = new TabItem();
                newTab.Header = title;
                ConversationTabControl.Items.Add(newTab);
            }
        }

        private bool TabExists(string title)
        {
            foreach (TabItem tab in ConversationTabControl.Items)
            {
                if (tab.Header.ToString() == title)
                    return true;
            }

            return false;
        }


    }
}
