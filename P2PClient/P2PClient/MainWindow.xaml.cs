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
using System.Windows.Threading;
using P2PClient.Models;

namespace P2PClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static object _syncLock = new object();
        ClientViewModel viewModel;
        public MainWindow()
        {
            InitializeComponent();

            viewModel = new ClientViewModel();

            BindingOperations.EnableCollectionSynchronization(viewModel.Clients, _syncLock);

            viewModel.NewConnection += NewMessageHanlder;
            this.DataContext = viewModel;
        }

        private void NewMessageHanlder(object sender, Client client)
        {
            AddTab(client.ToString());

            

            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action) delegate
                    {
                        //Switch to that tab and dispaly the conversation
                        for (int i = 0; i < ConversationTabControl.Items.Count; i++)
                        {
                            if (((TabItem) ConversationTabControl.Items[i]).Header == client.ToString())
                            {
                                ConversationTabControl.SelectedIndex = i;
                                ((TabItem) ConversationTabControl.Items[i]).Content = client.Conversation;
                            }
                        }
                    });
            }
            else
            {
                //Switch to that tab and dispaly the conversation
                for (int i = 0; i < ConversationTabControl.Items.Count; i++)
                {
                    if (((TabItem)ConversationTabControl.Items[i]).Header == client.ToString())
                    {
                        ConversationTabControl.SelectedIndex = i;
                        ((TabItem)ConversationTabControl.Items[i]).Content = client.Conversation;
                    }
                }
            }

            viewModel.SelectedClient = client;
        }


        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Add the tab if it doesnt exist
            AddTab((sender as ListBox).SelectedItem.ToString());

            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (Action)delegate
                {
                    for (int i = 0; i < ConversationTabControl.Items.Count; i++)
                        if (((TabItem)ConversationTabControl.Items[i]).Header == (sender as ListBox).SelectedItem.ToString())
                            ConversationTabControl.SelectedIndex = i;
                });
            }
            else
            {
                for (int i = 0; i < ConversationTabControl.Items.Count; i++)
                    if (((TabItem)ConversationTabControl.Items[i]).Header == (sender as ListBox).SelectedItem.ToString())
                        ConversationTabControl.SelectedIndex = i;
            }

            
        }

        private void AddTab(string title)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (Action)delegate
                {
                    if (!TabExists(title))
                    {
                        TabItem newTab = new TabItem();
                        newTab.Header = title;
                        ConversationTabControl.Items.Add(newTab);
                    }
                });
            }
            else
            {
                if (!TabExists(title))
                {
                    TabItem newTab = new TabItem();
                    newTab.Header = title;
                    ConversationTabControl.Items.Add(newTab);
                }
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


        private void ConversationTabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (Client client in viewModel.Clients)
            {
                if (client.UserName ==
                    ((TabItem) ConversationTabControl.Items[ConversationTabControl.SelectedIndex]).Header)
                    viewModel.SelectedClient = client;
            }
        }
    }
}
