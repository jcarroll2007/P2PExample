using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.ObjectModel;
using P2PClient.Models;


namespace P2PClient
{
    class ClientViewModel : ViewModelBase
    {
        public ObservableCollection<Client> clients;
        private string messages;

        public ClientViewModel()
        {
            clients = new ObservableCollection<Client>();
            Messages = "Hello";
        }

        #region Properties
        public string Messages
        {
            get
            {
                return messages;
            }
            set
            {
                if (messages != value)
                {
                    messages = value;
                    RaisePropertyChanged("Messages");
                }
            }
        }
        #endregion Properties

        #region Methods
        #endregion Methods

        #region Commands
        #endregion Commands

    }
}
