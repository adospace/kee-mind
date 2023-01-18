using KeeMind.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeeMind.Pages
{
    class StartupPage : Component
    {

        public override VisualNode Render()
        {
            return new ContentPage
            {
                new ActivityIndicator()
                    .VCenter()
                    .HCenter()
            };
        }
    }
}
