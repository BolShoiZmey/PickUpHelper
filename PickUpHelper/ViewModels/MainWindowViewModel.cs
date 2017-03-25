using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PickUpHelper.Models;

namespace PickUpHelper.ViewModels
{
    using Catel.MVVM;
    using System.Threading.Tasks;

    public class MainWindowViewModel : ViewModelBase
    {
        public Vk Vk;
        public string Name { get; set; }
        public string Pass { get; set; }
        public string IsOnline { get; set; } = "Off";
        public string Town { get; set; }
        public string SelectedIsOnline { get; set; }
        public string SelectedSex { get; set; }
        public string Massage { get; set; }
        public int AgeFrom { get; set; }
        public int AgeTo { get; set; }
        public int SelectedIsOnlineInt { get; set; }
        public int SelectedSexInt { get; set; }
        public int Count { get; set; }
        public TaskCommand SyncCommand { get; set; }
        public TaskCommand SendCommand { get; set; }
        public Dictionary<string,string> SearchParams { get; set; }
        public ObservableCollection<string> IsOnlineValue { get; set; }
        public ObservableCollection<string> SexValue { get; set; }
      
        public MainWindowViewModel()
        {
            Massage = "Q";
            Name = "schneidergerda@mail.ru";
            Pass = "";
            AgeFrom = 16;
            AgeTo = 28;
            Town = "Сочи";
            SelectedIsOnline = "Online";
            SelectedSex = "Женский";
            Count = 2;
            SearchParams = new Dictionary<string, string>();
            IsOnlineValue = new ObservableCollection<string>();
            SexValue = new ObservableCollection<string>();
            IsOnlineValue.Add("Online");
            IsOnlineValue.Add("Offline");
            SexValue.Add("Женский");
            SexValue.Add("Ya pidor)))");
            SyncCommand = new TaskCommand(StartWorking);
            SendCommand = new TaskCommand(SendMassage);
        }

        private async Task SendMassage()
        {
            SelectedIsOnlineInt = SelectedIsOnline == "Online" ? 1 : 0;
            SelectedSexInt = SelectedSex == "Женский" ? 1 : 2;
            await
                Vk.CreateList(Count.ToString(), Town, SelectedSexInt.ToString(), AgeFrom.ToString(), AgeTo.ToString(),
                    SelectedIsOnlineInt.ToString());
            await Vk.SendMsg(Massage);
        }

        public async Task StartWorking()
        {           
            Vk = new Vk(Name, Pass);
            await Vk.VkLogin();
            if (Vk.ReadyCheck()) IsOnline = "On";     
        }
    }
}
