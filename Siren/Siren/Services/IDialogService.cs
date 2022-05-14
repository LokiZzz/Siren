using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Siren.Services
{
    interface IDialogService
    {
        Task<bool> AskConfirmation(string title, string message);
    }
}
