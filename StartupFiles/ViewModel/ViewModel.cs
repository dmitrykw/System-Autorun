using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.ComponentModel;

namespace StartupFiles
{
     class ViewModel : INotifyPropertyChanged //Реализуем интерфейс INotifyPropertyChanged для св-ва SelectedPrgrm
    {
        public ObservableCollection<Prgrm> ProgramsCollection { get; set; } //Коллекция наших программ binding на ListView
       

        public ViewModel()
        {
            //Инициализируем список программ
            ProgramsCollection = new ObservableCollection<Prgrm>();
            FillCollection(); //Ассинхронно заполняем коллекцию
        }


        //Заполнение коллекции
        private async void FillCollection()
            {
            DataProvider dataProvider = new DataProvider();

            List<Prgrm> FilesList = new List<Prgrm>();
            FilesList.AddRange(await dataProvider.GetFilesListFromRegistry());
            FilesList.AddRange(await dataProvider.GetFilesListFromStartMenu());
            FilesList.AddRange(await dataProvider.GetFilesListFromSheduler());

            foreach (Prgrm prgrm in FilesList)
            {            
                //Записываем в коллекцию новый элемент
                ProgramsCollection.Add(prgrm);
            }

        }



        //Выбор текущего Prgrm
        private Prgrm selectedPrgrm;
        public Prgrm SelectedPrgrm
        {
            get { return selectedPrgrm; }
            set
            {
                selectedPrgrm = value;
                OnPropertyChanged("SelectedPrgrm");
            }
        }



        //Команда открыть папку выбранного Prgrm
        private RelayCommand openfilepathfoldercommand;
        public RelayCommand OpenFilePathFolderCommand
        {
            get
            {
                return openfilepathfoldercommand ??
                  (openfilepathfoldercommand = new RelayCommand(obj =>
                  {                      
                      Prgrm CurrnetPrgrm = (Prgrm)obj;//Приводим переданный объект к типу Prgrm                      

                      //Открываем папку текущего файла в Explorer и выделяем нужный файл
                      ProcessStartInfo  startInfo = new ProcessStartInfo("Explorer"); //Создаем экземпляр процесса
                      startInfo.UseShellExecute = false; //Не использовать оболочку операционной системы для запуска процесса
                      startInfo.Arguments = @"/select," + CurrnetPrgrm.FilePath;  //Аргументы
                      Process.Start(startInfo); //Запускаем процесс
                    

                  }));
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
