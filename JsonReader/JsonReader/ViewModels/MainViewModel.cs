using Bogus;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using JsonReader.Commands;
using JsonReader.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace JsonReader.ViewModels
{
    public class MainViewModel : DependencyObject
    {

        public Dispatcher MyDispatcher { get; set; } = Dispatcher.CurrentDispatcher;
        public CancellationTokenSource CancellationToken { get; set; } = new();


        public Commands.RelayCommand StartOperationCommand { get; set; }
        public Commands.RelayCommand CancelOperationCommand { get; set; }


        public const int carCount = 500;

        public ObservableCollection<Car> Cars
        {
            get { return (ObservableCollection<Car>)GetValue(CarsProperty); }
            set { SetValue(CarsProperty, value); }
        }

        public static readonly DependencyProperty CarsProperty =
            DependencyProperty.Register("Cars", typeof(ObservableCollection<Car>), typeof(MainViewModel));



        public string Time
        {
            get { return (string)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof(string), typeof(MainViewModel));


        public Stopwatch Stopwatch { get; set; } = new();

        public bool IsMultiThreadOperation { get; set; } = false;
        public bool ThreadStart { get; set; } = false;

        static MainViewModel()
        {
            if (Directory.Exists("Cars"))
            {
                if (Directory.GetFiles($@"{Environment.CurrentDirectory}\Cars").Length == carCount) return;
                else
                {
                    foreach (var item in Directory.GetFiles($@"{Environment.CurrentDirectory}\Cars")) File.Delete(item);
                }
            }
            if (!Directory.Exists("Cars")) Directory.CreateDirectory("Cars");

            var carGenerator = new Faker<Car>()
                .RuleFor(c => c.Model, f => f.Vehicle.Model())
                .RuleFor(c => c.Vendor, f => f.Vehicle.Manufacturer())
                .RuleFor(c => c.Year, f => f.Random.Number(2010, 2023))
                .RuleFor(c => c.ImagePath, f => f.Image.Transport());

            foreach (var item in carGenerator.Generate(carCount))
            {
                var json = JsonConvert.SerializeObject(item, Formatting.Indented);
                File.WriteAllText($@"{Environment.CurrentDirectory}\Cars\{item.Id}.json", json);
            }
        }

        public MainViewModel()
        {
            StartOperationCommand = new(
                execute: (sender) => StartOperation(),
                canExecute: (sender) => !ThreadStart
                );
            CancelOperationCommand = new((sender) => CancellationToken.Cancel(), (sender) => ThreadStart);
            Cars = new();
            Time = "00:00:00:0000";
        }

        private void StartOperation()
        {
            Cars.Clear();
            Time = "00:00:00:0000";

            ThreadStart = true;

            Stopwatch.Start();

            if (IsMultiThreadOperation)
            {
                foreach (var item in Directory.GetFiles($@"{Environment.CurrentDirectory}\Cars"))
                {
                    ThreadPool.QueueUserWorkItem(MultiThreadOperation, item);
                }
            }
            else
                ThreadPool.QueueUserWorkItem(SingleThreadOperation);

        }

        public void SingleThreadOperation(object? path)
        {
            foreach (var item in Directory.GetFiles($@"{Environment.CurrentDirectory}\Cars"))
            {
                if (item.EndsWith(".json"))
                {
                    var json = File.ReadAllText(item);
                    var car = JsonConvert.DeserializeObject<Car>(json);

                    if (CancellationToken.Token.IsCancellationRequested)
                    {
                        MyDispatcher.Invoke(() => Cars.Clear());

                        Reset();
                        return;
                    }
                    if (car is not null) MyDispatcher.Invoke(() => Cars.Add(car));
                }

                if (CancellationToken.Token.IsCancellationRequested)
                {
                    MyDispatcher.Invoke(() => Cars.Clear());

                    Reset();
                    return;
                }
            }
            Reset();
        }

        public void MultiThreadOperation(object? path)
        {
            lock (MyDispatcher.Invoke(() => Cars))
            {
                var json = File.ReadAllText(path.ToString());
                var car = JsonConvert.DeserializeObject<Car>(json);

                if (CancellationToken.Token.IsCancellationRequested)
                {
                    MyDispatcher.Invoke(() => Cars.Clear());

                    Reset();
                    return;
                }
                if (car is not null) MyDispatcher.Invoke(() => Cars.Add(car));
            }
            if (MyDispatcher.Invoke(() => Cars).Count == carCount) Reset();
        }

        public void Reset()
        {
            ThreadStart = false;
            Stopwatch.Stop();

            MyDispatcher.Invoke(() => Time = TimeSpan.FromMilliseconds(Stopwatch.ElapsedMilliseconds).ToString());

            CancellationToken = new();
            Stopwatch = new();
        }


    }
}
