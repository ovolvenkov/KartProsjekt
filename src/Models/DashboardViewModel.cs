using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace KartProsjoht.Models;

public partial class DashboardViewModel : ObservableObject
{
    private int _count;

    public int Count
    {
        get => _count;
        set => SetProperty(ref _count, value);
    }

    [RelayCommand]
    public void IncrementCount()
    {
        Count++;
    }
}
