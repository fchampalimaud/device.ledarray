using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Bonsai.Harp;
using Harp.LedArray.Design.Views;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Harp.LedArray.Design.ViewModels;


public class LedArrayViewModel : ViewModelBase
{
    public string AppVersion { get; set; } = string.Empty;
    public ReactiveCommand<Unit, Unit> LoadDeviceInformation { get; }

    #region Connection Information

    [Reactive] public ObservableCollection<string> Ports { get; set; }
    [Reactive] public string? SelectedPort { get; set; }
    [Reactive] public bool Connected { get; set; }
    [Reactive] public string ConnectButtonText { get; set; } = "Connect";
    public ReactiveCommand<Unit, Unit> ConnectAndGetBaseInfoCommand { get; }

    #endregion

    #region Operations

    public ReactiveCommand<bool, Unit> SaveConfigurationCommand { get; }
    public ReactiveCommand<Unit, Unit> ResetConfigurationCommand { get; }

    #endregion

    #region Device basic information

    [Reactive] public int DeviceID { get; set; }
    [Reactive] public string? DeviceName { get; set; }
    [Reactive] public HarpVersion? HardwareVersion { get; set; }
    [Reactive] public HarpVersion? FirmwareVersion { get; set; }
    [Reactive] public int SerialNumber { get; set; }

    #endregion

    #region Registers

    [Reactive] public LedState EnablePower { get; set; }
    [Reactive] public LedState EnableLedMode { get; set; }
    [Reactive] public LedState EnableLed { get; set; }
    [Reactive] public LedState EnableLedWrite { get; set; }
    [Reactive] public DigitalInputs DigitalInputState { get; set; }
    [Reactive] public DigitalOutputSyncPayload DigitalOutputSync { get; set; }
    [Reactive] public DigitalInputTriggerPayload DigitalInputTrigger { get; set; }
    [Reactive] public PulseModePayload PulseMode { get; set; }
    [Reactive] public byte Led0Power { get; set; }
    [Reactive] public byte Led1Power { get; set; }
    [Reactive] public float Led0PwmFrequency { get; set; }
    [Reactive] public float Led0PwmDutyCycle { get; set; }
    [Reactive] public ushort Led0PwmPulseCounter { get; set; }
    [Reactive] public ushort Led0PulseTimeOn { get; set; }
    [Reactive] public ushort Led0PulseTimeOff { get; set; }
    [Reactive] public ushort Led0PulseTimePulseCounter { get; set; }
    [Reactive] public ushort Led0PulseTimeTail { get; set; }
    [Reactive] public ushort Led0PulseRepeatCounter { get; set; }
    [Reactive] public float Led1PwmFrequency { get; set; }
    [Reactive] public float Led1PwmDutyCycle { get; set; }
    [Reactive] public ushort Led1PwmPulseCounter { get; set; }
    [Reactive] public ushort Led1PulseTimeOn { get; set; }
    [Reactive] public ushort Led1PulseTimeOff { get; set; }
    [Reactive] public ushort Led1PulseTimePulseCounter { get; set; }
    [Reactive] public ushort Led1PulseTimeTail { get; set; }
    [Reactive] public ushort Led1PulseRepeatCounter { get; set; }
    [Reactive] public float Led0PwmReal { get; set; }
    [Reactive] public float Led0PwmDutyCycleReal { get; set; }
    [Reactive] public float Led1PwmReal { get; set; }
    [Reactive] public float Led1PwmDutyCycleReal { get; set; }
    [Reactive] public AuxDigitalOutputs AuxDigitalOutputState { get; set; }
    [Reactive] public byte AuxLedPower { get; set; }
    [Reactive] public DigitalOutputs DigitalOutputState { get; set; }
    [Reactive] public LedArrayEvents EnableEvents { get; set; }

    #endregion

    #region Array collections

    #endregion

    #region Events Flags

    public bool IsEnableLedEnabled
    {
        get
        {
            return EnableEvents.HasFlag(LedArrayEvents.EnableLed);
        }
        set
        {
            if (value)
            {
                EnableEvents |= LedArrayEvents.EnableLed;
            }
            else
            {
                EnableEvents &= ~LedArrayEvents.EnableLed;
            }

            // Notify the UI about the change
            this.RaisePropertyChanged(nameof(IsEnableLedEnabled));
            this.RaisePropertyChanged(nameof(EnableEvents));
        }
    }

    public bool IsDigitalInputStateEnabled
    {
        get
        {
            return EnableEvents.HasFlag(LedArrayEvents.DigitalInputState);
        }
        set
        {
            if (value)
            {
                EnableEvents |= LedArrayEvents.DigitalInputState;
            }
            else
            {
                EnableEvents &= ~LedArrayEvents.DigitalInputState;
            }

            // Notify the UI about the change
            this.RaisePropertyChanged(nameof(IsDigitalInputStateEnabled));
            this.RaisePropertyChanged(nameof(EnableEvents));
        }
    }

