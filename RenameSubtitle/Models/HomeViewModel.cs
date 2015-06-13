using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RenameSubtitle.Models
{
    public class HomeViewModel
    {
        public string Message { get; set; }

        public HomeViewModel(string message)
        {
            this.Message = message;
        }

        public HomeViewModel()
        {
        }
    }
}