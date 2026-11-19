using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace KartProsjekt;

public partial class MainViewModel: ObservableObject
{
    [ObservablePropertyAttribute]
    public partial int Counter { get; set; } = 5;

    [RelayCommand]
    public void IncrementCounter()
    {
        Counter++;
    }
    [RelayCommand]
    public void DecrementCounter()
    {
        if (Counter > 0)
        { Counter--; }
        
    }
}