    #endregion

    #region LedState_EnablePower Flags

    public bool IsLed0Enabled_EnablePower
    {
        get => EnablePower.HasFlag(LedState.Led0On);
        set
        {
            var newValue = SetExclusiveFlagState(
                EnablePower,
                LedState.Led0On,
                LedState.Led0Off,
                value);

            if (EnablePower == newValue)
                return;

            EnablePower = newValue;
            this.RaisePropertyChanged(nameof(IsLed0Enabled_EnablePower));
        }
    }

    public bool IsLed1Enabled_EnablePower
    {
        get => EnablePower.HasFlag(LedState.Led1On);
        set
        {
            var newValue = SetExclusiveFlagState(
                EnablePower,
                LedState.Led1On,
                LedState.Led1Off,
                value);

            if (EnablePower == newValue)
                return;

            EnablePower = newValue;
            this.RaisePropertyChanged(nameof(IsLed1Enabled_EnablePower));
        }
    }

    #endregion

    #region LedState_EnableLedMode Flags

    public bool IsLed0Enabled_EnableLedMode
    {
        get => EnableLedMode.HasFlag(LedState.Led0On);
        set
        {
            var newValue = SetExclusiveFlagState(
                EnableLedMode,
                LedState.Led0On,
                LedState.Led0Off,
                value);

            if (EnableLedMode == newValue)
                return;

            EnableLedMode = newValue;
            this.RaisePropertyChanged(nameof(IsLed0Enabled_EnableLedMode));
        }
    }

    public bool IsLed1Enabled_EnableLedMode
    {
        get => EnableLedMode.HasFlag(LedState.Led1On);
        set
        {
            var newValue = SetExclusiveFlagState(
                EnableLedMode,
                LedState.Led1On,
                LedState.Led1Off,
                value);

            if (EnableLedMode == newValue)
                return;

            EnableLedMode = newValue;
            this.RaisePropertyChanged(nameof(IsLed1Enabled_EnableLedMode));
        }
    }

    #endregion

    #region LedState_EnableLed Flags (status/event only)

    public bool IsLed0Enabled_EnableLed => EnableLed.HasFlag(LedState.Led0On);

    public bool IsLed1Enabled_EnableLed => EnableLed.HasFlag(LedState.Led1On);

    #endregion
    
    #region LedState_EnableLedWrite Flags

    public bool IsLed0Enabled_EnableLedWrite
    {
        get => EnableLedWrite.HasFlag(LedState.Led0On);
        set
        {
            var newValue = SetExclusiveFlagState(
                EnableLedWrite,
                LedState.Led0On,
                LedState.Led0Off,
                value);

            if (EnableLedWrite == newValue)
                return;

            EnableLedWrite = newValue;
            this.RaisePropertyChanged(nameof(IsLed0Enabled_EnableLedWrite));
        }
    }

    public bool IsLed1Enabled_EnableLedWrite
    {
        get => EnableLedWrite.HasFlag(LedState.Led1On);
        set
        {
            var newValue = SetExclusiveFlagState(
                EnableLedWrite,
                LedState.Led1On,
                LedState.Led1Off,
                value);

            if (EnableLedWrite == newValue)
                return;

            EnableLedWrite = newValue;
            this.RaisePropertyChanged(nameof(IsLed1Enabled_EnableLedWrite));
        }
    }

    #endregion

    #region DigitalInputs_DigitalInputState Flags

    public bool IsDI0Enabled_DigitalInputState
    {
        get
        {
            return DigitalInputState.HasFlag(DigitalInputs.DI0);
        }
        set
        {
            if (value)
            {
                DigitalInputState |= DigitalInputs.DI0;
            }
            else
            {
                DigitalInputState &= ~DigitalInputs.DI0;
            }

            // Notify the UI about the change
            this.RaisePropertyChanged(nameof(IsDI0Enabled_DigitalInputState));
            this.RaisePropertyChanged(nameof(DigitalInputState));
        }
    }

    public bool IsDI1Enabled_DigitalInputState
    {
        get
        {
            return DigitalInputState.HasFlag(DigitalInputs.DI1);
        }
        set
        {
            if (value)
            {
                DigitalInputState |= DigitalInputs.DI1;
            }
            else
            {
                DigitalInputState &= ~DigitalInputs.DI1;
            }

            // Notify the UI about the change
            this.RaisePropertyChanged(nameof(IsDI1Enabled_DigitalInputState));
            this.RaisePropertyChanged(nameof(DigitalInputState));
        }
    }

    #endregion

    #region AuxDigitalOutputs_AuxDigitalOutputState Flags

