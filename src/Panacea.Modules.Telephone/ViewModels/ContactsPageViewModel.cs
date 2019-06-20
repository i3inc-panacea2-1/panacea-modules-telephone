using Panacea.Controls;
using Panacea.Core;
using Panacea.Modularity.UiManager;
using Panacea.Modules.Telephone.Models;
using Panacea.Modules.Telephone.Views;
using Panacea.Multilinguality;
using Panacea.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Panacea.Modules.Telephone.ViewModels
{
    [View(typeof(ContactsPage))]
    class ContactsPageViewModel:ViewModelBase
    {
        public ObservableCollection<UserSpeedDial> Contacts { get; }

        private readonly TaskCompletionSource<bool> _source;

        public ContactsPageViewModel(IEnumerable<UserSpeedDial> dials, TaskCompletionSource<bool> source, PanaceaServices core)
        {
            dials = dials ?? new ObservableCollection<UserSpeedDial>();
            Contacts = new ObservableCollection<UserSpeedDial>(dials.Select(d => d.Clone()));
            _source = source;
            if (Contacts.Count == 0)
            {
                Contacts.Add(new UserSpeedDial());
            }
            CancelCommand = new RelayCommand(args =>
            {
                if(core.TryGetUiManager(out IUiManager ui))
                {
                    ui.GoBack();
                }
            });
            RemoveCommand = new RelayCommand(args =>
            {
                var dial = (UserSpeedDial)args;
                Contacts.Remove(dial);

            });
            MoveUpCommand = new RelayCommand(args =>
            {
                var dial = (UserSpeedDial)args;
                int i = Contacts.IndexOf(dial);
                if (i > 0)
                {
                    Contacts.RemoveAt(i);
                    Contacts.Insert(i - 1, dial);
                }

            },
            args=>
            {
                var dial = (UserSpeedDial)args;
                return Contacts.IndexOf(dial) > 0;
            });
            MoveDownCommand = new RelayCommand(args =>
            {
                var dial = (UserSpeedDial)args;
                int i = Contacts.IndexOf(dial);
                if (i < Contacts.Count - 1)
                {
                    Contacts.RemoveAt(i);
                    Contacts.Insert(i + 1, dial);
                }
            },
            args=>
            {
                var dial = (UserSpeedDial)args;
                return Contacts.IndexOf(dial) < Contacts.Count - 1;
            });
            SaveCommand = new AsyncCommand(async args =>
            {
                string id = core.UserService.User.Id;
               
                foreach(var c in Contacts.ToList().Where(d => string.IsNullOrEmpty(d.Label) && string.IsNullOrEmpty(d.Number)))
                {
                    Contacts.Remove(c);
                }
                var regex = new Regex(@"\(?\d{3}\)?-? *\d{3}-? *-?\d{4}");
                foreach(var d in Contacts)
                {
                    if (!regex.IsMatch(d.Number))
                    {
                        return;
                    }
                }
                if (core.TryGetUiManager(out IUiManager ui))
                {
                    try
                    {
                        await core.HttpClient.SetCookieAsync("Telephone", Contacts);
                        source.TrySetResult(true);
                    }
                    catch
                    {
                        ui.Toast(new Translator("Telephone").Translate("Save failed. Try again later."));
                    }
                }
            });

            AddContactCommand = new RelayCommand(args =>
            {
                Contacts.Add(new UserSpeedDial());
            });
        }

        public override void Deactivate()
        {
            _source.TrySetResult(false);
        }

        public ICommand MoveUpCommand { get; }

        public ICommand MoveDownCommand { get; }

        public ICommand RemoveCommand { get; }

        public AsyncCommand SaveCommand { get; }

        public ICommand CancelCommand { get; }
        public ICommand AddContactCommand { get; }
    }
}
