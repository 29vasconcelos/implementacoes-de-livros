﻿using Capitulo05.Services;
using CasaDoCodigo.DAL;
using CasaDoCodigo.DataAccess;
using CasaDoCodigo.DataAccess.Interfaces;
using CasaDoCodigo.Models;
using Interfaces.Fotos;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Capitulo05.ViewModels.Pecas
{
    public class CRUDViewModel : BaseViewModel
    {
        private IDAL<Peca> pecasDAL;
        public Peca Peca { get; set; }
        private PecaService service;
        public ICommand GravarCommand { get; set; }
        public ICommand AlbumCommand { get; set; }

        public CRUDViewModel(Peca peca)
        {
            pecasDAL = new PecaDAL(DependencyService.Get<IDBPath>().GetDbPath());
            this.Peca = peca;
            RegistrarCommands();
            service = new PecaService();
        }

        public string Nome
        {
            get { return this.Peca.Nome; }
            set
            {
                this.Peca.Nome = value;
                ((Command)GravarCommand).ChangeCanExecute();
                OnPropertyChanged();
            }
        }

        private string valor;
        public string Valor
        {
            get { return valor; }
            set
            {
                this.valor = value;
                this.Peca.Valor = string.IsNullOrEmpty(value) ? 0 : Convert.ToDouble(valor);
                ((Command)GravarCommand).ChangeCanExecute();
                OnPropertyChanged();
            }
        }

        public string CaminhoImagem
        {
            get
            {
                if (this.Peca.ImagemEmBytes != null)
                    return string.Empty;
                if (string.IsNullOrEmpty(this.Peca.CaminhoImagem))
                    return "consultar.png";
                else if (this.Peca.CaminhoImagem.StartsWith("http"))
                    return this.Peca.CaminhoImagem;
                else
                    return DependencyService.Get<IFotoLoadMediaPlugin>().GetPathToPhoto(Peca.CaminhoImagem);
            }
            set
            {
                this.Peca.CaminhoImagem = value;
                OnPropertyChanged();
                ((Command)GravarCommand).ChangeCanExecute();
            }
        }

        private bool atualizando;
        public bool Atualizando
        {
            get { return this.atualizando; }
            set
            {
                this.atualizando = value;
                OnPropertyChanged();
            }
        }
        private void RegistrarCommands()
        {
            GravarCommand = new Command(async () =>
            {
                var resultadoGravacao = await GravarAsync();
                if (resultadoGravacao.ToLower().Equals("true"))
                    MessagingCenter.Send<string>("Atualização realizada com sucesso.", "InformacaoCRUD");
                else
                    MessagingCenter.Send<string>(resultadoGravacao.ToLower().Equals("false") ? "Erro na comunicação" : resultadoGravacao, "InformacaoCRUD");
            }, () =>
            {
                return !string.IsNullOrEmpty(this.Peca.Nome) && this.Peca.Valor > 0;
            });

            AlbumCommand = new Command(() =>
            {
                MessagingCenter.Send<Peca>(this.Peca, "Album");
            });

        }

        private async Task<string> GravarAsync()
        {
            var ehNovaPeca = (this.Peca.PecaID == null ? true : false);
            Atualizando = true;
            await pecasDAL.UpdateAsync(Peca, Peca.PecaID, nameof(Peca.PecaID));
            var result = await service.PostComArquivo(this.Peca);
            Atualizando = false;
            if (result.ToLower().Equals("false") || !result.ToLower().StartsWith("http"))
                return await Task.FromResult(result);
            else
            {
                if (File.Exists(DependencyService.Get<IFotoLoadMediaPlugin>().GetPathToPhoto(Peca.CaminhoImagem)))
                    File.Delete(DependencyService.Get<IFotoLoadMediaPlugin>().GetPathToPhoto(Peca.CaminhoImagem));
                this.Peca.CaminhoImagem = result;
                await pecasDAL.UpdateAsync(Peca, Peca.PecaID, nameof(Peca.PecaID), true);
            }
            AtualizarPropriedadesParaVisao(ehNovaPeca);
            return await Task.FromResult("true");
        }

        private void AtualizarPropriedadesParaVisao(bool ehNovaPeca)
        {
            if (ehNovaPeca)
            {
                this.Peca = new Peca();
                this.Nome = string.Empty;
                this.Valor = string.Empty;
                this.CaminhoImagem = "consultar.png";
            }
            else
            {
                this.Peca.NotificarListView = true;
            }
        }
        private bool carregandoImagem;
        public bool CarregandoImagem
        {
            get { return carregandoImagem; }
            set
            {
                carregandoImagem = value;
                OnPropertyChanged();
            }
        }
        public byte[] ImagemEmBytes { get { return Peca.ImagemEmBytes; } }

        // Método em CRUDViewModel
        public async Task<Byte[]> GetBytesFromImageAsync(Guid pecaID)
        {
            return await service.GetImagemByIdAsync(pecaID);
        }

        // Método em CRUDViewModel
        public void AtualizarImagemPeca()
        {
            Task.Run(async () => {
                if (Peca.ImagemEmBytes == null && Peca.PecaID != Guid.Empty)
                {
                    Peca.ImagemEmBytes = await GetBytesFromImageAsync(Peca.PecaID);
                    if (Peca.ImagemEmBytes != null)
                    {
                        Peca.CaminhoImagem = string.Empty;
                        OnPropertyChanged(nameof(CaminhoImagem));
                    }
                    OnPropertyChanged(nameof(ImagemEmBytes));
                }
                CarregandoImagem = false;
            });
        }
    }
}