    public bool IsAux0Enabled_AuxDigitalOutputState
    {
        get => AuxDigitalOutputState.HasFlag(AuxDigitalOutputs.Aux0Set);
        set
        {
            var newValue = SetExclusiveFlagState(
                AuxDigitalOutputState,
                AuxDigitalOutputs.Aux0Set,
                AuxDigitalOutputs.Aux0Clear,
                value);

            if (AuxDigitalOutputState == newValue)
                return;

            AuxDigitalOutputState = newValue;
            this.RaisePropertyChanged(nameof(IsAux0Enabled_AuxDigitalOutputState));
        }
    }

    public bool IsAux1Enabled_AuxDigitalOutputState
    {
        get => AuxDigitalOutputState.HasFlag(AuxDigitalOutputs.Aux1Set);
        set
        {
            var newValue = SetExclusiveFlagState(
                AuxDigitalOutputState,
                AuxDigitalOutputs.Aux1Set,
                AuxDigitalOutputs.Aux1Clear,
                value);

            if (AuxDigitalOutputState == newValue)
                return;

            AuxDigitalOutputState = newValue;
            this.RaisePropertyChanged(nameof(IsAux1Enabled_AuxDigitalOutputState));
        }
    }

    #endregion

    #region DigitalOutputs_DigitalOutputState Flags

    public bool IsDO0Enabled_DigitalOutputState
    {
        get => DigitalOutputState.HasFlag(DigitalOutputs.DO0Set);
        set
        {
            var newValue = SetExclusiveFlagState(
                DigitalOutputState,
                DigitalOutputs.DO0Set,
                DigitalOutputs.DO0Clear,
                value);

            if (DigitalOutputState == newValue)
                return;

            DigitalOutputState = newValue;
            this.RaisePropertyChanged(nameof(IsDO0Enabled_DigitalOutputState));
        }
    }

    public bool IsDO1Enabled_DigitalOutputState
    {
        get => DigitalOutputState.HasFlag(DigitalOutputs.DO1Set);
        set
        {
            var newValue = SetExclusiveFlagState(
                DigitalOutputState,
                DigitalOutputs.DO1Set,
                DigitalOutputs.DO1Clear,
                value);

            if (DigitalOutputState == newValue)
                return;

            DigitalOutputState = newValue;
            this.RaisePropertyChanged(nameof(IsDO1Enabled_DigitalOutputState));
        }
    }

    #endregion
    
    #region PulseMode
    
    public PulseModeConfig Led0SelectedPulseMode
    {
        get => PulseMode.Led0Mode;
        set
        {
            if (PulseMode.Led0Mode == value) return;
    
            PulseMode = new PulseModePayload(value, PulseMode.Led1Mode);
    
            this.RaisePropertyChanged(nameof(Led0SelectedPulseMode));
            this.RaisePropertyChanged(nameof(IsLed0PwmMode));
            this.RaisePropertyChanged(nameof(IsLed0IntervalMode));
        }
    }
    
    public PulseModeConfig Led1SelectedPulseMode
    {
        get => PulseMode.Led1Mode;
        set
        {
            if (PulseMode.Led1Mode == value) return;
    
            PulseMode = new PulseModePayload(PulseMode.Led0Mode, value);
    
            this.RaisePropertyChanged(nameof(Led1SelectedPulseMode));
            this.RaisePropertyChanged(nameof(IsLed1PwmMode));
            this.RaisePropertyChanged(nameof(IsLed1IntervalMode));
        }
    }
    
    public bool IsLed0PwmMode => Led0SelectedPulseMode == PulseModeConfig.Pwm;
    public bool IsLed0IntervalMode => Led0SelectedPulseMode == PulseModeConfig.PulseTime;
    public bool IsLed1PwmMode => Led1SelectedPulseMode == PulseModeConfig.Pwm;
    public bool IsLed1IntervalMode => Led1SelectedPulseMode == PulseModeConfig.PulseTime;
    
    #endregion

    #region Application State

    [ObservableAsProperty] public bool IsLoadingPorts { get; }
    [ObservableAsProperty] public bool IsConnecting { get; }
    [ObservableAsProperty] public bool IsResetting { get; }
    [ObservableAsProperty] public bool IsSaving { get; }

    [Reactive] public bool ShowWriteMessages { get; set; }
    [Reactive] public ObservableCollection<string> HarpEvents { get; set; } = new ObservableCollection<string>();
    [Reactive] public ObservableCollection<string> SentMessages { get; set; } = new ObservableCollection<string>();

    public ReactiveCommand<Unit, Unit> ShowAboutCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> ClearMessagesCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> ShowMessagesCommand { get; private set; }
    public ReactiveCommand<string, Unit> WriteRegisterCommand { get; private set; }

    #endregion

    private Harp.LedArray.AsyncDevice? _device;
    private IObservable<string>? _deviceEventsObservable;
    private IDisposable? _deviceEventsSubscription;

