using System.Windows;


namespace StartupFiles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Глобальные переменные и объекты
        ViewModel mainViewModel = new ViewModel(); //Создаем эземпляр viewmodel                
        
        public MainWindow()
        {
            this.DataContext = mainViewModel; //Задаем DataContext нашего VIEW в пространство ViewModel

            InitializeComponent();
        }



        //====== - ЭТОТ ОБРАБОТЧИК СОБЫТИЯ МОЖНО ИСПОЛЬЗОВАТЬ ДЛЯ ОБРАБОТКИ СОБЫТИЯ ДВОЙНОГО КЛИКА ПО ЭЛЕМЕНТУ ListView в CODEBEHIND
        //, если не хочется использовать System.Windows.Interactivity Reference для полной реализации MVVM
        //Для этого в XAML в секции Window.Resources надо указать:     
        //<Style TargetType = "ListViewItem" x:Key="listViewItemStyle">
        //  <EventSetter Event = "MouseDoubleClick" Handler="ListViewItem_MouseDoubleClick"/>
        //</Style>
        // Исключить System.Windows.Interactivity из References и удалить ссылку на пространство имен в XAML

        //  //Обработчик события двойного клика по любому элементу ListView ()
        //private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    //Выполняем команду во ViewModel передавая в нее контекст приведенный к ListViewItem
        //    mainViewModel.OpenFilePathFolderCommand.Execute(((ListViewItem)sender).Content);
        // }
        //============================================================================================================
    }
}

