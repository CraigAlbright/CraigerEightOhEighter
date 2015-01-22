using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CraigerEightOhEighter.ViewModels
{
    public interface IMainUiViewModel
    {
        void SetMainTempo(int main);
        void SetFineTempo(int fine);
    }
}
