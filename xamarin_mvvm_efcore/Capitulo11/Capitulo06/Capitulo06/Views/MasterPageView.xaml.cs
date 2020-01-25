﻿using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Capitulo06.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MasterPageView : ContentPage
	{
        public Models.MenuItem[] OpcoesMenu { get; set; }
        public ListView ListView { get; set; }

        public MasterPageView()
        {
            Icon = "menu.png";

            InitializeComponent();
            OpcoesMenu = new[]
            {
                    new Models.MenuItem { Id = 0, Title = "Clientes", TargetType = typeof(Clientes.ListagemView), IconSource="tab_clientes.png"},
                    new Models.MenuItem { Id = 1, Title = "Serviços", TargetType = typeof(Servicos.ListagemView), IconSource="tab_servicos.png"},
                    new Models.MenuItem { Id = 2, Title = "Atendimentos", TargetType = typeof(Atendimentos.ListagemView), IconSource="tab_atendimento.png"},
                    new Models.MenuItem { Id = 3, Title = "Peças", TargetType = typeof(Pecas.ListagemView), IconSource="tab_pecas.png"}
            };
            ListView = itensMenuListView;
            BindingContext = this;
        }
    }
}