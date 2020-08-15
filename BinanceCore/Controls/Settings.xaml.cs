using System.ComponentModel;
using System.Windows.Controls;

namespace BinanceCore.Controls
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : UserControl, INotifyPropertyChanged
    {
        public string Key
        {
            get => binkey.Text;
            set => binkey.Text = value;
        }
        public string Secret
        {
            get => binsecret.Text;
            set => binsecret.Text = value;
        }
        public string Token
        {
            get => tgmtoken.Text;
            set => tgmtoken.Text = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        long _master = 0;

        public long Master
        {
            get => _master;
            set
            {
                _master = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Master")); 
            }
        }

        public Settings()
        {
            InitializeComponent();
            TopLevelController.DataContext = this;
        }
    }
}
