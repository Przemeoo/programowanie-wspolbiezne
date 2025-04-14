//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.ObjectModel;
using TP.ConcurrentProgramming.Presentation.Model;
using TP.ConcurrentProgramming.Presentation.ViewModel.MVVMLight;
using ModelIBall = TP.ConcurrentProgramming.Presentation.Model.IBall;
using System.Reflection.Metadata;

namespace TP.ConcurrentProgramming.Presentation.ViewModel
{
    public class MainWindowViewModel : ViewModelBase, IDisposable, IDataErrorInfo
    {
        #region ctor

        public MainWindowViewModel() : this(null)
        { }

        internal MainWindowViewModel(ModelAbstractApi modelLayerAPI)
        {
            ModelLayer = modelLayerAPI == null ? ModelAbstractApi.CreateModel() : modelLayerAPI;
            Observer = ModelLayer.Subscribe<ModelIBall>(x => Balls.Add(x));
            StartCommand = new RelayCommand(StartMethod);

        }

        #endregion ctor
        

        #region public API
        public ICommand StartCommand { get; }

        public void Start(int numberOfBalls, double tableWidth, double tableHeight)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));
            ModelLayer.Start(numberOfBalls, tableWidth, tableHeight);
            Observer.Dispose();
        }

        public ObservableCollection<ModelIBall> Balls { get; } = new ObservableCollection<ModelIBall>();

        private double _tableWidth;
        public double TableWidth
        {
            get => _tableWidth;
            set
            {
                _tableWidth = value;
                RaisePropertyChanged(nameof(TableWidth));
            }
        }

        private double _tableHeight;
        public double TableHeight
        {
            get => _tableHeight;
            set
            {
                _tableHeight = value;
                RaisePropertyChanged(nameof(TableHeight));
            }
        }
        private bool inputEnabled = true;
        public bool InputEnabled
        {
            get => inputEnabled;
            set
            {
                if (inputEnabled != value)
                {
                    inputEnabled = value;
                    RaisePropertyChanged(nameof(InputEnabled));
                }
            }
        }

        private string _ballInput;
        public string BallInput
        {
            get => _ballInput;
            set
            {
                _ballInput = value;
                RaisePropertyChanged(nameof(BallInput));
            }
        }

        #endregion public API

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    Balls.Clear();
                    Observer.Dispose();
                    ModelLayer.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                Disposed = true;
            }
        }

        public void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region private

        private IDisposable Observer = null;
        private ModelAbstractApi ModelLayer;
        private bool Disposed = false;
        private bool inputValidation = false;




        private void StartMethod()
        {
            inputValidation = true;
            RaisePropertyChanged(nameof(BallInput));

            if (int.TryParse(BallInput, out int numberOfBalls) && numberOfBalls >= 1 && numberOfBalls <= 15)
            {
                Start(numberOfBalls, TableWidth, TableHeight);
                InputEnabled = false;
                
            }
            else
            {
                
            }
        }


        #endregion private

        #region IDataErrorInfo

        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                if (!inputValidation)
                    return null;

                if (columnName == nameof(BallInput))
                {
                    if (!int.TryParse(BallInput, out int value))
                        return "The value must be a number";

                    if (value < 1 || value > 15)
                        return "The number must be between 1 and 15";
                }

                return null;
            }
        }

        #endregion


    }
}