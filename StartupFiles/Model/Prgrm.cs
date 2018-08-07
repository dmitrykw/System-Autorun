using System.ComponentModel;
using System.Windows.Media.Imaging;
namespace StartupFiles
{
    class Prgrm : INotifyPropertyChanged //Реализуем интерфейс INotifyPropertyChanged для того чтобы уведомлять UI элементы подписанные binding ом на ObservableCollection (во ViewModel)
    {


        private BitmapFrame icon;
        private string filename;
        private string cmdlineparams;
        private string filepath;
        private string startuptype;
        private bool issignaturepresent;
        private bool issignaturecorrect;
        private string company;


        public BitmapFrame Icon
        {
            get { return icon; }
            set
            {
                icon = value;
                OnPropertyChanged("Icon");
            }
        }


        public string FileName
        {
            get { return filename; }
            set
            {
                filename = value;
                OnPropertyChanged("FileName");
            }
        }

        public string CmdlineParams
        {
            get { return cmdlineparams; }
            set
            {
                cmdlineparams = value;
                OnPropertyChanged("CmdlineParams");
            }
        }

        public string FilePath
        {
            get { return filepath; }
            set
            {
                filepath = value;
                OnPropertyChanged("FilePath");
            }
        }

        public string StartupType
        {
            get { return startuptype; }
            set
            {
                startuptype = value;
                OnPropertyChanged("StartupType");
            }
        }

        public bool IsSignaturePresent
        {
            get { return issignaturepresent; }
            set
            {
                issignaturepresent = value;
                OnPropertyChanged("IsSignaturePresent");
            }
        }

        public bool IsSignatureCorrect
        {
            get { return issignaturecorrect; }
            set
            {
                issignaturecorrect = value;
                OnPropertyChanged("IsSignatureCorrect");
            }
        }

        public string Company
        {
            get { return company; }
            set
            {
                company = value;
                OnPropertyChanged("Company");
            }
        }


        // реализация интерфейса INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