    public LedArrayViewModel()
    {
        var assembly = typeof(LedArrayViewModel).Assembly;
        var informationVersion = assembly.GetName().Version;
        if (informationVersion != null)
            AppVersion = $"v{informationVersion.Major}.{informationVersion.Minor}.{informationVersion.Build}";

        Ports = new ObservableCollection<string>();

        ClearMessagesCommand = ReactiveCommand.Create(() => { SentMessages.Clear(); });
        ShowMessagesCommand = ReactiveCommand.Create(() => { ShowWriteMessages = !ShowWriteMessages; });

        LoadDeviceInformation = ReactiveCommand.CreateFromObservable(LoadUsbInformation);
        LoadDeviceInformation.IsExecuting.ToPropertyEx(this, x => x.IsLoadingPorts);
        LoadDeviceInformation.ThrownExceptions.Subscribe(ex =>
            Console.WriteLine($"Error loading device information with exception: {ex.Message}"));
        //Log.Error(ex, "Error loading device information with exception: {Exception}", ex));

        // can connect if there is a selection and also if the new selection is different than the old one
        var canConnect = this.WhenAnyValue(x => x.SelectedPort)
            .Select(selectedPort => !string.IsNullOrEmpty(selectedPort));

        ShowAboutCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime owner &&
                owner.MainWindow is Window ownerWindow)
            {
                await new About() { DataContext = new AboutViewModel() }.ShowDialog(ownerWindow);
            }
        });

        ConnectAndGetBaseInfoCommand = ReactiveCommand.CreateFromTask(ConnectAndGetBaseInfo, canConnect);
        ConnectAndGetBaseInfoCommand.IsExecuting.ToPropertyEx(this, x => x.IsConnecting);
        ConnectAndGetBaseInfoCommand.ThrownExceptions.Subscribe(ex =>
            //Log.Error(ex, "Error connecting to device with error: {Exception}", ex));
            Console.WriteLine($"Error connecting to device with error: {ex}"));

        var canChangeConfig = this.WhenAnyValue(x => x.Connected).Select(connected => connected);
        // Handle Save and Reset
        SaveConfigurationCommand =
            ReactiveCommand.CreateFromObservable<bool, Unit>(SaveConfiguration, canChangeConfig);
        SaveConfigurationCommand.IsExecuting.ToPropertyEx(this, x => x.IsSaving);
        SaveConfigurationCommand.ThrownExceptions.Subscribe(ex =>
            //Log.Error(ex, "Error saving configuration with error: {Exception}", ex));
            Console.WriteLine($"Error saving configuration with error: {ex}"));

        ResetConfigurationCommand = ReactiveCommand.CreateFromObservable(ResetConfiguration, canChangeConfig);
        ResetConfigurationCommand.IsExecuting.ToPropertyEx(this, x => x.IsResetting);
        ResetConfigurationCommand.ThrownExceptions.Subscribe(ex =>
            //Log.Error(ex, "Error resetting device configuration with error: {Exception}", ex));
            Console.WriteLine($"Error resetting device configuration with error: {ex}"));

        WriteRegisterCommand = ReactiveCommand.CreateFromTask<string>(async registerName =>
        {
            if (_device == null) return;

            var propertyName = registerName;
            var writeMethodName = $"Write{registerName}Async";
            var logRegisterName = registerName;

            if (registerName == nameof(EnableLedWrite))
            {
                propertyName = nameof(EnableLedWrite);
                writeMethodName = "WriteEnableLedAsync";
                logRegisterName = nameof(EnableLed);
            }

            var property = GetType().GetProperty(propertyName);
            if (property == null) return;

            var value = property.GetValue(this);

            // Find the write method for this register
            var writeMethod = _device.GetType().GetMethod(writeMethodName);
            if (writeMethod != null)
            {
                if (writeMethod.Invoke(_device, new[] { value, CancellationToken.None }) is Task task)
                    await task;
                else
                    return;

                RxApp.MainThreadScheduler.Schedule(() =>
                {
                    SentMessages.Add($"{DateTime.Now:HH:mm:ss.fff} - Write {logRegisterName}: {value}");
                });
            }
        });
        WriteRegisterCommand.IsExecuting.ToPropertyEx(this, x => x.IsSaving);
        WriteRegisterCommand.ThrownExceptions.Subscribe(ex =>
            //Log.Error(ex, "Error writing register with error: {Exception}", ex));
            Console.WriteLine($"Error writing register with error: {ex}"));

        this.WhenAnyValue(x => x.Connected)
            .Subscribe(x => { ConnectButtonText = x ? "Disconnect" : "Connect"; });

        this.WhenAnyValue(x => x.EnableEvents)
            .Subscribe(x =>
            {
                IsEnableLedEnabled = x.HasFlag(LedArrayEvents.EnableLed);
                IsDigitalInputStateEnabled = x.HasFlag(LedArrayEvents.DigitalInputState);
            });

        // handle the events from the device
        // When Connected changes subscribe/unsubscribe the device events.
        this.WhenAnyValue(x => x.Connected)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(isConnected =>
            {
                if (isConnected && _deviceEventsObservable is not null)
                {
                    // Subscribe on the UI thread so that the HarpEvents collection can be updated safely.
                    SubscribeToEvents(_deviceEventsObservable);
                }
                else
                {
                    // Dispose subscription and clear messages.
                    _deviceEventsSubscription?.Dispose();
                    _deviceEventsSubscription = null;
                }
            });

        this.WhenAnyValue(x => x.EnablePower)
            .Subscribe(x =>
            {
                this.RaisePropertyChanged(nameof(IsLed0Enabled_EnablePower));
                this.RaisePropertyChanged(nameof(IsLed1Enabled_EnablePower));
            });

        this.WhenAnyValue(x => x.EnableLedMode)
            .Subscribe(x =>
            {
                this.RaisePropertyChanged(nameof(IsLed0Enabled_EnableLedMode));
                this.RaisePropertyChanged(nameof(IsLed1Enabled_EnableLedMode));
            });

        this.WhenAnyValue(x => x.EnableLed)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(IsLed0Enabled_EnableLed));
                this.RaisePropertyChanged(nameof(IsLed1Enabled_EnableLed));
            });

        this.WhenAnyValue(x => x.EnableLedWrite)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(IsLed0Enabled_EnableLedWrite));
                this.RaisePropertyChanged(nameof(IsLed1Enabled_EnableLedWrite));
            });

        this.WhenAnyValue(x => x.DigitalInputState)
            .Subscribe(x =>
            {
                IsDI0Enabled_DigitalInputState = x.HasFlag(DigitalInputs.DI0);
                IsDI1Enabled_DigitalInputState = x.HasFlag(DigitalInputs.DI1);
            });

        this.WhenAnyValue(x => x.AuxDigitalOutputState)
            .Subscribe(x =>
            {
                this.RaisePropertyChanged(nameof(IsAux0Enabled_AuxDigitalOutputState));
                this.RaisePropertyChanged(nameof(IsAux1Enabled_AuxDigitalOutputState));
            });

        this.WhenAnyValue(x => x.DigitalOutputState)
            .Subscribe(x =>
            {
                this.RaisePropertyChanged(nameof(IsDO0Enabled_DigitalOutputState));
                this.RaisePropertyChanged(nameof(IsDO1Enabled_DigitalOutputState));
            });

        this.WhenAnyValue(x => x.PulseMode)
            .Subscribe(x =>
            {
                this.RaisePropertyChanged(nameof(Led0SelectedPulseMode));
                this.RaisePropertyChanged(nameof(Led1SelectedPulseMode));
                this.RaisePropertyChanged(nameof(IsLed0PwmMode));
                this.RaisePropertyChanged(nameof(IsLed1PwmMode));
                this.RaisePropertyChanged(nameof(IsLed0IntervalMode));
                this.RaisePropertyChanged(nameof(IsLed1IntervalMode));
            });

        // force initial population of currently connected ports
        LoadUsbInformation();
    }

    private IObservable<Unit> LoadUsbInformation()
    {
        return Observable.Start(() =>
        {
            var devices = SerialPort.GetPortNames();

            if (OperatingSystem.IsMacOS())
                // except with Bluetooth in the name
                Ports = new ObservableCollection<string>(devices.Where(d => d.Contains("cu.")).Except(devices.Where(d => d.Contains("Bluetooth"))));
            else
                Ports = new ObservableCollection<string>(devices);

            Console.WriteLine("Loaded USB information");
            //Log.Information("Loaded USB information");
        });
    }

    private async Task ConnectAndGetBaseInfo()
    {
        if (string.IsNullOrEmpty(SelectedPort))
            throw new InvalidOperationException("invalid parameter");

        if (Connected)
        {
            _device?.Dispose();
            _device = null;
            Connected = false;
            SentMessages.Clear();
            return;
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
            _device = await Harp.LedArray.Device.CreateAsync(SelectedPort, cts.Token);
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine($"Error connecting to device with error: {ex}");
            //Log.Error(ex, "Error connecting to device with error: {Exception}", ex);
            var messageBoxStandardWindow = MessageBoxManager
                .GetMessageBoxStandard("Unexpected device found",
                    "Timeout when trying to connect to a device. Most likely not an Harp device.",
                    icon: Icon.Error);
            await messageBoxStandardWindow.ShowAsync();
            _device?.Dispose();
            _device = null;
            return;

        }
        catch (HarpException ex)
        {
            Console.WriteLine($"Error connecting to device with error: {ex}");
            //Log.Error(ex, "Error connecting to device with error: {Exception}", ex);

            var messageBoxStandardWindow = MessageBoxManager
                .GetMessageBoxStandard("Unexpected device found",
                    ex.Message,
                    icon: Icon.Error);
            await messageBoxStandardWindow.ShowAsync();

            _device?.Dispose();
            _device = null;
            return;
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"COM port still in use and most likely not the expected Harp device");
            var messageBoxStandardWindow = MessageBoxManager
                .GetMessageBoxStandard("Unexpected device found",
                    $"COM port still in use and most likely not the expected Harp device.{Environment.NewLine}Specific error: {ex.Message}",
                    icon: Icon.Error);
            await messageBoxStandardWindow.ShowAsync();

            _device?.Dispose();
            _device = null;
            return;
        }

        // Clear the sent messages list
        SentMessages.Clear();

        //Log.Information("Attempting connection to port \'{SelectedPort}\'", SelectedPort);
        Console.WriteLine($"Attempting connection to port \'{SelectedPort}\'");

        DeviceID = await _device.ReadWhoAmIAsync();
        DeviceName = await _device.ReadDeviceNameAsync();
        HardwareVersion = await _device.ReadHardwareVersionAsync();
        FirmwareVersion = await _device.ReadFirmwareVersionAsync();
        try
        {
            // some devices may not have a serial number
            SerialNumber = await _device.ReadSerialNumberAsync();
        }
        catch (HarpException)
        {
            // Device does not have a serial number, simply continue by ignoring the exception
        }

        EnablePower = await _device.ReadEnablePowerAsync();
        EnableLedMode = await _device.ReadEnableLedModeAsync();
        var enableLed = await _device.ReadEnableLedAsync();
        EnableLed = enableLed;
        EnableLedWrite = enableLed;
        DigitalInputState = await _device.ReadDigitalInputStateAsync();
        DigitalOutputSync = await _device.ReadDigitalOutputSyncAsync();
        DigitalInputTrigger = await _device.ReadDigitalInputTriggerAsync();
        PulseMode = await _device.ReadPulseModeAsync();
        Led0Power = await _device.ReadLed0PowerAsync();
        Led1Power = await _device.ReadLed1PowerAsync();
        Led0PwmFrequency = await _device.ReadLed0PwmFrequencyAsync();
        Led0PwmDutyCycle = await _device.ReadLed0PwmDutyCycleAsync();
        Led0PwmPulseCounter = await _device.ReadLed0PwmPulseCounterAsync();
        Led0PulseTimeOn = await _device.ReadLed0PulseTimeOnAsync();
        Led0PulseTimeOff = await _device.ReadLed0PulseTimeOffAsync();
        Led0PulseTimePulseCounter = await _device.ReadLed0PulseTimePulseCounterAsync();
        Led0PulseTimeTail = await _device.ReadLed0PulseTimeTailAsync();
        Led0PulseRepeatCounter = await _device.ReadLed0PulseRepeatCounterAsync();
        Led1PwmFrequency = await _device.ReadLed1PwmFrequencyAsync();
        Led1PwmDutyCycle = await _device.ReadLed1PwmDutyCycleAsync();
        Led1PwmPulseCounter = await _device.ReadLed1PwmPulseCounterAsync();
        Led1PulseTimeOn = await _device.ReadLed1PulseTimeOnAsync();
        Led1PulseTimeOff = await _device.ReadLed1PulseTimeOffAsync();
        Led1PulseTimePulseCounter = await _device.ReadLed1PulseTimePulseCounterAsync();
        Led1PulseTimeTail = await _device.ReadLed1PulseTimeTailAsync();
        Led1PulseRepeatCounter = await _device.ReadLed1PulseRepeatCounterAsync();
        Led0PwmReal = await _device.ReadLed0PwmRealAsync();
        Led0PwmDutyCycleReal = await _device.ReadLed0PwmDutyCycleRealAsync();
        Led1PwmReal = await _device.ReadLed1PwmRealAsync();
        Led1PwmDutyCycleReal = await _device.ReadLed1PwmDutyCycleRealAsync();
        AuxDigitalOutputState = await _device.ReadAuxDigitalOutputStateAsync();
        AuxLedPower = await _device.ReadAuxLedPowerAsync();
        DigitalOutputState = await _device.ReadDigitalOutputStateAsync();
        EnableEvents = await _device.ReadEnableEventsAsync();


        // generate observable for the _deviceSync
        _deviceEventsObservable = GenerateEventMessages();

        Connected = true;

        //Log.Information("Connected to device");
        Console.WriteLine("Connected to device");
    }

    public IObservable<string> GenerateEventMessages()
    {
        return Observable.Create<string>(async (observer, cancellationToken) =>
        {
            // Loop until cancellation is requested or the device is no longer available.
            while (!cancellationToken.IsCancellationRequested && _device != null)
            {
                // Capture local reference and check for null.
                var device = _device;
                if (device == null)
                {
                    observer.OnCompleted();
                    break;
                }

                try
                {
                    // Check if EnableLed event is enabled
                    if (IsEnableLedEnabled)
                    {
                        var result = await device.ReadEnableLedAsync(cancellationToken);
                        EnableLed = result;
                        observer.OnNext($"EnableLed: {result}");
                    }

                    // Check if DigitalInputState event is enabled
                    if (IsDigitalInputStateEnabled)
                    {
                        var result = await device.ReadDigitalInputStateAsync(cancellationToken);
                        // Update the corresponding property with the result
                        DigitalInputState = result;
                        observer.OnNext($"DigitalInputState: {result}");
                    }

                    // Wait a short while before polling again. Adjust delay as necessary.
                    await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                    break;
                }
            }
            observer.OnCompleted();
            return Disposable.Empty;
        });
    }

    private IObservable<Unit> SaveConfiguration(bool savePermanently)
    {
        return Observable.StartAsync(async () =>
        {
            if (_device == null)
                throw new Exception("You need to connect to the device first");

            await WriteAndLogAsync(
                value => _device.WriteEnablePowerAsync(value),
                EnablePower,
                "EnablePower");
            await WriteAndLogAsync(
                value => _device.WriteEnableLedAsync(value),
                EnableLedWrite,
                "EnableLed");
            await WriteAndLogAsync(
                value => _device.WriteEnableLedModeAsync(value),
                EnableLedMode,
                "EnableLedMode");
            await WriteAndLogAsync(
                value => _device.WriteDigitalOutputSyncAsync(value),
                DigitalOutputSync,
                "DigitalOutputSync");
            await WriteAndLogAsync(
                value => _device.WriteDigitalInputTriggerAsync(value),
                DigitalInputTrigger,
                "DigitalInputTrigger");
            await WriteAndLogAsync(
                value => _device.WritePulseModeAsync(value),
                PulseMode,
                "PulseMode");
            await WriteAndLogAsync(
                value => _device.WriteLed0PowerAsync(value),
                Led0Power,
                "Led0Power");
            await WriteAndLogAsync(
                value => _device.WriteLed1PowerAsync(value),
                Led1Power,
                "Led1Power");
            await WriteAndLogAsync(
                value => _device.WriteLed0PwmFrequencyAsync(value),
                Led0PwmFrequency,
                "Led0PwmFrequency");
            await WriteAndLogAsync(
                value => _device.WriteLed0PwmDutyCycleAsync(value),
                Led0PwmDutyCycle,
                "Led0PwmDutyCycle");
            await WriteAndLogAsync(
                value => _device.WriteLed0PwmPulseCounterAsync(value),
                Led0PwmPulseCounter,
                "Led0PwmPulseCounter");
            await WriteAndLogAsync(
                value => _device.WriteLed0PulseTimeOnAsync(value),
                Led0PulseTimeOn,
                "Led0PulseTimeOn");
            await WriteAndLogAsync(
                value => _device.WriteLed0PulseTimeOffAsync(value),
                Led0PulseTimeOff,
                "Led0PulseTimeOff");
            await WriteAndLogAsync(
                value => _device.WriteLed0PulseTimePulseCounterAsync(value),
                Led0PulseTimePulseCounter,
                "Led0PulseTimePulseCounter");
            await WriteAndLogAsync(
                value => _device.WriteLed0PulseTimeTailAsync(value),
                Led0PulseTimeTail,
                "Led0PulseTimeTail");
            await WriteAndLogAsync(
                value => _device.WriteLed0PulseRepeatCounterAsync(value),
                Led0PulseRepeatCounter,
                "Led0PulseRepeatCounter");
            await WriteAndLogAsync(
                value => _device.WriteLed1PwmFrequencyAsync(value),
                Led1PwmFrequency,
                "Led1PwmFrequency");
            await WriteAndLogAsync(
                value => _device.WriteLed1PwmDutyCycleAsync(value),
                Led1PwmDutyCycle,
                "Led1PwmDutyCycle");
            await WriteAndLogAsync(
                value => _device.WriteLed1PwmPulseCounterAsync(value),
                Led1PwmPulseCounter,
                "Led1PwmPulseCounter");
            await WriteAndLogAsync(
                value => _device.WriteLed1PulseTimeOnAsync(value),
                Led1PulseTimeOn,
                "Led1PulseTimeOn");
            await WriteAndLogAsync(
                value => _device.WriteLed1PulseTimeOffAsync(value),
                Led1PulseTimeOff,
                "Led1PulseTimeOff");
            await WriteAndLogAsync(
                value => _device.WriteLed1PulseTimePulseCounterAsync(value),
                Led1PulseTimePulseCounter,
                "Led1PulseTimePulseCounter");
            await WriteAndLogAsync(
                value => _device.WriteLed1PulseTimeTailAsync(value),
                Led1PulseTimeTail,
                "Led1PulseTimeTail");
            await WriteAndLogAsync(
                value => _device.WriteLed1PulseRepeatCounterAsync(value),
                Led1PulseRepeatCounter,
                "Led1PulseRepeatCounter");
            await WriteAndLogAsync(
                value => _device.WriteAuxDigitalOutputStateAsync(value),
                AuxDigitalOutputState,
                "AuxDigitalOutputState");
            await WriteAndLogAsync(
                value => _device.WriteAuxLedPowerAsync(value),
                AuxLedPower,
                "AuxLedPower");
            await WriteAndLogAsync(
                value => _device.WriteDigitalOutputStateAsync(value),
                DigitalOutputState,
                "DigitalOutputState");
            await WriteAndLogAsync(
                value => _device.WriteEnableEventsAsync(value),
                EnableEvents,
                "EnableEvents");

            // Save the configuration to the device permanently
            if (savePermanently && _deviceEventsObservable is not null)
            {
                // To prevent multiple calls to the device while it is resetting
                _deviceEventsSubscription?.Dispose();
                _deviceEventsSubscription = null;

                await WriteAndLogAsync(
                    value => _device.WriteResetDeviceAsync(value),
                    ResetFlags.Save,
                    "SavePermanently");

                // Wait to ensure the device is ready after the reset
                await Task.Delay(4000);

                // Re-subscribe to the device events observable
                SubscribeToEvents(_deviceEventsObservable);
            }

            // read the read-only values from the device again 
            await ReadRuntimeStatusRegistersAsync();
        });
    }

    private IObservable<Unit> ResetConfiguration()
    {
        return Observable.StartAsync(async () =>
        {
            if (_device != null)
            {
                await WriteAndLogAsync(
                    value => _device.WriteResetDeviceAsync(value),
                    ResetFlags.RestoreDefault,
                    "ResetDevice");
            }
        });
    }

    private async Task ReadRuntimeStatusRegistersAsync(CancellationToken ct = default)
    {
        if (_device == null)
            throw new Exception("Device is not connected");

        // On get read-only registers
        Led0PwmReal = await _device.ReadLed0PwmRealAsync(ct);
        Led0PwmDutyCycleReal = await _device.ReadLed0PwmDutyCycleRealAsync(ct);
        Led1PwmReal = await _device.ReadLed1PwmRealAsync(ct);
        Led1PwmDutyCycleReal = await _device.ReadLed1PwmDutyCycleRealAsync(ct);

        // Runtime output states
        AuxDigitalOutputState = await _device.ReadAuxDigitalOutputStateAsync(ct);
        DigitalOutputState = await _device.ReadDigitalOutputStateAsync(ct);

        // Other runtime-visible state you may want refreshed post-save
        EnableEvents = await _device.ReadEnableEventsAsync(ct);
    }

    private async Task WriteAndLogAsync<T>(Func<T, Task> writeFunc, T value, string registerName)
    {
        if (_device == null)
            throw new Exception("Device is not connected");

        await writeFunc(value);

        // Log the message to the SentMessages collection on the UI thread
        RxApp.MainThreadScheduler.Schedule(() =>
        {
            SentMessages.Add($"{DateTime.Now:HH:mm:ss.fff} - Write {registerName}: {value}");
        });
    }

    private void SubscribeToEvents(IObservable<string> deviceEvents)
    {
        _deviceEventsSubscription = deviceEvents
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(
                msg => HarpEvents.Add(msg.ToString()),
                ex => Debug.WriteLine($"Error in device events: {ex}")
            );
    }

    private static LedState SetExclusiveFlagState(
        LedState current,
        LedState onFlag,
        LedState offFlag,
        bool isEnabled)
    {
        current &= ~(onFlag | offFlag);
        current |= isEnabled ? onFlag : offFlag;
        return current;
    }
    
private static AuxDigitalOutputs SetExclusiveFlagState(
        AuxDigitalOutputs current,
        AuxDigitalOutputs setFlag,
        AuxDigitalOutputs clearFlag,
        bool isSet)
    {
        current &= ~(setFlag | clearFlag);
        current |= isSet ? setFlag : clearFlag;
        return current;
    }
    
    private static DigitalOutputs SetExclusiveFlagState(
        DigitalOutputs current,
        DigitalOutputs setFlag,
        DigitalOutputs clearFlag,
        bool isSet)
    {
        current &= ~(setFlag | clearFlag);
        current |= isSet ? setFlag : clearFlag;
        return current;
    }
    
}
