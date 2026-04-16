using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfApp1
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        // ObservableCollection - магия WPF. Она сама сообщает ListBox'у, когда список изменился.
        public ObservableCollection<Student> Students { get; set; }

        private Student _selectedStudent;
        public Student SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                if (_selectedStudent != value)
                {
                    _selectedStudent = value;
                    OnPropertyChanged(nameof(SelectedStudent));
                    // Команда удаления должна перепроверить, можно ли сейчас удалять
                    ((RelayCommand)DeleteCommand).CanExecute(null);
                }
            }
        }

        // Свойства для ввода нового студента
        public string NewFirstName { get; set; }
        public string NewLastName { get; set; }
        public int NewAge { get; set; }
        public string NewGroup { get; set; }

        // Команды (реакция на нажатия кнопок)
        public ICommand AddCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }

        public MainViewModel()
        {
            Students = new ObservableCollection<Student>();

            // Тестовые данные, чтобы при запуске не было пусто
            Students.Add(new Student { LastName = "Иванов", FirstName = "Иван", Age = 20, Group = "ИТ-101" });
            Students.Add(new Student { LastName = "Петрова", FirstName = "Анна", Age = 19, Group = "ПМ-102" });

            // Инициализация команд
            AddCommand = new RelayCommand(AddStudent);
            DeleteCommand = new RelayCommand(DeleteStudent, CanDeleteStudent);
        }

        private void AddStudent(object parameter)
        {
            if (!string.IsNullOrWhiteSpace(NewFirstName) && !string.IsNullOrWhiteSpace(NewLastName))
            {
                var student = new Student
                {
                    FirstName = NewFirstName,
                    LastName = NewLastName,
                    Age = NewAge,
                    Group = NewGroup
                };
                Students.Add(student);

                // Очистка полей ввода (просто сбрасываем свойства)
                NewFirstName = string.Empty;
                NewLastName = string.Empty;
                NewAge = 0;
                NewGroup = string.Empty;

                OnPropertyChanged(nameof(NewFirstName));
                OnPropertyChanged(nameof(NewLastName));
                OnPropertyChanged(nameof(NewAge));
                OnPropertyChanged(nameof(NewGroup));
            }
        }

        private bool CanDeleteStudent(object parameter)
        {
            return SelectedStudent != null;
        }

        private void DeleteStudent(object parameter)
        {
            if (SelectedStudent != null)
            {
                Students.Remove(SelectedStudent);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
