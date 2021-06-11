using System;
using System.Collections.Generic;

public interface IMenuService
{
    //List<MenuItem> AdditionalMenuItems { get; set; }
    event EventHandler<EventArgs> OnChanged;
    void NotifyChanged();
}