using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;
using IWshRuntimeLibrary;
using TaskScheduler;
using System.Windows.Media.Imaging;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;

namespace StartupFiles
{
    class DataProvider
    {

        //Получаем данные из реестра - возвращает список (List) файлов(Prgrm)
        public async Task<List<Prgrm>> GetFilesListFromRegistry()
        {
            return await Task.Run(() =>
            {
                List<Prgrm> Files = new List<Prgrm>(); //Создаем список программ которые будем возвращать

                string RunPath = @"Software\Microsoft\Windows\CurrentVersion\Run"; //Путь до ветки с автозагрузкой

                string[] CU_Run_Names = Registry.CurrentUser.OpenSubKey(RunPath, false).GetValueNames(); //Массив элеметов в Run в Current_User
                string[] LM_Run_Names = Registry.LocalMachine.OpenSubKey(RunPath, false).GetValueNames(); //Массив элеметов в Run в Local_Machine

                string[][] RegistryRoots = new string[2][]; //Двухмерный массив содержащий ветки реестра, на втором уровне имена элементов соответвующей ветки
                RegistryRoots[0] = CU_Run_Names;
                RegistryRoots[1] = LM_Run_Names;


                for (int s = 0; s < RegistryRoots.Count(); s++) //Перебираем ветки реестра HKCU и HKLM
                {
                    for (int i = 0; i < RegistryRoots[s].Count(); i++) //Перебираем имена элементов текущей ветке
                    {

                        string prefix = @"HKEY_CURRENT_USER\"; //Переключаем префикс в зависимости от того какая в данный момент обрабатывается ветка реестра
                        if (s == 1) { prefix = @"HKEY_LOCAL_MACHINE\"; }

                        string KeyName = RegistryRoots[s][i]; //Имя файла KeyName
                        string KeyValue = (string)Registry.GetValue(prefix + RunPath, KeyName, null); // Получаем значение текущего элемента и отрезаем кавычки если они есть                                        


                        string FileName = "";
                        string FilePath = "";
                        string FileParams = "";
                        BitmapFrame Icon = null;

                        if (KeyValue.Contains('"')) //Если путь к файлу содержит ковычки то возможно содержит и аргументы
                        {
                            FilePath = KeyValue.Substring(KeyValue.IndexOf("\"") + 1); //Отрезаем первую кавычку
                            FileParams = FilePath.Substring(FilePath.IndexOf("\"") + 1).Trim(); // Находим параметры, которые начинаются после второй кавычки и отрезаем проблеы
                            FilePath = FilePath.Substring(0, FilePath.IndexOf("\"")).Trim(); //Отрезаем последнюю кавычку и отрезаем пробелы

                                if (System.IO.File.Exists(FilePath))
                                {
                                    FileInfo fileinfo = new FileInfo(FilePath); //Если файл существет извлечем имя
                                    FileName = fileinfo.Name;
                                }
                        }
                        else //Если кавычек нет - то аргументы не содержаться
                        {
                                FilePath = KeyValue;
                                if (System.IO.File.Exists(FilePath)) //Если файл существет извлечем имя
                            {
                                    FileInfo fileinfo = new FileInfo(FilePath);
                                    FileName = fileinfo.Name;
                                    FileParams = ""; //А аргументы командной строки оставляем пустыми, так как если нет ковычек не может быть аргументов
                                }
                        }

                            //Задаем св-во StartupType для нового экземпляра Prgrm
                            string StartupType = "Registry - Current User";
                            if (s == 1) { StartupType = "Registry - Local Machine"; } //Если индекс перебираемого элемента 1 - значит это HKLM



                            if (System.IO.File.Exists(FilePath)) //Проверим файл на существование по этому пути
                            {
                                //Получаем Иконку
                                Icon = GetIcon(FilePath);
                                //Cоздаем экземпляр prgrm для добавления
                      
                            }

                            Prgrm prgrm = new Prgrm() { Icon = Icon, FileName = FileName, FilePath = FilePath, CmdlineParams = FileParams, StartupType = StartupType };

                            //Заполняем информацию о сертификатах и компании из сертификата либо из атрибутов файла при отсутвии сертификата
                            prgrm = FillCertData(prgrm);


                            //Формируем список екземпляров Prgrm  для return, заполняя его полученными значениями
                            Files.Add(prgrm);

                        }

                }
                return Files; //Возвращаем список файлов
            });
        }

        //Получаем данные из планировщика - возвращает список (List) файлов(Prgrm)
        public async Task<List<Prgrm>> GetFilesListFromSheduler()
        {
            return await Task.Run(() =>
            {
                    List<Prgrm> Files = new List<Prgrm>(); //Создаем список программ которые будем возвращать


                TaskScheduler.TaskScheduler ts = new TaskScheduler.TaskScheduler(); //Создаем экземпляр TaskScheduler

                ts.Connect(); //Подключаемся к localhost без параметров
                ITaskFolder folder = ts.GetFolder(@"\"); //Задаем подпапку планировщика
                IRegisteredTaskCollection Tasks = folder.GetTasks(flags: 0); //Получаем все задания из этой подпапки
                foreach (IRegisteredTask task in Tasks) //Перебираем эти задания
                {

                    ITriggerCollection tasktriggers = task.Definition.Triggers; //Коллекция триггеров текущего задания

                    bool HaveDesiredTriggerforCurrentTaskActions = false; //Флаг для помечания, что в задании есть хотябы один триггер, который запускает это задание при запуске системы

                    foreach (ITrigger trigger in tasktriggers) //Перебираем триггеры
                    {
                        //Если нашелся тригер являющийся триггером, срабатывающим при запуске системы, то помечаем флаг как true
                        if (trigger.Type == _TASK_TRIGGER_TYPE2.TASK_TRIGGER_BOOT) 
                        {
                            HaveDesiredTriggerforCurrentTaskActions = true;

                        }
                    }

                    if (HaveDesiredTriggerforCurrentTaskActions == true) //Если существует хоть один триггер, запускающий данное задание при запуске систмемы
                    {
                        IActionCollection TaskActions = task.Definition.Actions; //Находим коллекцию действий (Actions) текщего задания

                        foreach (IAction TaskAction in TaskActions) //Перебираем коллекцию Actions
                        {
                            if (TaskAction.Type == _TASK_ACTION_TYPE.TASK_ACTION_EXEC) //Если текущий Action является Action ом для запуска exe приложения
                            {
                                IExecAction execAction = (IExecAction)TaskAction; //Приводим наш TasAction к типу IExecAction, который содержит св-ва которые мы ищем

                                string FilePath = execAction.Path; //Путь к файлу
                                string FileParams = execAction.Arguments; //Аргументы командной строки

                                if (FilePath.Contains('"')) //Если путь к файлу содержит ковычки
                                {
                                    FilePath = FilePath.Substring(FilePath.IndexOf("\"") + 1); //Отрезаем первую кавычку                                
                                    FilePath = FilePath.Substring(0, FilePath.IndexOf("\"")).Trim(); //Отрезаем последнюю кавычку и отрезаем пробелы
                                }
                                string StartupType = "Task Scheduler"; //для св-ва тип запуска экземпляра Prgrm
                                BitmapFrame Icon = null; //Для св-ва Icon кземпляра Prgrm
                                string FileName = "";//Для св-ва FileName кземпляра Prgrm

                                if (System.IO.File.Exists(FilePath)) //Если файл по этому пути существует
                                {
                                    FileInfo fileinfo = new FileInfo(FilePath); //Находим имя
                                    FileName = fileinfo.Name;                               
                                
                                    //Получаем Иконку                                
                                    Icon = GetIcon(FilePath);                                                            
                                }
                                //Cоздаем экземпляр prgrm для добавления
                                Prgrm prgrm = new Prgrm() { Icon = Icon, FileName = FileName, FilePath = FilePath, CmdlineParams = FileParams, StartupType = StartupType };
                                    //Заполняем информацию о сертификатах и компании из сертификата либо из атрибутов файла при отсутвии сертификата
                                    prgrm = FillCertData(prgrm);

                                //Формируем список екземпляров Prgrm  для return, заполняя его полученными значениями
                                Files.Add(prgrm);
                            }

                        }
                    }

                }



           
                return Files;//Возвращаем список файлов
            });

        }



        //Получаем данные из StartMenu - возвращает список (List) файлов(Prgrm)
        public async Task<List<Prgrm>> GetFilesListFromStartMenu()
        {
            return await Task.Run(() =>
            {

                List<Prgrm> Files = new List<Prgrm>(); //Создаем список программ которые будем возвращать

                string[][] AllUsersAndCurrentUserPATHs = new string[2][]; //Создаем массив, который будет содержать пути к двум папкам автозапуска (для CurrentUSer и AllUsers), а на втором уровне пути к файлам этих веток автозапуска
                
                AllUsersAndCurrentUserPATHs[0] = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Startup)); //Current User
                AllUsersAndCurrentUserPATHs[1] = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup)); //All Users

                for (int i = 0; i < AllUsersAndCurrentUserPATHs.Count(); i++) //Перебираем эти пути к этим веткам
                {
                    foreach (string filepath in AllUsersAndCurrentUserPATHs[i]) //Перебираем пути к файлам текущей ветки
                    {
                        FileInfo fileinfo = new FileInfo(filepath); 
                        if (fileinfo.Extension == ".lnk") //Если расширение текущего файла явл '.lnk'
                        {

                            WshShell shell = new WshShell();
                            IWshShortcut link = (IWshShortcut)shell.CreateShortcut(filepath); //Создаем эземпляр ярлыка приводя его к типу IWshShortcut

                            //Задаем переменные для св-в Prgrm
                            string FilePath = link.TargetPath;
                            string FileName = fileinfo.Name;
                            string FileParams = link.Arguments;
                            BitmapFrame Icon = GetIcon(FilePath);                   //Получаем Иконку                                           
                            string StartupType = "Start Menu - Current User";
                            if (i == 1) { StartupType = "Start Menu - All Users"; } //Если обрабатываемая ветка вторая - то значит это тип AllUSers

                
                            //Cоздаем экземпляр prgrm для добавления
                            Prgrm prgrm = new Prgrm() { Icon = Icon, FileName = FileName, FilePath = FilePath, CmdlineParams = FileParams, StartupType = StartupType };

                            //Заполняем информацию о сертификатах и компании из сертификата либо из атрибутов файла при отсутвии сертификата
                            prgrm = FillCertData(prgrm);
                            //Формируем список екземпляров Prgrm  для return, заполняя его полученными значениями
                            Files.Add(prgrm);
                        }
                    }
                }
           

            return Files;//Возвращаем список файлов
            });





        }



        //Получение иконки из файла
        private BitmapFrame GetIcon(string FilePath)
        {
            try
            {
                //Получаем иконку
                using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(FilePath)) //Извлекаем иконку из файла
                {
                    using (System.Drawing.Bitmap bmp = icon.ToBitmap()) //Приводим к Bitmap
                    {
                        var stream = new MemoryStream();  //Создаем поток
                        bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png); //Cохраняем bmp в поток
                        return BitmapFrame.Create(stream); //Возвращаем BitFrame, созданный из потока
                    }

                }
            }
            catch
            {
                return null; //Если не получилось, возвращаем null
            }
        }



        //Получение данных о сертификате и названии компании (из сертификата или файла (при отсутвии сертификата))        
        private Prgrm FillCertData(Prgrm InputPrgrm)//Принимаем экземпляр Prgrm, заполняем необходимые св-ва в нем, после чего возвращаем его же
        {
            if (!System.IO.File.Exists(InputPrgrm.FilePath)) //Если файл не существуем, помечаем всё false и возвращаем
            {
                InputPrgrm.IsSignaturePresent = false;
                InputPrgrm.IsSignatureCorrect = false;                
                return InputPrgrm;
            }
            try
            {
                X509Certificate cert1 = X509Certificate.CreateFromSignedFile(InputPrgrm.FilePath);     //Создаем экземпляр сертификата
                InputPrgrm.IsSignaturePresent = true; //Если мы не получили исключение, значит сертификате есть. Помечаем соотв св-во.
                InputPrgrm.Company = cert1.Subject.Substring(3).Substring(0, cert1.Subject.IndexOf(",") - 3); //Извлекаем из сертификата название компании



                InputPrgrm.IsSignatureCorrect = false; //Заранее помечаем св-во корректность сертификата как false
                if (DateTime.Parse(cert1.GetExpirationDateString()) >= DateTime.Now) //Проверяем что сертификат не устарел
                {                    
                    //Проверяем валидность
                    var cert2 = new X509Certificate2(cert1.Handle);

                    if (cert2.Verify() == true)
                    {//Подпись верифицируется
                        InputPrgrm.IsSignatureCorrect = true;
                    }
                }



            }
            catch //Если пришло исключение значит цифровая подпись отсутвует
            { 
                //Помечаем св-ва как false
                InputPrgrm.IsSignaturePresent = false;
                InputPrgrm.IsSignatureCorrect = false;

                //Извлекаем название компании из ресурсов самомго файла
                FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(InputPrgrm.FilePath);
                InputPrgrm.Company = myFileVersionInfo.CompanyName;
            }

            return InputPrgrm; //Возвращаем экземпляр Prgrm
        }


    }
}
