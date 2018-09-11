using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.ComponentModel;
using Microsoft.Win32;
using System.Security.Cryptography.X509Certificates;

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




        //Команда открыть то место где прописана автозагрузка выбранной программы
        private RelayCommand openfileautorunplace;
        public RelayCommand OpenFileAutorunPlace
        {
            get
            {
                return openfileautorunplace ??
                  (openfileautorunplace = new RelayCommand(obj =>
                  {
                      Prgrm CurrnetPrgrm = (Prgrm)obj;//Приводим переданный объект к типу Prgrm                      


                      ProcessStartInfo startInfo = new ProcessStartInfo("Explorer"); //Создаем экземпляр процесса
                      string registryLastKey = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Applets\Regedit";
                      string registryLocation = "";

                      switch (CurrnetPrgrm.StartupType)
                      {
                          case "Registry - Current User":

                             
                              registryLocation = @"Computer\HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                              
                              try
                              {
                                  Registry.SetValue(registryLastKey, "LastKey", registryLocation); // Set LastKey value that regedit will go directly to
                                  Process.Start("regedit.exe");
                              }
                              catch{}
                              

                              break;

                          case "Registry - Local Machine":

                              registryLocation = @"Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run";                              
                              try
                              {
                                  Registry.SetValue(registryLastKey, "LastKey", registryLocation); // Set LastKey value that regedit will go directly to
                                  Process.Start("regedit.exe");
                              }
                              catch { }
                              break;

                          case "Task Scheduler":

                              Process.Start("taskschd.msc");

                              break;

                          case "Start Menu - Current User":                              

                              //Открываем папку текущего файла в Explorer и выделяем нужный файл                              
                              startInfo.UseShellExecute = false; //Не использовать оболочку операционной системы для запуска процесса                                                            
                              startInfo.Arguments = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Startup);  //Аргументы
                              Process.Start(startInfo); //Запускаем процесс

                              break;
                          case "Start Menu -All Users":
                              //Открываем папку текущего файла в Explorer и выделяем нужный файл                              
                              startInfo.UseShellExecute = false; //Не использовать оболочку операционной системы для запуска процесса                                                            
                              startInfo.Arguments = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonStartup);  //Аргументы
                              Process.Start(startInfo); //Запускаем процесс
                              break;
                          
                      }
                      
                     


                  }));
            }
        }

        //Команда открыть Информацию о сертификате выбранной программы
        private RelayCommand openfilecertinfo;
        public RelayCommand OpenFileCertInfo
        {
            get
            {
                return openfilecertinfo ??
                  (openfilecertinfo = new RelayCommand(obj =>
                  {
                      Prgrm CurrnetPrgrm = (Prgrm)obj;//Приводим переданный объект к типу Prgrm                      

                      if (!CurrnetPrgrm.IsSignaturePresent)
                      {
                          System.Windows.MessageBox.Show("No Signature");
                          return;
                      }

                      X509Certificate cert1 = X509Certificate.CreateFromSignedFile(CurrnetPrgrm.FilePath);     //Создаем экземпляр сертификата
                                                                                                                                                                                                           //Проверяем валидность
                      var cert2 = new X509Certificate2(cert1.Handle);
                      X509Certificate2UI.DisplayCertificate(cert2); //Показываем сертификат

                   

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
