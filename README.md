# System-Autorun
Showing AutoRuning Files from Registry, StartMenu, Sheduler  C#.Net WPF (MVVM) async/await

Приложение C#.Net WPF (MVVM) выводит в элемент ListView информацию о запускающихся при старте системы программах.

Информация получается из Registry, Start Menu, Scheduler

Выводимая информация
a. Иконка файла
b. Имя исполняемого файла.
c. Параметры командной строки для запуска файла.
d. Путь к файлу.
e. Тип автозапуска (Registry, Start Menu, Scheduler)
f. Флаг наличия цифровой подписи.
g. Флаг корректности цифровой подписи.
h. Имя компании производителя (из цифровой подписи если она присутствует, иначе из ресурсной информации).

При двойном клике по элементу списка открывается папку проводника, в которой размещен исполняемый файл.
Чтение данных выполняется в отдельном потоке. (async/await)
