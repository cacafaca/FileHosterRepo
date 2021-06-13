using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.WebAppBlazor
{
    public class MenuService : IMenuService
    {
        public event EventHandler<EventArgs> OnChanged;

        public void NotifyChanged()
        {
            OnChanged?.Invoke(this, new EventArgs());
        }
    }
}
